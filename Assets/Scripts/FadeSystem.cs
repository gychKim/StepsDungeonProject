using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public interface IFadeUseCase
{
	/// <summary>
	/// 점차 밝아짐
	/// </summary>
	UniTask FadeIn(float duration = 3f);

	/// <summary>
	/// 점차 어두워짐
	/// </summary>
	UniTask FadeOut(float duration = 3f);
}
public class FadeSystem : MonoBehaviour, IFadeUseCase
{
	public CanvasGroup fadeGroup;

	public AnimationCurve fadeCurve;

	private void Start()
	{
		fadeGroup.DOFade(1, 0f);
	}

	/// <summary>
	/// 점차 밝아짐
	/// </summary>
	public async UniTask FadeIn(float duration = 3f)
	{
		await fadeGroup.DOFade(0, duration)
			.SetEase(fadeCurve)
			.ToUniTask();
	}

	/// <summary>
	/// 점차 어두워짐
	/// </summary>
	public async UniTask FadeOut(float duration = 3f)
	{
		await fadeGroup.DOFade(1, duration)
			.SetEase(fadeCurve)
			.ToUniTask();
	}
}
