using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBindingIndicator : MonoBehaviour 
{
	public KeyboardInputManager.KeyBindings keyName;

	private TextMesh _textMesh;

	void Start()
	{
		_textMesh = GetComponent<TextMesh>();

		//set the string
		_textMesh.text = KeyboardInputManager.instance.GetKeyCode(keyName).ToString();

		KeyboardInputManager.keyChangedEvent += KeyBindingChanged;
	}

	void OnDestroy()
	{
		KeyboardInputManager.keyChangedEvent -= KeyBindingChanged;
	}

	void KeyBindingChanged(KeyboardInputManager.KeyBindings keyBinding, KeyCode keyCode)
	{
		if (keyBinding == keyName)
		{
			_textMesh.text = keyCode.ToString();
		}
	}
}
