using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.AddressableAssets;
using VContainer;

public class TitleModel : BaseModel
{
	private readonly IResourceService resourceService = null;

	public TitleModel(IResourceService resourceService) 
	{ 
		this.resourceService = resourceService;
	}

	/// <summary>
	/// ResourceSystem의 LoadAssetDataAsync를 실행해준다.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="assetRef"></param>
	/// <returns></returns>
	public async UniTask<T> LoadAssetDataAsync<T>(AssetReferenceSprite assetRef, CancellationToken ct)
	{
		var result = await resourceService.LoadAssetDataAsync<T>(assetRef, ct);
		return result;
	}


    public override void Dispose()
    {
        
    }

}
