using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using System.Threading.Tasks;
using VContainer;

public class BossFightModel : BaseModel, IBattleContext
{
    private readonly IBossBattleScore bossBattleScore;
    private readonly IPowerUseCase powerUseCase;
    private readonly ISkillUseCase skillUseCase;

    public IBossBattleScore BossBattleScore => bossBattleScore;

    public ReadOnlyReactiveProperty<int> CurrentScore => bossBattleScore.ScoreRP;
    public Observable<int> CountdownTimerObv => countdownTimer;
    private ReactiveProperty<int> countdownTimer = new ReactiveProperty<int>();

    private IntTimerAsync timer;

	public Observable<Unit> OnPlayerAttackObv => onPlayerAttackSub;
    private Subject<Unit> onPlayerAttackSub = new Subject<Unit>();

	public Observable<Unit> OnBattleEndObv => onBattleEndSub; // 종료 시 스킬 발동
    private Subject<Unit> onBattleEndSub = new Subject<Unit>();
    
    public CancellationToken BattleCT => battleCTS.Token;
    private CancellationTokenSource battleCTS;

    public Observable<Unit> EndBattleObv => endBattleSub; // 완전 종료 -> 스킬도 끝
    private Subject<Unit> endBattleSub = new Subject<Unit>();
	
    private CancellationTokenSource timerCTS;

    private CompositeDisposable battleTimerDisposable;

    public BossFightModel(IBossBattleScore bossBattleScore, IPowerUseCase powerUseCase, ISkillUseCase skillUseCase)
    {
        this.bossBattleScore = bossBattleScore;
        this.powerUseCase = powerUseCase;
        this.skillUseCase = skillUseCase;
    }

    /// <summary>
    /// 배틀 시작
    /// </summary>
    /// <returns></returns>
    public async UniTask StartBattle()
    {
        battleTimerDisposable?.Dispose();
        battleTimerDisposable = new CompositeDisposable();

        timerCTS = new CancellationTokenSource();
        battleCTS = new CancellationTokenSource();

        skillUseCase.ApplyAllSkill(this);

        timer = new IntTimerAsync(() => false, timerCTS.Token);
        timer.Value
            .Subscribe(time =>
            {
                countdownTimer.Value = time;
            }).AddTo(battleTimerDisposable);

        // 10초 대기
        await timer.Start(10);

        timer.Dispose();
        battleCTS.Dispose();

        onBattleEndSub.OnNext(Unit.Default);

        // 배틀 종료 통보
        endBattleSub.OnNext(Unit.Default);
    }

    public void ClickButton()
    {
        bossBattleScore.AddScore(powerUseCase.GetPower());
        onPlayerAttackSub.OnNext(Unit.Default);
    }

    public int GetScore() => bossBattleScore.GetScore();
    public override void Dispose()
    {
        battleTimerDisposable?.Dispose();
        Disposables?.Dispose();
    }

	public int GetCurrentPower()
	{
		return powerUseCase.GetPower();
	}
}