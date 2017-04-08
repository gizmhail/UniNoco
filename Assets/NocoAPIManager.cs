using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Noco;

public class NocoAPIManager : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		StartCoroutine ( authenticate() );
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator authenticate(){
		NocoOAuthAuthentificationRequest request = new NocoOAuthAuthentificationRequest ();
		String username = System.Environment.GetEnvironmentVariable ("NOCO_USER");
		String password = System.Environment.GetEnvironmentVariable ("NOCO_PASSWORD");
		String redirectionUri = "http://nolife.poivre.name/oauth/";
		yield return request.Launch(username, password, "NoLibTV");

		if (request.redirection != null) {
			Debug.Log ("Redirection: " + request.redirection);
			String code = request.redirection.Replace (redirectionUri + "?code=", "");
			code = code.Replace ("&state=STATE", "");
			Debug.Log ("Code: " + code);
			Debug.Log ("Full answer: " + request.result);
		} else {
			Debug.Log ("Authentication failed");
		}

	}
}
