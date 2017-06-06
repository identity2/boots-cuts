using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Conductor : MonoBehaviour 
{
	public enum Rank {PERFECT, GOOD, BAD, MISS};

	public delegate void BeatOnHitAction(int trackNumber, Rank rank);
	public static event BeatOnHitAction beatOnHitEvent;

	//song completion
	public delegate void SongCompletedAction();
	public static event SongCompletedAction songCompletedEvent;


	private float songLength;

	//if the whole game is paused
	public static bool paused = true;
	private bool songStarted = false;

	public static float pauseTimeStamp = -1f; //negative means not managed
	private float pausedTime = 0f;

	private SongInfo songInfo;

	[Header("Spawn Points")]
	public float[] trackSpawnPosX;

	public float startLineY;
	public float finishLineY;

	public float removeLineY;

	public float badOffsetY;
	public float goodOffsetY;
	public float perfectOffsetY;
	private const float MobileOffsetMultiplier = 1.4f;

	//different colors for each track
	public Color[] trackColors;

	//current song position
	public static float songposition;

	//how many seconds for each beat
	public static float crotchet;

	//index for each tracks
	private int[] trackNextIndices;
	
	//queue, saving the MusicNodes which currently on screen
	private Queue<MusicNode>[] queueForTracks;

	//multi-times notes might be paused on the finish line, but already dequed
	private MusicNode[] previousMusicNodes; 

	//keep a reference of the sound tracks
	private SongInfo.Track[] tracks;

	private float dsptimesong;

	public static float BeatsShownOnScreen = 4f;

	//count down canvas
	private const int StartCountDown = 3;
	public GameObject countDownCanvas;
	public Text countDownText;


	//layer each music node, so that the first one would be at the front
	private const float LayerOffsetZ = 0.001f;
	private const float FirstLayerZ = -6f;
	private float[] nextLayerZ;

	//total tracks
	private int len;
	private AudioSource audioSource { get { return GetComponent<AudioSource> (); } }

	void PlayerInputted(int trackNumber)
	{
		//check if multi-times node exists
		if (previousMusicNodes[trackNumber] != null)
		{
			//dispatch beat on hit event (multi-times node is always PERFECT)
			if (beatOnHitEvent != null) beatOnHitEvent(trackNumber, Rank.PERFECT);

			//check if the node should be removed
			if (previousMusicNodes[trackNumber].MultiTimesHit())
			{
				//print("Multi-Times Succeed!");
				previousMusicNodes[trackNumber] = null;
			}
		}
		else if (queueForTracks[trackNumber].Count != 0)
		{
			//peek the node in the queue
			MusicNode frontNode = queueForTracks[trackNumber].Peek();

			if (frontNode.times > 0) return; //multi-times node should be handled in the Update() func

			float offsetY = Mathf.Abs(frontNode.gameObject.transform.position.y - finishLineY);
			
			if (offsetY < perfectOffsetY) //perfect hit
			{
				frontNode.PerfectHit();
				//print("Perfect");

				//dispatch beat on hit event
				if (beatOnHitEvent != null) beatOnHitEvent(trackNumber, Rank.PERFECT);

				queueForTracks[trackNumber].Dequeue();
			}
			else if (offsetY < goodOffsetY) //good hit
			{
				frontNode.GoodHit();
				//print("Good");

				//dispatch beat on hit event
				if (beatOnHitEvent != null) beatOnHitEvent(trackNumber, Rank.GOOD);

				queueForTracks[trackNumber].Dequeue();
			}
			else if (offsetY < badOffsetY) //bad hit
			{
				frontNode.BadHit();

				//dispatch beat on hit event
				if (beatOnHitEvent != null) beatOnHitEvent(trackNumber, Rank.BAD);

				queueForTracks[trackNumber].Dequeue();
			}
		}
	}

	void Start()
	{
		//reset static variables
		paused = true;
		pauseTimeStamp = -1f;

		//if in mobile platforms, multiply the offsets
		#if UNITY_IOS || UNITY_ANDROID
		perfectOffsetY *= MobileOffsetMultiplier;
		goodOffsetY *= MobileOffsetMultiplier;
		badOffsetY *= MobileOffsetMultiplier;
		#endif

		//display countdown canvas
		countDownCanvas.SetActive(true);

		//get the song info from messenger
		songInfo = SongInfoMessenger.Instance.currentSong;

		//listen to player input
		PlayerInputControl.inputtedEvent += PlayerInputted;

		//initialize fields
		crotchet = 60f / songInfo.bpm;
		songLength = songInfo.song.length;

		//initialize arrays
		len = trackSpawnPosX.Length;
		trackNextIndices = new int[len];
		nextLayerZ = new float[len];
		queueForTracks = new Queue<MusicNode>[len];
		previousMusicNodes = new MusicNode[len];
		for (int i = 0; i < len; i++)
		{
			trackNextIndices[i] = 0;
			queueForTracks[i] = new Queue<MusicNode>();
			previousMusicNodes[i] = null;
			nextLayerZ[i] = FirstLayerZ;
		}

		tracks = songInfo.tracks; //keep a reference of the tracks


		//initialize audioSource
		audioSource.clip = songInfo.song;

		//start countdown
		StartCoroutine(CountDown());
	}

	void StartSong()
	{
		//get dsptime
		dsptimesong = (float) AudioSettings.dspTime;

		//play song
		audioSource.Play();

		//unpause
		Conductor.paused = false;
		songStarted = true;
	}

	void Update()
	{
		//for count down
		if (!songStarted) return;
		
		//for pausing
		if (paused)
		{
			if (pauseTimeStamp < 0f) //not managed
			{
				pauseTimeStamp = (float) AudioSettings.dspTime;
				//print("pausetimestamp:" + pauseTimeStamp.ToString());
				audioSource.Pause();
			}

			return;
		}
		else if (pauseTimeStamp > 0f) //resume not managed
		{
			pausedTime += (float) AudioSettings.dspTime - pauseTimeStamp;
			//print("resumetimestamp:"+AudioSettings.dspTime.ToString());
			//print("offset"+pausedTime.ToString());
			audioSource.Play();

			pauseTimeStamp = -1f;
		}

		//calculate songposition
		songposition = (float) (AudioSettings.dspTime - dsptimesong - pausedTime) * audioSource.pitch - songInfo.songOffset;
		//print (songposition);

		//check if need to instantiate new nodes
		float beatToShow = songposition / crotchet + BeatsShownOnScreen;
		
		//loop the tracks for new MusicNodes
		for (int i = 0; i < len; i++)
		{
			int nextIndex = trackNextIndices[i];
			SongInfo.Track currTrack = tracks[i];
			
			if (nextIndex < currTrack.notes.Length && currTrack.notes[nextIndex].note < beatToShow)
			{
				SongInfo.Note currNote = currTrack.notes[nextIndex];

				//set z position
				float layerZ = nextLayerZ[i];
				nextLayerZ[i] += LayerOffsetZ;

				//get a new node
				MusicNode musicNode = MusicNodePool.instance.GetNode(trackSpawnPosX[i], startLineY, finishLineY, removeLineY, layerZ, currNote.note, currNote.times, trackColors[i]);
				
				//enqueue
				queueForTracks[i].Enqueue(musicNode);

				//update the next index
				trackNextIndices[i]++;
			}

		}

		//loop the queue to check if any of them reaches the finish line
		for (int i = 0; i < len; i++)
		{
			//empty queue, continue
			if (queueForTracks[i].Count == 0) continue;

			MusicNode currNode = queueForTracks[i].Peek();

			//multi-times note
			if (currNode.times > 0 && currNode.transform.position.y <= finishLineY + goodOffsetY)
			{
				//have previous note stuck on the finish line
				if (previousMusicNodes[i] != null)
				{
					previousMusicNodes[i].MultiTimesFailed();

					//dispatch miss event
					if (beatOnHitEvent != null) beatOnHitEvent(i, Rank.MISS);
				}

				//pause the note
				currNode.paused = true;

				//align to finish line
				currNode.transform.position = new Vector3(currNode.transform.position.x, finishLineY, currNode.transform.position.z);

				//deque, but keep a reference
				previousMusicNodes[i] = currNode;
				queueForTracks[i].Dequeue();
			}
			else if (currNode.transform.position.y <= finishLineY - goodOffsetY)   //single time note
			{
				//have previous note stuck on the finish line
				if (previousMusicNodes[i] != null)
				{
					previousMusicNodes[i].MultiTimesFailed();
					previousMusicNodes[i] = null;

					//dispatch miss event
					if (beatOnHitEvent != null) beatOnHitEvent(i, Rank.MISS);
				}

				//deque
				queueForTracks[i].Dequeue();

				//dispatch miss event (if a multi-times note is missed, its next single note would also be missed)
				if (beatOnHitEvent != null) beatOnHitEvent(i, Rank.MISS);
			}
		}


		//check to see if the song reaches its end
		if (songposition > songLength)
		{
			songStarted = false;

			if (songCompletedEvent != null)
				songCompletedEvent();
		}
	}

	IEnumerator CountDown()
	{
		yield return new WaitForSeconds(1f);

		for (int i = StartCountDown; i >= 1; i--)
		{
			countDownText.text = i.ToString();
			yield return new WaitForSeconds(1f);				
		}
		countDownCanvas.SetActive(false);

		StartSong();
	}

	void OnDestroy()
	{
		PlayerInputControl.inputtedEvent -= PlayerInputted;
	}
}
