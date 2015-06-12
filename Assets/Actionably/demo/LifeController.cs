using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LifeController : Singleton<LifeController>
{
	void Start ()
	{
		// initialize actionably.
		Actionably.Init ("your-test-id-here", "LifeController");

		InitializePlayerLives ();
	}

	// actionably callback function when the number of messages changes.
	public void OnMessageCountChange (int count)
	{
		SetNumMessages (count);
	}
	
	// actionably callback function when gifts are accepted or other prizes awarded.
	public void OnPrizeAward (int count)
	{
		SetNumLives (GetNumLives () + count);
	}

	public void OnError (string message)
	{
		Debug.LogError (message);
	}

	public void ShareButtonClicked ()
	{
		Actionably.OpenMessages ();
	}
	
	public void MessagesButtonClicked ()
	{
		Actionably.OpenMessages ();
	}
	
	public void LoseLifeButtonClicked ()
	{
		int lives = GetNumLives ();
		if (lives > 0) {
			SetNumLives (GetNumLives () - 1);
		}
	}
	
	private void SetNumMessages (int count)
	{
		_numMessagesText.text = count.ToString ();
	}

	private void SetNumLives (int lives)
	{
		PlayerPrefs.SetInt ("lives", lives);
		_numLivesText.text = lives.ToString ();
		if (GetNumLives () > 0) {
			_loseLifeButton.SetActive (true);
			_shareButton.SetActive (false);
			_shareText.enabled = false;
		} else {
			_loseLifeButton.SetActive (false);
			_shareButton.SetActive (true);
			_shareText.enabled = true;
		}
	}
	
	private int GetNumLives ()
	{
		return PlayerPrefs.GetInt ("lives");
	}

	private void InitializePlayerLives ()
	{
		// see if it's the first time we've run this
		if (PlayerPrefs.GetString ("firstTimeEverPlayed") != "nope") {
			SetNumLives (3);
			PlayerPrefs.SetString ("firstTimeEverPlayed", "nope");
		} else {
			SetNumLives (GetNumLives ());
		}
		if (Application.platform == RuntimePlatform.OSXEditor) {
			SetNumLives (3);
		}
	}

	void Update ()
	{
		// reset lives for testing.		
		if (Input.touchCount == 5) {
			SetNumLives (3);
		}
	}
	
	public Text _numLivesText;
	public Text _numMessagesText;
	public Text _shareText;
	
	public GameObject _shareButton;
	public GameObject _loseLifeButton;
	
}