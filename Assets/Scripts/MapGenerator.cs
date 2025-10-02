using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    // 맵 생성에 필요한 private 변수들
    int[] _floorPlan;           // 맵 전체를 1D 배열로 저장하는 공간

    int _floorPlanCount;        // 지금까지 생성된 방
    int _minRooms;              // 최소 방 개수
    int _maxRooms;              // 최대 방 개수
    List<int> _endRooms;        // 현재 생성된 방들 중 끝 방의 인덱스를 담는 리스트

    int _bossRoomIndex;         // 보스 방 셀의 인덱스
    int _secretRoomIndex;       // 비밀 방 셀의 인덱스
    int _shopRoomIndex;         // 상점 방 셀의 인덱스
    int _itemRoomIndex;         // 아이템 방 셀의 인덱스

    public Cell CellPrefab;     // 실제 화면에 찍어줄 방 Prefab
    float _cellSize;            // 셀 사이 간격
    Queue<int> _cellQueue;      // 단계적 방 생성 시 사용할 큐
    List<Cell> _spawnCells;     // Scene에 실제로 Instantiate한 방 오브젝트

    [Header("---- 스프라이트 참조 ----")]
    [SerializeField] Sprite _item;
    [SerializeField] Sprite _shop;
    [SerializeField] Sprite _boss;
    [SerializeField] Sprite _secret;

    void Start()
    {
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

        SetUpSpecialRooms();
    }


    void SetUpSpecialRooms() { }

    void UpdateSpecialRoomVisuals() { }

    int RandomEndRoom()
    {
        return -1;
    }

    int PickSecretRoom()
    {
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

        _spawnCells.Add(newCell);
    }
}
