using Cysharp.Threading.Tasks;
using R3;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct UIChoiceResultDTO
{
	public string content;
}
public class UIChoiceResult : MonoBehaviour, IUIPopup<Unit>, IUIInit<UIChoiceResultDTO>
{
	public TextMeshProUGUI choiceResultContent;
	public Button confirmButton;

	private CompositeDisposable disposables;

	private UniTaskCompletionSource confirmCTS;
	public void Init(UIChoiceResultDTO data)
	{
		disposables = new CompositeDisposable();
		confirmCTS = new UniTaskCompletionSource();

		choiceResultContent.text = data.content;

		confirmButton
			.OnClickAsObservable()
			.Subscribe(_ =>
			{
				confirmCTS.TrySetResult();
			}).AddTo(disposables);
	}

	public async UniTask<Unit> OpenAsync(CancellationToken cancelToken)
	{
		// Open 애니메이션 실행
		gameObject.SetActive(true);

		// 플레이어 확인 클릭 대기
		await WaitConfirmAsync();

		// Close 애니메이션 실행

		return Unit.Default;
	}

	private async UniTask WaitConfirmAsync()
	{
		await confirmCTS.Task;
	}

	public void Close()
	{
		disposables.Dispose();

		gameObject.SetActive(false);
	}

}
