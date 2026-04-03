// using Cysharp.Threading.Tasks;
// using R3;
// using System.Threading;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public class UIBossFightContext
// {
// 	public float duration;

// 	public string scoreText;

// 	public Subject<Unit> leftButtonSub;
// 	public Subject<Unit> rightButtonSub;

// 	public ReactiveProperty<int> scoreRP;
// }

// public class BossFightViews : MonoBehaviour, IUIPopup<Unit>
// {
// 	public TextMeshProUGUI countDownText;

// 	public TextMeshProUGUI scoreText;

// 	public Button leftButton;
// 	public Button rightButton;

// 	private UIBossFightContext context;

// 	private CompositeDisposable disposables;

// 	private float duration;

// 	public void Init(UIBossFightContext data)
// 	{
// 		disposables = new CompositeDisposable();
// 		context = data;

// 		duration = context.duration;
// 		countDownText.SetText(duration.ToString());
// 		scoreText.SetText(context.scoreText);

// 		leftButton
// 			.OnClickAsObservable()
// 			.ThrottleFirst(System.TimeSpan.FromSeconds(0.15))
// 			.Subscribe(_ =>
// 			{
// 				context.leftButtonSub.OnNext(_);
// 			}).AddTo(disposables);

// 		rightButton
// 			.OnClickAsObservable()
// 			.ThrottleFirst(System.TimeSpan.FromSeconds(0.15))
// 			.Subscribe(_ =>
// 			{
// 				context.rightButtonSub.OnNext(_);
// 			}).AddTo(disposables);

// 		context.scoreRP
// 			.Subscribe(score =>
// 			{
// 				scoreText.SetText(score.ToString());
// 			}).AddTo(disposables);
// 	}

// 	public async UniTask<Unit> OpenAsync(CancellationToken cancelToken)
// 	{
// 		gameObject.SetActive(true);
// 		// 10초 동안 동작하는 UniTask를 대기한다.
// 		IntTimerAsync timer = new IntTimerAsync(() => false, cancelToken);

// 		timer.Value
// 			.Subscribe(value =>
// 			{
// 				countDownText.SetText(value.ToString());
// 			});

// 		await timer.Start(10);

// 		timer.Dispose();

// 		return Unit.Default;
// 	}

// 	public void Close()
// 	{
// 		disposables.Dispose();
// 		disposables = null;

// 		gameObject.SetActive(false);
// 	}
// }
