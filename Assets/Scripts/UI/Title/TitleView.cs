using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class TitleView : BaseView
{
	public Image BackgroundImage => backgroundImage;
	[SerializeField]
	private Image backgroundImage;

	public AssetReferenceSprite backgroundAssetRef;
}
