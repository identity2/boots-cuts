using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayingUIController : MonoBehaviour 
{
	//frame & avatar
	public CharacterImageSet[] characterImageSets;
	public Image frame;
	public Image avatar;
	public Color[] frameColors;
	public Color frameFailedColor;
	private CharacterImageSet currCharacterImage;
	private Coroutine changeImageCoroutine = null;
	private const float PopAnimationCoefficient = 0.35f;
	private float lastbeat;
	private const float PopAnimationScale = 1.25f;
	private const float ImageChangeDuration = 0.2f;

	//title
	public GameObject title;
	public Text titleText;

	//combo
	public Image comboCircle;
	public Text comboScoreText;
	public GameObject comboTextBackground;
	public Text comboText;

	//perfection
	public Image perfectionCircle;
	public Text perfectionScoreText;
	public GameObject perfectionTextBackground;
	public Text perfectionText;

	//colors for scores
	public Color zeroColor;
	public Color startColor;
	public Color fullColor;

	//save score
	private int maxCombo = 0;
	private int currCombo = 0;
	private int fullNoteCounts;
	private int currPerfection = 0; //good: +1, perfect: +2 (full: fullCount * 2)
	
	//pause scene
	public GameObject pauseButton;
	public GameObject pauseScene;
	private float pauseTimeStamp;

	//win scene
	public GameObject winBlackBackground;
	public GameObject winTitle;
	public Text winTitleText;
	public Image winComboCircle;
	public Text winComboScoreText;
	public GameObject winComboTextBackground;
	public Text winComboText;
	public Image winPerfectCircle;
	public Text winPerfectScoreText;
	public Text winPerfectText;
	public GameObject winPerfectTextBackground;
	public GameObject winFrame;
	public Image winPicture;
	public GameObject winNextButton;
	public GameObject winRetryButton;
	public GameObject winSceneBackground;
	private const float DelayBetweenElements = 0.75f;
	private const float NumberAnimationDuration = 2f;

	private const string HomeSceneName = "Songlist";


	public AudioSource songAudioSource; //bad design, but whatever!

	void Start()
	{
		//get the length of all notes from messenger
		fullNoteCounts = SongInfoMessenger.Instance.currentSong.TotalHitCounts();

		//get the title
		titleText.text = SongInfoMessenger.Instance.currentSong.songTitle;

		//set the corredponding character image set
		currCharacterImage = characterImageSets[SongInfoMessenger.Instance.characterIndex];

		// Set the initial avatar image.
		avatar.sprite = currCharacterImage.normalImage;

		lastbeat = 0f;

		//register to events
		Conductor.beatOnHitEvent += BeatOnHit;
		Conductor.songCompletedEvent += SongCompleted;
	}

	void Update()
	{
		// Perform Pop Animation every beat.
		if (Conductor.songposition > lastbeat + Conductor.crotchet)
		{
			StartCoroutine(PopAvatarCoroutine());
			lastbeat += Conductor.crotchet;
		}
	}

	void OnDestroy()
	{
		//unregister from events
		Conductor.beatOnHitEvent -= BeatOnHit;
		Conductor.songCompletedEvent -= SongCompleted;
	}

	//called by event
	void BeatOnHit(int trackNumber, Conductor.Rank rank)
	{
		if (rank == Conductor.Rank.PERFECT)
		{
			//update perfection
			currPerfection += 2;

			//update combo
			currCombo++;
			
			// Change Avatar Image
			ChangeAvatarImage(trackNumber);

			// Change Frame Color.
			frame.color = frameColors[trackNumber];
		}
		else if (rank == Conductor.Rank.GOOD)
		{
			//update perfection
			currPerfection++;
			
			//update combo
			currCombo++;

			// Change Avatar Image.
			ChangeAvatarImage(trackNumber);

			// Change Frame Color.
			frame.color = frameColors[trackNumber];
		}
		else   //bad or miss
		{
			//check if it is max combo
			maxCombo = Mathf.Max(maxCombo, currCombo);

			//update combo
			currCombo = 0;

			// Change avatar image.
			ChangeAvatarImage(null);

			// Change Frame Color.
			frame.color = frameFailedColor;
		}

		UpdateScoreUI();
	}

	// If failed, imageIndex is null.
	void ChangeAvatarImage(int? imageIndex)
	{
		float imageChangeDuration = ImageChangeDuration;

		// Terminate the previous animation if it is still running.
		if (changeImageCoroutine != null)
		{
			StopCoroutine(changeImageCoroutine);
		}

		if (imageIndex.HasValue)
		{
			// Change Image according to image indices.
			avatar.sprite = currCharacterImage.beatImages[imageIndex.Value];
		}
		else
		{
			// Change to failed image.
			avatar.sprite = currCharacterImage.failedImage;

			// Failed would have double changed duration.
			imageChangeDuration *= 2f;
		}
	
		// Change back to the original image after certain amount of time.
		changeImageCoroutine = StartCoroutine(ChangeAvatarImageCoroutine(imageChangeDuration));
	}

	IEnumerator ChangeAvatarImageCoroutine(float duration)
	{
		yield return new WaitForSeconds(duration);

		// Change back to the original avatar image.
		avatar.sprite = currCharacterImage.normalImage;
	}

	IEnumerator PopAvatarCoroutine()
	{
		float duration = Conductor.crotchet * PopAnimationCoefficient;

		float i = 0f;
		while (i <= 1f)
		{
			i += Time.deltaTime / duration;
			avatar.transform.localScale = Vector2.Lerp(Vector2.one * PopAnimationScale, Vector2.one, i);
			yield return null;
		}

	}

	void UpdateScoreUI()
	{
		//text
		comboScoreText.text = currCombo == 0 ? "-" : currCombo.ToString();
		perfectionScoreText.text = currPerfection == 0 ? "-" : string.Format("{0:F1}%", ((float) currPerfection / (float) (fullNoteCounts * 2) * 100f));

		//color
		Color comboColor = currCombo == 0 ? zeroColor : Color.Lerp(startColor, fullColor, (float) currCombo / (float) fullNoteCounts);
		Color perfectionColor = currPerfection == 0 ? zeroColor : Color.Lerp(startColor, fullColor, (float) currPerfection / (float) (fullNoteCounts * 2));
		
		comboScoreText.color = comboColor;
		comboText.color = comboColor;
		comboCircle.color = comboColor;

		perfectionCircle.color = perfectionColor;
		perfectionScoreText.color = perfectionColor;
		perfectionText.color = perfectionColor;
	
	}

	public void PauseButtonOnClick()
	{
		//display pause scene
		pauseScene.SetActive(true);

		Conductor.paused = true;
	}

	//pause scene
	public void ResumeButtonOnClick()
	{
		//disable pause scene
		pauseScene.SetActive(false);

		Conductor.paused = false;
	}

	public void RetryButtonOnClick()
	{
		//reload current scene
		SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
	}

	public void HomeButtonOnClick()
	{
		//go to home scene
		SceneManager.LoadSceneAsync(HomeSceneName);
	}

	public void KeyButtonOnClick()
	{
		KeyboardInputManager.instance.ShowKeySettings();
	}

	void SongCompleted()
	{
		StartCoroutine(ShowWinScene());
	}

	IEnumerator ShowWinScene()
	{
		//background gradually appears
		winBlackBackground.SetActive(true);
		winSceneBackground.SetActive(true);

		//disable game playing UIs
		frame.gameObject.SetActive(false);
		avatar.gameObject.SetActive(false);
		comboCircle.gameObject.SetActive(false);
		comboTextBackground.SetActive(false);
		perfectionCircle.gameObject.SetActive(false);
		perfectionTextBackground.SetActive(false);
		title.SetActive(false);
		pauseButton.SetActive(false);

		yield return new WaitForSeconds(DelayBetweenElements);

		//show title
		winTitleText.text = titleText.text;
		winTitle.SetActive(true);

		yield return new WaitForSeconds(DelayBetweenElements);

		//show combo
		winComboCircle.gameObject.SetActive(true);
		winComboTextBackground.SetActive(true);
		
		//increase by animation
		float i = 0f;
		while (i <= 1f)
		{
			print(2);
			i += Time.deltaTime / NumberAnimationDuration;

			int newCombo = (int) Mathf.Lerp(0f, (float) maxCombo, i);
			Color newColor = Color.Lerp(startColor, fullColor, (float) newCombo / (float) fullNoteCounts);

			//set text
			winComboScoreText.text = newCombo.ToString();
			
			//set color
			winComboCircle.color = newColor;
			winComboScoreText.color = newColor;
			winComboText.color = newColor;

			yield return null;
		}

		//ensure that the final score is shown correct
		winComboScoreText.text = maxCombo.ToString();

		yield return new WaitForSeconds(DelayBetweenElements);

		//show perfection
		winPerfectCircle.gameObject.SetActive(true);
		winPerfectTextBackground.SetActive(true);

		float perfectionPercentage = (float) currPerfection / (float) (fullNoteCounts * 2) * 100f;

		i = 0f;
		while (i <= 1f)
		{
			i += Time.deltaTime / NumberAnimationDuration;

			float newPerfection = Mathf.Lerp(0f, perfectionPercentage, i);
			Color newColor = Color.Lerp(startColor, fullColor, newPerfection / 100f);

			//show text
			winPerfectScoreText.text = string.Format("{0:F1}%", newPerfection);

			//show color
			winPerfectText.color = newColor;
			winPerfectCircle.color = newColor;
			winPerfectScoreText.color = newColor;

			yield return null;	
		}

		//ensure the correct perfection is shown
		winPerfectScoreText.text = string.Format("{0:F1}%", perfectionPercentage);

		yield return new WaitForSeconds(DelayBetweenElements);

		//show frame & picture
		winPicture.gameObject.SetActive(true);
		winFrame.SetActive(true);

		yield return new WaitForSeconds(DelayBetweenElements);

		//show retry & next buttons
		winRetryButton.SetActive(true);
		winNextButton.SetActive(true);
		
	}


}
