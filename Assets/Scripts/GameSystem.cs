using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VContainer.Unity;



public interface IGameUseCase
{
	UniTask GameResult();
}

public class GameSystem : IGameUseCase/* , IGameStateTransitionUseCase */, IInitializable, IPostStartable
{
	private readonly IGameStateUseCase gameStateUseCase;
	private readonly IDungeonUseCase dungeonUseCase;
	private readonly IUIUseCase uiUseCase;
	private readonly IBossBattleScore bossBattleScore;

	private CompositeDisposable disposables;

	private float startTime;

	public GameSystem(/* IGameStateUseCase gameStateUseCase, */ IDungeonUseCase dungeonUseCase, IUIUseCase uiUseCase, IBossBattleScore bossBattleScore)
	{
		disposables = new CompositeDisposable();

		/* this.gameStateUseCase = gameStateUseCase;
		gameStateUseCase.Init(this); */
		this.dungeonUseCase = dungeonUseCase;
		this.uiUseCase = uiUseCase;
		this.bossBattleScore = bossBattleScore;
	}

	public void Initialize()
	{
		// gameStateUseCase.GameStateObv
		// 	.Subscribe(state =>
		// 	{
		// 		switch(state)
		// 		{
		// 			case GameState.Result:
		// 				GameResult().Forget();
		// 				break;
		// 		}
		// 	}).AddTo(disposables);
	}

	/* public async UniTask OnStateEnterAsync(GameState gameState, CancellationToken cts = default)
	{
		switch(gameState)
		{
			case GameState.Result:
				break;
		}
	} */

	public void PostStart()
	{
		StartGame();
	}

	public void StartGame()
	{
		startTime = Time.time;

		dungeonUseCase.GameStart();
	}

	public void ExitGame()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
	}

	public async UniTask GameResult()
	{
		CancellationTokenSource cts = new CancellationTokenSource();

		float playTime = Time.time - startTime;

		int min = Mathf.FloorToInt(playTime / 60);
		int sec = Mathf.FloorToInt(playTime % 60);

		UIResultContext context = new UIResultContext
		{
			score = bossBattleScore.GetScore(),
			playTime = $"{min:00}:{sec:00}"
		};

		await uiUseCase.OpenPopupAsync(UIKeys.Result, context, cts.Token);

		ExitGame();
	}
}
