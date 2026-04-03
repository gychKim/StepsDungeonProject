using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

public interface IAudioService
{
	UniTask PlayBGM(string bgmKey, CancellationToken ct);
}

public class AudioSystem : IAudioService, IInitializable
{
	private readonly IResourceService resourceService = null;

	private GameObject audioRootObject;
	private AudioSource bgmSource;

	public AudioSystem(IResourceService resourceService)
	{
		this.resourceService = resourceService;
	}

	public void Initialize()
	{
		audioRootObject = new GameObject("AudioRoot");

		bgmSource = audioRootObject.AddComponent<AudioSource>();
		bgmSource.loop = true;

		GameObject.DontDestroyOnLoad(audioRootObject);
	}

	public async UniTask PlayBGM(string bgmKey, CancellationToken ct)
	{
		var bgmClip = await resourceService.LoadAssetDataAsync<AudioClip>(bgmKey, ct);
		bgmSource.clip = bgmClip;

		bgmSource.Play();
	}
}
