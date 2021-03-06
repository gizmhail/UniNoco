﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using EasyCaching;

namespace Noco
{
	[Serializable]
	public class AccessTokenDescriptor {
		public string access_token = null;
		public int expires_in = 0;
		public string token_type = null;
		public string scope = null;
		public string refresh_token = null;

		public override string ToString(){
			return JsonUtility.ToJson(this);
		}

		public string authorizationString(){
			return String.Format ("{0} {1}", this.token_type, this.access_token);
		}

		/// <summary>
		/// Save this token to user preference (for caching)
		/// </summary>
		public void Save() {
			if (this.access_token != null) {
				CachedString cache = new CachedString ("NocoAccessToken");
				cache.Save(this.ToString (), this.expires_in);
			}
		}
	}


	public class NocoOAuthAuthentificationRequest
	{
		/// Auth call raw result (usually not needed)
		public String result;
		/// OAuth redirection url containing OAuth code  (usually not needed)
		public String redirection = null;
		/// OAuth code in case of successfull login
		public String code = null;
		/// True if call is finished
		public bool callFinished;

		private const string handshakeCacheTimeoutKey = "MONO_TLS_SESSION_CACHE_TIMEOUT";

		private RemoteCertificateValidationCallback defaultCertificateValidationCallback;
		private string handshakeCacheTimeoutValue;

		// See http://answers.unity3d.com/questions/792342/how-to-validate-ssl-certificates-when-using-httpwe.html
		private static bool RemoteCertificateValidationCallback(
			object sender,
			X509Certificate certificate,
			X509Chain chain,
			SslPolicyErrors sslPolicyErrors)
		{
			Debug.Log ("[Warning] Using unsafe certification");
			return true;
		}

		private static X509Certificate userCertificateSelectionCallback(
			object sender, 
			string targetHost, 
			X509CertificateCollection localCertificates, 
			X509Certificate remoteCertificate, 
			string[] acceptableIssuers)
		{
			Debug.Log ("[Warning] Using unsafe certification.");
			return new X509Certificate();
		}

		private void PrepareNetwork()
		{
			// Sometimes, SslStream do not take into account its RemoteCertificateValidationCallback param
			// Hence this global setting to check if it correct those cases.
			defaultCertificateValidationCallback = ServicePointManager.ServerCertificateValidationCallback;
			ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

			// Problem with handshake cache can occurs. See https://bugzilla.xamarin.com/show_bug.cgi?id=19141
			handshakeCacheTimeoutValue = Environment.GetEnvironmentVariable (handshakeCacheTimeoutKey);
			Environment.SetEnvironmentVariable (handshakeCacheTimeoutKey, "0");
		}

		private void RestoreNetworkNormalState()
		{
			ServicePointManager.ServerCertificateValidationCallback = defaultCertificateValidationCallback;
			Environment.SetEnvironmentVariable (handshakeCacheTimeoutKey, handshakeCacheTimeoutValue);
		}

		public IEnumerator Launch(String username, String password, String appName) {
			callFinished = false;
			String host = Noco.Configuration.baseUrl;
			String authorizationUrl = "/1.1/OAuth2/authorize.php?response_type=code&client_id="+appName+"&state=STATE";
			String body = "username=" + WWW.EscapeURL(username) + "&password=" + WWW.EscapeURL(password) + "&login=1";
			int contentLength = System.Text.Encoding.UTF8.GetBytes(body).Length;

			PrepareNetwork ();

			TcpClient client = null;
			NetworkStream networkStream = null;
			SslStream sslStream = null;

			client = new TcpClient(host, 443);
			networkStream = client.GetStream();
			sslStream = new SslStream(networkStream,false);
			sslStream.AuthenticateAsClient (host);//, new X509Certificate2Collection (), SslProtocols.Tls, true);

			while (!sslStream.IsAuthenticated) {
				yield return null;
			}
			StreamReader reader = new StreamReader(sslStream);
			StreamWriter writer = new  StreamWriter(sslStream);

			String requestData = "POST " + authorizationUrl + " HTTP/1.1\r\n";
			requestData = requestData + "Host: " + host + "\r\n";
			requestData = requestData + "User-Agent: UniNoco\r\n";
			requestData = requestData + "Accept: */*\r\n";
			requestData = requestData + "Content-Length: " + contentLength + "\r\n";
			requestData = requestData + "Content-Type: application/x-www-form-urlencoded\r\n";
			requestData = requestData + "\r\n";
			requestData = requestData + body + "\r\n";
			requestData = requestData + "\r\n";
			requestData = requestData + "\r\n";
			writer.Write (requestData);
			writer.Flush ();
			result = "";
			String lastLine = null;
			while (true) {
				if (!client.Connected) {
					break;
				}

				if (sslStream.CanRead) {
					string line = reader.ReadLine ();
					if (line != null) {
						String redirectionHeader = "Location: ";
						if(line.StartsWith(redirectionHeader)){
							this.redirection = line.Substring (redirectionHeader.Length, line.Length - redirectionHeader.Length);
							string[] redirectionParts = this.redirection.Split ('?');
							if (redirectionParts.Length >= 2) {
								string query = redirectionParts [1];
								string[] paramsString = query.Split ('&');
								foreach(string paramString in paramsString) {
									string[] paramValues = paramString.Split ('=');
									if(paramValues.Length >= 2 && paramValues[0] == "code"){
										this.code = paramValues [1];
									}
								}

							}
						}
						if (result != "") {
							result = result + "\n";
						}

						if (lastLine == "" && line == "0") {
							break;
						}
						lastLine = line;
						result = result + line;
					} else {
						break;
					}
				}
				yield return null;
			}

			sslStream.Close();
			networkStream.Close ();
			client.Close();
			callFinished = true;
			RestoreNetworkNormalState();
		}
	}

	public class NocoOAuthAccessTokenRequest {
		public AccessTokenDescriptor oauthAccessToken = null;
		public String clientId = null;
		public String clientSecret = null;
		public string result = null;

		public NocoOAuthAccessTokenRequest (String clientId, String clientSecret) {
			this.clientId = clientId;
			this.clientSecret = clientSecret;
		}

		public IEnumerator FetchAccessTokenFromRefreshToken(string refreshToken){
			yield return FetchAccessToken (grantType: "refresh_token", value: refreshToken);
		}

		public IEnumerator FetchAccessTokenFromCode(string code) {
			yield return FetchAccessToken (grantType: "authorization_code", value: code);
		}

		public IEnumerator FetchAccessToken(string grantType, string value) {

			string authenticationStr = this.clientId + ":" + this.clientSecret;
			string urlStr = "https://api.noco.tv/1.1/OAuth2/token.php";

			var authenticationBytes = System.Text.Encoding.UTF8.GetBytes(authenticationStr);
			string authenticationB64 = System.Convert.ToBase64String(authenticationBytes);

			WWWForm form = new WWWForm();
			form.AddField("grant_type", grantType);
			if (grantType == "authorization_code") {
				form.AddField("code", value);
			} else {
				form.AddField(grantType, value);
			}
			Dictionary<string, string> headers = form.headers;
			headers ["Authorization"] = "Basic " + authenticationB64;
			byte[] rawData = form.data;
			WWW request = new WWW (urlStr, rawData, headers);
			yield return request;
			result = request.text;
			this.oauthAccessToken = JsonUtility.FromJson <AccessTokenDescriptor>(result);
		}
	}
}

