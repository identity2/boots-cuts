using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoScene : MonoBehaviour 
{
	public void LoadSceneByName(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}
}
