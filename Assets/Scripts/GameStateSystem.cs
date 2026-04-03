using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;

public interface IGameStateTransitionUseCase
{
	/// <summary>
	/// gameState로 넘어가기 전 해야할 일
	/// </summary>
	/// <param name="gameState"></param>
	/// <param name="cts"></param>
	/// <returns></returns>
	UniTask OnStateEnterAsync(GameState gameState, CancellationToken cts = default);
}

public enum GameState
{
	// SettingRoom,
	EnterRoom,
	ChoiceSelect,
	ApplyReward,
	MoveNext,
	ReadyToTransition,
	ClearRoom,
	EnterBossRoom,
	BossIntro,
	BossFight,
	BossEnding,
	Result,
}

public interface IGameStateUseCase
{
	Observable<GameState> GameStateObv { get; }

	void SetGameState(GameState gameState);
	
	void Init(IGameStateTransitionUseCase gameStateTransitionUseCase);
}

public class GameStateSystem : IGameStateUseCase
{
	public Observable<GameState> GameStateObv => gameStateSub;
	private Subject<GameState> gameStateSub;

	private List<IGameStateTransitionUseCase> gameStateTransitionUseCaseList;

	private GameState currentGameState;
	
	public GameStateSystem()
	{
		gameStateSub = new Subject<GameState>();
		gameStateTransitionUseCaseList = new List<IGameStateTransitionUseCase>();
	}

	public void Init(IGameStateTransitionUseCase gameStateTransitionUseCase)
	{
		gameStateTransitionUseCaseList.Add(gameStateTransitionUseCase);
	}

	/// <summary>
	/// 게임 상태 변경
	/// </summary>
	/// <param name="newGameState"></param>
	public async void SetGameState(GameState newGameState)
	{
		CancellationTokenSource cts = new CancellationTokenSource();

		await ChangeGameStateAsync(newGameState, cts.Token); // 사전 준비

		gameStateSub.OnNext(newGameState);
		currentGameState = newGameState;

		// await UniTask.WaitForSeconds(0.1f);

		// gameStateSub.OnNext(newGameState);
	}

	/// <summary>
	/// 새로운 상태로 전환 하기 전 사전작업을 모두 수행한다.
	/// </summary>
	/// <param name="newGameState"></param>
	/// <param name="cts"></param>
	/// <returns></returns>
	private async UniTask ChangeGameStateAsync(GameState newGameState, CancellationToken cts = default)
	{
		await UniTask.WhenAll(gameStateTransitionUseCaseList
					.Select(useCase => useCase.OnStateEnterAsync(newGameState, cts)));

	}
}
