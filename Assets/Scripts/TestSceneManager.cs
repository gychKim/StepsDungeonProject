using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

public class TestSceneManager : MonoBehaviour
{
	private IAudioService audioService;
	private ISaveService saveService;

	private CancellationTokenSource bgmCTS = new CancellationTokenSource();

	[Inject]
	public void Configure(IAudioService audioService, ISaveService saveService)
	{
		this.audioService = audioService;
		this.saveService = saveService;
	}

	public void NewGame()
	{
		saveService.Delete();
		SceneManager.LoadScene("Game");
	}

	public void Continue()
	{
		SceneManager.LoadScene("Game");
	}

	public void PlayBGM(string bgmKey)
	{
		audioService.PlayBGM(bgmKey, bgmCTS.Token);
	}

    public void LoadScene()
	{
		SceneManager.LoadScene("Game");
	}
}
