using UnityEngine;

public class PooledObject : MonoBehaviour
{
    // This tells the pool which prefab this object came from.
    public GameObject SourcePrefab { get; set; }
}