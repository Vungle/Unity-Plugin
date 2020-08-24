using UnityEngine;

public class ViewManager : MonoBehaviour
{
	public Canvas[] canvases;
	private Canvas activeCanvas;
	static ViewManager mInstance;

	public static ViewManager Instance
	{
		get
		{
			return mInstance;
		}
	}

	private void Start()
	{
		if (mInstance != null)
		{
			Destroy(this);
			return;
		}
		mInstance = this;
		LoadView(ViewId.FullAd);
	}

	public void LoadView(ViewId id)
	{
		if (activeCanvas != null)
		{
			activeCanvas.gameObject.SetActive(false);
		}
		canvases[(int)id].gameObject.SetActive(true);
		activeCanvas = canvases[(int)id];
	}

	public enum ViewId
	{
		FullAd = 0,
		BannerAd = 1,
	}
}
