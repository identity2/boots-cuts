using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageColorAnimation
{
	private float duration;
	public float Duration { set { duration = value; } }

	private Color startColor;
	private Color endColor;
	private Image targetImage;

	public ImageColorAnimation(Image targetImage, Color startColor, Color endColor, float duration)
	{
		this.targetImage = targetImage;
		this.startColor = startColor;
		this.endColor = endColor;
		this.duration = duration;
	}

	public IEnumerator AnimationCoroutine(bool back = false)
	{
		float i = 0f;
		while (i <= 1f) {
			i += Time.deltaTime / duration;
			targetImage.color = Color.Lerp (startColor, endColor, back ? (1f - i) : i);
			yield return null;
		}
	}
}
