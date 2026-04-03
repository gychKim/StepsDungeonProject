using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// 오브젝트(플레이어와 상호작용 하는) 스폰
/// </summary>
public class ObjectSpawner
{
	private readonly IResourceService resourceService;
	private readonly IPoolService poolService;
	private List<GameObject> spawnedObjectList = new List<GameObject>(); // 생성된 구조물 리스트

	public ObjectSpawner(IResourceService resourceService, IPoolService poolService)
	{
		this.resourceService = resourceService;
		this.poolService = poolService;
	}

	/// <summary>
	/// 룸 데이터에 따라 오브젝트 생성
	/// </summary>
	/// <param name="roomData"></param>
	/// <returns></returns>
	public async UniTask SpawnObjectAsync(NormalRoomData roomData, float groundYPos, Transform parentTrans = null, CancellationToken ct = default)
	{
		if(!poolService.IsPool($"Object_{roomData.ObjectType}"))
		{
			await poolService.CreatePooledItemAsync($"Object_{roomData.ObjectType}", 3, 10, ct);
		}

		GameObject spawnedObject = poolService.Get($"Object_{roomData.ObjectType}");
		spawnedObject.transform.position = new Vector3(roomData.ObjectPosition.x, groundYPos, roomData.ObjectPosition.z);
		spawnedObject.transform.rotation = Quaternion.identity;
		spawnedObjectList.Add(spawnedObject);

		// uiFocusTrans.position = roomData.ObjectPosition + new Vector3(2, 2);
	}

	/// <summary>
	/// 다음 층 이동수단 오브젝트 생성
	/// </summary>
	/// <param name="objectPrefab"></param>
	/// <param name="createPos"></param>
	public async UniTask SpawnTransportObjectsAsync(NormalRoomData roomData, float groundYPos, Transform parentTrans = null, CancellationToken ct = default)
	{
		if(!poolService.IsPool($"Transport_{roomData.Transport01Type}"))
		{
			await poolService.CreatePooledItemAsync($"Transport_{roomData.Transport01Type}", ct: ct);
		}

		GameObject spawnedTransport01 = poolService.Get($"Transport_{roomData.Transport01Type}");
		spawnedTransport01.transform.position = new Vector3(roomData.Transport01Position.x, groundYPos, roomData.Transport01Position.z);
		spawnedTransport01.transform.rotation = Quaternion.identity;
		spawnedObjectList.Add(spawnedTransport01);

		if(!poolService.IsPool($"Transport_{roomData.Transport02Type}"))
		{
			await poolService.CreatePooledItemAsync($"Transport_{roomData.Transport02Type}", ct: ct);
		}

		GameObject spawnedTransport02 = poolService.Get($"Transport_{roomData.Transport02Type}");
		spawnedTransport02.transform.position = new Vector3(roomData.Transport02Position.x, groundYPos, roomData.Transport02Position.z);
		spawnedTransport02.transform.rotation = Quaternion.identity;
		spawnedObjectList.Add(spawnedTransport02);
	}	

	/// <summary>
	/// 생성된 오브젝트들 삭제
	/// </summary>
	public void ClearSpawnedObjects()
	{
		foreach (var obj in spawnedObjectList)
		{
			poolService.Release(obj);
		}

		spawnedObjectList.Clear();
	}
}
