using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using VContainer.Unity;


public interface IGameLoad : IAsyncStartable
{
	/// <summary>
	/// 로딩이 완료되었는지 여부
	/// </summary>
	bool IsLoaded { get; }
	event Action<float> OnProgressChanged;
}
public class LoadingSystem : IAsyncStartable, IDisposable
{
	private readonly List<IGameLoad> gameLoadList;
	private readonly UILoading loadingUI;

	// 각 시스템별 현재 진행률 저장소 (Key: 시스템, Value: 0.0~1.0)
	private Dictionary<IGameLoad, float> progressDict = new Dictionary<IGameLoad, float>();

	// 이벤트 구독 핸들러
	private Dictionary<IGameLoad, Action<float>> eventHandlerDict = new Dictionary<IGameLoad, Action<float>>();

	public LoadingSystem(UILoading loadingUI, IEnumerable<IGameLoad> gameLoads)
	{
		this.loadingUI = loadingUI;
		gameLoadList = gameLoads.ToList();
	}

	public async UniTask StartAsync(CancellationToken cancellation = default)
	{
		foreach(var loading in gameLoadList.Where(load => !load.IsLoaded))
		{
			progressDict[loading] = 0f;

			Action<float> updateAction = (progress) => UpdateProgress(loading, progress);
			eventHandlerDict.Add(loading, updateAction);

			loading.OnProgressChanged += updateAction;
		}


		var loadingTasks = gameLoadList.Select(load => load.StartAsync(cancellation));
		await UniTask.WhenAll(loadingTasks);

		loadingUI.SetProgress(1f);

		DebugX.BlueLog("데이터 로딩 종료!!!");
	}

	private void UpdateProgress(IGameLoad loading, float loadingProgress)
	{
		progressDict[loading] = loadingProgress;

		float totalProgress = progressDict.Values.Sum() / gameLoadList.Count;

		loadingUI.SetProgress(totalProgress);
	}

	public void Dispose()
	{
		// 이벤트 구독 해제
		foreach (var loading in gameLoadList)
		{
			if(eventHandlerDict.TryGetValue(loading, out var handler))
			{
				loading.OnProgressChanged -= handler;
			}
		}
	}
}
