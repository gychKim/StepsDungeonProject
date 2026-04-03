using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;
using Cysharp.Threading.Tasks;
using System.Threading;

public class BossFightView : BaseView, IUIPopup<Unit>
{
    [SerializeField]
    private TextMeshProUGUI countDownText;

    [SerializeField]
	private TextMeshProUGUI scoreText;

    [SerializeField]
	private Button leftButton;

    [SerializeField]
	private Button rightButton;

    public Observable<Unit> LeftButtonObv => leftButton.OnClickAsObservable();
    public Observable<Unit> RightButtonObv => rightButton.OnClickAsObservable();

    public Observable<Unit> StartBattleObv => startBattleSub;
    private Subject<Unit> startBattleSub = new Subject<Unit>();
	private UniTaskCompletionSource battleEndUCS;

    public void SetCountDown(string text)
    {
        countDownText.text = text;
    }

    public void SetScore(string text)
    {
        scoreText.text = text;
    }

	public async UniTask<Unit> OpenAsync(CancellationToken cancelToken)
	{
		battleEndUCS = new UniTaskCompletionSource();

		Show();

        startBattleSub.OnNext(Unit.Default);

		await battleEndUCS.Task;

		return Unit.Default;
	}

    public void EndBattle()
    {
        battleEndUCS.TrySetResult();
    }

	public void Close()
	{
		Hide();
	}
}