using UnityEngine;
using UnityEngine.UI;

public class UILoading : MonoBehaviour
{
	[SerializeField]
	private Slider loadingSlider;

	[SerializeField]
	private GameObject titleUI;

    public void SetProgress(float value)
	{
		loadingSlider.value = value;

		if(loadingSlider.value >= 1.0f)
		{
			titleUI.SetActive(true);
			gameObject.SetActive(false);
		}
	}
}
