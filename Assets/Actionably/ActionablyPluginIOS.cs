using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class ActionablyPluginIOS
{
	[DllImport("__Internal")]
	public static extern int getContactStatus ();

	[DllImport("__Internal")]
	public static extern void listContacts (string objectNameCallback, string functionNameCallback, 
	                                        string errFunctionNameCallback);

	[DllImport("__Internal")]
	public static extern void sendSms (string objectNameCallback, string functionNameCallback, 
	                                   string phoneNumber, string message, string file);

	[DllImport("__Internal")]
	public static extern string getWebViewUserAgent ();
}
 