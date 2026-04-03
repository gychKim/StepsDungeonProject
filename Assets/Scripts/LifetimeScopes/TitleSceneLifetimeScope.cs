using VContainer;
using VContainer.Unity;

public class TitleSceneLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
		builder.RegisterEntryPoint<LoadingSystem>(Lifetime.Singleton)
			.As<IAsyncStartable>();

		// ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ이하 UIㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
		builder.RegisterComponentInHierarchy<TitleView>();

		builder.RegisterComponentInHierarchy<UILoading>();

		builder.RegisterEntryPoint<TitlePresenter>(Lifetime.Singleton)
			.As<IInitializable, IAsyncStartable>();

		builder.RegisterEntryPoint<TitleModel>(Lifetime.Singleton)
			.AsSelf();

		builder.RegisterComponentInHierarchy<TestSceneManager>();

		
	}
}
