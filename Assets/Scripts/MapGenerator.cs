using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    // 맵 생성에 필요한 private 변수들
    int[] _floorPlan;           // 맵 전체를 1D 배열로 저장하는 공간
    public int[] GetFloorPlan => _floorPlan;

    int _floorPlanCount;        // 지금까지 생성된 방
    int _minRooms;              // 최소 방 개수
    int _maxRooms;              // 최대 방 개수
    List<int> _endRooms;        // 현재 생성된 방들 중 끝 방의 인덱스를 담는 리스트
    List<int> _bigRoomIndexes;  // 큰 방들의 인덱스를 담는 리스트

    int _bossRoomIndex;         // 보스 방 셀의 인덱스
    int _secretRoomIndex;       // 비밀 방 셀의 인덱스
    int _shopRoomIndex;         // 상점 방 셀의 인덱스
    int _itemRoomIndex;         // 아이템 방 셀의 인덱스

    public Cell CellPrefab;     // 실제 화면에 찍어줄 방 Prefab
    float _cellSize;            // 셀 사이 간격
    Queue<int> _cellQueue;      // 단계적 방 생성 시 사용할 큐
    List<Cell> _spawnCells;     // Scene에 실제로 Instantiate한 방 오브젝트

    public List<Cell> GetSpawnCells => _spawnCells;

    [Header("---- 스프라이트 참조 ----")]
    [SerializeField] Sprite _item;
    [SerializeField] Sprite _shop;
    [SerializeField] Sprite _boss;
    [SerializeField] Sprite _secret;

    [Header("---- 변형 방들 ----")]
    [SerializeField] Sprite _largeRoom;
    [SerializeField] Sprite _verticalRoom;
    [SerializeField] Sprite _horizontalRoom;
    [SerializeField] Sprite _LShapeRoom;

    // 다른 스크립트에서 MapGenerator.instance로 접근 가능
    public static MapGenerator instance;

    /// <summary>
    /// 방 모양에 대한 읽기전용 Static 리스트
    /// </summary>
    static readonly List<int[]> _roomShapes = new()
    {
        // 1x2 크기의 가로 방 모양
        new int[] {-1},
        new int[] {1},

        // 2x1 크기의 세로 방 모양
        new int[] {10},
        new int[] {-10},

        // L자 모양 방
        new int[] {1, 10},
        new int[] {1, 11},
        new int[] {10, 11},

        new int[] {9, 10},
        new int[] {-1, 9},
        new int[] {-1, 10},

        new int[] {1, -10},
        new int[] {1, -9},
        new int[] {-9, -10},

        new int[] {-1, -10},
        new int[] {-1, -11},
        new int[] {-10, -11},

        // 2x2 크기의 사각형 방 모양
        new int[] {1, 10, 11},
        new int[] {1, -9, -10},
        new int[] {-1, 9, 10},
        new int[] {-1, -10, -11}
    };

    void Start()
    {
        instance = this;

        // 방 크기 및 셀 사이즈
        _minRooms = 7;
        _maxRooms = 15;
        _cellSize = 1f;

        // 새로운 리스트 생성
        _spawnCells = new();

        SetUpDungeon();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetUpDungeon();
        }
    }

    /// <summary>
    /// 기존 셀 파괴 및 값 초기화
    /// </summary>
    void SetUpDungeon() 
    {
        for(int i = 0; i < _spawnCells.Count; i++)
        {
            Destroy(_spawnCells[i].gameObject);
        }

        _spawnCells.Clear();

        _floorPlan = new int[100];
        _floorPlanCount = default;
        _cellQueue = new Queue<int>();
        _endRooms = new List<int>();
        _bigRoomIndexes = new List<int>();

        // 중간 값 45로 설정
        VisitCell(45);

        // 던전 생성 함수 호출
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        while (_cellQueue.Count > 0)   // 큐가 빌 때까지 반복
        {
            // Dequeue : 큐에서 데이터를 꺼내는 연산
            int index = _cellQueue.Dequeue();

            // index → 1차원 배열 상의 위치값
            // x → 가로좌표 (열 번호)
            int x = index % 10;

            bool created = false;

            // 왼쪽 셀 확인 (x > 1이면, 즉 맨 왼쪽 벽이 아니면)
            // |= 비트 OR 대입 연산자
            // A |= B -> A = A|B
            if (x > 1) created |= VisitCell(index - 1);

            // 오른쪽 셀 확인 (x < 9이면, 즉 맨 오른쪽 벽이 아니면)
            if (x < 9) created |= VisitCell(index + 1);

            // 위쪽 셀 확인 (index > 20이면, 위로 올라가도 범위 안임)
            if (index > 20) created |= VisitCell(index - 10);

            // 아래쪽 셀 확인 (index < 70이면, 아래로 내려가도 범위 안임)
            if (index < 70) created |= VisitCell(index + 10);

            // 최종 방 목록에 추가
            if (created == false)
            {
                _endRooms.Add(index);
            }
        }

        if(_floorPlanCount < _minRooms)
        {
            SetUpDungeon();
            return;
        }

        CleanEndRoomsList();

        SetUpSpecialRooms();
    }

    /// <summary>
    /// 최종 방 목록을 업데이트하여 실수로 큰 방이 포함되지 않도록 함
    /// 큰 방은 EndRoom이 될수는 있지만 SpecialRoom이 되지는 원치 않기 때문
    /// </summary>
    void CleanEndRoomsList()
    {
        _endRooms.RemoveAll(item => _bigRoomIndexes.Contains(item) || GetNeighbourCount(item) > 1);
    }

    void SetUpSpecialRooms() 
    {
        // 삼향 연산자 : _endRooms에 방이 하나라도 있으면 마지막 방을 보스방으로, 아니면 -1로 표시
        _bossRoomIndex = _endRooms.Count > 0 ? _endRooms[_endRooms.Count - 1] : -1;

        // 보스 방이 유효하다면
        if(_bossRoomIndex != -1)
        {
            // _endRooms 목록에서 그 보스 방을 제거(중복 배정 방지)
            _endRooms.RemoveAt(_endRooms.Count - 1);
        }

        // 나머지 특수 방들을 막다른 방들에서 랜덤으로 뽑음(실패 시 -1)
        _itemRoomIndex = RandomEndRoom();
        _shopRoomIndex = RandomEndRoom();
        _secretRoomIndex = PickSecretRoom();

        // 하나라도 실패거나 보스 방이 없다면 던전 셋업 다시 진행
        if(_itemRoomIndex == -1 ||  _shopRoomIndex == -1 || _secretRoomIndex == -1 || _bossRoomIndex == -1)
        {
            SetUpDungeon();
            return;
        }

        SpawnRoom(_secretRoomIndex);

        UpdateSpecialRoomVisuals();
        RoomManager.instance.SetUpRooms(_spawnCells);
    }

    void UpdateSpecialRoomVisuals()
    {
        foreach(var cell in _spawnCells)
        {
            if(cell.index == _itemRoomIndex)
            {
                cell.SetSpecialRoomSprite(_item);
                cell.SetRoomType(RoomType.Item);
            }

            if(cell.index == _shopRoomIndex)
            {
                cell.SetSpecialRoomSprite(_shop);
                cell.SetRoomType(RoomType.Shop);
            }

            if(cell.index == _secretRoomIndex)
            {
                cell.SetSpecialRoomSprite(_secret);
                cell.SetRoomType(RoomType.Secret);
            }

            if( cell.index == _bossRoomIndex)
            {
                cell.SetSpecialRoomSprite(_boss);
                cell.SetRoomType(RoomType.Boss);
            }
        }
    }

    int RandomEndRoom()
    {
        // _endRooms 리스트가 비어있으면 더이상 선택할 방이 없으므로 -1 리턴
        if (_endRooms.Count == 0) return -1;

        // 0 이상 _endRooms.Count 범위 내의 랜덤한 정수 뽑기
        int randomRoom = Random.Range(0, _endRooms.Count);

        // _endRooms 리스트에서 해당 인덱스의 실제 방 번호(Id)를 가져옴
        int index = _endRooms[randomRoom];

        // 방 선택 후, 중복 선택을 막기 위해 리스트에서 제거
        _endRooms.RemoveAt(randomRoom);

        // 선택된 방의 인덱스 리턴
        return index;
    }

    int PickSecretRoom()
    {
        // 너무 오래 못 찾는 경우를 막기위한 안전장치 : 최대 900번  시도
        for(int attempt = 0; attempt < 900; attempt++)
        {
            // x는 [1~9], y는 [2~9] 범위에서 무작위 선택
            // 경계(벽)로부터 1칸씩 띄어놓기 위해
            int x = Mathf.FloorToInt(Random.Range(0f, 1f) * 9) + 1;
            int y = Mathf.FloorToInt(Random.Range(0f, 1f) * 8) + 2;

            // 1차원 Index 
            int index = y * 10 + x;

            // 빈 칸(=0)만 비밀방 후보로 사용. 0이 아니면 이미 방/벽 등이 이미 점유
            if (_floorPlan[index] != 0)
            {
                continue;
            } 

            // 보스 방과 상 하 좌 우로 인접하지 않도록
            if(_bossRoomIndex == index - 1 || _bossRoomIndex == index + 1 ||  _bossRoomIndex == index + 10 || _bossRoomIndex == index - 10)
            {
                continue;
            }

            // 이웃 인덱스가 배열 범위 밖이면 제외시킴
            if(index -1 < 0 || index + 1 > _floorPlan.Length || index - 10 < 0 || index + 10 > _floorPlan.Length)
            {
                continue;
            }

            // 이웃 방 개수 구하기
            int neihbours = GetNeighbourCount(index);

            // 허용 기준을 점점 완화
            if(neihbours >= 3 || (attempt > 300 && neihbours >= 2) || (attempt > 600 && neihbours >= 1))
            {
                return index;
            }
        }

        // 900번 안에 못 찾으면 실패
        return -1;
    }

    /// <summary>
    /// 배열 요소의 인덱스 검색 후
    /// 해당 요소의 점유 상태 확인 후 0~4 사이의 값을 리턴
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private int GetNeighbourCount(int index)
    {
        // 상 하 좌 우의 몇 개의 방이 있는 지 확인
        // 만약 _floorPlan[index - 1]만 1이고 나머지가 0이면 이웃 방 개수 1
        return _floorPlan[index - 10] + _floorPlan[index - 1] + _floorPlan[index + 1] + _floorPlan[index + 10];  
    }

    /// <summary>
    /// 셀을 방문하는 함수
    /// 몇 가지 확인에서 성공하면 현재 Cell 설정 전에 false 반환
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private bool VisitCell(int index)
    {
        // 비어 있는 셀이 아닌지 확인
        if (_floorPlan[index] != 0) return false;

        // 이웃 수가 1보다 큰지 확인
        if (GetNeighbourCount(index) > 1) return false;

        // 지금까지 생성된 방이 _maxRooms의 수보다 큰지 확인
        if (_floorPlanCount > _maxRooms) return false;

        // 무작위성을 도입하여 맵이 너무 촘촘하지 않게, 생성에 변형(랜덤성)을 주기 위해 넣은 장치
        // 50% 확률로 이 셀 생성
        if (Random.value < 0.5f) return false;

        // 30% 확률로 실제로 큰 방을 만듦
        if(Random.value < 0.3f && index != 45)
        {
            // OrderBy : 항상 지정된 순서대로 만들지 않도록
            foreach(var shape in _roomShapes.OrderBy(_ => Random.value))
            {
                if (TryPlaceRoom(index, shape))
                {
                    // 루프 취소 후 배치. 큰 방이 우선적으로 적용
                    return true;
                }
            }
        }

        // 모든 if 통과 시 업데이트 할 수 있는 셀
        // Enqueue : 큐에 데이터를 넣는 연산
        _cellQueue.Enqueue(index);

        // 해당 방의 index 1로 바꾸고 현재 방 개수 + 1
        _floorPlan[index] = 1;
        _floorPlanCount++;

        // 방 생성
        SpawnRoom(index);

        return true;
    }

    private void SpawnRoom(int index)
    {
        // index를 x, y 좌표로 변환
        int x = index % 10;
        int y = index / 10;

        // Unity의 y는 위쪽이 양수 아래쪽이 음수
        // 보통 2D map 만들 시 '화면 맨 위부터 순차적으로 내려온다' 라고 생각하기에 -y -> y = 0이 Unity 좌표계에서 맨 위 행
        // 셀 간 간격 만큼 떨어뜨리기
        Vector2 position = new Vector2(x * _cellSize, -y * _cellSize);

        // Instantiate(prefab, pos, rot)
        // 미리 만든 게임 오브젝트 원본(Prefab), 생성될 오브젝트 좌표(pos), 생성될 오브젝트 회전값(rot)
        Cell newCell = Instantiate(CellPrefab, position, Quaternion.identity);

        // 방 타입을 나타내는 값 EX) 1: 일반 방 2: 보스 방 etc
        newCell.value = 1;

        // 이 방이 어떤 위치에 해당하는지
        newCell.index = index;

        // 방 모양 설정
        newCell.SetRoomShape(RoomShape.OneByOne);

        // 방 타입 설정
        newCell.SetRoomType(RoomType.Regular);

        // 셀 List 검색하여 현재 인덱스 추가
        newCell._cellList.Add(index);

        _spawnCells.Add(newCell);
    }

    bool TryPlaceRoom(int origin, int[] offsets)
    {
        // 이 번에 만들 방(큰 방)을 구성할 인덱스들을 담는 임시 List
        List<int> currentRoomIndexes = new List<int>() { origin };

        foreach(var offset in offsets)
        {
            // 각 오프셋(Ex +1, +10, +11 etc)을 origin에 더해 실제 타일 index 계산
            int currentRoomChecked = origin + offset;

            // 후보 칸의 위 또는 아래가 맵 밖이면 배치 불가로 간주
            if(currentRoomChecked - 10 < 0 || currentRoomChecked + 10 >= _floorPlan.Length)
            {
                return false;
            }

            // 후보 칸이 이미 점유 되어 있으면 실패
            if (_floorPlan[currentRoomChecked] != 0)
            {
                return false;
            }

            // 후보 칸이 자기 자신이면 스킵
            if (currentRoomChecked == origin) continue;

            // 열 index가 가장 왼쪽 열인 칸은 스킵
            if (currentRoomChecked % 10 == 0) continue;

            // 검증이 통과된 큰 방 구성 List에 추가
            currentRoomIndexes.Add(currentRoomChecked);
        }

        if (currentRoomIndexes.Count == 1) return false;

        // 최종 확정된 큰 방 구성 칸들을 실제 맵에 Commit
        foreach (int index in currentRoomIndexes)
        {
            // 0에서 1로 표시(점유되었다는 의미)
            _floorPlan[index] = 1;

            // 전체 카운트 증가
            _floorPlanCount++;

            // 셀 큐에 등록
            _cellQueue.Enqueue(index);
            
            // 큰 방 index 목록에 기록
            _bigRoomIndexes.Add(index);
        }

        SpawnLargeRoom(currentRoomIndexes);

        return true;
    }

    /// <summary>
    /// 큰 방(2x2, L, 1x2 / 2x1)을 구성하는 셀들의 인덱스 목록을 받아 하나의 Prefab으로 배치
    /// </summary>
    /// <param name="largeRoomIndexes"></param>
    void SpawnLargeRoom(List<int> largeRoomIndexes)
    {
        // 생성한 Prefab 핸들
        Cell newCell = null;

        // 그리드 좌표(x, y)의 합(평균이나 중심 계산에 사용)
        int combinedX = 0, combinedY = 0;
        
        // 셀 중심으로 이동하기 위한 half-cell offset
        float offset = _cellSize / 2f;

        // 전달된 모든 셀 index 순회
        for(int i = 0; i < largeRoomIndexes.Count; i++)
        {
            // 1D -> 2D : 열 index
            int x = largeRoomIndexes[i] % 10;
            // 1D -> 2D : 행 index
            int y = largeRoomIndexes[i] / 10;

            // x, y 좌표 누적
            combinedX += x;
            combinedY += y;
        }

        // 2x2 형태의 객실
        if(largeRoomIndexes.Count == 4)
        {
            // 평균(정수 나눗셈으로 바닥값) * _cellSize + (0.5cell) -> 2x2 블록의 정확한 중심
            // y는 위에서 아래로 증가하도록 음수로 매핑
            Vector2 position = new Vector2(combinedX / 4 * _cellSize + offset, -combinedY / 4 * _cellSize - offset);

            // prefab 생성
            newCell = Instantiate(CellPrefab, position ,Quaternion.identity);
            newCell.SetRoomSprite(_largeRoom);
            newCell.SetRoomShape(RoomShape.TwoByTwo);
        }

        // L 형태의 객실
        if(largeRoomIndexes.Count == 3)
        {
            // 2x2 박스의 중심에 맞추기 위해 평균 + half-cell
            Vector2 position = new Vector2(combinedX / 3 * _cellSize + offset, -combinedY / 3 * _cellSize - offset);

            newCell = Instantiate(CellPrefab, position, Quaternion.identity);
            newCell.SetRoomSprite(_LShapeRoom);
            newCell.SetRoomShape(RoomShape.LShape);

            // 누락된 코너 위치를 기반으로 올바른 방향으로 회전
            newCell.RotateCell(largeRoomIndexes);
        }
        
        // 1x2 혹은 2x1 형태의 객실
        if(largeRoomIndexes.Count == 2)
        {
            // 수직 인접
            if (largeRoomIndexes[0] + 10 == largeRoomIndexes[1] || largeRoomIndexes[0] - 10 == largeRoomIndexes[1])
            {
                // 수직 모양이기에 x는 두 셀이 같으므로 평균 그대로. y는 서로 다르므로 중앙이 반칸 -> -offset
                Vector2 position = new Vector2(combinedX / 2 * _cellSize, -combinedY / 2 * _cellSize - offset);

                newCell = Instantiate(CellPrefab, position, Quaternion.identity);
                newCell.SetRoomSprite(_verticalRoom);
                newCell.SetRoomShape(RoomShape.OneByTwo);
            }

            // 수평 인접
            if (largeRoomIndexes[0] + 1 == largeRoomIndexes[1] || largeRoomIndexes[0] - 1 == largeRoomIndexes[1])
            {
                // 수평 모양이기에 x는 중앙이 반칸 -> +offset, y는 두 셀이 같으므로 평균
                Vector2 position = new Vector2(combinedX / 2 * _cellSize + offset, -combinedY / 2 * _cellSize);

                newCell = Instantiate(CellPrefab, position, Quaternion.identity);
                newCell.SetRoomSprite(_horizontalRoom);
                newCell.SetRoomShape(RoomShape.TwoByOne);
            }
        }

        newCell._cellList = largeRoomIndexes;
        newCell._cellList.Sort();

        // 관리 / 참조 를 위해 리스트에 추가
        _spawnCells.Add(newCell);
    }
}
