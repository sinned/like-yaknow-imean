using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Text;
using System.IO;  
using SimpleJSON;
using uPromise;

public class Actionably : Singleton<Actionably>
{

	public static void Init (string apiKey, string callbackObjectName)
	{
		if (apiKey == "your-test-id-here") {

			Debug.LogError ("*** ERROR ERROR ERROR *************************************\n Please use your own Actionably API Key. *** ");
			Debug.LogError ("Substituting Actionably Test ID for now. If you launch with this ID you will be eaten by a grue.");

			apiKey = "5511be18efd37d70072f3a95";

		}
		Instance.InitInternal (apiKey, callbackObjectName);
	}

	public static void OpenMessages ()
	{
		Instance.OpenMessagesInternal ();
	}

	private void InitInternal (string apiKey, string callbackObjectName)
	{
		_apiKey = apiKey;
		_callbackObjectName = callbackObjectName;
	}

	private static string BASE_URL = "http://www.actionably.com";

	private String _apiKey;
	private String _callbackObjectName;
	private Deferred<String> _pendingSmsDeferred;
	private float _lastCountCheckTime = 0;
	private bool _tokenSent = false;
	private bool _contactsLoaded = false;
	private bool _webViewLoaded = false;
	private UniWebView _webView = null;
	private int _messageCount = -1;
	private Behaviour[] _disabledCanvasList;
	private Behaviour[] _disabledCameras;

	void Start ()
	{
		//TODO: when should we should do this.
		UnityEngine.iOS.NotificationServices.RegisterForNotifications (UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound);

		_webView = gameObject.AddComponent<UniWebView> ();
		_webView.insets = new UniWebViewEdgeInsets (0, 0, 0, 0);
		_webView.OnReceivedMessage += OnWebReceivedMessage;
		_webView.OnLoadComplete += OnLoadComplete;
	}

	void  FixedUpdate ()
	{
		if (!_tokenSent) {
			byte[] token = UnityEngine.iOS.NotificationServices.deviceToken;
			if (token != null) {
				string tokenString = System.BitConverter.ToString (token).Replace ("-", "").ToLower ();
				SavePushToken (tokenString);
				_tokenSent = true;
			}
		}
		// check every 10 seconds
		if (_lastCountCheckTime == 0 || Time.time - _lastCountCheckTime > 10.0) {
			RefreshShareCount ();
		}
	}
	
	void OnApplicationFocus (bool status)
	{
		if (status) {
			RefreshShareCount ();
		}
	}

	void OnLoadComplete (UniWebView webView, bool success, string errorMessage)
	{
		if (!success) {
			CloseWebView (errorMessage);
		}
	}

	void RequestContacts ()
	{
		ActionablyPlugin.listContacts (gameObject.name, "SendContactsToJavascript", "ContactsError");
	}

	void ContactsError (string message)
	{
		CloseWebView (message);
	}

	void SendContactsToJavascript (string contactsFile)
	{
		string contacts = LoadFile (contactsFile);
		contacts = contacts.Replace ("\n", ""); 
		contacts = contacts.Replace ("'", "\\'"); 
		contacts = contacts.Replace ("\\\"", "\\\\\""); 
		var js = "saveContacts('" + contacts + "')";
		_webView.EvaluatingJavaScript (js);
	}

	void OnWebReceivedMessage (UniWebView webView, UniWebViewMessage message)
	{
		//Debug.Log (message.rawMessage);
		if (string.Equals (message.path, "sendSmsMessage")) {
			string imageUrl = null;
			if (message.args.ContainsKey ("imageUrl")) {
				imageUrl = message.args ["imageUrl"];
			}
			SendSmsMessage (message.args ["phoneNumber"], message.args ["message"],
			               imageUrl).Then ((string result) => {
				bool success = "sent".Equals (result);
				var js = "sendSmsMessageFinished('" + message.args ["phoneNumber"] + "',"
					+ success.ToString ().ToLower () + ")";
				_webView.EvaluatingJavaScript (js);
			});
		} else if (string.Equals (message.path, "close")) {
			string errorMessage = null;
			if (message.args.ContainsKey ("errorMessage")) {
				errorMessage = message.args ["errorMessage"];
			}
			CloseWebView (errorMessage);
		} else if (string.Equals (message.path, "requestContacts")) {
			RequestContacts ();
		} else if (string.Equals (message.path, "contactsLoaded")) {
			_contactsLoaded = true;
		} else if (string.Equals (message.path, "webViewLoaded")) {
			_webViewLoaded = true;
		} else if (string.Equals (message.path, "log")) {
			Debug.Log (message.args ["message"]);
		} else if (string.Equals (message.path, "awardPrizes")) {
			var count = int.Parse (message.args ["count"]);
			CallFunction ("OnPrizeAward", count);
		}
	}

	void CloseWebView (string errorMessage)
	{
		_webView.Hide ();
		SetEnabledAll (_disabledCanvasList, true);
		SetEnabledAll (_disabledCameras, true);
		_disabledCanvasList = null;
		_disabledCameras = null;
		if (errorMessage == null) {
			RefreshShareCount ();
		} else {
			CallFunction ("OnError", errorMessage);
		}
	}
	
	void RefreshShareCount ()
	{
		_lastCountCheckTime = Time.time;
		bool firstTime = false;
		if (PlayerPrefs.GetString ("ActionablyFirstTimeEverPlayed") != "false") {
			PlayerPrefs.SetString ("ActionablyFirstTimeEverPlayed", "false");
			firstTime = true;
		}
		FindShareCount (firstTime)
			.Then ((string jsonString) => {
			var data = JSON.Parse (jsonString);
			var count = data ["count"].AsInt;
			if (_messageCount == -1 || count != _messageCount) {
				_messageCount = count;
				CallFunction ("OnMessageCountChange", count);
			}
		});
	}
	
	public Promise<string> SendSmsMessage (string phoneNumber, string message, string imageUrl)
	{
		_pendingSmsDeferred = new Deferred<String> ();
		if (imageUrl == null) {
			ActionablyPlugin.sendSms (gameObject.name, "SmsSendFinished", phoneNumber, message, null);
		}
		ImageFetch (imageUrl).Then ((string imageFile) => {
			ActionablyPlugin.sendSms (gameObject.name, "SmsSendFinished", phoneNumber, message, imageFile);
		});
		return _pendingSmsDeferred.Promise;
	}

	public void SmsSendFinished (string status)
	{
		if (_pendingSmsDeferred != null) {
			_pendingSmsDeferred.Resolve (status);
			_pendingSmsDeferred = null;
		}
	}


	private void SetEnabledAll (Behaviour[] behaviours, bool value)
	{
		for (var i = 0; i< behaviours.Length; i++) {
			Behaviour behaviour = behaviours [i];
			behaviour.enabled = value;
		}
	}

	public void OpenMessagesInternal ()
	{
		_disabledCanvasList = FindObjectsOfType<Canvas> () as Behaviour[];
		SetEnabledAll (_disabledCanvasList, false);
		_disabledCameras = Camera.allCameras;
		SetEnabledAll (_disabledCameras, false);

		if (_contactsLoaded || _webViewLoaded) {
			_webView.Show ();
			var js = "refreshContacts('" + ActionablyPlugin.getContactStatus () + "')";
			_webView.EvaluatingJavaScript (js);
		} else {
			_webView.url = BuildUrlWithContactPermissionAndPlatform ("mobile/#!/share");
			_webView.Show ();
			_webView.Load ();
		}
	}
	
	private Promise<string> FindShareCount (bool firstTime)
	{
		string urlFragement = "shares/findShareCount";
		Deferred<string> deferred = new Deferred<string> (); 
		string wvUserAgent = ActionablyPlugin.getWebViewUserAgent ();
		JSONClass data = new JSONClass ();
		data ["wvUserAgent"] = wvUserAgent;
		if (firstTime) {
			data ["firstTime"] = firstTime.ToString ();
		}
		JsonPost (urlFragement, data.ToString ())
			.Then (jsonString => {
			deferred.Resolve (jsonString);
		});
		return deferred.Promise;
	}
	
	private Promise SavePushToken (string pushToken)
	{
		JSONClass data = new JSONClass ();
		data ["pushToken"] = pushToken;
		return JsonPost ("savePushToken", data.ToString ());
	}
	
	private Promise<string> ImageFetch (string url)
	{
		Deferred<string> deferred = new Deferred<string> ();
		WWW www = new WWW (url);
		StartCoroutine (imageFetchReturn (www, deferred));
		return deferred.Promise;	
	}
	
	private IEnumerator imageFetchReturn (WWW www, Deferred<string> deferred)
	{
		yield return www;
		
		// check for errors
		if (www.error == null) {
			String file = Application.persistentDataPath + "/image.jpg";
			File.WriteAllBytes (file, www.bytes);
			deferred.Resolve (file);
		} else {
			deferred.Reject (www.error);
		}    
	}    
	
	private string GetBaseUrl ()
	{
		if (PlayerPrefs.GetString ("ActionablyServerUrl") == "") {
			return BASE_URL;
		} else {
			return PlayerPrefs.GetString ("ActionablyServerUrl");
		}
	}
	
	private string BuildUrl (string urlFragment)
	{
		return GetBaseUrl () + "/" + urlFragment + "/" + _apiKey + "/" + SystemInfo.deviceUniqueIdentifier;
	}
	
	private string BuildUrlWithContactPermissionAndPlatform (string urlFragment)
	{
		return BuildUrl (urlFragment) + "/" + ActionablyPlugin.getContactStatus () + "/" + Application.platform;
	}
	
	private Promise<string> JsonPost (string urlFragment, string data)
	{
		string url = BuildUrl (urlFragment);
		Dictionary<string,string> headers = new Dictionary<string,string> ();
		headers.Add ("Content-Type", "application/json");
		byte[] pData = Encoding.ASCII.GetBytes (data.ToCharArray ());
		//Debug.Log ("url = " + url);
		Deferred<string> deferred = new Deferred<string> ();
		WWW www = new WWW (url, pData, headers);
		StartCoroutine (JsonPostReturn (www, deferred));
		return deferred.Promise;	
	}
	
	private IEnumerator JsonPostReturn (WWW www, Deferred<string> deferred)
	{
		yield return www;
		
		// check for errors
		if (www.error == null) {
			//Debug.Log ("got response " + www.text);
			deferred.Resolve (www.text);
		} else {
			deferred.Reject (www.error);
		}    
	}    

	private void CallFunction (string functionName, object message)
	{
		GameObject callback = GameObject.Find (_callbackObjectName);
		if (callback == null) {
			Debug.LogError ("Actionably Error Object: '" + _callbackObjectName + "' does not exist");
			return;
		}
		callback.SendMessage (functionName, message);
	}

	private string LoadFile (string fileName)
	{
		// Handle any problems that might arise when reading the text
		try {
			
			string json = "";
			
			string line;
			// Create a new StreamReader, tell it which file to read and what encoding the file
			// was saved as
			StreamReader theReader = new StreamReader (fileName, Encoding.Default);
			
			// Immediately clean up the reader after this block of code is done.
			// You generally use the "using" statement for potentially memory-intensive objects
			// instead of relying on garbage collection.
			// (Do not confuse this with the using directive for namespace at the 
			// beginning of a class!)
			using (theReader) {
				// While there's lines left in the text file, do this:
				do {
					line = theReader.ReadLine ();
					
					if (line != null) {
						json = json + line + "\n";
					}
				} while (line != null);
				
				// Done reading, close the reader and return true to broadcast success    
				theReader.Close ();
				return (json);
			}
		}
		
		// If anything broke in the try block, we throw an exception with information
		// on what didn't work
		catch (Exception e) {
			Debug.Log ("Unabled to load file: " + fileName + " " + e);
			return null;
		}
	}
}
