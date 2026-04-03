using R3;
using System;
using UnityEngine;
using VContainer.Unity;


public class GameSceneUIController : IInitializable, IDisposable
{
	private readonly IGameStateUseCase gameStateUseCase;
	private readonly IUIUseCase uiUseCase;

	private readonly CompositeDisposable disposables;
    public GameSceneUIController(IUIUseCase uiUseCase, IGameStateUseCase gameStateUseCase)
	{
		disposables = new CompositeDisposable();

		this.uiUseCase = uiUseCase;
		this.gameStateUseCase = gameStateUseCase;
	}

	public void Initialize()
	{
		
	}

	public void Dispose()
	{
		disposables.Dispose();
	}
}
