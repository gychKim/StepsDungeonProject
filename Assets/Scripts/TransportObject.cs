using UnityEngine;

public class TransportObject : MonoBehaviour, IPoolable
{   
	public string PoolKey { get; set; }
}
