using System;
using System.Collections;
using System.Collections.Generic;
using InGame.Item;
using UnityEngine;
using UnityEngine.UI;

public class Dropitem : MonoBehaviour
{
    [SerializeField] private SpriteRenderer itemImage;
    public string itemId { get; private set; }
    
    private const float rotationSpeed = 120;

    public void Init(Item item)
    {
        itemId = AddressableHelper.Instance.FindItemId(item);
        itemImage.sprite = item.GetItemPreview();
    }

    private void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
