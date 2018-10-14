using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteColorLerp
{
	public SpriteRenderer sr;
	public Color startColor;
	public Color endColor;
	public float duration;

	public SpriteColorLerp(SpriteRenderer sr, Color startColor, Color endColor, float duration) 
	{
		this.sr = sr;
		this.startColor = startColor;
		this.endColor = endColor;
		this.duration = duration;
	}

	public IEnumerator AnimationCoroutine()
	{
		float i = 0f;
		while (i <= 1f)
		{
			i += Time.deltaTime / duration;
			sr.color = Color.Lerp(startColor, endColor, i);
			yield return null;
		}
	}
}
