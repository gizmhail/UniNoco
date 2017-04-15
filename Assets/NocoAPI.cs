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

		public delegate void NocoAPIAuthentificationDelegate(bool authorizationSucceeded);
		public NocoAPIAuthentificationDelegate authentificationCompletionHandler;

		public NocoAPI (String clientId, String clientsecret, String redirectUri = null)
		{
			this.clientId = clientId;
			this.clientSecret = clientsecret;
			this.redirectUri = redirectUri;
		}

		private void loadCachedOAuthInfo()
		{
			this.oauthAccessToken = null;
		}

		public IEnumerator Authenticate(String username, String password, NocoAPIAuthentificationDelegate completionHandler = null){
			if (completionHandler != null) {
				this.authentificationCompletionHandler += completionHandler;
			}
			if (username == null || username == "" || password == null || username == "") {
				Debug.Log ("Missing credentials");
				yield return null;
				this.authentificationCompletionHandler (authorizationSucceeded: false);
			} else {
				// Authentification
				if(debugAuthentification) Debug.Log("Noco Authentification...");
				NocoOAuthAuthentificationRequest authentificationRequest = new NocoOAuthAuthentificationRequest ();
				yield return authentificationRequest.Launch (username, password, this.clientId);
				if(debugAuthentification) Debug.Log("Authentifiction call finished");
				this.oauthCode = authentificationRequest.code;	
				if (this.oauthCode != null) {
					yield return FetchAccessTokenFromCode ();
					if (this.authentificationCompletionHandler != null) {
						bool authSuccess = this.oauthAccessToken != null && this.oauthAccessToken.access_token != null;
						this.authentificationCompletionHandler (authorizationSucceeded: authSuccess);
					}
				} else {
					Debug.Log("Authentifiction failed");
				}

			}
		}

		public IEnumerator FetchAccessTokenFromCode() {
			string authenticationStr = this.clientId + ":" + this.clientSecret;
			string urlStr = "https://api.noco.tv/1.1/OAuth2/token.php";

			List<IMultipartFormSection> postData = new List<IMultipartFormSection>();
			postData.Add( new MultipartFormDataSection("grant_type=authorization_code&code=" + this.oauthCode) );

			var authenticationBytes = System.Text.Encoding.UTF8.GetBytes(authenticationStr);
			string authenticationB64 = System.Convert.ToBase64String(authenticationBytes);

			/*
			UnityWebRequest request = UnityWebRequest.Post(urlStr, postData);
			request.SetRequestHeader ("Authorization", "Basic " + authenticationB64);

			yield return request.Send();
			string result = request.downloadHandler.text;
			*/

			WWWForm form = new WWWForm();
			form.AddField("grant_type", "authorization_code");
			form.AddField("code", this.oauthCode);
			Dictionary<string, string> headers = form.headers;
			headers ["Authorization"] = "Basic " + authenticationB64;
			byte[] rawData = form.data;

			WWW request = new WWW (urlStr, rawData, headers);
			yield return request;
			string result = request.text;
			this.oauthAccessToken = JsonUtility.FromJson <AccessTokenDescriptor>(result);
			if(debugAuthentification) Debug.Log("Token descriptor: "+this.oauthAccessToken);
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
			return new X509Certificate();
		}

		public IEnumerator Launch(String username, String password, String appName) {
			callFinished = false;
			String host = "api.noco.tv";
			String authorizationUrl = "/1.1/OAuth2/authorize.php?response_type=code&client_id="+appName+"&state=STATE";
			String body = "username=" + WWW.EscapeURL(username) + "&password=" + WWW.EscapeURL(password) + "&login=1";
			int contentLength = System.Text.Encoding.UTF8.GetBytes(body).Length;

			TcpClient client = new TcpClient(host, 443);
			NetworkStream networkStream = client.GetStream();
			SslStream sslStream = new SslStream(networkStream
				,false
				,new RemoteCertificateValidationCallback(RemoteCertificateValidationCallback)
				, new LocalCertificateSelectionCallback(userCertificateSelectionCallback)
			);

			// SSL might fail, hence the retries
			// See http://stackoverflow.com/questions/16270347/mono-https-exception-the-authentication-or-decryption-has-failed
			int attempts = 3;
			while (attempts > 0) {
				attempts--;
				try {
					sslStream.AuthenticateAsClient (host);//, new X509Certificate2Collection (), SslProtocols.Tls, true);
					break;
				} catch {
					// Debug.Log ("ssl error" );
					if (attempts <= 0) {
						throw;
					}
				}
			}

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
			client.Close();
			callFinished = true;
		}
	}
}

