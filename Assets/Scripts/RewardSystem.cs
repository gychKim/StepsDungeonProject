using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RewardData
{
	public bool isSuccess; // 성공여부
	public string resultText; // 결과 텍스트
	public List<EffectData> effectList = new List<EffectData>();
}

public enum EffectType
{
	None,
	PowerChange,
	AddSkillById,
	AddSkillByRank,
}

public class EffectData
{
	public EffectType type;
	public int value;
	public int target;
}
public interface IRewardUseCase
{
	UniTask ApplyReward(RewardData rewardData, CancellationToken ct);
}

public class RewardSystem : IRewardUseCase
{
	private readonly IPowerUseCase powerUseCase;
	private readonly ISkillUseCase skillUseCase;
    public RewardSystem(IPowerUseCase powerUseCase, ISkillUseCase skillUseCase)
	{
		this.powerUseCase = powerUseCase;
		this.skillUseCase = skillUseCase;
	}

	public async UniTask ApplyReward(RewardData rewardData, CancellationToken ct)
	{
		foreach(var effect in rewardData.effectList)
		{
			await ApplyEffect(effect, ct);
		}
	}

	private async UniTask ApplyEffect(EffectData data, CancellationToken ct)
	{
		switch(data.type)
		{
			case EffectType.PowerChange:
				powerUseCase.ChangePower(data.value);
				break;
			case EffectType.AddSkillByRank:
				if (data.value == -1)
					skillUseCase.RemoveSkill(data.target);
				else
					await skillUseCase.AddSkillByRank((Rank)data.value, ct);
				break;
		}

		return;
	}
}
