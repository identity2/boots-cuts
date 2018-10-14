using UnityEngine;
using System;

//passed from songlist (currentSong) -> "beat record" (recordedBeats) -> playing scene
// -> complete -> go back to the song in songlist
public class SongInfoMessenger : MonoBehaviour 
{
	public static SongInfoMessenger Instance = null;

	[NonSerialized] public int characterIndex;

	[NonSerialized] public SongInfo currentSong;

	[NonSerialized] public AudioClip[] recordedBeats;

	void Start()
	{
		Instance = this;

		DontDestroyOnLoad(gameObject);
	}

}
