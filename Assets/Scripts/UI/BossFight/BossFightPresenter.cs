using Cysharp.Threading.Tasks;
using R3;

public class BossFightPresenter : BasePresenter<BossFightView, BossFightModel>
{
    public BossFightPresenter(BossFightView view, BossFightModel model) : base(view, model)
    {
        
    }

    public override void Initialize()
    {
        view.StartBattleObv
            .Subscribe(_ =>
            {
                model.StartBattle().Forget();
            });

        view.LeftButtonObv
            .ThrottleFirst(System.TimeSpan.FromSeconds(0.15))
            .Subscribe(_ =>
            {
                model.ClickButton();
            }).AddTo(Disposables);

        view.RightButtonObv
            .ThrottleFirst(System.TimeSpan.FromSeconds(0.15))
            .Subscribe(_ =>
            {
                model.ClickButton();
            }).AddTo(Disposables);

        model.CurrentScore
            .Subscribe(score =>
            {
                view.SetScore(score.ToString());
            }).AddTo(Disposables);

        model.CountdownTimerObv
            .Subscribe(time =>
            {
                view.SetCountDown(time.ToString());
            }).AddTo(Disposables);

        model.EndBattleObv
            .Subscribe(_ =>
            {
                view.EndBattle();
            }).AddTo(Disposables);
    }

    public override void Dispose()
    {
        Disposables.Dispose();
    }
}