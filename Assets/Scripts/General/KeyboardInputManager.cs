using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardInputManager : MonoBehaviour 
{
	//singleton
	public static KeyboardInputManager instance;

	//UI elements
	public GameObject board;
	public Text[] keyTexts;
	public GameObject[] keyBackgrounds;
	public GameObject saveButton;

	public enum KeyBindings {Track1 = 0, Track2, Track3, Track4, Pause};
	private const string Default1 = "Z";
	private const string Default2 = "X";
	private const string Default3 = "N";
	private const string Default4 = "M";
	private const string DefaultPause = "P";

	//key changed event
	public delegate void KeyChangedAction(KeyBindings keyBinding, KeyCode keyCode);
	public static KeyChangedAction keyChangedEvent;

	private KeyCode[] keys;

	//change key
	private KeyBindings? waitingForKeySetting = null;

	public void ShowKeySettings()
	{
		board.SetActive(true);
	}

	public KeyCode GetKeyCode(KeyBindings keyBinding)
	{
		return keys[(int) keyBinding];
	}

	//UI triggered events
	public void SaveButtonOnClick()
	{
		board.SetActive(false);
	}

	public void DefaultButtonOnClick()
	{
		ResetPreviousKey();

		//set to default
		SetKey(KeyBindings.Track1, (KeyCode) System.Enum.Parse(typeof(KeyCode), Default1));
		SetKey(KeyBindings.Track2, (KeyCode) System.Enum.Parse(typeof(KeyCode), Default2));
		SetKey(KeyBindings.Track3, (KeyCode) System.Enum.Parse(typeof(KeyCode), Default3));
		SetKey(KeyBindings.Track4, (KeyCode) System.Enum.Parse(typeof(KeyCode), Default4));
		SetKey(KeyBindings.Pause, (KeyCode) System.Enum.Parse(typeof(KeyCode), DefaultPause));
	}

	public void KeySlotPressed(int keyBindingIndex)
	{
		ResetPreviousKey();	

		//set current key slot to waiting for key setting
		waitingForKeySetting = (KeyBindings) keyBindingIndex;

		//disable start button
		saveButton.SetActive(false);

		//make text disappear
		keyTexts[keyBindingIndex].gameObject.SetActive(false);

		//make background image appear
		keyBackgrounds[keyBindingIndex].SetActive(true);
	}

	//private funcs
	void Awake()
	{
		//singleton
		if (instance == null)
		{
			DontDestroyOnLoad(gameObject);
			instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		//configure the key according to player prefs
		keys = new KeyCode[keyTexts.Length];
		keys[(int) KeyBindings.Track1] = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(KeyBindings.Track1.ToString(), Default1));
		keys[(int) KeyBindings.Track2] = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(KeyBindings.Track2.ToString(), Default2));
		keys[(int) KeyBindings.Track3] = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(KeyBindings.Track3.ToString(), Default3));
		keys[(int) KeyBindings.Track4] = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(KeyBindings.Track4.ToString(), Default4));
		keys[(int) KeyBindings.Pause]  = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString(KeyBindings.Pause.ToString(), DefaultPause));

		//change appearance
		for (int i = 0; i < keyTexts.Length; i++)
		{
			keyTexts[i].text = keys[i].ToString();
		}
	}

	void Update()
	{
		if (waitingForKeySetting != null)
		{
			foreach (char ch in Input.inputString)
			{
				//only single letter or numeral is allowed, no repitition is allowed
				if (char.IsLetter(ch) || char.IsNumber(ch))
				{
					string savedKey = ch.ToString().ToUpper();

					//cannot repeat the same key
					if (KeyRepeated((int) waitingForKeySetting, savedKey)) continue;

					//set key
					SetKey(waitingForKeySetting ?? 0, (KeyCode) System.Enum.Parse(typeof(KeyCode), savedKey));

					return;
				}
			}
		}
	}

	void SetKey(KeyBindings keyBinding, KeyCode keyCode)
	{
		int keySlotIndex = (int) keyBinding;
		string keyCodeString = keyCode.ToString();

		//configure key
		keys[keySlotIndex] = keyCode;

		//update text
		keyTexts[keySlotIndex].text = keyCodeString;

		//reset appearance
		ResetPreviousKey();
		saveButton.SetActive(true);

		//save to player pref
		PlayerPrefs.SetString(keyBinding.ToString(), keyCodeString);

		//inform others
		if (keyChangedEvent != null) keyChangedEvent(keyBinding, keyCode);

		waitingForKeySetting = null;

	}

	void ResetPreviousKey()
	{
		//if previous key not configured
		if (waitingForKeySetting != null)
		{
			//reset the appearance of previous key
			keyTexts[(int) waitingForKeySetting].gameObject.SetActive(true);
			keyBackgrounds[(int) waitingForKeySetting].SetActive(false);
		}
	}

	bool KeyRepeated(int currKeyIndex, string savedKey)
	{
		for (int i = 0; i < keyTexts.Length; i++)
		{
			if (currKeyIndex == i) continue;

			if (savedKey == keyTexts[i].text) return true;
		}

		return false;
	}
}
