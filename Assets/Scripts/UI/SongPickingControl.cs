using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

// Era Picker & Fuel indicator -> SoundPicker (2 boards) -> Choose Character
public class SongPickingControl : MonoBehaviour 
{
	private SongInfo currSong;

	//Era Board
	public RectTransform eraBoardTransform;

	public SongCollection[] songCollections;

	//Song Board
	[Serializable]
	public class SongBoard
	{
		public RectTransform rectTransform;
		public Text titleText;
		public Image backgroundImage;
		public ToggleBar difficultyToggle;
		public GameObject leftButton;
		public GameObject rightButton;
		public Text bestComboText;
		public Text bestPerfectionText;
	}
	public SongBoard currSongBoard;
	public SongBoard altSongBoard;

	//Song Info Popup
	public RectTransform songInfoPopUp;
	public Text songInfoTitleText;
	public Text songInfoComposerText;
	public Text songInfoArrangerText;

	//Character Board
	public GameObject characterBoard;

	//curr song properties
	public int currCollectionIndex;
	public int currSongSetIndex;
	public bool currEasyDifficulty;

	//the messenger to pass through other scenes
	public GameObject songInfoMessengerPrefab;

	//animation
	private RotationAnimation flipInAnimation;
	private RotationAnimation flipOutAnimation;
	private const float FlipAnimationDuration = 0.25f;
	private bool animating = false;

	//rotation
	private Vector3 verticalFlip = new Vector3(90f, 0f, 0f);
	private Vector3 horizontalFlip = new Vector3(0f, 90f, 0f);
	private Vector3 foldFlip = new Vector3(56f, 90f, 0f);

	//pivot
	private Vector2 topEdgePivot = new Vector2(0.5f, 1f);
	private Vector2 bottomEdgePivot = new Vector2(0.5f, 0f);
	private Vector2 leftEdgePivot = new Vector2(0f, 0.5f);
	private Vector2 rightEdgePivot = new Vector2(1f, 0.5f);
	private Vector2 centerPivot = new Vector2(0.5f, 0.5f);

	void Awake()
	{
		//init animation
		flipInAnimation = new RotationAnimation();
		flipOutAnimation = new RotationAnimation();

		//if song messenger exists (aka. exits from Record Beat or Playing scenes)
		if (SongInfoMessenger.Instance != null)
		{
			//set curr song
			currSong = SongInfoMessenger.Instance.currentSong;

			//set indices
			currCollectionIndex = currSong.collection;
			currSongSetIndex = currSong.songNum;
			currEasyDifficulty = currSong.easyDifficulty;

			//configure curr board
			ConfigureCurrSongBoard();

			//make era board disappear
			eraBoardTransform.transform.localRotation = Quaternion.Euler(horizontalFlip);

			//make curr song board appear
			currSongBoard.rectTransform.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}
		else   //if not exist, create new SongInfoMessenger
		{
			Instantiate(songInfoMessengerPrefab);
		}
	}

	//era board
	public void EraImageClicked(int songSetIndex)
	{
		if (animating) return;

		//set collection
		currCollectionIndex = songSetIndex;
		currSongSetIndex = 0;
		currEasyDifficulty = true;

		//configure song board
		ConfigureCurrSongBoard();

		//perform board transition animation
		FlipToSongFromEra();
	}

	public void EraBoardBackButtonClicked()
	{
		//back to main menu
		//
	}

	//song board
	public void SongBoardBackButtonClicked()
	{
		if (animating) return;

		//perform board transition animation
		FlipToEraFromSong();

		//reset collection, song, difficulty
		currCollectionIndex = 0;
		currSongSetIndex = 0;
		currEasyDifficulty = true;
	}

	public void SongBoardInfoButtonClicked()
	{
		if (animating) return;

		ConfigureSongInfoBoard();

		//pop up animation
		SongInfoPopUp();
	}

	public void SongBoardLeftButtonClicked()
	{
		if (animating) return;

		//swap curr & alt boards
		SongBoard temp = currSongBoard;
		currSongBoard = altSongBoard;
		altSongBoard = temp;

		//update indices
		currSongSetIndex--;

		//configure curr board
		ConfigureCurrSongBoard();

		//perform animation
		FlipToLeftSong();
	}

	public void SongBoardRightButtonClicked()
	{
		if (animating) return;

		//swap curr & alt boards
		SongBoard temp = currSongBoard;
		currSongBoard = altSongBoard;
		altSongBoard = temp;

		//update indices
		currSongSetIndex++;

		//configure curr board
		ConfigureCurrSongBoard();

		//perform animation
		FlipToRightSong();
	}

	void ConfigureCurrSongBoard()
	{
		currSong = currEasyDifficulty ?
			songCollections[currCollectionIndex].songSets[currSongSetIndex].easy :
			songCollections[currCollectionIndex].songSets[currSongSetIndex].hard ;
		
		//title
		currSongBoard.titleText.text = currSong.songTitle;

		//difficulty (left is easy)
		currSongBoard.difficultyToggle.Initialize(inLeft: currSong.easyDifficulty);

		//background image
		//

		//best combo text
		//

		//best perfection text

		//left button
		if (currSongSetIndex == 0)
		{
			currSongBoard.leftButton.SetActive(false);
		}
		else
		{
			currSongBoard.leftButton.SetActive(true);
		}

		//right button
		if (currSongSetIndex == songCollections[currCollectionIndex].songSets.Length - 1)
		{
			currSongBoard.rightButton.SetActive(false);
		}
		else
		{
			currSongBoard.rightButton.SetActive(true);
		}
	}

	public void SongBoardDifficultyToggled()
	{
		if (animating) return;

		//play animation
		currSongBoard.difficultyToggle.Toggled();

		//change difficulty
		currEasyDifficulty = !currEasyDifficulty;

		//update currSong
		currSong = currEasyDifficulty ?
			songCollections[currCollectionIndex].songSets[currSongSetIndex].easy :
			songCollections[currCollectionIndex].songSets[currSongSetIndex].hard ;
	
		//update combo & perfection
		//
	}

	public void SongBoardGoButtonClicked()
	{
		characterBoard.SetActive(true);
	}

	//song info board
	public void SongInfoBoardBackButtonClicked()
	{
		if (animating) return;

		//pop up collapse animation
		SongInfoCollapse();
	}

	void ConfigureSongInfoBoard()
	{
		songInfoTitleText.text = currSong.songTitle;
		songInfoComposerText.text = currSong.composer;
		songInfoArrangerText.text = currSong.arranger;
	}

	//character board
	public void CharacterImageClicked(int characterIndex)
	{
		//configure song info messenger
		SongInfoMessenger.Instance.characterIndex = characterIndex;
		SongInfoMessenger.Instance.currentSong = currSong;

		//show loading screen
		//

		//load next level
		SceneManager.LoadSceneAsync("Record Beat");
	}

	public void CharacterBoardBackButtonClicked()
	{
		characterBoard.SetActive(false);
	}

	//animation
	void FlipToEraFromSong()
	{
		animating = true;
		StartCoroutine(flipOutAnimation.AnimationCoroutine(
			currSongBoard.rectTransform,
			Vector3.zero,
			verticalFlip,
			FlipAnimationDuration,
			currSongBoard.rectTransform.pivot,
			bottomEdgePivot
		));
		StartCoroutine(flipInAnimation.AnimationCoroutine(
			eraBoardTransform,
			verticalFlip,
			Vector3.zero,
			FlipAnimationDuration,
			eraBoardTransform.pivot,
			topEdgePivot
		));
		Invoke("RestoreAnimating", FlipAnimationDuration);
	}

	void FlipToSongFromEra()
	{
		animating = true;
		StartCoroutine(flipOutAnimation.AnimationCoroutine(
			eraBoardTransform,
			Vector3.zero,
			verticalFlip,
			FlipAnimationDuration,
			eraBoardTransform.pivot,
			topEdgePivot
		));
		StartCoroutine(flipInAnimation.AnimationCoroutine(
			currSongBoard.rectTransform,
			verticalFlip,
			Vector3.zero,
			FlipAnimationDuration,
			currSongBoard.rectTransform.pivot,
			bottomEdgePivot
		));
		Invoke("RestoreAnimating", FlipAnimationDuration);
	}

	void FlipToRightSong()
	{
		animating = true;
		StartCoroutine(flipOutAnimation.AnimationCoroutine(
			altSongBoard.rectTransform,
			Vector3.zero,
			horizontalFlip,
			FlipAnimationDuration,
			altSongBoard.rectTransform.pivot,
			leftEdgePivot
		));
		StartCoroutine(flipInAnimation.AnimationCoroutine(
			currSongBoard.rectTransform,
			horizontalFlip,
			Vector3.zero,
			FlipAnimationDuration,
			currSongBoard.rectTransform.pivot,
			rightEdgePivot
		));
		Invoke("RestoreAnimating", FlipAnimationDuration);
	}

	void FlipToLeftSong()
	{
		animating = true;
		StartCoroutine(flipOutAnimation.AnimationCoroutine(
			altSongBoard.rectTransform,
			Vector3.zero,
			horizontalFlip,
			FlipAnimationDuration,
			altSongBoard.rectTransform.pivot,
			rightEdgePivot
		));
		StartCoroutine(flipInAnimation.AnimationCoroutine(
			currSongBoard.rectTransform,
			horizontalFlip,
			Vector3.zero,
			FlipAnimationDuration,
			currSongBoard.rectTransform.pivot,
			leftEdgePivot
		));
		Invoke("RestoreAnimating", FlipAnimationDuration);
	}

	void SongInfoPopUp()
	{
		animating = true;

		//flip
		StartCoroutine(flipInAnimation.AnimationCoroutine(
			songInfoPopUp,
			foldFlip,
			Vector3.zero,
			FlipAnimationDuration,
			songInfoPopUp.pivot,
			centerPivot
		));

		Invoke("RestoreAnimating", FlipAnimationDuration);
	}

	void SongInfoCollapse()
	{
		animating = true;

		//flip
		StartCoroutine(flipOutAnimation.AnimationCoroutine(
			songInfoPopUp,
			Vector3.zero,
			foldFlip,
			FlipAnimationDuration,
			songInfoPopUp.pivot,
			centerPivot
		));

		Invoke("RestoreAnimating", FlipAnimationDuration);
	}

	void RestoreAnimating()
	{
		animating = false;
	}
}
