using R3;

public interface IModel : System.IDisposable
{
	CompositeDisposable Disposables { get; }
}
public abstract class BaseModel : IModel
{
	public abstract void Dispose();

	public CompositeDisposable Disposables { get; } = new CompositeDisposable();
}
