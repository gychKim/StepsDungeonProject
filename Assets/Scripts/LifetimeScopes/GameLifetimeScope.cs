using System;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
		builder.Register<DataSystem>(Lifetime.Singleton)
			.As<IDataService, IGameLoad>();

		builder.Register<SaveSystem>(Lifetime.Singleton)
			.As<ISaveService, IInitializable, IDisposable>();

		builder.Register<AudioSystem>(Lifetime.Singleton)
			.As<IAudioService, IInitializable>();

		builder.Register<ResourceSystem>(Lifetime.Singleton)
			.As<IResourceService>();

		builder.Register<GoogleSheetManager>(Lifetime.Singleton)
			.As<IGoogleSheetService>();


	}
}
