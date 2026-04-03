using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using System.Threading;
using UnityEngine;


public interface ICharacterUseCase
{
	/// <summary>
	/// 캐릭터 이동,
	/// destination에 도달하면 await에서 빠져나온다.
	/// </summary>
	/// <param name="destination"></param>
	/// <param name="duration"></param>
	/// <returns></returns>
	UniTask MoveAsync(Vector3 destination, float duration);

	/// <summary>
	/// 캐릭터 이동,
	/// destination에 도달하면 await에서 빠져나온다.
	/// </summary>
	/// <param name="destination"></param>
	/// <param name="duration"></param>
	/// <returns></returns>
	UniTask MoveAsync(Vector3 destination, float duration, Ease ease);

	/// <summary>
	/// 기존 실행중인 Tween을 제거한다.
	/// </summary>
	void KillTweener();

	/// <summary>
	/// 캐릭터 애니메이션 변경
	/// </summary>
	/// <param name="animationName"></param>
	void SetAnimation(string animationName);

	/// <summary>
	/// 캐릭터의 현재 위치를 받아온다.
	/// </summary>
	/// <returns></returns>
	Vector3 GetCurrentPosition();
}

public struct TransportData
{
	public int index;
	public Vector3 position;
}

public class CharacterSystem : MonoBehaviour, ICharacterUseCase
{

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private AnimationCurve moveCurve;

	private DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions> moveTweener = null;
	public async UniTask MoveAsync(Vector3 destination, float duration)
	{
		moveTweener = transform.DOMove(destination, duration)
			.SetEase(moveCurve);

		await moveTweener.ToUniTask();
	}

	public async UniTask MoveAsync(Vector3 destination, float duration, Ease ease)
	{
		moveTweener = transform.DOMove(destination, duration)
			.SetEase(ease);

		await moveTweener.ToUniTask();
	}

	public void SetAnimation(string animationName)
	{
		animator.Play(animationName);
	}

	public Vector3 GetCurrentPosition() => transform.position;
	public void KillTweener() => moveTweener.Kill();
}
