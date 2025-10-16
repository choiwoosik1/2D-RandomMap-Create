using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Door", menuName = "Scriptable Objects/Door")]
public class DoorScriptable : ScriptableObject
{
    public RoomType _roomType;
    public Sprite _upDoor;
    public Sprite _downDoor;
    public Sprite _leftDoor;
    public Sprite _rightDoor;
}
