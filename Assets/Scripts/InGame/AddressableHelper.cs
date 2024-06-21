using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InGame.Item;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AddressableHelper
{
    private static AddressableHelper instance = null;
    public static AddressableHelper Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AddressableHelper();
            }

            return instance;
        }
    }

    private int totalItemsToLoad;
    private int itemsLoaded;
    private Dictionary<string, Item> cachedItems;
    private readonly object cacheLock = new object();
    private bool isFullyLoaded;

    // Event to notify when all items are loaded
    public event Action OnAllItemsLoaded;

    private AddressableHelper()
    {
        cachedItems = new Dictionary<string, Item>();
        isFullyLoaded = false;

        // Load resource locations and initiate asset loading
        Addressables.LoadResourceLocationsAsync("item").Completed += OnResourceLocationsLoaded;
    }

    private void OnResourceLocationsLoaded(AsyncOperationHandle<IList<IResourceLocation>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            totalItemsToLoad = handle.Result.Count;
            itemsLoaded = 0;

            foreach (var location in handle.Result)
            {
                Addressables.LoadAssetAsync<Item>(location).Completed += op =>
                {
                    var key = location.PrimaryKey; // Get the primary key for the asset
                    
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        lock (cacheLock)
                        {
                            cachedItems[key] = op.Result; // Add the asset to the dictionary with its key
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to load asset : " + key);
                    }

                    // Update the loaded items count and check if all items are loaded
                    lock (cacheLock)
                    {
                        itemsLoaded++;
                        if (itemsLoaded == totalItemsToLoad)
                        {
                            isFullyLoaded = true;
                            OnAllItemsLoaded?.Invoke(); // Invoke the event
                        }
                    }
                };
            }
        }
        else
        {
            Debug.LogError("Failed to load assets");
        }
    }

    public Item GetItem(string itemId)
    {
        lock (cacheLock)
        {
            return cachedItems.GetValueOrDefault(itemId);
        }
    }

    public string FindItemId(Item item)
    {
        return cachedItems.First(itemPair => itemPair.Value == item).Key;
    }

    public bool IsFullyLoaded()
    {
        lock (cacheLock)
        {
            return isFullyLoaded;
        }
    }
}
