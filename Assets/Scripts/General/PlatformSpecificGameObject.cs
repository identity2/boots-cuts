using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSpecificGameObject : MonoBehaviour 
{
	public bool android;
	public bool ios;
	public bool standalone;

	void Awake()
	{
		#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
		if (!standalone) gameObject.SetActive(false);
		#elif UNITY_ANDROID
		if (!android) gameObject.SetActive(false);
		#elif UNITY_IOS
		if (!ios) gameObject.SetActive(false);
		#endif
	}
}
