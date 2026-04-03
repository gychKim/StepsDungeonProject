using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using UnityEngine;

public interface IUIScreen
{
	void Open();
	void Close();
}

public interface IUIPopupBase
{
	void Close(); // Close는 in/out의 제네릭 타입이 필요 없으니 Base에 놔둔다.
}
public interface IUIPopup<TResult> : IUIPopupBase
{
	Cysharp.Threading.Tasks.UniTask<TResult> OpenAsync(CancellationToken cancelToken);
}

public interface IUIInit<in TData>
{
	void Init(TData data);
}

public enum ScreenId
{

}

public enum PopupId
{
	None = -1,
	Choice,
	ChoiceResult,
	BossIntro,
	BossFight,
	Result
}

public readonly struct ScreenKey
{
	public readonly ScreenId Id;
	public ScreenKey(ScreenId id)
	{
		Id = id;
	}
}

public readonly struct PopupKey<TResult>
{
	public readonly PopupId Id;
	public PopupKey(PopupId id)
	{
		Id = id;
	}
}

public readonly struct PopupKey<TData, TResult>
{
	public readonly PopupId Id;
	public PopupKey(PopupId id)
	{
		Id = id;
	}
}

public static class UIKeys
{
	public static readonly PopupKey<UIChoiceContext, int> Choice = new PopupKey<UIChoiceContext, int>(PopupId.Choice);
	public static readonly PopupKey<UIChoiceResultDTO, Unit> ChoiceResult = new PopupKey<UIChoiceResultDTO, Unit>(PopupId.ChoiceResult);
	public static readonly PopupKey<UIBossIntroContext, Unit> BossIntro = new PopupKey<UIBossIntroContext, Unit>(PopupId.BossIntro);
	public static readonly PopupKey<Unit> BossFight = new PopupKey<Unit>(PopupId.BossFight);
	public static readonly PopupKey<UIResultContext, Unit> Result = new PopupKey<UIResultContext, Unit>(PopupId.Result);

}

[Serializable]
public class ScreenEntry
{
	public ScreenId id;
	public MonoBehaviour view; // IUIScreen 구현 오브젝트 넣기
}

[Serializable]
public class PopupEntry
{
	public PopupId id;
	public MonoBehaviour view; // IUIPopup 구현 오브젝트 넣기
}

public interface IUIUseCase
{
	void OpenScreen(ScreenKey key);
	UniTask<TResult> OpenPopupAsync<TResult>(PopupKey<TResult> key, CancellationToken cancelToken);
	UniTask<TResult> OpenPopupAsync<TData, TResult>(PopupKey<TData, TResult> key, TData data, CancellationToken cancelToken);
	void CloseTopPopup();
	void CloseAllPopup();
	void Back();
}

public class UISystem : IUIUseCase
{
	private readonly UIController uiController;

	public UISystem(UIController uiController)
	{
		this.uiController = uiController;
	}

	public void OpenScreen(ScreenKey key)
	{
		uiController.OpenScreen(key);
	}
	public UniTask<TResult> OpenPopupAsync<TData, TResult>(PopupKey<TData, TResult> key, TData data, CancellationToken cancelToken)
	{
		return uiController.OpenPopupAsync(key, data, cancelToken);
	}

	public UniTask<TResult> OpenPopupAsync<TResult>(PopupKey<TResult> key, CancellationToken cancelToken)
	{
		return uiController.OpenPopupAsync(key, cancelToken);
	}

	public void CloseTopPopup()
	{
		uiController.CloseTopPopup();
	}

	public void CloseAllPopup()
	{
		uiController.CloseAllPopup();
	}

	public void Back()
	{
		uiController.Back();
	}
}
