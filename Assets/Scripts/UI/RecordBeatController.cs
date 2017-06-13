using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class RecordBeatController : MonoBehaviour
{
	public static RecordBeatController Instance {get; set;}

	public string playingSceneName;

	private int len;

	public Color[] beatColors;
	
	private AudioClip[] defaultClips; //cache the default clips

	//main menu
	[HeaderAttribute("Main Menu")]
	public RectTransform mainMenu;
	public Vector2 mainMenuOriginalPivot;
	public Vector2 mainMenuAnimationPivot;
	public Image[] mainMenuSlots;
	public Image[] mainMenuSlotInners;
	public float touchedAlpha;
	public float originalAlpha;
	public Image[] micIcon;
	public Image[] checkMark;
	public GameObject goButton;
	private AudioClip[] savedClips;

	//board
	[HeaderAttribute("Record Board")]
	public CharacterImageSet[] characterImageSets;
	public Image boardMicIcon;
	public Image boardMicInner;
	public Image boardMicPlayIcon;
	public AudioSource micAudioSource;
	public AudioSource defaultAudioSource;
	public Image boardDefaultInner;
	public RectTransform board;
	public Vector2 boardOriginalPivot;
	public Vector2 boardAnimationPivot;
	public ToggleBar toggleBar;
	public GameObject okButton;
	public Image frame;
	public Image picture;
	public GameObject restoreButtonInner;
	private int edittingIndex;
	private AudioClip recordedClip;
	private AudioClip defaultClip;

	private float RecButtonDownTimeStamp;
	private const float RecClipMinimumLength = 0.25f;


	//flip animation
	private RotationAnimation mainFlipDownAnimation;
	private RotationAnimation boardFlipUpAnimation;
	private const float AnimationDuration = 0.25f;
	private Vector3 flipRotation = new Vector3(90f, 0f, 0f);
	private ImageColorAnimation boardRecInnerAnimation;
	private ImageColorAnimation boardDefaultInnerAnimation;
	private Coroutine boardRecAnimationCoroutine = null;
	private Coroutine boardDefaultAnimationCoroutine = null;

	private bool animating = false;

	private const int MaxClipLength = 10;
	
	void Awake()
	{
		Instance = this;

		//init fields
		defaultClips = SongInfoMessenger.Instance.currentSong.defaultBeats;
		len = defaultClips.Length;
		savedClips = new AudioClip[len];
		for (int i = 0; i < len; i++)
		{
			savedClips[i] = null;
		}

		//animations
		mainFlipDownAnimation = new RotationAnimation();
		boardFlipUpAnimation = new RotationAnimation();
		boardRecInnerAnimation = new ImageColorAnimation(
			boardMicInner,
			new Color(boardMicInner.color.r, boardMicInner.color.g, boardMicInner.color.b, 1f),
			boardMicInner.color,
			0f  //configure after the clip is recorded
		);
		boardDefaultInnerAnimation = new ImageColorAnimation(
			boardDefaultInner,
			new Color(boardDefaultInner.color.r, boardDefaultInner.color.g, boardDefaultInner.color.b, 1f),
			boardDefaultInner.color,
			0f //configure when the board is flipped
		);

		//toggle event
		ToggleBar.toggledEvent += Toggled;	

		//init mic
		Microphone.Start(null, false, 1, 44100);
		iPhoneSpeaker.ForceToSpeaker();
		Microphone.End(null);	
	}

	void OnDestroy()
	{
		ToggleBar.toggledEvent -= Toggled;
	}

	//main menu
	public void MainMenuSlotPointerDown(int index)
	{
		if (animating) return;

		Color targetColor = beatColors[index];

		//change the color of the frame and microphone
		mainMenuSlots[index].color = targetColor;
		micIcon[index].color = targetColor;

		//change the color of the inner
		mainMenuSlotInners[index].color = new Color(targetColor.r, targetColor.g, targetColor.b, touchedAlpha);
	}

	public void MainMenuSlotPointerUp(int index)
	{
		//not recorded
		if (savedClips[index] == null)
		{
			//change back the color of the frame and microphone
			mainMenuSlots[index].color = Color.white;
			micIcon[index].color = Color.white;

			//change back the color of the inner
			mainMenuSlotInners[index].color = new Color(255f, 255f, 255f, originalAlpha);
		}
		else      //recorded
		{
			//change back the color of the inner
			mainMenuSlotInners[index].color = new Color(beatColors[index].r, beatColors[index].g, beatColors[index].b, originalAlpha);
		}

		TransitionToBoard();
		ConfigureRecordBoard(index);
	}

	public void MainMenuGoButtonClicked()
	{
		//save the recorded beats to messenger
		SongInfoMessenger.Instance.recordedBeats = new AudioClip[len];
		for (int i = 0; i < len; i++)
		{
			SongInfoMessenger.Instance.recordedBeats[i] = savedClips[i];
		}

		//goto scene
		SceneManager.LoadScene(playingSceneName);
	}

	//record board

	void ConfigureRecordBoard(int index)
	{
		edittingIndex = index;

		//title

		//frame
		frame.color = beatColors[index];

		//avatar
		picture.sprite = characterImageSets[SongInfoMessenger.Instance.characterIndex].beatImages[index];

		//AudioClip
		defaultClip = defaultClips[index];
		defaultAudioSource.clip = defaultClip;

		//configure default inner animation duration
		boardDefaultInnerAnimation.Duration = defaultClip.length;

		//inner circle to transparent
		boardMicInner.color = new Color(boardMicInner.color.r, boardMicInner.color.g, boardMicInner.color.b, 0f);
		boardDefaultInner.color = new Color(0f, 0f, 0f, 0f);

		//check if saved
		if (savedClips[index] != null)
		{
			okButton.SetActive(true);

			//check if default
			if (savedClips[index] == defaultClip)
			{
				//init toggle to the left
				toggleBar.Initialize(inLeft: true);

				//configure recorded clip
				recordedClip = null;
				micAudioSource.clip = null;

				//set the icon to mic
				boardMicIcon.gameObject.SetActive(true);
				boardMicPlayIcon.gameObject.SetActive(false);
			}
			else  //not default
			{
				//init toggle to the right
				toggleBar.Initialize(inLeft: false);

				//configure recorded clip
				recordedClip = savedClips[index];
				micAudioSource.clip = recordedClip;

				//set animation duration
				boardRecInnerAnimation.Duration = recordedClip.length;
				
				//set the icon to play
				boardMicIcon.gameObject.SetActive(false);
				boardMicPlayIcon.gameObject.SetActive(true);
			}
		}
		else   //not saved
		{
			//hide ok button
			okButton.SetActive(false);

			//init toggle to the right
			toggleBar.Initialize(inLeft: false);

			//no recorded clip
			recordedClip = null;

			//set the icon to mic
			boardMicIcon.gameObject.SetActive(true);
			boardMicPlayIcon.gameObject.SetActive(false);
		}
	}

	void Toggled(bool toLeft)
	{
		if (toLeft)
		{
			okButton.SetActive(true);
		}
		else
		{
			//have recordedClip
			if (recordedClip != null)
			{
				okButton.SetActive(true);
			}
			else
			{
				okButton.SetActive(false);
			}
		}
	}

	public void RecButtonPointerDown()
	{
		RecButtonDownTimeStamp = Time.timeSinceLevelLoad;

		//if have recordedClip
		if (recordedClip != null)
		{
			//stop previous animation
			if (boardRecAnimationCoroutine != null)
			{
				StopCoroutine(boardRecAnimationCoroutine);
			}

			//play the clip and start animation
			micAudioSource.Play();
			boardRecAnimationCoroutine = StartCoroutine(boardRecInnerAnimation.AnimationCoroutine());
		}
		else   //no recordedClip
		{
			//if is currently recording, return
			if (Microphone.IsRecording(null)) return;

			//start the mic
			recordedClip = Microphone.Start(null, false, MaxClipLength, 44100);
			iPhoneSpeaker.ForceToSpeaker();

			//change color of the inner circle
			boardMicInner.color = new Color(boardMicInner.color.r, boardMicInner.color.g, boardMicInner.color.b, touchedAlpha);

			//if it exceeds maximum length, call touch up func
			Invoke("RecButtonPointerUp", (float) MaxClipLength);
		}
		
	}

	public void RecButtonPointerUp()
	{
		//switching board
		if (animating) return;

		//not recording
		if (!Microphone.IsRecording(null)) return;

		//if too short, call this later
		float timePassed = Time.timeSinceLevelLoad - RecButtonDownTimeStamp;
		if (timePassed < RecClipMinimumLength)
		{
			Invoke("RecButtonPointerUp", RecClipMinimumLength - timePassed);
			return;
		}

		CancelInvoke();

		//change back the color of the inner circle
		boardMicInner.color = new Color(boardMicInner.color.r, boardMicInner.color.g, boardMicInner.color.b, 0f);

		//get mic position in samples
		int lastTime = Microphone.GetPosition(null);

		//end mic
		Microphone.End(null);

		//if too short, return
		if (lastTime == 0) return;

		//get the full sample and store them in an array
		float[] samples = new float[recordedClip.samples];
		recordedClip.GetData(samples, 0);

		//store the clip samples to a new array
		float[] clipSamples = new float[lastTime];
		Array.Copy(samples, clipSamples, clipSamples.Length);

		//create a new AudioClip and set the data from the clipped samples
		recordedClip = AudioClip.Create("new", clipSamples.Length, 1, 44100, false);
		recordedClip.SetData(clipSamples, 0);

		//set the AudioSource
		micAudioSource.clip = recordedClip;

		//configure the duration of the animation
		boardRecInnerAnimation.Duration = recordedClip.length;

		//reveal ok button, set icon to play
		okButton.SetActive(true);
		boardMicIcon.gameObject.SetActive(false);
		boardMicPlayIcon.gameObject.SetActive(true);
	}

	public void okButtonOnClick()
	{
		if (animating) return;

		TransitionToMainMenu();

		//save the recorded audio
		if (toggleBar.InLeft)
		{
			savedClips[edittingIndex] = defaultClip;
		}
		else
		{
			savedClips[edittingIndex] = recordedClip;
		}

		//change the color of main menu slots
		checkMark[edittingIndex].color = beatColors[edittingIndex];
		checkMark[edittingIndex].gameObject.SetActive(true);
		micIcon[edittingIndex].gameObject.SetActive(false);
		mainMenuSlots[edittingIndex].color = beatColors[edittingIndex];
		mainMenuSlotInners[edittingIndex].color = new Color(beatColors[edittingIndex].r, beatColors[edittingIndex].g, beatColors[edittingIndex].b, originalAlpha);
	
		//see if all slots have been recorded
		foreach (AudioClip clip in savedClips)
		{
			if (clip == null) return;
		}
		goButton.SetActive(true);
	}

	public void RestoreButtonOnClick()
	{
		//clear the recorded clip
		recordedClip = null;

		//inner disappears
		restoreButtonInner.SetActive(false);

		//check if okButton should disappear
		if (!toggleBar.InLeft)
		{
			okButton.SetActive(false);
		}

		//update icons
		boardMicIcon.gameObject.SetActive(true);
		boardMicPlayIcon.gameObject.SetActive(false);
	}

	public void RestoreButtonPointerDown()
	{
		//inner appears
		restoreButtonInner.SetActive(true);
	}

	public void DefaultPlayButtonPointerDown()
	{
		if (boardDefaultAnimationCoroutine != null)
		{
			StopCoroutine(boardDefaultAnimationCoroutine);
		}

		//play the AudioClip and start animation
		defaultAudioSource.Play();
		boardDefaultAnimationCoroutine = StartCoroutine(boardDefaultInnerAnimation.AnimationCoroutine());	
	}

	//animation
	void TransitionToBoard()
	{
		animating = true;
		StartCoroutine(boardFlipUpAnimation.AnimationCoroutine(board, flipRotation, Vector3.zero, AnimationDuration, boardOriginalPivot, boardAnimationPivot));
		StartCoroutine(mainFlipDownAnimation.AnimationCoroutine(mainMenu, Vector3.zero, flipRotation, AnimationDuration, mainMenuOriginalPivot, mainMenuAnimationPivot));
		Invoke("RestoreAnimating", AnimationDuration);
	}

	void TransitionToMainMenu()
	{
		animating = true;
		StartCoroutine(boardFlipUpAnimation.AnimationCoroutine(board, Vector3.zero, flipRotation, AnimationDuration, boardOriginalPivot, boardAnimationPivot));
		StartCoroutine(mainFlipDownAnimation.AnimationCoroutine(mainMenu, flipRotation, Vector3.zero, AnimationDuration, mainMenuOriginalPivot, mainMenuAnimationPivot));
		Invoke("RestoreAnimating", AnimationDuration);
	}

	void RestoreAnimating()
	{
		animating = false;
	}
}
