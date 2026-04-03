using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;

public class BossController : MonoBehaviour
{
	public Animator animator;

	private DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions> moveTweener = null;
	
	public void SetAnimation(string animationName)
	{
		animator.Play(animationName);
	}

	public async UniTask SetAnimationAsync(string animationName)
	{
		CancellationTokenSource cts = new CancellationTokenSource();

		animator.Play(animationName);

		await animator.WaitEndCurrentAnimationAsync(cts.Token);
	}

	public async UniTask SetBossPositionAsync(Vector3 targetPosition, float duration)
	{
		Vector3 startPosition = transform.position;
		float elapsedTime = 0f;

		while (elapsedTime < duration)
		{
			transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
			elapsedTime += Time.deltaTime;
			await UniTask.Yield();
		}

		transform.position = targetPosition;
	}

	public async UniTask MoveAsync(Vector3 destination, float duration, Ease ease)
	{
		moveTweener = transform.DOMove(destination, duration)
			.SetEase(ease);

		await moveTweener.ToUniTask();
	}

	public Vector3 GetPosition() => transform.position;

	

	// 애니메이션 콜백 이벤트를 아예 await?
	// 애니메이션 종료를 await?
}
