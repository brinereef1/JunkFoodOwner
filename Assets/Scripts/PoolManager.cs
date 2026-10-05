using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    // One pool per prefab. This lets us reuse objects instead of creating new ones.
    private Dictionary<GameObject, ObjectPool<GameObject>> pools = new();

    private void Awake()
    {
        // Keep only one pool manager in the scene.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public GameObject Get(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent = null)
    {
        // Make a pool if this prefab has not been used before.
        if (!pools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
        {
            pool = CreatePool(prefab);
            pools.Add(prefab, pool);
        }

        GameObject obj = pool.Get();

        obj.transform.SetParent(parent);
        obj.transform.SetPositionAndRotation(position, rotation);

        return obj;
    }

    public void Release(GameObject obj)
    {
        PooledObject pooledObject = obj.GetComponent<PooledObject>();

        if (pooledObject == null)
        {
            Debug.LogWarning(
                obj.name + " does not have a PooledObject component."
            );

            obj.SetActive(false);
            return;
        }

        if (!pools.TryGetValue(
                pooledObject.SourcePrefab,
                out ObjectPool<GameObject> pool))
        {
            obj.SetActive(false);
            return;
        }

        pool.Release(obj);
    }

    private ObjectPool<GameObject> CreatePool(GameObject prefab)
    {
        ObjectPool<GameObject> pool = null;

        pool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject obj = Instantiate(prefab, transform);

                PooledObject pooledObject =
                    obj.GetComponent<PooledObject>();

                if (pooledObject == null)
                {
                    pooledObject = obj.AddComponent<PooledObject>();
                }

                pooledObject.SourcePrefab = prefab;

                obj.SetActive(false);

                return obj;
            },

            actionOnGet: obj =>
            {
                obj.SetActive(true);
            },

            actionOnRelease: obj =>
            {
                obj.transform.SetParent(transform);
                obj.SetActive(false);
            },

            actionOnDestroy: obj =>
            {
                Destroy(obj);
            },

            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 50
        );

        return pool;
    }
}