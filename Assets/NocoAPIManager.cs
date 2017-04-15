using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Noco;

public class NocoAPIManager : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		StartCoroutine ( Authenticate() );
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator Authenticate(){
		String clientId = System.Environment.GetEnvironmentVariable ("NOCO_CLIENTID");
		String clientsecret = System.Environment.GetEnvironmentVariable ("NOCO_CLIENTSECRET");
		NocoAPI api = new NocoAPI (clientId: clientId, clientsecret: clientsecret);
		String username = System.Environment.GetEnvironmentVariable ("NOCO_USER");
		String password = System.Environment.GetEnvironmentVariable ("NOCO_PASSWORD");
		if (!api.IsAuthenticated ()) {
			Debug.Log ("Loging in ...");
			yield return api.Authenticate (username, password);
		} else {
			Debug.Log ("Already logged in");
		}
		if (api.IsAuthenticated() ) {
			Debug.Log ("oauthCode: " + api.oauthCode);
			Debug.Log ("oauthAccessTokenInfo: " + api.oauthAccessToken);
		} else {
			Debug.Log ("Authentification failed");
		}			
	}
}
