using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using R3;
using System;
using System.ComponentModel.Design;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

public interface IBossUseCase
{
	Observable<BossController> BossSpawnedObv { get; }
}

public class BossSystem : IBossUseCase, IGameStateTransitionUseCase, IInitializable, IDisposable
{
	private readonly IGameUseCase gameUseCase;
	private readonly IGameStateUseCase gameStateUseCase;
	private readonly IDungeonUseCase dungeonUseCase;
	private readonly IUIUseCase uiUseCase;
	private readonly IDataService dataService;
	private readonly IPowerUseCase powerUseCase;
	private readonly ISkillUseCase skillUseCase;
	private readonly IResourceService resourceService;

	private BossRoomData currentBossRoomData;

	private BossController bossController;

	private CompositeDisposable disposables;

	public Observable<BossController> BossSpawnedObv => bossSpawnedSub;
	private Subject<BossController> bossSpawnedSub = new Subject<BossController>();
	private UniTaskCompletionSource<BossRoomData> bossRoomDataUCS;

	public BossSystem(IGameUseCase gameUseCase, IGameStateUseCase gameStateUseCase, IDungeonUseCase dungeonUseCase, IUIUseCase uiUseCase, IDataService dataService, IPowerUseCase powerUseCase, ISkillUseCase skillUseCase, IResourceService resourceService)
	{
		disposables = new CompositeDisposable();
		bossRoomDataUCS = new UniTaskCompletionSource<BossRoomData>();

		this.gameUseCase = gameUseCase;
		this.gameStateUseCase = gameStateUseCase;
		gameStateUseCase.Init(this);
		this.dungeonUseCase = dungeonUseCase;
		this.uiUseCase = uiUseCase;
		this.dataService = dataService;
		this.powerUseCase = powerUseCase;
		this.skillUseCase = skillUseCase;
		this.resourceService = resourceService;
	}

	public void Initialize()
	{
		dungeonUseCase.BossRoomDataInitObv
			.Subscribe(bossData =>
			{
				bossRoomDataUCS.TrySetResult(bossData);

				CancellationTokenSource bossSpawnCTS = new CancellationTokenSource();
				currentBossRoomData = bossData;
				BossSpawnAsync(bossSpawnCTS.Token).Forget();

			}).AddTo(disposables);

		gameStateUseCase.GameStateObv
			.Subscribe(state =>
			{
				switch(state)
				{
					case GameState.BossIntro:
						EnterBossRoom().Forget();
						break;
					case GameState.Result:

						break;
				}
			}).AddTo(disposables);
	}

	public async UniTask OnStateEnterAsync(GameState gameState, CancellationToken ct = default)
	{
		switch(gameState)
		{
			case GameState.BossIntro:
				await BossSpawnAsync(ct);
				break;
		}
	}

	private async UniTask BossSpawnAsync(CancellationToken ct)
	{
		await bossRoomDataUCS.Task; // 보스룸 데이터를 받을 때 까지 대기

		GameObject bossPrefab = await resourceService.LoadAssetDataAsync<GameObject>($"Boss_{currentBossRoomData.DungeonType}", ct);
		var bossObj = GameObject.Instantiate(bossPrefab, currentBossRoomData.BossPosition, Quaternion.identity);
		bossController = bossObj.GetComponent<BossController>();

		bossSpawnedSub.OnNext(bossController);
	}

	private async UniTask EnterBossRoom()
	{
		CancellationTokenSource cancelToken = new CancellationTokenSource();
		// 보스 위치 지정
		// 보스 생성
		// 보스 애니메이션 지정

		// Intro UI Open
		UIBossIntroContext introContext = new UIBossIntroContext()
		{
			bossTitle = currentBossRoomData.BossTitle,
			bossContent = currentBossRoomData.BossContent,
			bossFightButtonText = currentBossRoomData.BossFightButtonText,
		};

		// 인트로 대기
		await uiUseCase.OpenPopupAsync(UIKeys.BossIntro, introContext, cancelToken.Token);

		// 배틀 UI 활성화
		await uiUseCase.OpenPopupAsync(UIKeys.BossFight, cancelToken.Token);

		DebugX.PurpleLog("보스전 종료");

		// 게임 시스템에게 점수를 전달해야하는데,
		// 1. BossSystem이 게임 시스템에게 전달한다.

		gameUseCase.GameResult().Forget();
		// gameStateUseCase.SetGameState(GameState.BossEnding);
		// 등등
	}

	public void Dispose()
	{
		disposables.Dispose();
	}
}
