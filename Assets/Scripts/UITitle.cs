
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class UITitle : MonoBehaviour
{
	[SerializeField]
	private Image backgroundImage;

	public AssetReferenceSprite backgroundSprite;

    void Start()
    {
		backgroundSprite.LoadAssetAsync().Completed += (op) =>
		{
			if(op.Status == AsyncOperationStatus.Succeeded)
			{
				backgroundImage.sprite = op.Result;
				Addressables.Release(op);
			}
		};

		
	}
}
