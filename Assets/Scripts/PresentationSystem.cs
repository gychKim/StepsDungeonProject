using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

public interface IPresentationUseCase
{

}

public class PresentationSystem : IPresentationUseCase, IGameStateTransitionUseCase, IInitializable, IDisposable
{
	private readonly IGameStateUseCase gameStateUseCase;
	private readonly IDungeonUseCase dungeonUseCase;
	private readonly ICharacterUseCase characterUseCase;
	private readonly IBossUseCase bossUseCase;
	private readonly ICameraUseCase cameraUseCase;
	private readonly IFadeUseCase fadeUseCase;

	private readonly CompositeDisposable disposables;

	private BossController bossController;

	private NormalRoomData currentRoomData;
	private RoomDecoDataSO currentRoomDecoData;
	private BossRoomData currentBossData;

	private UniTaskCompletionSource settingRoomCts;
	private UniTaskCompletionSource settingRoomDecoCts;

    public PresentationSystem(IGameStateUseCase gameStateUseCase, IDungeonUseCase dungeonUseCase, ICharacterUseCase characterUseCase, IBossUseCase bossUseCase, ICameraUseCase cameraUseCase, IFadeUseCase fadeUseCase)
	{
		disposables = new CompositeDisposable();
		settingRoomCts = new UniTaskCompletionSource();
		settingRoomDecoCts = new UniTaskCompletionSource();
		

		this.gameStateUseCase = gameStateUseCase;
		gameStateUseCase.Init(this);
		this.dungeonUseCase = dungeonUseCase;
		this.characterUseCase = characterUseCase;
		this.bossUseCase = bossUseCase;
		this.cameraUseCase = cameraUseCase;
		this.fadeUseCase = fadeUseCase;
	}

	public void Initialize()
	{
		gameStateUseCase.GameStateObv
			.Subscribe(state =>
			{
				ChangedState(state);
			}).AddTo(disposables);

		dungeonUseCase.RoomDataInitObv
			.Subscribe(roomData =>
			{
				currentRoomData = roomData;
				settingRoomCts.TrySetResult();
			}).AddTo(disposables);

		dungeonUseCase.RoomDecoDataInitObv
			.Subscribe(roomDecoData =>
			{
				currentRoomDecoData = roomDecoData;
				settingRoomDecoCts.TrySetResult();
			}).AddTo(disposables);

		dungeonUseCase.BossRoomDataInitObv
			.Subscribe(bossData =>
			{
				currentBossData = bossData;
			}).AddTo(disposables);

		bossUseCase.BossSpawnedObv
			.Subscribe(bossController =>
			{
				this.bossController = bossController;
			}).AddTo(disposables);
	}

	public async UniTask OnStateEnterAsync(GameState gameState, CancellationToken cts = default)
	{
		switch(gameState)
		{
			case GameState.EnterRoom:
				await SettingRoom();
				break;
		}
	}

	private void ChangedState(GameState state)
	{
		switch(state)
		{
			case GameState.EnterRoom:
				EnterRoomPresentation().Forget();
				break;

			case GameState.ChoiceSelect:
				ChoiceSelectPresentation();
				break;
			case GameState.ApplyReward:
				ApplyRewardPresentation();
				break;
			case GameState.MoveNext:
				MoveNextPresentation().Forget();
				break;
			case GameState.ReadyToTransition:
				ReadToTransitionPresentation().Forget();
				break;
			case GameState.ClearRoom:
				// ClearRoomPresentation();
				break;
			case GameState.EnterBossRoom:
				EnterBossRoomPresentation().Forget();
				break;
			case GameState.BossIntro:
				BossIntroPresentation();
				break;
			case GameState.BossFight:
				break;
			case GameState.BossEnding:
				BossEndingPresentation().Forget();
				break;
			case GameState.Result:
				break;
		}
	}

	private async UniTask SettingRoom()
	{
		await UniTask.WhenAll(settingRoomCts.Task, settingRoomDecoCts.Task); // 방 데이터와 방 장식 데이터가 준비될 때 까지 대기

		characterUseCase.KillTweener();
		characterUseCase.MoveAsync(new Vector3(currentRoomData.PlayerStartPosition.x, currentRoomDecoData.groundLevelY, currentRoomData.PlayerStartPosition.z), 0f, DG.Tweening.Ease.INTERNAL_Zero).Forget();
	}

	/// <summary>
	/// 방 입장 시 연출
	/// </summary>
	private async UniTaskVoid EnterRoomPresentation()
	{
		// 페이드 인
		fadeUseCase.FadeIn(5f).Forget();
		
		// 캐릭터 이동
		characterUseCase.SetAnimation("Move");
		await characterUseCase.MoveAsync(new Vector3(currentRoomData.PlayerMovePosition.x, currentRoomDecoData.groundLevelY, currentRoomData.PlayerMovePosition.z), 7f);

		// 게임 상태 변경
		gameStateUseCase.SetGameState(GameState.ChoiceSelect);
	}


	/// <summary>
	/// 선택지 일 때 연출
	/// </summary>
	private void ChoiceSelectPresentation()
	{
		cameraUseCase.SetCamera(CameraType.UI).Forget();

		// 캐릭터 애니메이션 변경
		characterUseCase.SetAnimation("Idle");

	}

	/// <summary>
	/// 보상 지급 시 연출
	/// </summary>
	private void ApplyRewardPresentation()
	{
		// ApplyReward에서는 상자 같은 오브젝트 및 캐릭터가 애니메이션을 실행해야 한다.
		// 사실 이건 상자 오브젝트 자체에서 수행하는 편이 좋을 듯
	}

	/// <summary>
	/// 다음 방으로 이동하는 연출 => Transport로 이동하는 연출
	/// </summary>
	/// <returns></returns>
	private async UniTaskVoid MoveNextPresentation()
	{
		cameraUseCase.SetCamera(CameraType.Player).Forget();

		characterUseCase.SetAnimation("Move");

		Vector3 transportPos = Vector3.zero;
		float duration = 0f;
		switch(dungeonUseCase.GetRecentSelectdIndex())
		{
			case 1:
				transportPos = new Vector3(currentRoomData.Transport01Position.x, currentRoomDecoData.groundLevelY, currentRoomData.Transport01Position.z);
				duration = 3f;
				break;
			case 2:
				transportPos = new Vector3(currentRoomData.Transport02Position.x, currentRoomDecoData.groundLevelY, currentRoomData.Transport02Position.z);
				duration = 6f;
				break;
		}

		// 이동 완료 대기
		await characterUseCase.MoveAsync(transportPos, duration);

		// 게임 상태 변경
		gameStateUseCase.SetGameState(GameState.ReadyToTransition);
	}

	/// <summary>
	/// 이동 수단 실행 연출
	/// </summary>
	/// <returns></returns>
	private async UniTaskVoid ReadToTransitionPresentation()
	{
		TransportType transportType = default;
		switch (dungeonUseCase.GetRecentSelectdIndex())
		{
			case 1:
				transportType = currentRoomData.Transport01Type;
				break;
			case 2:
				transportType = currentRoomData.Transport02Type;
				break;
		}

		Vector3 movePos = Vector3.zero;
		float duration = 0f;

		switch(transportType)
		{
			case TransportType.Ladder:
				characterUseCase.SetAnimation("Climb");
				movePos = characterUseCase.GetCurrentPosition() + new Vector3(0, 5, 0);
				duration = 5;
				break;
			case TransportType.UpStair:
				characterUseCase.SetAnimation("Move");
				movePos = characterUseCase.GetCurrentPosition() + new Vector3(5, 3, 0);
				duration = 5;
				break;
			case TransportType.DownStair:
				characterUseCase.SetAnimation("Move");
				movePos = characterUseCase.GetCurrentPosition() + new Vector3(5, -3, 0);
				duration = 5;
				break;
		}

		// 캐릭터 이동
		characterUseCase.MoveAsync(movePos, duration).Forget();

		settingRoomCts = new UniTaskCompletionSource();
		settingRoomDecoCts = new UniTaskCompletionSource();

		// 페이드 아웃
		await fadeUseCase.FadeOut(3f);

		// 게임 상태 변경
		gameStateUseCase.SetGameState(GameState.EnterRoom);
	}

	// private async UniTask ClearRoomPresentation()
	// {
	// 	await UniTask.WaitForSeconds(1f); // 이 부분 수정해야 한다. 1초 같은 하드코딩이 아닌 

	// 	gameStateUseCase.SetGameState(GameState.SettingRoom);
	// }

	private async UniTaskVoid EnterBossRoomPresentation()
	{
		// 보스가 생성되고 시스템이 전달받을 때 까지 대기
		await UniTask.WaitUntil(() => bossController != null);

		// 페이드 인
		fadeUseCase.FadeIn(5f).Forget();


		// 보스 대기
		bossController.SetAnimation("Idle");

		// 보스 위치 초기화
		bossController.MoveAsync(bossController.GetPosition() + currentRoomDecoData.groundLevelY * Vector3.up, 0f, DG.Tweening.Ease.INTERNAL_Zero).Forget();

		// 캐릭터 위치 초기화
		characterUseCase.MoveAsync(new Vector3(currentBossData.PlayerStartPosition.x, currentRoomDecoData.groundLevelY, currentBossData.PlayerStartPosition.z), 0f, DG.Tweening.Ease.INTERNAL_Zero).Forget();

		// 캐릭터 이동
		characterUseCase.SetAnimation("Move");

		await characterUseCase.MoveAsync(new Vector3(currentBossData.PlayerMovePosition.x, currentRoomDecoData.groundLevelY, currentBossData.PlayerMovePosition.z), 7f);

		// 게임 상태 변경
		gameStateUseCase.SetGameState(GameState.BossIntro);
	}

	private void BossIntroPresentation()
	{
		// 캐릭터 대기
		characterUseCase.SetAnimation("Idle");
	}

	private async UniTaskVoid BossEndingPresentation()
	{
		// 캐릭터 공격 애니메이션 반복 유지
		characterUseCase.SetAnimation("Attack_Punch");

		// 보스 대기
		bossController.SetAnimation("Idle");

		// 보스 애니메이션 리스트 수행
		// 현재는 그냥 5초 대기로 설정
		await UniTask.WaitForSeconds(5f);

		// 보스 공격
		bossController.SetAnimation("Attack_Punch");

		// 플레이어 사망
		characterUseCase.SetAnimation("Dead");

		// 게임 상태 변경
		gameStateUseCase.SetGameState(GameState.Result);
	}

	public void Dispose()
	{
		disposables.Dispose();
	}
}
