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
		NocoAPI api = new NocoAPI (clientId: "", clientsecret: "");
		String username = System.Environment.GetEnvironmentVariable ("NOCO_USER");
		String password = System.Environment.GetEnvironmentVariable ("NOCO_PASSWORD");
		yield return api.Authenticate(username, password, "NoLibTV", authSucess => {
			if (authSucess) {
				Debug.Log ("oauthCode: " + api.oauthCode);
			} else {
				Debug.Log ("Authentication failed");
			}			
		});
	}
}
