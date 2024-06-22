using System;
using System.Collections.Generic;
using System.Linq;
using InGame.Player;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace InGame.UI.Pages
{
    public class InventoryController : MonoBehaviour, IPlayerListener
    {
        [SerializeField] private RectTransform inventoryTransform;
        [SerializeField] private ItemInformationModal itemInformationModal;

        [SerializeField] private GameObject inventoryItemPrefab;
        
        public IObjectPool<GameObject> InventoryItemPool { get; private set; }

        private Dictionary<Vector2Int, InventoryItem> inventoryItems;
        
        private const int inventoryItemSize = 70;
        private const int inventoryItemWidth = 8;

        private int startRow = 0;
        private bool isInitialized = false;

        private void Init()
        {
            InventoryItemPool = new ObjectPool<GameObject>(
                CreatePooledItem, OnGetFromPool, OnReleaseFromPool, OnDestroyPoolObject);

            inventoryItems = new Dictionary<Vector2Int, InventoryItem>();

            isInitialized = true;
        }

        public void OnPlayerRefresh(Player.Player _player)
        {
            if(!isInitialized)
                Init();
            
            var itemCount = _player.PlayerItemData.GetAllItemCount();

            int row, column;

            if (itemCount.Count < inventoryItems.Count)
            {
                for (int index = itemCount.Count; index < inventoryItems.Count(); ++index)
                {
                    row = index / inventoryItemWidth;
                    column = index % inventoryItemWidth;
                    var pos = new Vector2Int(row, column);
                    var inventoryItem = inventoryItems[pos];
                    
                    inventoryItems.Remove(pos);
                    InventoryItemPool.Release(inventoryItem.gameObject);
                }
            }

            row = -1;
            column = -1;
            
            foreach (var (itemId, count) in itemCount)
            {
                ++column;
                if (column % inventoryItemWidth == 0)
                    ++row;
                    
                var pos = new Vector2Int(row, column);

                InventoryItem inventoryItem;
                
                if (inventoryItems.ContainsKey(pos))
                {
                    inventoryItem = inventoryItems[pos];
                }
                else
                {
                    var inventoryItemGameObject = InventoryItemPool.Get();
                    
                    inventoryItem = inventoryItemGameObject.GetComponent<InventoryItem>();
                    inventoryItems.Add(pos, inventoryItem);
                    inventoryItemGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                        column * inventoryItemSize,
                        -row * inventoryItemSize
                    );
                }

                if (inventoryItem.GetItemId() != itemId || inventoryItem.GetItemCount() != count)
                {
                    inventoryItem.GetComponent<Button>().onClick.RemoveAllListeners();
                    inventoryItem.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        itemInformationModal.SetValue("itemId", itemId);
                        itemInformationModal.gameObject.SetActive(true);
                    });
                    inventoryItem.SetData(itemId, count);
                }
            }

            var inventorySize = inventoryTransform.sizeDelta;
            inventorySize.y = inventoryItemSize * (row + 1);
            inventoryTransform.sizeDelta = inventorySize;
        }
        
        private GameObject CreatePooledItem()
        {
            GameObject gameObject = Instantiate(inventoryItemPrefab, inventoryTransform);
            gameObject.GetComponent<InventoryItem>().InventoryItemPool = InventoryItemPool;
            return gameObject;
        }

        private void OnGetFromPool(GameObject gameObject)
        {
            gameObject.name = "inventory_item";
            gameObject.SetActive(true);
        }

        private void OnReleaseFromPool(GameObject gameObject)
        {
            gameObject.name = "unloaded_inventory_item";
            gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(GameObject gameObject)
        {
            Destroy(gameObject);
        }
    }
}