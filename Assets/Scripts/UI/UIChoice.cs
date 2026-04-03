using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChoiceContext
{
	public string choiceTitle;
	public string choiceContent;

	public string choice01ButtonText;
	public string choice01ChanceText;
	public string choice01NextLevelText;

	public string choice02ButtonText;
	public string choice02ChanceText;
	public string choice02NextLevelText;
	
}

public class UIChoice : MonoBehaviour, IUIPopup<int>, IUIInit<UIChoiceContext>
{
	public TextMeshProUGUI choiceTitle;
	public TextMeshProUGUI choiceContent;

	public Button choice01Button;
	public TextMeshProUGUI choice01ButtonText;

	/// <summary>
	/// 선택지 01이 성공 확률을 보여주는 텍스트
	/// </summary>
	public TextMeshProUGUI choice01ChanceText; 

	/// <summary>
	/// 선택지 01을 선택할 시 다음 룸의 난이도를 보여주는 텍스트
	/// </summary>
	public TextMeshProUGUI choice01NextLevelText;

	public Button choice02Button;
	public TextMeshProUGUI choice02ButtonText;

	/// <summary>
	/// 선택지 02이 성공 확률을 보여주는 텍스트
	/// </summary>
	public TextMeshProUGUI choice02ChanceText;

	/// <summary>
	/// 선택지 02을 선택할 시 다음 룸의 난이도를 보여주는 텍스트
	/// </summary>
	public TextMeshProUGUI choice02NextLevelText;
	
	public DOTweenAnimation dotweenAnim;

	private CompositeDisposable disposables;

	private UniTaskCompletionSource<int> selectCTS;

	public void Init(UIChoiceContext data)
	{
		disposables = new CompositeDisposable();
		selectCTS = new UniTaskCompletionSource<int>();

		choiceTitle.text = data.choiceTitle;
		choiceContent.text = data.choiceContent;

		choice01ButtonText.text = data.choice01ButtonText;
		choice01ChanceText.text = data.choice01ChanceText + "%";
		choice01NextLevelText.text = data.choice01NextLevelText;

		choice02ButtonText.text = data.choice02ButtonText;
		choice02ChanceText.text = data.choice02ChanceText + "%";
		choice02NextLevelText.text = data.choice02NextLevelText;

		choice01Button
			.OnClickAsObservable()
			.Subscribe(_ =>
			{
				selectCTS.TrySetResult(1);
				
			}).AddTo(disposables);

		choice02Button
			.OnClickAsObservable()
			.Subscribe(_ =>
			{
				selectCTS.TrySetResult(2);
			}).AddTo(disposables);
	}

	public async UniTask<int> OpenAsync(CancellationToken cancelToken)
	{
		gameObject.SetActive(true);

		// Open 애니메이션 실행

		// 플레이어 선택 대기
		int index = await WaitSelectAsync();

		// Close 애니메이션 실행

		return index;
	}

	/// <summary>
	/// 플레이어 입력 대기
	/// </summary>
	/// <returns></returns>
	private async UniTask<int> WaitSelectAsync()
	{
		int index = await selectCTS.Task;
		return index;
	}

	public void Close()
	{
		disposables.Dispose();
		disposables = null;

		gameObject.SetActive(false);
	}

	
}
