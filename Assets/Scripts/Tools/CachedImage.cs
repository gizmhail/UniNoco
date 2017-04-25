using System;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace EasyCaching
{
	public class CachedWWW
	{
		string url;
		string cacheId;
		long maxAge;
		public WWW www;
		public bool debug = false;

		public CachedWWW (string url, long maxAge = 3600*24, string cacheId = null)
		{
			this.url = url;
			this.cacheId = cacheId;
			this.maxAge = maxAge;
			if (this.cacheId == null) 
			{
				this.cacheId = String.Format("%l", this.url.GetHashCode() );
			}
		}

		public bool IsCached()
		{
			string filePath = CacheFilePath ();
			return System.IO.File.Exists (filePath);
		}

		string CacheFilePath()
		{
			string filePath = Application.persistentDataPath;
			filePath += "/" + this.cacheId;
			return filePath;	
		}

		// Source: lassiwa, http://answers.unity3d.com/questions/436730/can-i-access-previously-downloaded-textures.html
		public IEnumerator Load()
		{
			string filePath = CacheFilePath ();
			bool web = false;
			bool useCached = false;
			useCached = System.IO.File.Exists(filePath);
			if (useCached)
			{
				//check how old
				System.DateTime written = File.GetLastWriteTimeUtc(filePath);
				System.DateTime now = System.DateTime.UtcNow;
				double totalHours = now.Subtract(written).TotalHours;
				if (totalHours > this.maxAge)
					useCached = false;
			}
			if (useCached)
			{
				string pathforwww = "file://" + filePath;
				this.www = new WWW(pathforwww);
			}
			else
			{
				web = true;
				this.www = new WWW(url);
			}
			yield return this.www;

			if (www.error == null)
			{
				if (web)
				{
					if(this.debug) Debug.Log("[CachedWWW] Saving  " + this.www.url + " to " + filePath);
					File.WriteAllBytes(filePath, www.bytes);
				}
				else
				{
					if(this.debug) Debug.Log("[CachedWWW] Cached result found for " + this.www.url);
				}
			}
			else
			{
				if (!web)
				{
					File.Delete(filePath);
				}
				Debug.Log("[CachedWWW] Web request error: " + this.www.error);
			}
		}
	}
}

