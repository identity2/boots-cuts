using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Song Collection")]
public class SongCollection : ScriptableObject
{
	public SongSet[] songSets;

	[Serializable]
	public class SongSet
	{
		public SongInfo easy;
		public SongInfo hard;
	}
}
