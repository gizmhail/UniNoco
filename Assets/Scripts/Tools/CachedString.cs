using System;
using UnityEngine;
using System.Globalization;

namespace EasyCaching
{
	public static class CachedStringConstants
	{
		public const string expirationDateSuffix = "ExpirationTime";
	}

	public class CachedString
	{
		private string id;
		public CachedString (string id)
		{
			this.id = id;
		}

		public string GetValidString()
		{
			if (IsValid ()) {
				return GetString ();
			} else {
				return null;
			}
		}
		public string GetString()
		{
			return PlayerPrefs.GetString (id);
		}

		public double GetExpirationUTCUnixTime()
		{
			string expirationTimeStr = PlayerPrefs.GetString (id + CachedStringConstants.expirationDateSuffix);
			double expirationTime = 0;
			expirationTimeStr = expirationTimeStr.Replace (",", ".");
			Double.TryParse (expirationTimeStr, NumberStyles.Number,CultureInfo.InvariantCulture, out expirationTime);

			if (double.IsNaN(expirationTime) || double.IsInfinity(expirationTime)) {
				expirationTime = 0;
			}

			return expirationTime;
		}

		public double RemainingValidity(){
			double expirationTime = GetExpirationUTCUnixTime ();
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			double currentUnixTime = (System.DateTime.UtcNow - epoch).TotalSeconds;
			double remaining = expirationTime - currentUnixTime;
			if (remaining > 0) {
				return remaining;
			} else {
				return 0;
			}
		}

		public bool IsValid(){
			double expirationTime = GetExpirationUTCUnixTime ();
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			double currentUnixTime = (System.DateTime.UtcNow - epoch).TotalSeconds;
			if (expirationTime >= currentUnixTime && GetString() != null) {
				return true;
			}
			return false;
		}

		public void Save(string content, int cacheValidityInSeconds){
			DateTime expirationDate = System.DateTime.UtcNow + new TimeSpan (0, 0, cacheValidityInSeconds);
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			double expirationUnixTime = (expirationDate - epoch).TotalSeconds;
			string expirationUnixTimeStr = expirationUnixTime.ToString(null, CultureInfo.InvariantCulture);
			PlayerPrefs.SetString (this.id, content);
			PlayerPrefs.SetString (this.id + CachedStringConstants.expirationDateSuffix, expirationUnixTimeStr);
			PlayerPrefs.Save ();
		}
	}
}

