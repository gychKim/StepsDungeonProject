using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public CinemachineBrain mainCamera;

	public CinemachineCamera playerCamera;
	public CinemachineCamera uiCamera;

	public void SetCamera(CameraType type)
	{
		switch(type)
		{
			case CameraType.Player:
				playerCamera.Priority = 1;
				uiCamera.Priority = 0;
				break;
			case CameraType.UI:
				playerCamera.Priority = 0;
				uiCamera.Priority = 1;
				break;
		}
	}
}
