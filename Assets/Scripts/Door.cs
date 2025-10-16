using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public SpriteRenderer _spriteRenderer;

    public void SetDoorSprite(Sprite door)
    {
        _spriteRenderer.sprite = door;
    }
}
