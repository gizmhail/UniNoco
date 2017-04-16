using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace Noco
{
	public class NocoRequest
	{
		protected NocoAPI api = null;
		protected string queryStr = null;
		public string result = null;

		public NocoRequest(NocoAPI api) {
			this.api = api;
		}

		public IEnumerator Launch(){
			string host = Noco.Configuration.baseUrl;
			string urlStr = String.Format ("https://{0}/1.1{1}", host, this.queryStr);
			///*
			UnityWebRequest request = null;
			request = new UnityWebRequest ();
			request.url = urlStr;
			request.downloadHandler = new DownloadHandlerBuffer ();
			request.SetRequestHeader ("Authorization", "Basic " + this.api.oauthAccessToken.authorizationString() );
			Debug.Log (request.url);
			yield return request.Send ();
			this.result = request.downloadHandler.text;
			if (this.result != null) {
				Parse ();
			}
		}

		// Should be overriden in subclasses
		virtual protected void Parse(){
			
		}
	}
}

