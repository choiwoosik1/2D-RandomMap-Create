using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    List<Room> createdRooms;

    [Header("---- Offset 변수")]
    public float _offsetX;
    public float _offsetY;

    [Header("---- Prefab 참조 ----")]
    public Door _doorPrefab;
    public Room _roomPrefab;

    [Header("---- Scriptable Object 참조 ----")]
    public DoorScriptable[] _doors;
    public RoomScriptable[] _rooms;

    public static RoomManager instance;

    private void Awake()
    {
        instance = this;
        createdRooms = new List<Room>();
    }

    /// <summary>
    /// 이미 배치된 Cell들(spawnCells)를 보고 실제 Room Prefab을 Scene에 깔아주는 함수
    /// </summary>
    /// <param name="spawnCells"></param>
    public void SetUpRooms(List<Cell> spawnCells)
    {
        // 이전에 생성해둔 룸 오브젝트들을 역순으로 전부 파괴
        // 역순인 이유 -> 리스트 삭제 시 인덱스 꼬임 방지
        for(int i = createdRooms.Count - 1; i >= 0; i--)
        {
            Destroy(createdRooms[i].gameObject);
        }

        // 관리 리스트 리셋
        createdRooms.Clear();

        // Map Generator가 만든 Cell(큰 방, 작은 방 포함)을 하나씩 읽어 대응 Room 생성
        foreach(var currentCell in spawnCells)
        {
            // 룸 카탈로그(_rooms)에서 모양이 같고, 타입이 같고, 타일 점유 패턴이 일치하는 템플릿 룸을 하나 찾아옴
            var foundRoom = _rooms.FirstOrDefault(x => x._roomShape == currentCell._roomShape 
            && x._roomType == currentCell._roomType && DoesTileMatchCell(x.occupiedTiles,currentCell));

            // Vector2
            // Cell의 월드 좌표를 읽음
            var currentPosition = currentCell.transform.position;

            // Vector2
            // 좌표 스케일 변환
            var convertedPosition = new Vector2(currentPosition.x * _offsetX, currentPosition.y * _offsetY);

            // RoomPrefab을 변환된 위치에 설정
            var spawnedRoom = Instantiate(_roomPrefab, convertedPosition, Quaternion.identity);

            spawnedRoom.SetUpRoom(currentCell, foundRoom);

            createdRooms.Add(spawnedRoom);
        }
    }

    /// <summary>
    /// 같은 타일인지 확인하는 함수
    /// </summary>
    /// <param name="occupiedTiles">룸 탬플릿이 차지하는 상대 패턴</param>
    /// <param name="cell">Scene에 생성된 실제 방 Prefab</param>
    /// <returns></returns>
    bool DoesTileMatchCell(int[] occupiedTiles, Cell cell)
    {
        // 타일 개수가 다르면 모양이 같을 수 없음
        if (occupiedTiles.Length != cell._cellList.Count) return false;

        // 현재 cell 모양의 최좌상단을 기준으로 잡음
        // 그렇게 해야 어디에 있든 같은 모양은 동일한 패턴으로 비교가 가능하다.
        int minIndex = cell._cellList.Min();

        // 정규화된 상대 패턴을 담을 리스트
        List<int> normalizedCell = new List<int>();

        // 각 셀 인덱스를 2D로 풀어서 행(x), 열(y)로 변환한 뒤 minIndex의 (x, y)와의 상대 좌표(dx, dy)를 구한다.
        // 이후 상대 좌표를 다시 1D 형태(dx * 10 + dy)로 패턴 키로 변환 후 리스트에 추가
        foreach(int index in cell._cellList)
        {
            int dx = (index % 10) - (minIndex % 10);
            int dy = (index / 10) - (minIndex / 10);

            // 여기서 10은 가로 너비를 의미함.
            normalizedCell.Add(dy * 10 + dx);
        }

        // 정렬 : 순서에 상관없이 같은 집합이면 동일한 배열이 되도록
        normalizedCell.Sort();

        // 탬플릿 패턴도 복제 후 정렬(원본 보존, 집합 비교 준비)
        int[] sortedOccupied = (int[])occupiedTiles.Clone();
        Array.Sort(sortedOccupied);

        // 정규화된 실제 모양과 템플릿 패턴이 원소까지 완전히 동일하면 매치
        return normalizedCell.SequenceEqual(sortedOccupied);
    }
}
