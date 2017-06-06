using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Camera))]
public class ScaleCameraInAspectRatio : MonoBehaviour 
{
	public float aspectRatio = 16f / 9f;
	void Start()
	{
		//only standalone would have letterboxing
		#if UNITY_STANDALONE 
		Camera cam = GetComponent<Camera> ();
		float screenRatio = (float)Screen.width / (float)Screen.height;
		float scale = screenRatio / aspectRatio;

		if (scale > 1f) {
			Rect pixRect = cam.pixelRect;
			pixRect.width = pixRect.height * aspectRatio;
			pixRect.y = 0f;
			pixRect.x = ((float)Screen.width - pixRect.width) / 2f;
			cam.pixelRect = pixRect;
		} else {
			Rect pixRect = cam.pixelRect;
			pixRect.height = pixRect.width / aspectRatio;
			pixRect.x = 0f;
			pixRect.y = ((float)Screen.height - pixRect.height) / 2f;
			cam.pixelRect = pixRect;
		}
		#endif
	}
}
