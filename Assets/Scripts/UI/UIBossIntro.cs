using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBossIntroContext
{
	public string bossTitle;
	public string bossContent;

	public string bossFightButtonText;
}

public class UIBossIntro : MonoBehaviour, IUIPopup<Unit>, IUIInit<UIBossIntroContext>
{
	public TextMeshProUGUI bossTitle;
	public TextMeshProUGUI bossContent;

	public Button bossFightButton;

	public TextMeshProUGUI bossFightButtonText;

	private CompositeDisposable disposables;

	private UniTaskCompletionSource fightCTS;

	public void Init(UIBossIntroContext data)
	{
		disposables = new CompositeDisposable();
		fightCTS = new UniTaskCompletionSource();

		bossTitle.text = data.bossTitle;
		bossContent.text = data.bossContent;

		bossFightButtonText.text = data.bossFightButtonText;

		bossFightButton
			.OnClickAsObservable()
			.Subscribe(_ =>
			{
				fightCTS.TrySetResult();
			}).AddTo(disposables);
	}

	public async UniTask<Unit> OpenAsync(CancellationToken cancelToken)
	{
		gameObject.SetActive(true);

		await WaitStartAsync();

		return Unit.Default;
	}

	/// <summary>
	/// 시작 버튼 클릭 대기
	/// </summary>
	/// <returns></returns>
	private async UniTask WaitStartAsync()
	{
		await fightCTS.Task;
	}

	public void Close()
	{
		disposables.Dispose();
		disposables = null;

		gameObject.SetActive(false);
	}

	
}
