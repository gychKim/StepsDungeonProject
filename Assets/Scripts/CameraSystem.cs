using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

public interface ICameraUseCase
{
	UniTask SetCamera(CameraType cameraType);
}

public enum CameraType
{
	Player,
	UI,
}

public class CameraSystem : ICameraUseCase/* , IGameStateTransitionUseCase */, IInitializable, IDisposable
{
	private readonly IGameStateUseCase gameStateUseCase;
	public CameraController cameraController;

	private readonly CompositeDisposable disposables = null;
	public CameraSystem(CameraController cameraController, IGameStateUseCase gameStateUseCase)
	{
		disposables = new CompositeDisposable();

		this.cameraController = cameraController;
		// this.gameStateUseCase = gameStateUseCase;
		// gameStateUseCase.Init(this);
	}

	public void Initialize()
	{
		// gameStateUseCase.GameStateObv
		// 	.Subscribe(state =>
		// 	{
		// 		switch(state)
		// 		{
		// 			case GameState.ChoiceSelect: SetCamera(CameraType.UI); break;
		// 			case GameState.MoveNext: SetCamera(CameraType.Player); break;
		// 		}
		// 	}).AddTo(disposables);
	}

	// public async UniTask OnStateEnterAsync(GameState gameState, CancellationToken cts = default)
	// {
	// 	switch(gameState)
	// 	{
	// 		case GameState.ChoiceSelect:
	// 			break;
	// 		case GameState.MoveNext:
	// 			break;
	// 	}
	// }

	public UniTask SetCamera(CameraType cameraType)
	{
		cameraController.SetCamera(cameraType);
		return UniTask.CompletedTask;
	}

	public void Dispose()
	{
		disposables.Dispose();
	}
}
