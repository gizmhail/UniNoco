using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using System.Net.Sockets;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using EasyCaching;

namespace Noco
{
	public static class Configuration
	{
		public const string baseUrl = "api.noco.tv";
	}

	public class NocoAPI
	{

		public bool debugAuthentification = false;

		// Credentials
		public String clientId = null;
		public String clientSecret = null;
		public String redirectUri = null;
		// Cached 
		public String oauthCode = null;
		public AccessTokenDescriptor oauthAccessToken = null;

		public NocoAPI (String clientId, String clientsecret, String redirectUri = null)
		{
			this.clientId = clientId;
			this.clientSecret = clientsecret;
			this.redirectUri = redirectUri;
			if(clientId == "" || clientId == null || clientSecret == "" || clientSecret == null){
				Debug.Log ("[NocoAPI] Missing credentials");
			}
		}

		public IEnumerator LoadArchivedAccessToken()
		{
			CachedString cache = new CachedString ("NocoAccessToken");
			string tokenDescription = cache.GetString ();
			if (tokenDescription != null) {
				AccessTokenDescriptor previousTokenDescriptor = JsonUtility.FromJson<AccessTokenDescriptor> (tokenDescription);
				if (cache.IsValid()) {
					if (debugAuthentification) {
						Debug.Log ("Access token still valid for " + cache.RemainingValidity () + " seconds");
					}
					this.oauthAccessToken = previousTokenDescriptor;
				} else if(previousTokenDescriptor != null ) {
					if (debugAuthentification) {
						Debug.Log ("Trying to use refresh token after access token lapsing " + tokenDescription);
					}
					NocoOAuthAccessTokenRequest request = new NocoOAuthAccessTokenRequest (this.clientId, this.clientSecret);
					yield return request.FetchAccessTokenFromRefreshToken (previousTokenDescriptor.refresh_token);
					if (debugAuthentification) {
						Debug.Log ("Refresh token call result:" + request.result);
					}
					this.oauthAccessToken = request.oauthAccessToken;
					if (IsAuthenticated()) {
						if (debugAuthentification) {
							Debug.Log ("Refresh token succed: new access token found: " + this.oauthAccessToken);
						}
						this.oauthAccessToken.Save ();
					}
				}
			}
		}

		private void loadCachedOAuthInfo()
		{
			this.oauthAccessToken = null;
		}

		public bool IsAuthenticated(){
			//TODO Add expiration date check
			return this.oauthAccessToken != null && this.oauthAccessToken.access_token != null;
		}

		public IEnumerator Authenticate(String username, String password){
			if (username == null || username == "" || password == null || username == "") {
				Debug.Log ("Missing credentials");
				yield return null;
			} else {
				// Authentification
				if(debugAuthentification) Debug.Log("Noco Authentification...");
				NocoOAuthAuthentificationRequest authentificationRequest = new NocoOAuthAuthentificationRequest ();
				yield return authentificationRequest.Launch (username, password, this.clientId);
				if(debugAuthentification) Debug.Log("Authentifiction call finished");
				this.oauthCode = authentificationRequest.code;	

				if (this.oauthCode != null) {
					NocoOAuthAccessTokenRequest request = new NocoOAuthAccessTokenRequest(this.clientId, this.clientSecret);
					yield return request.FetchAccessTokenFromCode (this.oauthCode);
					this.oauthAccessToken = request.oauthAccessToken;
					if (IsAuthenticated()) {
						this.oauthAccessToken.Save ();
					}
				} else {
					Debug.Log("Authentifiction failed");
				}
			}
		}
	}
}

