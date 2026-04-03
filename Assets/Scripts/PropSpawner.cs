using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using R3;
using System;
using System.Linq;
using UnityEngine;
using System.Threading;

/// <summary>
/// 장식품 스폰
/// </summary>
public class PropSpawner
{
	private readonly IResourceService resourceService;
	private readonly IPoolService poolService;

	private List<GameObject> spawnedPropList = new List<GameObject>(); // 생성된 구조물 리스트

	public bool[] isOccupied; // 배치 되었는지 여부

	public List<Vector2> testPlacementList = new List<Vector2>();

	public PropSpawner(IResourceService resourceService, IPoolService poolService)
	{
		this.resourceService = resourceService;
		this.poolService = poolService;
	}

	public async UniTask SpawnProps(RoomDecoDataSO data, Transform parentTrans = null, CancellationToken ct = default)
	{
		isOccupied = new bool[data.tileDataArr.Length]; // 방의 크기만큼 생성

		// 테마에 맞는 Prop 테이블을 가져온다.
		PropsTable propsTable = await resourceService.LoadAssetDataAsync<PropsTable>($"PropTable_{data.roomTheme}", ct);

		// Prop테이블에 존재하는 Prop들(AssetReference)을 오름차순으로 정렬시킨다.
		var propsArr = propsTable.propDataArr.OrderByDescending((x) => x.placementPriority)
			.ToArray();

		List<Vector2> placementList = PoissonDiscSampling.GeneratePoint(2.5f, data.roomSize, 30, new Vector2(0, -data.groundLevelY), new Vector2(0, data.groundLevelY - 1)); // 바닥도 포함해야 하므로 -1을 해준다.

		// 오프셋 정렬
		placementList = placementList.Select((vec) => vec = new Vector2(Mathf.CeilToInt(vec.x), Mathf.CeilToInt(vec.y))).ToList();

		testPlacementList = placementList;

		// 생성
		foreach (var propData in propsArr)
		{
			// 맵의 빈 공간을 찾고 가중치 확률 계산 (아직 프리팹 생성 안 함)
			if (CanPlace(propData, data, placementList, out Vector2 spawnPos))
			{
				GameObject propObj = null;

				if(!poolService.IsPool(propData.prefab))
				{
					await poolService.CreatePooledItem(propData.prefab, 5, 10);
				}

				propObj = poolService.Get(propData.prefab);
				propObj.transform.position = new Vector3(spawnPos.x, spawnPos.y, 1f);
				propObj.transform.rotation = Quaternion.identity;

				spawnedPropList.Add(propObj);

				// 점유 마킹
				MarkOccupied(spawnPos, propData.size, data.roomSize);
			}
		}
	}

	/// <summary>
	/// SpawnPos에 장식물 배치가능 여부
	/// </summary>
	/// <param name="propData"></param>
	/// <param name="roomDecoData"></param>
	/// <param name="spawnPos"></param>
	/// <returns></returns>
	public bool CanPlace(PropObject propData, RoomDecoDataSO roomDecoData, List<Vector2> placementList, out Vector2 spawnPos)
	{
		spawnPos = new Vector2Int(-1, -1);

		// PlacementType을 확인하고 roomDecoData에서 Place를 찾는다.
		PlacementType placeType = propData.placementType;

		// RoomDeco에서 type과 알맞은 장소를 찾는다.
		List<Vector2> placePosList;

		placePosList = placementList;

		bool canPlace = false;

		foreach (Vector2 pos in placePosList)
		{
			int x = propData.size.x;
			int y = propData.size.y;

			if (placeType == PlacementType.Wall && pos.y < roomDecoData.groundLevelY + 1) // 벽인데 바닥 쪽에 너무 붙어 있는 곳은 배치 불가능
				continue;

			if(placeType == PlacementType.Ground && pos.y != roomDecoData.groundLevelY) // 바닥의 위치가 아니라면 바닥은 배치 불가능
				continue;

			canPlace = true;

			// 배치 후보지 리스트

			// 배치 하려하는 Vector2Int 기준으로 propData의 size만큼의 영역을 계산해서 isOccupied를 확인한다.
			// 예시) propData.size가 (2,3)이고, pos가 (5,5)라면, (5,5)~(6,7)까지의 영역이 빈 곳인지 확인해야 함
			List<Vector2> placementArea = new List<Vector2>(); // 배치 가능한 영역의 좌표 리스트
			for (int py = 0; py < y; py++)
			{
				for (int px = 0; px < x; px++)
				{
					placementArea.Add(new Vector2(pos.x + px, pos.y + py));
				}
			}

			// 배치하려는 영역이 모두 빈 곳인지 확인
			// 리팩토링 필요
			for(int i = 0; i < placementArea.Count; i++)
			{
				Vector2 checkPos = placementArea[i];
				// 방의 크기를 벗어나거나, isOccupied가 true인 곳이 하나라도 있으면 현재 위치엔 배치 불가능
				if(checkPos.x < 0 || checkPos.x >= roomDecoData.roomSize.x || checkPos.y < 0 || checkPos.y >= roomDecoData.roomSize.y || isOccupied[(int)checkPos.y * roomDecoData.roomSize.x + (int)checkPos.x])
				{
					canPlace = false;
					break;
				}
			}

			// 배치할 위치를 찾았으면 가챠를 돌려 배치를 확정하면 해당 위치를 저장한다.
			if(canPlace)
			{
				spawnPos = pos;
				break;
			}
		}

		return canPlace;
	}

	/// <summary>
	/// spawnPos부터 size까지 isOccupied를 true로 마킹해서 해당 영역이 점유되었다고 표시.
	/// </summary>
	/// <param name="spawnPos"></param>
	/// <param name="size"></param>
	public void MarkOccupied(Vector2 spawnPos, Vector2Int size, Vector2Int roomSize)
	{
		try
		{
			if(spawnPos == new Vector2Int(-1, -1))
			{
				return;
			}

			for (int py = 0; py < size.y; py++)
			{
				for (int px = 0; px < size.x; px++)
				{
					isOccupied[(int)(spawnPos.y + py) * roomSize.x + (int)spawnPos.x + px] = true; // 해당 좌표를 점유로 마킹
				}
			}
		}
		catch(Exception e)
		{
			DebugX.LogError($"MarkOccupied Error: spawnPos {spawnPos}, size {size}, roomSize {roomSize}, Exception {e}");
		}
	}

	public void ClearSpawnedProps()
	{
		foreach (var prop in spawnedPropList)
		{
			poolService.Release(prop);
		}

		spawnedPropList.Clear();
	}
}
