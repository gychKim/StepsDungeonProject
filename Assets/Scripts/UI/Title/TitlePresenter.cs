using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

public class TitlePresenter : BasePresenter<TitleView, TitleModel>, IAsyncStartable
{
    public TitlePresenter(TitleView view, TitleModel model) : base(view, model)
    {

    }

    public override void Initialize()
    {
		
    }

	public async UniTask StartAsync(CancellationToken cancellation = default)
	{
		// 배경 이미지 로드 후 적용
		var backgroundSprite = await model.LoadAssetDataAsync<Sprite>(view.backgroundAssetRef, cancellation);
		view.BackgroundImage.sprite = backgroundSprite;


	}

	public override void Dispose()
    {
        Disposables.Dispose();
    }

	
}
