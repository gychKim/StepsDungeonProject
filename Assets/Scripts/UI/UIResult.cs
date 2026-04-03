using Cysharp.Threading.Tasks;
using R3;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIResultContext
{
	public int score;
	public string playTime;
}

public class UIResult : MonoBehaviour, IUIPopup<Unit>, IUIInit<UIResultContext>
{
	public TextMeshProUGUI scoreText;
	public TextMeshProUGUI playTimeText;

	public Button confirmButton;

	private UniTaskCompletionSource confirmCTS;

	public void Init(UIResultContext data)
	{
		confirmCTS = new UniTaskCompletionSource();

		scoreText.SetText(data.score.ToString());
		playTimeText.SetText(data.playTime);

		confirmButton
			.OnClickAsObservable()
			.Take(1)
			.Subscribe(_ =>
			{
				confirmCTS.TrySetResult();
			});
	}

	public async UniTask<Unit> OpenAsync(CancellationToken cancelToken)
	{
		gameObject.SetActive(true);

		await WaitConfirmAsync();

		return Unit.Default;
	}

	private async UniTask WaitConfirmAsync()
	{
		await confirmCTS.Task;
	}

	public void Close()
	{
		gameObject.SetActive(false);
	}
}
