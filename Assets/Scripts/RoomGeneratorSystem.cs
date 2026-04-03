using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using R3;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using VContainer.Unity;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// 룸 난이도
/// </summary>
public enum RoomLevel
{
	Low,
	Middle,
	High,
}

/// <summary>
/// 룸 타입(테마)
/// </summary>
public enum RoomTheme
{
	Hallway, // 복도(가장 Default)
}

/// <summary>
/// 룸 오브젝트 타입
/// </summary>
public enum ObjectType
{
	Chest, // 상자(가장 Default)
}

/// <summary>
/// 룸 이동 수단 타입
/// </summary>
public enum TransportType
{
	Ladder, // 사다리
	UpStair, // 위 계단
	DownStair, // 아래 계단
}

public enum TileType
{
	Empty,
	Ground,
	Wall,
}

public class RoomData
{
	public int Index;

	[JsonConverter(typeof(StringEnumConverter))]
	public RoomTheme RoomTheme;
	[JsonConverter(typeof(Vector3IntConverter))]
	public Vector3Int PlayerStartPosition;

	[JsonConverter(typeof(Vector3IntConverter))]
	public Vector3Int PlayerMovePosition;
}

public class NormalRoomData : RoomData
{
	[JsonConverter(typeof(StringEnumConverter))]
	public RoomLevel RoomLevel;

	[JsonConverter(typeof(StringEnumConverter))]
	public ObjectType ObjectType;

	// 시트에서 온 원본 문자열 (예: "[30, 0, 0]" 또는 "30, 0, 0")
	[JsonConverter(typeof(Vector3IntConverter))]
	public Vector3Int ObjectPosition;

	[JsonConverter(typeof(StringEnumConverter))]
	public TransportType Transport01Type;

	[JsonConverter(typeof(Vector3IntConverter))]
	public Vector3Int Transport01Position;

	[JsonConverter(typeof(StringEnumConverter))]
	public TransportType Transport02Type;

	[JsonConverter(typeof(Vector3IntConverter))]
	public Vector3Int Transport02Position;
}

public class BossRoomData : RoomData
{
	[JsonConverter(typeof(StringEnumConverter))]
	public DungeonType DungeonType;

	[JsonConverter(typeof(Vector3IntConverter))]
	public Vector3Int BossPosition;
	public string BossTitle;
	public string BossContent;
	public string BossFightButtonText;
}

public interface IRoomGeneratorUseCase
{

}

public class RoomGeneratorSystem : IRoomGeneratorUseCase, IGameStateTransitionUseCase, IInitializable, IDisposable
{
	private readonly IResourceService resourceService;
	private readonly IGameStateUseCase gameStateUseCase;
	private readonly IDungeonUseCase dungeonUseCase;

	private readonly CompositeDisposable disposables;

	private readonly RoomController roomController;

	private TestPoissonDiscSampling testPoissonDiscSampling;

	private UniTaskCompletionSource<NormalRoomData> roomDataUCS;
	private UniTaskCompletionSource<BossRoomData> bossRoomDataUCS;
	
	public RoomGeneratorSystem(IResourceService resourceService, IGameStateUseCase gameStateUseCase, IDungeonUseCase dungeonUseCase, RoomController tileMap, TestPoissonDiscSampling testPoissonDiscSampling)
	{
		disposables = new CompositeDisposable();
		roomDataUCS = new UniTaskCompletionSource<NormalRoomData>();
		bossRoomDataUCS = new UniTaskCompletionSource<BossRoomData>();

		this.resourceService = resourceService;
		this.gameStateUseCase = gameStateUseCase;
		gameStateUseCase.Init(this);
		
		this.dungeonUseCase = dungeonUseCase;
		this.roomController = tileMap;
		this.testPoissonDiscSampling = testPoissonDiscSampling;
	}

	public void Initialize()
	{
		dungeonUseCase.RoomDataInitObv
			.Subscribe(roomData =>
			{
				roomDataUCS.TrySetResult(roomData);
			}).AddTo(disposables);

		dungeonUseCase.BossRoomDataInitObv
			.Subscribe(bossData =>
			{
				bossRoomDataUCS.TrySetResult(bossData);
			}).AddTo(disposables);
	}

	public async UniTask OnStateEnterAsync(GameState gameState, CancellationToken ct = default)
	{
		switch(gameState)
		{
			case GameState.EnterRoom:
				await EnterRoomAsync(ct);
				break;
			case GameState.EnterBossRoom:
				await EnterBossRoomAsync(ct);
				break;
		}
	}

	private async UniTask EnterRoomAsync(CancellationToken ct)
	{
		NormalRoomData data = await roomDataUCS.Task; // 룸 데이터를 받을 때 까지 대기
		roomDataUCS = new UniTaskCompletionSource<NormalRoomData>(); // 룸 데이터 대기 토큰 새로 갱신

		await RoomGenerate(data, ct); // 룸 생성
	}

	private async UniTask EnterBossRoomAsync(CancellationToken ct)
	{
		BossRoomData data = await bossRoomDataUCS.Task; // 룸 데이터를 받을 때 까지 대기
		bossRoomDataUCS = new UniTaskCompletionSource<BossRoomData>(); // 룸 데이터 대기 토큰 새로 갱신

		await RoomGenerate(data, ct); // 룸 생성
	}

	private async UniTask RoomGenerate(NormalRoomData roomData, CancellationToken ct)
	{
		roomController.ClearAll();

		RoomDecoDataSO roomDecoDataSO = await resourceService.LoadAssetDataAsync<RoomDecoDataSO>("RoomDeco_" + roomData.RoomTheme.ToString(), ct);

		for (int i = 0; i < roomDecoDataSO.tileDataArr.Length; i++)
		{
			int x = i % roomDecoDataSO.roomSize.x;
			int y = i / roomDecoDataSO.roomSize.x;

			Vector3Int pos = new Vector3Int(x, y, 0);
			roomController.SetGroundTile(pos, roomDecoDataSO.tileDataArr[i]);
		}

		await roomController.SpawnProps(roomDecoDataSO);

		testPoissonDiscSampling.Test(roomController.testPlacementList);

		await UniTask.WhenAll(roomController.SpawnObjectAsync(roomData, roomDecoDataSO.groundLevelY, ct), roomController.SpawnTransportObjectsAsync(roomData, roomDecoDataSO.groundLevelY, ct));
	}

	private async UniTask RoomGenerate(BossRoomData bossData, CancellationToken ct)
	{
		roomController.ClearAll();

		RoomDecoDataSO roomDecoDataSO = await resourceService.LoadAssetDataAsync<RoomDecoDataSO>("RoomDeco_" + bossData.RoomTheme.ToString(), ct);

		for (int i = 0; i < roomDecoDataSO.tileDataArr.Length; i++)
		{
			int x = i % roomDecoDataSO.roomSize.x;
			int y = i / roomDecoDataSO.roomSize.x;

			Vector3Int pos = new Vector3Int(x, y, 0);
			roomController.SetGroundTile(pos, roomDecoDataSO.tileDataArr[i]);
		}

		await roomController.SpawnProps(roomDecoDataSO);

		testPoissonDiscSampling.Test(roomController.testPlacementList);

		// await roomController.SpawnBossAsync(bossData, roomDecoDataSO.groundLevelY, ct);
	}

	public void Dispose()
	{
		disposables.Dispose();
	}
}
