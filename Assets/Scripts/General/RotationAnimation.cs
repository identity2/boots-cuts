using System.Collections;
using UnityEngine;

public class RotationAnimation
{

	public RotationAnimation(){}

	public IEnumerator AnimationCoroutine(RectTransform rectT, Vector3 start, Vector3 end, float duration, Vector2 originalPivot, Vector2 animationPivot)
	{
		rectT.pivot = animationPivot;
		float i = 0f;
		while (i <= 1f)
		{
			i += Time.deltaTime / duration;
			rectT.transform.localRotation = Quaternion.Euler(Vector3.Lerp(start, end, i));
			yield return null;
		}
		rectT.pivot = originalPivot;
	}
}
