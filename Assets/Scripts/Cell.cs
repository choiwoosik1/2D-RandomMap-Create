using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 방 유형
/// </summary>
public enum RoomType
{
    Regular,
    Item,
    Shop,
    Boss,
    Secret
}

/// <summary>
/// 방 형태
/// </summary>
public enum RoomShape
{
    OneByOne,
    OneByTwo,
    TwoByOne,
    TwoByTwo,
    LShape
}

public class Cell : MonoBehaviour
{
    public RoomType _roomType;
    public RoomShape _roomShape;

    // 두 개의 필요한 public 변수 선언
    public int index;
    public int value;

    public SpriteRenderer _spriteRenderer;
    public SpriteRenderer _roomSprite;

    public List<int> _cellList = new List<int>();

    public void SetSpecialRoomSprite(Sprite Icon)
    {
        _spriteRenderer.sprite = Icon;
    }

    public void SetRoomSprite(Sprite Icon)
    {
        _roomSprite.sprite = Icon;
    }

    public void SetRoomType(RoomType newRoomType)
    {
        _roomType = newRoomType;
    }

    public void SetRoomShape(RoomShape newRoomShape)
    {
        _roomShape = newRoomShape;
    }

    public void RotateCell(List<int> connectedCells)
    {
        connectedCells.Sort();

        // 기준 셀 index 결정
        index = connectedCells[0];      

        // 오른쪽과 아래쪽 확인
        if (connectedCells.Contains(index + 1) && connectedCells.Contains(index + 10))
        {
            // 오른쪽 - 아래 L자
            ApplyRotation(-90);
        }

        // 오른쪽과 오른쪽 - 아래 대각선
        if(connectedCells.Contains(index + 1) && connectedCells.Contains(index + 11))
        {
            // 오른쪽 - 오른쪽 - 아래 대각 L자
            ApplyRotation(180);
        }

        // 왼쪽 - 아래 대각선과 아래쪽 확인
        if(connectedCells.Contains(index + 9) && connectedCells.Contains(index + 10))
        {
            // 왼쪽 - 아래 대각선 - 아래 L자
            ApplyRotation(90);
        }
    }

    /// <summary>
    /// L자형 방에 적용되는 회전 함수
    /// </summary>
    /// <param name="angle"></param>
    public void ApplyRotation(float angle)
    {
        // angle의 값만큼 z축으로 회전
        // Euler는 직관적인 회전 값으로 Inspector View의 Transform에서 볼 수 있는 X, Y, Z축 회전 값
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
