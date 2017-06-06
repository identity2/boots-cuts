using System.Collections;
using UnityEngine;

public class UISlideAnimation 
{
	public IEnumerator AnimationCoroutine(GameObject current, GameObject next, float width, float duration, bool toRight)
	{

		float i = 0f;
		while (i <= 1f)
		{
			i += Time.deltaTime / duration;
			yield return null;
		}
	}
}
