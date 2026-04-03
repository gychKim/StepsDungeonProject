using R3;

/// <summary>
/// Presenter 인터페이스
/// </summary>
/// <typeparam name="TView"></typeparam>
/// <typeparam name="TModel"></typeparam>
public interface IPresenter<TView, TModel> : VContainer.Unity.IInitializable, System.IDisposable where TView : IUIView
{
	CompositeDisposable Disposables { get; }
}

public abstract class BasePresenter<TView, TModel> : IPresenter<TView, TModel> where TView : IUIView
{
    protected TView view;
    protected TModel model;

    public BasePresenter(TView view, TModel model)
    {
        this.view = view;
        this.model = model;
    }

	public CompositeDisposable Disposables { get; } = new CompositeDisposable();

	public abstract void Dispose();

	public abstract void Initialize();
}
