using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;


public interface IPoolable
{
    public string PoolKey { get; set;}
}

public interface IPoolService
{
    UniTask CreatePooledItemAsync(string key, int initSize = 3, int maxSize = 10, CancellationToken ct = default);
    UniTask CreatePooledItem(AssetReference assetReference, int initSize = 3, int maxSize = 10, CancellationToken ct = default);
    GameObject Get(string key);
    GameObject Get(AssetReference key);
    void Release(GameObject item);
    bool IsPool(string key);
    bool IsPool(AssetReference assetReference);
}

public class PoolSystem : IPoolService, IDisposable
{
    private Dictionary<string, IObjectPool<GameObject>> poolDict = new Dictionary<string, IObjectPool<GameObject>>();
    private Dictionary<string, GameObject> loadedPrefabDict = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> poolRootDict = new Dictionary<string, GameObject>();

    private readonly IResourceService resourceService;

    public PoolSystem(IResourceService resourceService)
    {
        this.resourceService = resourceService;
    }

    public async UniTask CreatePooledItemAsync(string key, int initSize = 3, int maxSize = 10, CancellationToken ct = default)
    {
        if(!loadedPrefabDict.ContainsKey(key))
        {
            var prefab = await resourceService.LoadAssetDataAsync<GameObject>(key, ct);

            if(prefab == null)
                return;

            loadedPrefabDict.Add(key, prefab);
        }

        if(!poolDict.TryGetValue(key, out var pool))
        {
            GameObject poolParents = new GameObject($"{key}Pool Root");
            poolRootDict.Add(key, poolParents);
            poolDict.Add(key, new ObjectPool<GameObject>(
                createFunc : () =>  CreateInstance(key, poolParents.transform), // 동기적으로 로드하여 GameObject를 생성
                actionOnGet : item => GetItem(item),
                actionOnRelease : item => ReleaseItem(item, poolParents.transform),
                actionOnDestroy : item => Destroy(item),
                true, initSize, maxSize));

            GameObject[] poolItems = new GameObject[initSize];
            for(int i = 0; i < initSize; i++)
            {
                poolItems[i] = poolDict[key].Get();
            }

            for(int i = 0; i < initSize; i++)
            {
                poolDict[key].Release(poolItems[i]);
            }
        }
    }

    public async UniTask CreatePooledItem(AssetReference assetReference, int initSize = 3, int maxSize = 10, CancellationToken ct = default)
    {
        await CreatePooledItemAsync(assetReference.RuntimeKey.ToString(), initSize, maxSize, ct);
    }

    private GameObject CreateInstance(string key, Transform parents)
    {
        var prefab = loadedPrefabDict[key];
        
        var obj = GameObject.Instantiate(prefab, parents);
        
        if(obj.TryGetComponent<IPoolable>(out var poolable))
        {
            poolable.PoolKey = key;
        }
        else
        {
            DebugX.LogWarning($"{obj.name}은 IPoolable을 구현하지 않았습니다. 풀링 시스템에서 관리되지 않습니다.");
        }

        return obj;
    }

    private void GetItem(GameObject item)
    {
        item.SetActive(true);
    }

    public void ReleaseItem(GameObject item, Transform parents)
    {
        item.transform.SetParent(parents);
        item.SetActive(false);
    }

    public void Destroy(GameObject item)
    {
        GameObject.Destroy(item);
    }

    public GameObject Get(string key)
    {
        if(poolDict.TryGetValue(key, out var pool))
        {
            return pool.Get();
        }
        else
        {
            DebugX.LogWarning($"{key}에 대한 풀링 시스템이 존재하지 않습니다. 풀링 시스템에서 관리되지 않습니다.");
            return null;
        }
    }

    public GameObject Get(AssetReference asset)
    {
        return Get(asset.RuntimeKey.ToString());
    }

    public void Release(GameObject item)
    {
        if(item.TryGetComponent<IPoolable>(out var poolable))
        {
            string key = poolable.PoolKey;

            if(poolDict.TryGetValue(key, out var pool))
            {
                pool.Release(item);
            }
            else
            {
                DebugX.LogWarning($"{key} 풀이 존재하지 않으므로 {item.name}을 강제 파괴.");
                Destroy(item);
            }
        }
        else
        {
            DebugX.LogWarning($"{item.name}은 IPoolable을 구현하지 않았습니다. 풀링 시스템에서 관리되지 않습니다.");
        }
    }

    public bool IsPool(string key)
    {
        return poolDict.ContainsKey(key);
    }

    public bool IsPool(AssetReference assetReference)
    {
        return IsPool(assetReference.RuntimeKey.ToString());
    }

    private void DestroyPool()
    {
        // IObjectPool을 초기화한다.

        foreach(var item in poolRootDict)
        {
            Destroy(item.Value);
        }

        // 캐싱된 메모리를 해제한다.
        foreach(var item in loadedPrefabDict)
        {
            resourceService.ReleaseAssetDataAsync(item.Key).Forget();
        }

        poolDict.Clear();
        loadedPrefabDict.Clear();
        poolRootDict.Clear();

    }

	public void Dispose()
	{
		DestroyPool();
	}
}