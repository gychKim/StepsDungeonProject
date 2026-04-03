using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomController : MonoBehaviour
{
	[Header("Layers")]
	public Tilemap groundLayer;
	public Tilemap wallLayer;

	public Transform propTransform;
	public Transform objectTransform;
	private ObjectSpawner objectSpawner;
	private PropSpawner propSpawner;
	private BossSpawner bossSpawner;


	public Transform uiFocusTrans;

	public TileBase groundTileBase;
	public TileBase wallTileBase;

	[VContainer.Inject]
	public void Construct(ObjectSpawner objectSpawner, PropSpawner propSpawner, BossSpawner bossSpawner)
	{
		this.objectSpawner = objectSpawner;
		this.propSpawner = propSpawner;
		this.bossSpawner = bossSpawner;
	}

	public void SetGroundTile(Vector3Int pos, TileType tileType)
	{
		groundLayer.SetTile(pos, GetTileBase(tileType));
	}

	private TileBase GetTileBase(TileType type)
	{
		switch(type)
		{
			case TileType.Ground: return groundTileBase;
			case TileType.Wall: return wallTileBase;
			default: return null;
		}
	}
	public void SetGroundTile(Vector3Int pos, TileBase tile)
	{
		groundLayer.SetTile(pos, tile);
	}

	public void SetWallTile(Vector3Int pos, TileBase tile)
	{
		wallLayer.SetTile(pos, tile);
	}

	public async UniTask SpawnProps(RoomDecoDataSO roomDecoDataSO)
	{
		await propSpawner.SpawnProps(roomDecoDataSO, propTransform);
	}

	public async UniTask SpawnObjectAsync(NormalRoomData roomData, float groundYPos, CancellationToken ct)
	{
		await objectSpawner.SpawnObjectAsync(roomData, groundYPos, objectTransform, ct);

		uiFocusTrans.position = roomData.ObjectPosition + new Vector3(2, 2);
	}

	public async UniTask SpawnTransportObjectsAsync(NormalRoomData roomData, float groundYPos, CancellationToken ct)
	{
		await objectSpawner.SpawnTransportObjectsAsync(roomData, groundYPos, objectTransform, ct);

		uiFocusTrans.position = roomData.ObjectPosition + new Vector3(2, 2);
	}

	public async UniTask SpawnBossAsync(BossRoomData bossData, float groundYPos, CancellationToken ct)
	{
		await bossSpawner.SpawnBossAsync(bossData, groundYPos, objectTransform, ct);

		uiFocusTrans.position = bossData.BossPosition;
	}

	public List<Vector2> testPlacementList => propSpawner.testPlacementList;

	// 편의를 위한 초기화 함수
	public void ClearAll()
	{
		groundLayer.ClearAllTiles();
		wallLayer.ClearAllTiles();

		propSpawner.ClearSpawnedProps();
		objectSpawner.ClearSpawnedObjects();
		bossSpawner.ClearSpawnedObjects();
	}
}