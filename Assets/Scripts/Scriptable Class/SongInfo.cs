using UnityEngine;

[CreateAssetMenu(menuName = "Song Info")]
public class SongInfo : ScriptableObject
{
	public AudioClip song;
	
	[Header("Every Song Has a Unique ID")]
	public int songID;
	public int collection;
	public int songNum;
	public bool easyDifficulty;

	[Header("Song Text Information")]
	public string songTitle;
	public string composer;
	public string arranger;

	[Header("Song Image Information")]
	public Sprite songCover;

	[Header("Playing Information")]
	public AudioClip[] defaultBeats;

	public float songOffset;
	public float bpm;

	[Header("At least > 3 for the first note.")]
	public Track[] tracks;

	// when not calculated, it's -1
	private int totalHits = -1;

	//get the total hits of the song
	public int TotalHitCounts()
	{
		if (totalHits != -1) return totalHits;

		totalHits = 0;
		foreach (Track track in tracks)
		{
			foreach (Note note in track.notes)
			{
				if (note.times == 0)
				{
					totalHits += 1;
				}
				else
				{
					totalHits += note.times;
				}
			}
		}

		return totalHits;
	}
	
	[System.Serializable]
	public class Note {
		public float note;
		public int times;
	}

	[System.Serializable]
	public class Track {
		public Note[] notes;
	}
}
