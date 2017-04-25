using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noco
{
	public class NocoMediasRequest:NocoRequest
	{
		public NocoMediasRequest(NocoAPI api, int showId) : base(api){
			this.queryStr = String.Format("/shows/{0}/medias", showId);
		}

		override protected void Parse(){
		}
	}

	///shows/%i/video/%@/%@", show.id_show,qualityKey,audioLang
	public class NocoVideoRequest:NocoRequest
	{
		public NocoVideo video = null;

		public NocoVideoRequest(NocoAPI api, int showId, string qualityKey = "HQ"/*LQ*/, string audioLang = "fr") : base(api){
			this.queryStr = String.Format("/shows/{0}/video/{1}/{2}", showId, qualityKey, audioLang);
		}	

		override protected void Parse(){
			this.video = JsonUtility.FromJson <NocoVideo> (this.result);
		}
	}

	[Serializable]
	public class NocoVideo
	{
		// Video url
		public string file;
		public int code_reponse;
		public int cross_access;
		public int guest_free;
		public int is_abo;
		public string quality_key;
		public int quotafr_free;
		public int user_free;
	}
}

