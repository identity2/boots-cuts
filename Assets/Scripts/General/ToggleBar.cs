using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleBar : MonoBehaviour 
{
	public delegate void ToggledAction(bool inLeft);
	public static event ToggledAction toggledEvent;

	public Color leftColor;
	public Color rightColor;

	public GameObject knob;

	public float LeftPos = -33.5f;
	public float RightPos = 33.5f;
	private const float AnimationDuration = 0.2f;
	
	private bool animating = false;

	public bool InLeft { get { return inLeft;} }
	private bool inLeft = false;

	private Image _image { get { return GetComponent<Image>();} }
	private RectTransform knobTransform { get { return knob.GetComponent<RectTransform>();} }
	private Image knobImage { get { return knob.GetComponent<Image>(); } }

	public void Initialize(bool inLeft)
	{
		this.inLeft = inLeft;

		if (inLeft)
		{
			knobTransform.localPosition = new Vector2(LeftPos, knobTransform.localPosition.y);
			knobImage.color = leftColor;
			_image.color = leftColor;
		}
		else
		{
			knobTransform.localPosition = new Vector2(RightPos, knobTransform.localPosition.y);
			knobImage.color = rightColor;
			_image.color = rightColor;
		}
	}

	public void Toggled()
	{
		if (!animating) StartCoroutine(ToggledAnimation());
	}	

	IEnumerator ToggledAnimation()
	{
		animating = true;
		float i = 0f;
		while (i < 1f)
		{
			i += Time.deltaTime / AnimationDuration;

			//color
			Color newColor = Color.Lerp(leftColor, rightColor, inLeft ? i : (1f-i));
			knobImage.color = newColor;
			_image.color = newColor;

			//RectTransform
			knobTransform.localPosition = new Vector2(Mathf.Lerp(LeftPos, RightPos, inLeft ? i : (1f-i)), knobTransform.localPosition.y);

			yield return null;
		}
		inLeft = !inLeft;

		//inform listeners
		if (toggledEvent != null) toggledEvent(inLeft);
		animating = false;
	}
}
