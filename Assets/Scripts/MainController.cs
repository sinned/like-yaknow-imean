using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainController : Singleton<MainController>
{

	public GameObject WelcomePanel;
	public GameObject MainPanel;
	public GameObject ConfigPanel;

	public Text ButtonTextA;
	public Text ButtonTextB;
	public Text ButtonTextC;

	// sounds
	public AudioSource Ding;
	public Text SoundButtonText;

	/* Actionably Callbacks */
	// actionably callback function when the number of messages changes.
	public void OnMessageCountChange (int count)
	{
		Debug.Log ("OnMessageCountChange: " + count);
	}
	
	// actionably callback function when gifts are accepted or other prizes awarded.
	public void OnPrizeAward (int count)
	{
		Debug.Log ("OnPrizeAward: " + count);
	}
	
	public void OnError (string message)
	{
		Debug.LogError ("Actionably Error: " + message);
	}

	// Use this for initialization
	void Start ()
	{

		//Debug.Log ("Sound: " + PlayerPrefs.GetInt ("sound").ToString ());

		WelcomePanel.SetActive (true);
		MainPanel.SetActive (false);
		ConfigPanel.SetActive (false);

		if (PlayerPrefs.GetString ("textA") == "") {
			PlayerPrefs.SetString ("textA", "Like");
		}
		if (PlayerPrefs.GetString ("textB") == "") {
			PlayerPrefs.SetString ("textB", "Ya Know");
		}
		if (PlayerPrefs.GetString ("textC") == "") {
			PlayerPrefs.SetString ("textC", "I Mean");
		}

		Actionably.Init ("55771e9d5e50c00b00f570a9", "MainController");
	}
	
	// Update is called once per frame
	void Update ()
	{
		ButtonTextA.text = PlayerPrefs.GetString ("textA") + "\n" + PlayerPrefs.GetInt ("countA").ToString ();
		ButtonTextB.text = PlayerPrefs.GetString ("textB") + "\n" + PlayerPrefs.GetInt ("countB").ToString ();
		ButtonTextC.text = PlayerPrefs.GetString ("textC") + "\n" + PlayerPrefs.GetInt ("countC").ToString ();

		if (PlayerPrefs.GetInt ("sound") == 0) {
			SoundButtonText.text = "Turn Sound Off";
		} else {
			SoundButtonText.text = "Turn Sound On";
		}
	}

	public void ShowPanel (string panelName)
	{
		//Debug.Log ("Showing Panel: " + panelName);
		WelcomePanel.SetActive (false);
		MainPanel.SetActive (false);
		ConfigPanel.SetActive (false);
		
		if (panelName == WelcomePanel.name) {
			WelcomePanel.SetActive (true);
		}

		if (panelName == MainPanel.name) {
			MainPanel.SetActive (true);
		}

		if (panelName == ConfigPanel.name) {
			ConfigPanel.SetActive (true);
		}
	}

	public void StartCounts ()
	{
		ResetCounts ();
		WelcomePanel.SetActive (false);
		MainPanel.SetActive (true);
	}

	public void ResetCounts ()
	{
		PlayerPrefs.SetInt ("countA", 0);
		PlayerPrefs.SetInt ("countB", 0);
		PlayerPrefs.SetInt ("countC", 0);
	}

	public void IncrementCount (string countName)
	{
		int newCount;
		newCount = PlayerPrefs.GetInt ("count" + countName) + 1;
		PlayerPrefs.SetInt ("count" + countName, newCount);
		if (PlayerPrefs.GetInt ("sound") == 0) {
			Ding.Play ();
		}
	}


	public void InviteFriends ()
	{
		Actionably.OpenMessages ();

	}

	public void ToggleSound ()
	{
		// 0 means sound is ON
		if (PlayerPrefs.GetInt ("sound") == 0) {
			PlayerPrefs.SetInt ("sound", 1);
		} else {
			PlayerPrefs.SetInt ("sound", 0);
		}
	}
}
