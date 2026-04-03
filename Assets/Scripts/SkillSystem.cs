using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// 스킬 발동 조건
/// </summary>
public interface ISkillTrigger
{
	void Action(IBattleContext context, float triggerValue, Action onTriggered); // 실행
}

/// <summary>
/// 스킬 즉시 발동 > 즉시 적용
/// </summary>
public class ImmediateTrigger : ISkillTrigger
{
	public void Action(IBattleContext context, float triggerValue, Action onTriggered)
	{
		onTriggered.Invoke();
	}
}

/// <summary>
/// 인터벌한 시간마다 스킬 발동 > N초마다 실행
/// </summary>
public class IntervalTimerTrigger : ISkillTrigger
{
	public void Action(IBattleContext context, float triggerValue, Action onTriggered)
	{
		Observable.Interval(TimeSpan.FromSeconds(triggerValue), context.BattleCT)
			.Subscribe(_ =>
			{
				onTriggered.Invoke();
			}).AddTo(context.BattleCT);
	}
}

/// <summary>
/// 공격 횟수에 따라 스킬 발동 > N번 히트 시 실행
/// </summary>
public class AttackCounterTrigger : ISkillTrigger
{

	private int currentAttackCount;
	public void Action(IBattleContext context, float triggerValue, Action onTriggered)
	{
		context.OnPlayerAttackObv
			.Subscribe(_ =>
			{
				currentAttackCount++;
				if(triggerValue <= currentAttackCount)
				{
					currentAttackCount = 0;
					onTriggered.Invoke();
				}
			}).AddTo(context.BattleCT);
	}
}

/// <summary>
/// 남은 시간에 비례한 스킬 발동 > N초 이하 남았을 시 실행
/// </summary>
public class TimeTrigger : ISkillTrigger
{
	public void Action(IBattleContext context, float triggerValue, Action onTriggered)
	{
		context.CountdownTimerObv
			.Subscribe(time =>
			{
				if(time >= triggerValue)
				{
					onTriggered.Invoke();
				}
			}).AddTo(context.BattleCT);
	}
}

/// <summary>
/// 배틀 종료 했을 시 발동 > 죽음의 메아리 같은
/// </summary>
public class BattleEndTrigger : ISkillTrigger
{
	public void Action(IBattleContext context, float triggerValue, Action onTriggered)
	{
		context.OnBattleEndObv
			.Subscribe(_ =>
			{
				onTriggered.Invoke();
			}).AddTo(context.BattleCT);
	}
}


/// <summary>
/// 스킬 효과
/// </summary>
public interface ISkillEffect
{
	void Execute(IBattleContext context, float value); // 효과 적용
}

/// <summary>
/// 점수 증가 효과
/// </summary>
public class AddScoreEffect : ISkillEffect
{
	public void Execute(IBattleContext context, float value)
	{
		int damage = (int)(context.GetCurrentPower() * value * 0.01f);
		context.BossBattleScore.AddScore(damage);
	}
}

/// <summary>
/// 힘 배율 증가
/// </summary>
public class PowerMultiplierEffect : ISkillEffect
{
	public readonly IPowerUseCase powerUseCase;

	public PowerMultiplierEffect(IPowerUseCase powerUseCase)
	{
		this.powerUseCase = powerUseCase;
	}

	public void Execute(IBattleContext context, float value)
	{
		powerUseCase.ChangePower((int)(value * 0.01f));
	}
}

/// <summary>
/// 데미지 연출
/// </summary>
public class PresentationDamgeEffect : ISkillEffect
{

	public PresentationDamgeEffect()
	{
		
	}

	public void Execute(IBattleContext context, float value)
	{
		
	}
}

/// <summary>
/// 여러 개의 스킬 이펙트를 사용할 때 필요
/// </summary>
public class CompositeSkillEffect : ISkillEffect
{
	public List<ISkillEffect> skillEffectList;

	public void Execute(IBattleContext context, float value)
	{
		foreach(var effect in skillEffectList)
		{
			effect.Execute(context, value);
		}
	}
}

/// <summary>
/// 보스 배틀 시 사용하는 Context
/// </summary>
public interface IBattleContext
{
	Observable<int> CountdownTimerObv { get; }
	Observable<Unit> OnPlayerAttackObv { get; }
	Observable<Unit> OnBattleEndObv { get; }
	IBossBattleScore BossBattleScore{ get; }
	CancellationToken BattleCT { get; }
	int GetCurrentPower();
}

public enum SkillEffectType
{
	AddScore,
	PowerMultiplier,
	PresentationDamge,
}

public enum SkillTriggerType
{
	Immediate,
	IntervalTimer,
	AttackCounter,
	RemainingTime,
	BattleEnd,
}

public enum Rank
{
	C,
	B,
	A,
	S,
}

public class SkillData
{
	public int Index;
	public string SkillName;
	public int SkillTriggerValue;
	public int SkillValue;
	public string skillIcon; // 어드레서블의 ID값으로 가져온다.

	public SkillTriggerType SkillTriggerType;
	public SkillEffectType SkillEffectType;

	public Rank SkillRank;

}

public class Skill
{
	public int skillId;
	public string skillName;

	public int skillTriggerValue;
	public int skillValue;
	public Sprite skillIcon;

	public SkillTriggerType skillTriggerType;
	public SkillEffectType skillEffectType;

	public Rank skillRank;

	public ISkillEffect skillEffect;

	public ISkillTrigger skillTrigger;

	public Skill(ISkillTrigger skillTrigger, ISkillEffect skillEffect)
	{
		this.skillTrigger = skillTrigger;
		this.skillEffect = skillEffect;
	}

	public void Init(SkillData data, Sprite icon)
	{
		skillId = data.Index;
		skillName = data.SkillName;
		skillTriggerValue = data.SkillTriggerValue;
		skillValue = data.SkillValue;
		skillEffectType = data.SkillEffectType;
		skillTriggerType = data.SkillTriggerType;
		skillRank = data.SkillRank;

		skillIcon = icon;
	}

	public void Apply(IBattleContext battleContext)
	{
		skillTrigger.Action(battleContext, skillTriggerValue, () => skillEffect.Execute(battleContext, skillValue));
	}
}

public class SkillFactory
{
	private readonly IObjectResolver container; // IObjectResolver가 컨테이너 역할
	private readonly IDataService dataService;
	private readonly IResourceService resourceService;

	private readonly Dictionary<SkillTriggerType, Func<ISkillTrigger>> skillTriggerDict = null;
	private readonly Dictionary<SkillEffectType, Func<ISkillEffect>> skillEffectDict = null;

	public SkillFactory(IObjectResolver container, IDataService dataService, IResourceService resourceService)
	{
		this.container = container;
		this.dataService = dataService;
		this.resourceService = resourceService;

		skillTriggerDict = new ()
		{
			{ SkillTriggerType.Immediate, () => container.Resolve<ImmediateTrigger>()},
			{ SkillTriggerType.IntervalTimer, () => container.Resolve<IntervalTimerTrigger>() },
			{ SkillTriggerType.AttackCounter, () => container.Resolve<AttackCounterTrigger>() },
			{ SkillTriggerType.RemainingTime, () => container.Resolve<TimeTrigger>() },
			{ SkillTriggerType.BattleEnd, () => container.Resolve<BattleEndTrigger>() },
		};

		skillEffectDict = new ()
		{
			{ SkillEffectType.AddScore, () => container.Resolve<AddScoreEffect>()},
			{ SkillEffectType.PowerMultiplier, () => container.Resolve<PowerMultiplierEffect>() },
			{ SkillEffectType.PresentationDamge, () => container.Resolve<PresentationDamgeEffect>() },
		};
	}

	public async UniTask<Skill> Create(int skillId, CancellationToken ct)
	{
		SkillData data = dataService.GetSkillData(skillId);

		ISkillTrigger trigger = skillTriggerDict[data.SkillTriggerType]();
		ISkillEffect effect = skillEffectDict[data.SkillEffectType]();
		Skill skill = new Skill(trigger, effect);

		if (skill != null)
		{
			var icon = await resourceService.LoadAssetDataAsync<Sprite>(data.skillIcon, ct);
			skill.Init(data, icon);
		}

		return skill;
	}
}


public interface ISkillUseCase
{
	SkillData GetSkill(int skillId);
	UniTask AddSkill(int skillId, CancellationToken ct);
	UniTask AddSkillByRank(Rank rank, CancellationToken ct);
	void ApplyAllSkill(IBattleContext battleContext);
	void RemoveSkill(int skillId);
}

public class SkillSystem : ISkillUseCase, IInitializable
{
	private readonly IDataService dataService = null;
	private readonly ISaveService saveService = null;
	private readonly SkillFactory skillFactory = null; // Factory 주입

	private List<int> currentSkillIdList = null;
	private List<Skill> currentSkillList = null;

	private CancellationTokenSource addSkillCTS = new CancellationTokenSource();
	public SkillSystem(IDataService dataService, ISaveService saveService, SkillFactory skillFactory)
	{
		currentSkillIdList = new List<int>();
		currentSkillList = new List<Skill>();

		this.dataService = dataService;
		this.saveService = saveService;
		this.skillFactory = skillFactory;
	}

	public void Initialize()
	{
		// 저장된 스킬 ID 리스트가 없다면 빈 리스트를 생성하여 저장
		if(saveService.SaveData.currentSkillIdList == null)
		{
			currentSkillIdList = new List<int>();
			saveService.SaveData.currentSkillIdList = currentSkillIdList;
		}
		// 저장된 스킬 ID 리스트가 있으면 currentSkillIdList에 복사
		else
		{
			foreach (int id in saveService.SaveData.currentSkillIdList)
			{
				currentSkillIdList.Add(id);
				AddSkill(id, addSkillCTS.Token).Forget(); // 저장된 스킬 ID로 스킬을 추가하여 currentSkillList도 초기화
			}
		}
	}

	// 3. UI가 GetSkill같은걸 호출할 때, DB데이터로 가져오나? 아님 런타임 데이터?
	// => DB데이터로 해도 된다. 왜? 내 스킬 데이터는 고정값이므로 런타임 중 변하지 않는다, 즉 DB데이터 만으로도 충분
	public SkillData GetSkill(int skillId) => dataService.GetSkillData(skillId);

	/// <summary>
	/// 스킬 추가.
	/// 스킬 팩토리를 통해 스킬을 생성하고 초기화한 후, 현재 스킬 리스트에 추가한다.
	/// </summary>
	/// <param name="skillId"></param>
	public async UniTask AddSkill(int skillId, CancellationToken ct)
	{
		Skill skill = await skillFactory.Create(skillId, ct);
		if(skill != null)
		{
			currentSkillIdList.Add(skillId);
			currentSkillList.Add(skill);

			// 저장 데이터에 즉시 적용
			if (!saveService.SaveData.currentSkillIdList.Contains(skillId))
			{
				saveService.SaveData.currentSkillIdList.Add(skillId);
			}
		}
	}

	public async UniTask AddSkillByRank(Rank rank, CancellationToken ct)
	{
		SkillData skillData = dataService.GetSkillData(rank);
		if (skillData != null)
		{
			await AddSkill(skillData.Index, ct);
		}
	}

	/// <summary>
	/// 모든 스킬 적용.
	/// </summary>
	/// <param name="skillId"></param>
	public void ApplyAllSkill(IBattleContext battleContext)
	{
		foreach(Skill skill in currentSkillList)
		{
			skill.Apply(battleContext);
		}
	}

	/// <summary>
	/// 스킬 제거.
	/// </summary>
	/// <param name="skillId"></param>
	public void RemoveSkill(int skillId)
	{
		currentSkillIdList.Remove(skillId);

		foreach(Skill skill in currentSkillList)
		{
			if(skill.skillId == skillId)
			{
				currentSkillList.Remove(skill);
				break;
			}
		}

		if (saveService.SaveData.currentSkillIdList.Contains(skillId))
		{
			saveService.SaveData.currentSkillIdList.Remove(skillId);
		}
	}
}
