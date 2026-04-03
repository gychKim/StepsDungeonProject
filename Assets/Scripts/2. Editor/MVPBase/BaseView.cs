using R3;
using UnityEngine;

/// <summary>
/// UIView 인터페이스
/// </summary>
public interface IUIView
{
	/// <summary>
	/// 최상위 UI 오브젝트
	/// </summary>
	UnityEngine.GameObject RootObject { get; }

	/// <summary>
	/// Dispose를 묶은 객체
	/// </summary>
	R3.CompositeDisposable Disposables { get; }

	/// <summary>
	/// 활성화
	/// </summary>
	void Show();

	/// <summary>
	/// 비활성화
	/// </summary>
	void Hide();
}

public abstract class BaseView : MonoBehaviour, IUIView
{
    public GameObject RootObject => gameObject;
    public CompositeDisposable Disposables { get; } = new CompositeDisposable();

	public void Show() => RootObject.SetActive(true);
	public void Hide() => RootObject.SetActive(false);

	private void OnDestroy()
    {
        Disposables.Dispose();
    }
}
