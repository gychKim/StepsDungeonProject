using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using System.Threading;

public static class UniTaskExtension
{
	/// <summary>
	/// 현재 애니메이션의 상태가 종료될 때 까지 대기
	/// </summary>
	/// <param name="animator"></param>
	/// <returns></returns>
	public static async UniTask WaitEndCurrentAnimationAsync(this Animator animator, CancellationToken ct = default)
	{
		try
		{
			float length = animator.GetCurrentAnimatorStateInfo(0).length;
			await UniTask.Delay(TimeSpan.FromSeconds(length), cancellationToken: ct);
		}
		catch(OperationCanceledException)
		{

		}
		
	}

	/// <summary>
	/// 애니메이션의 상태가 종료될 때 까지 대기
	/// </summary>
	/// <param name="animator"></param>
	/// <returns></returns>
	public static async UniTask WaitEndAnimationAsync(this AnimatorStateInfo stateInfo, CancellationToken ct = default)
	{
		try
		{
			float length = stateInfo.length;
			await UniTask.Delay(TimeSpan.FromSeconds(length), cancellationToken: ct);
		}
		catch (OperationCanceledException)
		{

		}
		
	}
}
