using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainController : MonoBehaviour
{

	public GameObject WelcomePanel;
	public GameObject MainPanel;

	public Text ButtonTextA;
	public Text ButtonTextB;
	public Text ButtonTextC;

	// Use this for initialization
	void Start ()
	{

		WelcomePanel.SetActive (true);
		MainPanel.SetActive (false);

		if (PlayerPrefs.GetString ("textA") == "") {
			PlayerPrefs.SetString ("textA", "Like");
		}
		if (PlayerPrefs.GetString ("textB") == "") {
			PlayerPrefs.SetString ("textB", "Ya Know");
		}
		if (PlayerPrefs.GetString ("textC") == "") {
			PlayerPrefs.SetString ("textC", "I Mean");
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		ButtonTextA.text = PlayerPrefs.GetString ("textA") + "\n" + PlayerPrefs.GetInt ("countA").ToString ();
		ButtonTextB.text = PlayerPrefs.GetString ("textB") + "\n" + PlayerPrefs.GetInt ("countB").ToString ();
		ButtonTextC.text = PlayerPrefs.GetString ("textC") + "\n" + PlayerPrefs.GetInt ("countC").ToString ();

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

	public void IncrementCountA ()
	{
		int newCount;
		newCount = PlayerPrefs.GetInt ("countA") + 1;
		PlayerPrefs.SetInt ("countA", newCount);

	}
	public void IncrementCountB ()
	{
		int newCount;
		newCount = PlayerPrefs.GetInt ("countB") + 1;
		PlayerPrefs.SetInt ("countB", newCount);
		
	}
	public void IncrementCountC ()
	{
		int newCount;
		newCount = PlayerPrefs.GetInt ("countC") + 1;
		PlayerPrefs.SetInt ("countC", newCount);
		
	}
}
