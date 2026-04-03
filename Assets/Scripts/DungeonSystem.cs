using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

public enum DungeonType
{
	Tower
}
public class DungeonData
{
	public int dungeonSeed;
	public DungeonType dungeonType;
	public int currentFloor;
	public List<ChoiceHistoryData> choiceHistoryDataList; // 선택 데이터 히스토리
}

public class CurrentRoomData
{
	public RoomTheme roomTheme; // 현재 룸 테마
	public RoomLevel roomLevel; // 현재 룸 난이도
	public ObjectType objectType; // 현재 룸 오브젝트 타입

	public Vector3Int objectPosition;
	public TransportType Transport01Type;
	public Vector3Int Transport01Position;
	public TransportType Transport02Type;
	public Vector3Int Transport02Position;

}

public interface IDungeonUseCase
{
	/// <summary>
	/// 게임 시작
	/// </summary>
	void GameStart();

	/// <summary>
	/// 이전 선택지 데이터 추가
	/// </summary>
	/// <param name="choiceData"></param>
	void AddChoiceHistory(ChoiceHistoryData choiceData);

	/// <summary>
	/// 가장 최근 선택한 선택지 인덱스 반환
	/// </summary>
	/// <returns></returns>
	int GetRecentSelectdIndex();

	/// <summary>
	/// 룸 데이터 전달 Observable
	/// </summary>
	Observable<NormalRoomData> RoomDataInitObv { get; }

	Observable<RoomDecoDataSO> RoomDecoDataInitObv { get; }

	Observable<BossRoomData> BossRoomDataInitObv { get; }
}

public class DungeonSystem : IDungeonUseCase, IGameStateTransitionUseCase, IInitializable, IDisposable
{
	private readonly ISaveService saveService;
	private readonly IDataService dataService;
	private readonly IResourceService resourceService;
	private readonly IGameStateUseCase gameStateUseCase;

	private readonly CompositeDisposable disposables;

	private DungeonData currentDungeonData;
	private NormalRoomData currentRoomData;
	private BossRoomData currentBossData;

	public Observable<NormalRoomData> RoomDataInitObv => roomDataInitSub;
	public Observable<RoomDecoDataSO> RoomDecoDataInitObv => roomDecoDataInitSub;

	private Subject<NormalRoomData> roomDataInitSub;
	private Subject<RoomDecoDataSO> roomDecoDataInitSub;

	public Observable<BossRoomData> BossRoomDataInitObv => bossRoomDataInitSub;
	private Subject<BossRoomData> bossRoomDataInitSub;

	public DungeonSystem(ISaveService saveService, IDataService dataService, IResourceService resourceService, IGameStateUseCase gameStateUseCase)
	{
		disposables = new CompositeDisposable();
		roomDataInitSub = new Subject<NormalRoomData>();
		roomDecoDataInitSub = new Subject<RoomDecoDataSO>();
		bossRoomDataInitSub = new Subject<BossRoomData>();

		this.saveService = saveService;
		this.dataService = dataService;
		this.resourceService = resourceService;
		this.gameStateUseCase = gameStateUseCase;
		gameStateUseCase.Init(this);
	}

	public void Initialize()
	{
		gameStateUseCase.GameStateObv
			.Subscribe(gameState =>
			{
				switch (gameState)
				{
					case GameState.MoveNext: MoveNext(); break;

				}
			}).AddTo(disposables);
	}

	public async UniTask OnStateEnterAsync(GameState gameState, CancellationToken ct = default)
	{
		switch(gameState)
		{
			case GameState.EnterRoom:
				await SettingRoom(ct);
				break;
			// case GameState.EnterRoom:
			// 	await SettingRoom(ct);
			// 	break;
			case GameState.MoveNext:
				break;
		}
	}

	public void GameStart()
	{
		currentDungeonData = saveService.SaveData.dungeonData;

		if (currentDungeonData == null)
		{
			currentDungeonData = new DungeonData()
			{
				dungeonSeed = (int)System.DateTime.Now.Ticks,
				currentFloor = 1,
				choiceHistoryDataList = new List<ChoiceHistoryData>()
			};
			saveService.SaveData.dungeonData = currentDungeonData;

			saveService.Save();
		}

		Random.SetSeed(currentDungeonData.dungeonSeed);

		gameStateUseCase.SetGameState(GameState.EnterRoom);
	}

	private async UniTask SettingRoom(CancellationToken ct)
	{
		// 현재 생성할 방을 판단한다.
		// 보스룸인지, 일반 룸인지 판단
		// 보스 룸이라면
		if (currentDungeonData.currentFloor >= 9)
		{
			// 현재 던전의 보스 데이터를 가져온다.
			// 
			BossRoomData currentBossData = dataService.GetBossRoomData(currentDungeonData.dungeonType);
			RoomDecoDataSO roomDecoDataSO = await resourceService.LoadAssetDataAsync<RoomDecoDataSO>("RoomDeco_" + currentBossData.RoomTheme.ToString(), ct);

			this.currentBossData = currentBossData;

			bossRoomDataInitSub.OnNext(currentBossData);
			roomDecoDataInitSub.OnNext(roomDecoDataSO);

			// fadeUseCase.FadeIn(5f).Forget();

			// 게임 상태 변경 => 룸 진입
			gameStateUseCase.SetGameState(GameState.EnterBossRoom);
		}
		else
		{
			// 지난 번 선택지의 데이터를 토대로 새로운 랜덤한 방의 데이터를 초기화/생성한다.
			// 그 데이터를 생성했다면 RoomGenerator과 Choice에게 알려준다.

			// 이전 선택지 데이터를 가져온다.
			var choiceHistoryDataList = currentDungeonData.choiceHistoryDataList;

			// 룸 데이터 초기화
			RoomLevel setRoomLevel;

			// 이전 선택지 데이터를 가져온다.
			ChoiceHistoryData choiceHistoryData = null;
			if (choiceHistoryDataList.Any())
			{
				choiceHistoryData = choiceHistoryDataList[choiceHistoryDataList.Count - 1];

				setRoomLevel = choiceHistoryData.nextRoomLevel; // 이전 선택지 결과를 기반으로 새로운 룸/보상을 생성한다.
			}
			else
			{
				setRoomLevel = RoomLevel.Low;
			}

			NormalRoomData currentRoomData = dataService.GetRoomData(setRoomLevel);
			RoomDecoDataSO roomDecoDataSO = await resourceService.LoadAssetDataAsync<RoomDecoDataSO>("RoomDeco_" + currentRoomData.RoomTheme.ToString(), ct);

			// 1. 던전이 NormalRoomData와 RoomDecoDataSO를 뿌린다.
			// 		- NormalRoomData는 방의 테마, 난이도, 오브젝트 타입과 위치 같은 정보를 담고 있다.
			// 		- RoomDecoDataSO는 방에 존재하는 구조물(Object, Transport 등), 장식(Props)에 대한 타입과 위치 그리고 플레이어의 시작/종료 위치 등의 정보를 담고 있다.
			// 2. RoomGenerator는 그 데이터를 받아서 방을 생성한다.
			//		- RoomGenerator는 NormalRoomData의 테마와 난이도 정보를 활용해서 방의 구조를 생성하고, 오브젝트와 트랜스포트는 NormalRoomData의 정보를 활용해서 배치한다. 그리고 Props는 RoomDecoDataSO의 정보를 활용해서 배치한다.
			// 3. Presentation은 그 데이터를 받아서 연출을 한다.
			//		- Presentation은 RoomDecoDataSO의 플레이어 시작 위치 정보를 활용해서 캐릭터를 이동시키고, 방에 대한 연출을 한다.
			// 4. 캐릭터는 그 데이터를 받아서 위치를 세팅한다.
			// 5. Choice는 그 데이터를 받아서 선택지를 세팅한다.
			// 6. 시스템은 룸 세팅이 끝났다는 것을 Presentation에게 알려준다.

			this.currentRoomData = currentRoomData;

			roomDataInitSub.OnNext(currentRoomData);
			roomDecoDataInitSub.OnNext(roomDecoDataSO);

			// fadeUseCase.FadeIn(5f).Forget();

			// 게임 상태 변경 => 룸 진입
			// gameStateUseCase.SetGameState(GameState.EnterRoom);
		}

		// 룸 초기화 끝날 때 까지 대기? or 2초정도만 대기?

		// 페이드 인

	}

	private UniTask MoveNext()
	{
		currentDungeonData.currentFloor++;
		return UniTask.CompletedTask;
	}

	public void AddChoiceHistory(ChoiceHistoryData choiceData)
	{
		currentDungeonData.choiceHistoryDataList.Add(choiceData);
	}

	public int GetRecentSelectdIndex()
	{
		return currentDungeonData.choiceHistoryDataList.Last().selectedIndex;
	}


	public void Dispose()
	{
		disposables.Dispose();
		disposables.Clear();
	}
}
