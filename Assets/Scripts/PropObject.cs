using UnityEngine;
using UnityEngine.AddressableAssets;

public enum PlacementType
{
	Wall,
	Ground
}

public class PropObject : MonoBehaviour, IPoolable
{
	public PlacementType placementType;

	public Vector2Int size;

	public float placementPriority;

	public AssetReference prefab;

	public string PoolKey { get; set; }
}
