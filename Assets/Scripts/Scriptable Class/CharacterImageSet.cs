using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Image Set")]
public class CharacterImageSet : ScriptableObject
{
	public Sprite normalImage;
	public Sprite[] beatImages;
	public Sprite failedImage;
}
