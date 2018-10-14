using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class ButtonColorSwitch : MonoBehaviour 
{
	public Image[] images;
	public Text[] texts;
	public float durationMin;
	public float durationMax;
	public Color startColor;
	public Color endColor;

	private bool forth = true;

	void OnEnable()
	{
		StartCoroutine(AnimationCoroutine());
	}

	void OnDisable()
	{
		StopAllCoroutines();
	}

	IEnumerator AnimationCoroutine()
	{
		float duration = UnityEngine.Random.Range(durationMin, durationMax);
		while (true)
		{
			
			float i = 0f;
			while (i <= 1f)
			{
				i += Time.deltaTime / duration;
				Color newColor = Color.Lerp(startColor, endColor, forth ? i : (1f - i));
				foreach (Image image in images)
				{
					image.color = newColor;
				}

				foreach (Text text in texts)
				{
					text.color = newColor;
				}
				yield return null;
			}

			forth = !forth;
		}
	}

}
