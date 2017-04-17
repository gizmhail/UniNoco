using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using EasyCaching;

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
			int cacheValidity = RequestKindCacheValidity ();
			CachedString cache = new CachedString (this.queryStr);
			if(cacheValidity > 0 && cache.IsValid() ){
				Debug.Log ("*** Using cache result for "+queryStr+" ; remaining validity: "+cache.RemainingValidity());
				this.result = cache.GetString ();
			} else {
				string host = Noco.Configuration.baseUrl;
				string urlStr = String.Format ("https://{0}/1.1{1}", host, this.queryStr);
				UnityWebRequest request = null;
				request = new UnityWebRequest ();
				request.url = urlStr;
				request.downloadHandler = new DownloadHandlerBuffer ();
				request.SetRequestHeader ("Authorization", "Basic " + this.api.oauthAccessToken.authorizationString() );
				Debug.Log ("--- Fetching: "+request.url);
				yield return request.Send ();
				this.result = request.downloadHandler.text;
				if (this.result != null && cacheValidity > 0 && this.result.IndexOf("\"error\"") == -1 ) {
					cache.Save (this.result, cacheValidity);
				}
			}
			if (this.result != null) {
				Parse ();
			}
		}

		// Should be overriden in subclasses
		virtual protected void Parse(){
			
		}

		// Should be overriden in subclasses
		virtual protected int RequestKindCacheValidity(){
			return 0;
		}
	}
}

