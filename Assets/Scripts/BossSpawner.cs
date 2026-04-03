using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BossSpawner
{
	private readonly IResourceService resourceService;
	private GameObject spawnedBoss = null; // 생성된 보스
	public BossSpawner(IResourceService resourceService)
	{
		this.resourceService = resourceService;
	}

	/// <summary>
	/// 룸 데이터에 따라 보스 생성
	/// </summary>
	/// <param name="bossRoomData"></param>
	/// <returns></returns>
	public async UniTask SpawnBossAsync(BossRoomData bossRoomData, float groundYPos, Transform parentTrans = null, CancellationToken ct = default)
	{
		GameObject bossPrefab = await resourceService.LoadAssetDataAsync<GameObject>($"Boss_{bossRoomData.DungeonType}", ct);
		GameObject spawnedBoss = GameObject.Instantiate(bossPrefab, bossRoomData.BossPosition, Quaternion.identity);

		spawnedBoss.transform.position = new Vector3(bossRoomData.BossPosition.x, groundYPos, bossRoomData.BossPosition.z);
		spawnedBoss.transform.rotation = Quaternion.identity;

		this.spawnedBoss = spawnedBoss;
		// uiFocusTrans.position = bossRoomData.ObjectPosition + new Vector3(2, 2);
	}

	/// <summary>
	/// 생성된 보스를 삭제
	/// </summary>
	public void ClearSpawnedObjects()
	{
		if(spawnedBoss == null) 
			return;

		GameObject.Destroy(spawnedBoss);
		spawnedBoss = null;
	}
}
