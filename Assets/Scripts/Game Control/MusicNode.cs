using System;
using UnityEngine;

public class MusicNode : MonoBehaviour
{
	public TextMesh timesText;
	public GameObject timesTextBackground;
	public Sprite[] backgroundSprites;
	public SpriteRenderer ringSprite;
	[NonSerialized] public float startY;
	[NonSerialized] public float endY;
	[NonSerialized] public float removeLineY;
	[NonSerialized] public float beat;
	[NonSerialized] public int times;
	[NonSerialized] public bool paused;

	public void Initialize(float posX, float startY, float endY, float removeLineY, float posZ, float targetBeat, int times, Color color)
	{
		this.startY = startY;
		this.endY = endY;
		this.beat = targetBeat;
		this.times = times;
		this.removeLineY = removeLineY;

		paused = false;

		//set position
		transform.position = new Vector3(posX, startY, posZ);
		
		//set color
		ringSprite.color = color;

		//randomize background
		GetComponent<SpriteRenderer>().sprite = backgroundSprites[UnityEngine.Random.Range(0, backgroundSprites.Length)];

		//set times
		if (times > 0)
		{
			timesText.text = times.ToString();
			timesTextBackground.SetActive(true);
		}
		else
		{
			timesTextBackground.SetActive(false);

			//randomize rotation
			//transform.rotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 359f));
		}
		
	}

	void Update()
	{
		if (Conductor.pauseTimeStamp > 0f) return; //resume not managed

		if (paused) return; //multi-times notes might be paused on the finish line

		transform.position = new Vector3(transform.position.x, startY + (endY - startY) * (1f - (beat - Conductor.songposition / Conductor.crotchet) / Conductor.BeatsShownOnScreen), transform.position.z);
	
		//remove itself when out of the screen (remove line)
		if (transform.position.y < removeLineY)
		{
			gameObject.SetActive(false);
		}
	}

	//remove (multi-times note failed), might apply some animations later
	public void MultiTimesFailed()
	{
		gameObject.SetActive(false);
	}

	//if the node is removed, return true
	public bool MultiTimesHit()
	{
		//update text
		times--;
		if (times == 0)
		{
			gameObject.SetActive(false);
			return true;
		}

		timesText.text = times.ToString();
			
		return false;
	}

	public void PerfectHit()
	{
		gameObject.SetActive(false);
	}

	public void GoodHit()
	{
		gameObject.SetActive(false);
	}

	public void BadHit()
	{
		gameObject.SetActive(false);
	}
}
