using UnityEngine;
using VContainer.Unity;

public interface IPowerUseCase
{
	/// <summary>
	/// 현재 파워를 가져온다
	/// </summary>
	/// <returns></returns>
	int GetPower();

	/// <summary>
	/// 파워를 변화시킨다.
	/// </summary>
	/// <param name="value"></param>
	void ChangePower(int value);
}

public class PowerSystem : IPowerUseCase, IInitializable
{
	private readonly ISaveService saveService;

	private int currentPower;

    public PowerSystem(ISaveService saveService)
	{
		this.saveService = saveService;
	}

	public void Initialize()
	{
		currentPower = saveService.SaveData.currentPower;
	}

	public int GetPower() => currentPower;

	public void ChangePower(int value)
	{
		currentPower += value;

		saveService.SaveData.currentPower = currentPower;
	}
	
}
