using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class ActionablyPlugin
{
	public enum ContactStatus
	{
		NotDetermined,
		NotAllowed,
		Allowed 
	}

	public static ContactStatus getContactStatus ()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			int value = ActionablyPluginIOS.getContactStatus ();
			if (value == 0) {
				return ContactStatus.NotDetermined;
			} else if (value == 1) {
				return ContactStatus.NotAllowed;
			} else {
				return ContactStatus.Allowed;
			}
		} else {
			return ContactStatus.Allowed;
		}
	}
	
	public static void listContacts (string objectNameCallback, string functionNameCallback, 
	                                 string errFunctionNameCallback)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			ActionablyPluginIOS.listContacts (objectNameCallback, functionNameCallback, errFunctionNameCallback);
		} else {
			String file = System.IO.Path.Combine (Application.streamingAssetsPath, "contacts.json");
			Actionably.Instance.SendMessage (functionNameCallback, file);
		}
	}
	
	public static void sendSms (string objectNameCallback, string functionNameCallback, 
	                                   string phoneNumber, string message, string file)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			ActionablyPluginIOS.sendSms (objectNameCallback, functionNameCallback, phoneNumber, message, file);
		} else {
			Actionably.Instance.SendMessage (functionNameCallback, "sent");
		}		
	}
	                              
	public static string getWebViewUserAgent ()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			return ActionablyPluginIOS.getWebViewUserAgent ();
		} else {
			// chrome desktop user agent.
			return "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.90 Safari/537.36";
		}		
	}
}
 