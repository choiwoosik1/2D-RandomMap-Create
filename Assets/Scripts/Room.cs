using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EdgeDirection
{
    Up,
    Down,
    Left,
    Right,
}

public class Room : MonoBehaviour
{
    public SpriteRenderer _renderer;

    public void SetUpRoom(Cell currentCell, RoomScriptable room)
    {
        if (_renderer == null)
        {
            Debug.LogError("[Room] _renderer is null. Did you forget to assign it in the Inspector or call GetComponent<SpriteRenderer>()?");
            return;
        }

        if (room == null)
        {
            Debug.LogError("[Room] RoomScriptable is null! This means SetUpRooms() passed a null 'room' argument.");
            return;
        }

        if (room._roomVariations == null || room._roomVariations.Length == 0)
        {
            Debug.LogError($"[Room] room._roomVariations is empty or null. Check RoomScriptable '{room.name}' in the inspector.");
            return;
        }

        _renderer.sprite = room._roomVariations[Random.Range(0, room._roomVariations.Length)];

        if (currentCell._roomType == RoomType.Secret) return;

        var floorPlan = MapGenerator.instance.GetFloorPlan;
        var cellList = MapGenerator.instance.GetSpawnCells;

        switch (currentCell._roomShape)
        {
            case RoomShape.OneByOne:
                SetUpOneByOne(currentCell, floorPlan, cellList); 
                break;

            case RoomShape.OneByTwo:
                SetUpOneByTwo(currentCell, floorPlan, cellList);
                break;

            case RoomShape.TwoByOne:
                SetUpTwoByOne(currentCell, floorPlan, cellList);
                break;

            case RoomShape.TwoByTwo:
                SetUpTwoByTwo(currentCell, floorPlan, cellList);
                break;

            case RoomShape.LShape:
                SetUpLShapeRoom(currentCell, floorPlan, cellList);
                break;

            default:
                break;
        }
    }

    public void SetUpOneByOne(Cell cell, int[] floorPlan, List<Cell> cellList)
    {
        var currentCell = cell._cellList[0];

        TryPlaceDoor(currentCell, new Vector2(0, 4f), EdgeDirection.Up, floorPlan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(0, -4f), EdgeDirection.Down, floorPlan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(-9.4f, 0), EdgeDirection.Left, floorPlan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(9.4f, 0), EdgeDirection.Right, floorPlan, cellList, cell);

    }
    public void SetUpOneByTwo(Cell cell, int[] floorPlan, List<Cell> cellList)
    {
        var cellA = cell._cellList[0];
        var cellB = cell._cellList[1];

        TryPlaceDoor(cellA, new Vector2(0f, 9.4f), EdgeDirection.Up, floorPlan, cellList, cell);
        TryPlaceDoor(cellA, new Vector2(-9.4f, 5.1f), EdgeDirection.Left, floorPlan, cellList, cell);
        TryPlaceDoor(cellA, new Vector2(9.4f, 5.1f), EdgeDirection.Right, floorPlan, cellList, cell);

        TryPlaceDoor(cellB, new Vector2(0f, -9.4f), EdgeDirection.Down, floorPlan, cellList, cell);
        TryPlaceDoor(cellB, new Vector2(-9.4f, -5.1f), EdgeDirection.Left, floorPlan, cellList, cell);
        TryPlaceDoor(cellB, new Vector2(9.4f, -5.1f), EdgeDirection.Right, floorPlan, cellList, cell);
    }
    public void SetUpTwoByOne(Cell cell, int[] floorPlan, List<Cell> cellList)
    {
        var cellA = cell._cellList[0];
        var cellB = cell._cellList[1];

        TryPlaceDoor(cellA, new Vector2(-10.8f, 4.21f), EdgeDirection.Up, floorPlan, cellList, cell);
        TryPlaceDoor(cellA, new Vector2(-20f, 0f), EdgeDirection.Left, floorPlan, cellList, cell);
        TryPlaceDoor(cellA, new Vector2(-10.8f, -4.21f), EdgeDirection.Down, floorPlan, cellList, cell);

        TryPlaceDoor(cellB, new Vector2(10.8f, 4.21f), EdgeDirection.Up, floorPlan, cellList, cell);
        TryPlaceDoor(cellB, new Vector2(10.8f, -4.21f), EdgeDirection.Down, floorPlan, cellList, cell);
        TryPlaceDoor(cellB, new Vector2(20f, 0f), EdgeDirection.Right, floorPlan, cellList, cell);
    }
    public void SetUpTwoByTwo(Cell cell, int[] floorPlan, List<Cell> cellList)
    {
        var cellA = cell._cellList[0];
        var cellB = cell._cellList[1];
        var cellC = cell._cellList[2];
        var cellD = cell._cellList[3];

        TryPlaceDoor(cellA, new Vector2(-10.65f, 9.26f), EdgeDirection.Up, floorPlan, cellList, cell);
        TryPlaceDoor(cellB, new Vector2(10.65f, 9.26f), EdgeDirection.Up, floorPlan, cellList, cell);

        TryPlaceDoor(cellA, new Vector2(-20f, 5.21f), EdgeDirection.Left, floorPlan, cellList, cell);
        TryPlaceDoor(cellC, new Vector2(-20f, -5.21f), EdgeDirection.Left, floorPlan, cellList, cell);

        TryPlaceDoor(cellC, new Vector2(-10.65f, -9.26f), EdgeDirection.Down, floorPlan, cellList, cell);
        TryPlaceDoor(cellD, new Vector2(10.65f, -9.26f), EdgeDirection.Down, floorPlan, cellList, cell);

        TryPlaceDoor(cellB, new Vector2(20f, 5.21f), EdgeDirection.Right, floorPlan, cellList, cell);
        TryPlaceDoor(cellD, new Vector2(20f, -5.21f), EdgeDirection.Right, floorPlan, cellList, cell);
    }
    public void SetUpLShapeRoom(Cell cell, int[] floorPlan, List<Cell> cellList)
    {
        var cellA = cell._cellList[0];
        var cellB = cell._cellList[1];
        var cellC = cell._cellList[2];

        if (cellA + 1 == cellB && cellA + 10 == cellC)
        {
            TryPlaceDoor(cellA, new Vector2(-10.64f, 9.41f), EdgeDirection.Up, floorPlan, cellList, cell);
            TryPlaceDoor(cellA, new Vector2(-20f, 5f), EdgeDirection.Left, floorPlan, cellList, cell);

            TryPlaceDoor(cellB, new Vector2(10.64f, 9.41f), EdgeDirection.Up, floorPlan, cellList, cell);
            TryPlaceDoor(cellB, new Vector2(20f, 5.26f), EdgeDirection.Right, floorPlan, cellList, cell);
            TryPlaceDoor(cellB, new Vector2(10.64f, 1f), EdgeDirection.Down, floorPlan, cellList, cell);

            TryPlaceDoor(cellC, new Vector2(-10.64f, -9.41f), EdgeDirection.Down, floorPlan, cellList, cell);
            TryPlaceDoor(cellC, new Vector2(-1f, -5.26f), EdgeDirection.Right, floorPlan, cellList, cell);
            TryPlaceDoor(cellC, new Vector2(-20f, -5f), EdgeDirection.Left, floorPlan, cellList, cell);
        }
        else if (cellA + 1 == cellB && cellB + 10 == cellC)
        {
            TryPlaceDoor(cellA, new Vector2(-10.64f, 9.41f), EdgeDirection.Up, floorPlan, cellList, cell);
            TryPlaceDoor(cellA, new Vector2(-20f, 5f), EdgeDirection.Left, floorPlan, cellList, cell);
            TryPlaceDoor(cellA, new Vector2(-10.64f, 1f), EdgeDirection.Down, floorPlan, cellList, cell);

            TryPlaceDoor(cellB, new Vector2(10.64f, 9.41f), EdgeDirection.Up, floorPlan, cellList, cell);
            TryPlaceDoor(cellB, new Vector2(20f, 5f), EdgeDirection.Right, floorPlan, cellList, cell);

            TryPlaceDoor(cellC, new Vector2(10.64f, -9.41f), EdgeDirection.Down, floorPlan, cellList, cell);
            TryPlaceDoor(cellC, new Vector2(20f, -5.26f), EdgeDirection.Right, floorPlan, cellList, cell);
            TryPlaceDoor(cellC, new Vector2(1, -5f), EdgeDirection.Left, floorPlan, cellList, cell);
        }
        else if (cellA + 10 == cellB)
        {
            TryPlaceDoor(cellA, new Vector2(-10.64f, 9.41f), EdgeDirection.Up, floorPlan, cellList, cell);
            TryPlaceDoor(cellA, new Vector2(-20f, 5f), EdgeDirection.Left, floorPlan, cellList, cell);
            TryPlaceDoor(cellA, new Vector2(-1f, 5f), EdgeDirection.Right, floorPlan, cellList, cell);

            TryPlaceDoor(cellB, new Vector2(-10.64f, -9.41f), EdgeDirection.Down, floorPlan, cellList, cell);
            TryPlaceDoor(cellB, new Vector2(-20f, -5f), EdgeDirection.Left, floorPlan, cellList, cell);

            TryPlaceDoor(cellC, new Vector2(10.64f, -1), EdgeDirection.Up, floorPlan, cellList, cell);
            TryPlaceDoor(cellC, new Vector2(10.64f, -9.41f), EdgeDirection.Down, floorPlan, cellList, cell);
            TryPlaceDoor(cellC, new Vector2(20f, -5.26f), EdgeDirection.Right, floorPlan, cellList, cell);
        }
        else if (cellA + 10 == cellC)
        {
            TryPlaceDoor(cellA, new Vector2(10.64f, 9.41f), EdgeDirection.Up, floorPlan, cellList, cell);
            TryPlaceDoor(cellA, new Vector2(1, 5f), EdgeDirection.Left, floorPlan, cellList, cell);
            TryPlaceDoor(cellA, new Vector2(20f, 5f), EdgeDirection.Right, floorPlan, cellList, cell);

            TryPlaceDoor(cellB, new Vector2(-5.3125f, -1f), EdgeDirection.Up, floorPlan, cellList, cell);
            TryPlaceDoor(cellB, new Vector2(-10.64f, -9.41f), EdgeDirection.Down, floorPlan, cellList, cell);
            TryPlaceDoor(cellB, new Vector2(-20f, -5f), EdgeDirection.Left, floorPlan, cellList, cell);

            TryPlaceDoor(cellC, new Vector2(10.64f, -9.41f), EdgeDirection.Down, floorPlan, cellList, cell);
            TryPlaceDoor(cellC, new Vector2(20f, -5f), EdgeDirection.Right, floorPlan, cellList, cell);
        }
    }

    void TryPlaceDoor(int fromIndex, Vector2 positionOffset, EdgeDirection direction, int[] floorPlan, List<Cell> cellList, Cell currentCell)
    {
        int neighbourIndex = fromIndex + GetOffset(direction);

        if (neighbourIndex < 0 || neighbourIndex >= floorPlan.Length) return;

        if (floorPlan[neighbourIndex] != 1) return;

        var foundCell = cellList.FirstOrDefault(x => x._cellList.Contains(neighbourIndex));

        if (foundCell._roomType == RoomType.Secret) return;

        var door = Instantiate(RoomManager.instance._doorPrefab, transform);

        door.transform.position = (Vector2)transform.position + positionOffset;

        SetUpDoor(door, direction, currentCell._roomType == RoomType.Regular ? foundCell._roomType : currentCell._roomType);
    }

    void SetUpDoor(Door door, EdgeDirection direction, RoomType roomType)
    {
        var doorTypes = GetDoorOptions(roomType);

        switch (direction)
        {
            case EdgeDirection.Up:
                door.SetDoorSprite(doorTypes._upDoor);
                break;

            case EdgeDirection.Down:
                door.SetDoorSprite(doorTypes._downDoor);
                break;

            case EdgeDirection.Left:
                door.SetDoorSprite(doorTypes._leftDoor);
                break;

            case EdgeDirection.Right:
                door.SetDoorSprite(doorTypes._rightDoor);
                break;

            default:
                break;
        }
    }

    DoorScriptable GetDoorOptions(RoomType roomType)
    {
        return RoomManager.instance._doors.FirstOrDefault(x => x._roomType == roomType);
    }

    int GetOffset(EdgeDirection direction)
    {
        switch (direction)
        {
            case EdgeDirection.Up:
                return -10;

            case EdgeDirection.Down:
                return 10;

            case EdgeDirection.Left:
                return -1;

            case EdgeDirection.Right:
                return 1;
        }

        return 0;
    }
}
