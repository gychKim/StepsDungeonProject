using VContainer;
using VContainer.Unity;

public class GameSceneLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
		builder.RegisterEntryPoint<PresentationSystem>(Lifetime.Singleton)
			.As<IPresentationUseCase, IGameStateTransitionUseCase, IInitializable>();

		builder.Register<RewardSystem>(Lifetime.Singleton)
			.As<IRewardUseCase>();

		builder.RegisterEntryPoint<CameraSystem>(Lifetime.Singleton)
			.As<ICameraUseCase/* , IGameStateTransitionUseCase */>();

		builder.RegisterComponentInHierarchy<CameraController>();

		builder.Register<UISystem>(Lifetime.Singleton)
			.As<IUIUseCase>();

		builder.RegisterComponentInHierarchy<UIController>();

		builder.Register<GameStateSystem>(Lifetime.Singleton)
			.As<IGameStateUseCase>();

		builder.RegisterEntryPoint<DungeonSystem>(Lifetime.Singleton)
			.As<IDungeonUseCase, IGameStateTransitionUseCase, IInitializable>();

		builder.RegisterEntryPoint<ChoiceSystem>(Lifetime.Singleton)
			.As<IChoiceUseCase, IGameStateTransitionUseCase>();

		builder.RegisterEntryPoint<RoomGeneratorSystem>(Lifetime.Singleton)
			.As<IRoomGeneratorUseCase, IGameStateTransitionUseCase, IInitializable>();

		builder.RegisterEntryPoint<GameSystem>(Lifetime.Singleton)
			.As<IGameUseCase, IInitializable, IPostStartable>();

		builder.RegisterEntryPoint<PowerSystem>(Lifetime.Singleton)
			.As<IPowerUseCase, IInitializable>();

		builder.RegisterEntryPoint<SkillSystem>(Lifetime.Singleton)
			.As<ISkillUseCase, IInitializable>();

		builder.Register<SkillFactory>(Lifetime.Singleton);

		builder.Register<ImmediateTrigger>(Lifetime.Transient);
		builder.Register<IntervalTimerTrigger>(Lifetime.Transient);
		builder.Register<AttackCounterTrigger>(Lifetime.Transient);
		builder.Register<TimeTrigger>(Lifetime.Transient);
		builder.Register<BattleEndTrigger>(Lifetime.Transient);
		builder.Register<AddScoreEffect>(Lifetime.Transient);
		builder.Register<PowerMultiplierEffect>(Lifetime.Transient);
		builder.Register<PresentationDamgeEffect>(Lifetime.Transient);

		builder.RegisterComponentInHierarchy<RoomController>();

		builder.Register<PropSpawner>(Lifetime.Singleton);
		builder.Register<ObjectSpawner>(Lifetime.Singleton);
		builder.Register<BossSpawner>(Lifetime.Singleton);

		builder.RegisterComponentInHierarchy<CharacterSystem>()
			.As<ICharacterUseCase/* , IGameStateTransitionUseCase */>();

		builder.RegisterComponentInHierarchy<FadeSystem>()
			.As<IFadeUseCase>();

		builder.RegisterEntryPoint<BossSystem>(Lifetime.Singleton)
			.As<IBossUseCase, IGameStateTransitionUseCase, IInitializable>();

		builder.RegisterComponentInHierarchy<TestPoissonDiscSampling>();

		builder.Register<PoolSystem>(Lifetime.Singleton)
			.As<IPoolService>();

		builder.Register<BossBattleScore>(Lifetime.Singleton)
			.As<IBossBattleScore>();

		builder.RegisterComponentInHierarchy<BossFightView>();

		builder.RegisterEntryPoint<BossFightModel>(Lifetime.Singleton)
			.As<IBattleContext>()
			.AsSelf();

		builder.RegisterEntryPoint<BossFightPresenter>(Lifetime.Singleton)
			.As<IInitializable>()
			.AsSelf();
	}
}
