using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Noco;

public class NocoAPIManager : MonoBehaviour {
	public VideoPlayer videoPLayer;
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
		String username = System.Environment.GetEnvironmentVariable ("NOCO_USER");
		String password = System.Environment.GetEnvironmentVariable ("NOCO_PASSWORD");

		NocoAPI api = new NocoAPI (clientId: clientId, clientsecret: clientsecret);
		bool forceAuthent = false;
		if (forceAuthent || !api.IsAuthenticated ()) {
			Debug.Log ("Loging in ...");
			yield return api.Authenticate (username, password);
		} else {
			Debug.Log ("Already logged in");
		}
		if (api.IsAuthenticated() ) {
			Debug.Log ("oauthAccessTokenInfo: " + api.oauthAccessToken);


			var request = new NocoShowsRequest (api);
			yield return request.Launch ();
			Debug.Log ("res: "+request.result);
			int i = 0;
			NocoShow latestShow = null;
			foreach(NocoShow show in request.showSet.shows){
				if (latestShow == null) {
					latestShow = show;
				}
				Debug.Log ("-------- res ["+i+"] -----------");
				Debug.Log ("Id: "+show.id_show);
				Debug.Log (show.Description1L());
				Debug.Log (show.show_resume);
				i++;
			}
			yield return new WaitForSeconds (1);

			if (latestShow != null) {
				var videoRequest = new NocoVideoRequest (api, latestShow.id_show);
				yield return videoRequest.Launch ();
				Debug.Log ("res: "+videoRequest.result);
				Debug.Log ("res: "+videoRequest.video.file);
				videoPLayer.url = videoRequest.video.file;
				videoPLayer.Play ();			
			}
		} else {
			Debug.Log ("Authentification failed");
		}			
	}
}
