using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public interface IResourceService : IDisposable
{
	UniTask<T> LoadAssetDataAsync<T>(string key, CancellationToken ct);
	UniTask<T> LoadAssetDataAsync<T>(AssetReference assetRef, CancellationToken ct);
	UniTask ReleaseAssetDataAsync(string key);
	UniTask ReleaseAssetDataAsync(AssetReference assetRef);

}

public class ResourceSystem : IResourceService
{
	// ResourceSystem은 Addressable을 보완해주는 역할을 한다. 메모리 누수 방지와, 캐싱이다.
	// 외부에서 Addressable으로 로딩을 하고 데이터를 가져왔다. 그럼 Addressable Handle은 언제 어떻게 해제해야하는지 시스템은 그런걸 관리하고 싶지 않다.
	// 그래서 이를 ResourceSystem이 대신 해주는 것이다.
	// 추가로 ResourceSystem이 Handle을 관리해준다면 ResourceSystem이 이 Handle을 캐싱하고, 나중에 시스템이 다시 호출할 때 빠르게 Handle을 전달해주면 효율도 올라간다.

	// 1. 외부에서 본인이 필요할 때 Addressable을 Load한다.
	// 2. 그 후 Handle을 리소스 시스템에 맡긴다.

	private readonly Dictionary<string, AsyncLazy<AsyncOperationHandle>> handleCacheDict = null;

	public ResourceSystem()
	{
		handleCacheDict = new Dictionary<string, AsyncLazy<AsyncOperationHandle>>();
	}

	/// <summary>
	/// 어드레서블 데이터의 Key값을 받아와 로드 시켜준다.
	/// </summary>
	/// <param name="key"></param>
	public async UniTask<T> LoadAssetDataAsync<T>(string key, CancellationToken ct)
	{

		if(!handleCacheDict.TryGetValue(key, out var lazyTask))
		{
			// key에 해당하는 Handle이 캐싱되어 있지 않다면, 새롭게 Handle을 로드하는 Task를 생성하여 캐싱한다.
			// 여러 곳에서 동시에 같은 key로 데이터를 요청할 수 있기 때문에, 중복 로드 및 레이스 컨디션을 방지하기 위해 AsyncLazy를 사용한다.
			lazyTask = UniTask.Lazy(async () =>
			{
				var handle = Addressables.LoadAssetAsync<T>(key);
				await handle.ToUniTask(cancellationToken: ct);
				return (AsyncOperationHandle)handle; // 유니티 내부적으로 구현해 둔 구조체이므로 명시적 변환을 해도 힙 할당 즉 박싱이 발생하지 않는다.
			});

			handleCacheDict.Add(key, lazyTask);
		}

		var loadHandle = await lazyTask.Task;

		if(loadHandle.Status == AsyncOperationStatus.Succeeded)
		{
			return (T)loadHandle.Result;
		}
		
		return default;
	}

	/// <summary>
	/// AssetReference 데이터를 로드한다.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="assetRef"></param>
	/// <returns></returns>
	public async UniTask<T> LoadAssetDataAsync<T>(AssetReference assetRef, CancellationToken ct)
	{
		// RuntimeKey를 가져온다 == 어드레서블 주소(키)
		string key = assetRef.RuntimeKey.ToString();

		// key를 로드한다.
		var result = await LoadAssetDataAsync<T>(key, ct);

		// 결과 리턴
		return result;
	}

	public void Dispose()
	{
		var keys = handleCacheDict.Keys.ToArray(); // 복사본을 만든다 => Collection was modified 에러 방지
		foreach (var key in keys)
		{
			ReleaseAssetDataAsync(key).Forget();
		}
	}

	#region 해제 로직

	/// <summary>
	/// 어드레서블 데이터의 Key값에 해당하는 Handle을 메모리에서 해제한다.
	/// </summary>
	/// <param name="key"></param>
	public async UniTask ReleaseAssetDataAsync(string key)
	{
		if (handleCacheDict.TryGetValue(key, out var lazyHandle))
		{
			try
			{
				var handle = await lazyHandle.Task; // Handle이 로드될 때까지 기다린다.

				// handle의 값이 살아있다면 Handle을 메모리에서 해제한다.
				if (handle.IsValid())
				{
					Addressables.Release(handle); // 메모리에서 해제.
					DebugX.CyanLog($"{key} Handle 메모리에서 해제.");
				}
			}
			catch (Exception ex)
			{
				DebugX.RedLog($"Handle 해제 중 예외 발생: {ex.Message}");
			}
			finally
			{
				// 예외가 발생하든 안하든 캐시에서 제거한다. 그래야 시스템이 꼬이지 않는다.
				handleCacheDict.Remove(key);
			}
		}
	}

	/// <summary>
	/// AssetReference 데이터를 해제한다.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="assetRef"></param>
	public UniTask ReleaseAssetDataAsync(AssetReference assetRef)
	{
		// RuntimeKey를 가져온다 == 어드레서블 주소(키)
		string key = assetRef.RuntimeKey.ToString();

		return ReleaseAssetDataAsync(key);
	}

	#endregion

}
