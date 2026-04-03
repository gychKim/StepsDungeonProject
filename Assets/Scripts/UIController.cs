using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class UIController : MonoBehaviour
{
	public List<PopupEntry> popupEntryList = new List<PopupEntry>();
	public List<ScreenEntry> screenEntryList = new List<ScreenEntry>();

	private readonly Dictionary<ScreenId, IUIScreen> screenDict = new();
	private readonly Dictionary<PopupId, MonoBehaviour> popupDict = new();

	private readonly Stack<IUIScreen> screenStack = new();
	private readonly Stack<PopupId> popupStack = new();

	private void Awake()
	{
		// 미리 배치된 Screen UI 등록
		foreach (var entry in screenEntryList)
		{
			if (entry.view is not IUIScreen screen)
				throw new InvalidOperationException($"{entry.id} Screen View가 IUIScreen을 상속받지 않고 있습니다.");

			screenDict[entry.id] = screen;

			// 최초엔 담아두는 걸 권장(원하는 초기 상태면 조정바람)

			//if(entry.view is UIHome home)
			//	home.gameObject.SetActive(true);
			//else
			//	entry.view.gameObject.SetActive(false);
		}

		// 미리 배치된 Popup UI 등록
		foreach (var entry in popupEntryList)
		{
			popupDict[entry.id] = entry.view;
			//entry.view.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// ScreenUI 활성화
	/// </summary>
	/// <param name="key"></param>
	public void OpenScreen(ScreenKey key)
	{
		var next = GetScreenUI(key.Id);

		// 현재 Screen을 숨긴다. => 살아있음
		if (screenStack.TryPeek(out var current))
			current.Close();

		screenStack.Push(next);
		next.Open();
	}

	private IUIScreen GetScreenUI(ScreenId id)
	{
		if (!screenDict.TryGetValue(id, out IUIScreen screenUI))
			throw new KeyNotFoundException($"{id}에 해당하는 Screen이 등록되어 있지 않습니다.");

		return screenUI;
	}

	public async UniTask<TResult> OpenPopupAsync<TResult>(PopupKey<TResult> key, CancellationToken cancelToken)
	{
		var popupUI = GetPopupView(key.Id);

		if (popupUI is not IUIPopup<TResult> typed)
			throw new InvalidOperationException($"{key.Id} Popup View가 IUIPopup<{typeof(TResult).Name}>을 상속받지 않고 있습니다.");

		popupStack.Push(key.Id);
		//popupUI.gameObject.SetActive(true);

		try
		{
			// 팝업이 내부에서 버튼/연출로 완료되면 UniTask가 끝남
			var result = await typed.OpenAsync(cancelToken);
			return result;
		}
		finally
		{
			// Close는 멱등하게(연산을 여러 번 적용해도 결과가 달라지지 않음) 설계하는 걸 권장
			typed.Close();
			//popupUI.gameObject.SetActive(false);

			// Stack 정리(중간에 강제 CloseTopPopup()이 됐을 수도 있으니 방어적으로 처리)
			if (popupStack.Count > 0 && popupStack.Peek() == key.Id)
				popupStack.Pop();
			else
				RemoveFromPopupStack(key.Id);
		}
	}

	public async UniTask<TResult> OpenPopupAsync<TData, TResult>(PopupKey<TData, TResult> key, TData data, CancellationToken cancelToken)
	{
		var popupUI = GetPopupView(key.Id);

		if (popupUI is not IUIPopup<TResult> typed)
			throw new InvalidOperationException($"{key.Id} Popup View가 IUIPopup<{typeof(TResult).Name}>을 상속받지 않고 있습니다.");

		// IUIInit을 지니고 있는 경우 Init 호출
		if (popupUI is IUIInit<TData> init)
			init.Init(data);

		popupStack.Push(key.Id);
		//popupUI.gameObject.SetActive(true);

		try
		{
			var result = await typed.OpenAsync(cancelToken);
			return result;
		}
		finally
		{
			typed.Close();
			//popupUI.gameObject.SetActive(false);

			if (popupStack.Count > 0 && popupStack.Peek() == key.Id)
				popupStack.Pop();
			else
				RemoveFromPopupStack(key.Id);
		}
	}

	private MonoBehaviour GetPopupView(PopupId id)
	{
		if (!popupDict.TryGetValue(id, out MonoBehaviour popupView))
			throw new KeyNotFoundException($"{id}에 해당하는 Popup이 등록되어 있지 않습니다.");

		return popupView;
	}

	/// <summary>
	/// Stack에서 특정 항목 제거(거의 없지만 방어용으로)
	/// </summary>
	/// <param name="id"></param>
	private void RemoveFromPopupStack(PopupId id)
	{
		if (popupStack.Count <= 0)
			return;

		var temp = new Stack<PopupId>();

		while (popupStack.Count > 0)
		{
			var top = popupStack.Pop();
			if (top.Equals(id))
				break;
			temp.Push(top);
		}

		while (temp.Count > 0)
			popupStack.Push(temp.Pop());
	}
	public void CloseTopPopup()
	{
		if (popupStack.Count <= 0)
			return;

		var id = popupStack.Pop();
		var view = GetPopupView(id);
		(view as IUIPopupBase).Close();
		//view.gameObject.SetActive(false);

		// 팝업이 IUIPopup<T> 어떤 T인지 모르므로, Close는 “표준 Close”를 하나 더 두는 것도 방법
		// 지금은 단순 비활성으로 끝내지만, 필요하면 아래처럼 공통 Close 인터페이스를 추가해도 됨:
		// (view as IUIPopupBase)?.Close(); 이런 식으로
	}

	public void CloseAllPopup()
	{
		if (popupStack.Count <= 0)
			return;

		var id = popupStack.Pop();
		var view = GetPopupView(id);
		//view.gameObject.SetActive(false);
	}

	/// <summary>
	/// UI 닫기
	/// </summary>
	public void Back()
	{
		// 팝업 존재 시 팝업 닫기
		if (popupStack.Count > 0)
		{
			CloseTopPopup();
			return;
		}

		// Screen은 1개 남기고 pop
		if (screenStack.Count <= 1)
			return;

		var top = screenStack.Pop();
		top.Close();

		var prev = screenStack.Peek();
		prev.Open();
	}
}
