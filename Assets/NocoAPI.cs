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
	public class NocoAPI
	{
		// Credentials
		public String clientId = null;
		public String clientSecret = null;
		public String redirectUri = null;
		// Cached 
		public String oauthCode = null;
		public String oauthAccessToken = null;
		public String oauthRefreshToken = null;
		public String oauthExpirationDate = null;
		public String oauthTokenType = null;

		public NocoAPI (String clientId, String clientsecret, String redirectUri = null)
		{
			this.clientId = clientId;
			this.clientSecret = clientsecret;
			this.redirectUri = redirectUri;
		}

		private void loadCachedOAuthInfo()
		{
			this.oauthAccessToken = null;
			this.oauthRefreshToken = null;
			this.oauthExpirationDate = null;
			this.oauthTokenType = null;

			//Debug to test refresh token
			//this.oauthExpirationDate = 10;
		}

		public IEnumerator Authenticate(String username, String password, String appName){
			if (username == null || username == "" || password == null || username == "") {
				Debug.Log ("Missing credentials");
				yield return null;
			} else {
				// Authentification
				Debug.Log("Noco Authentification...");
				NocoOAuthAuthentificationRequest authentificationRequest = new NocoOAuthAuthentificationRequest ();
				yield return authentificationRequest.Launch (username, password, appName);
				while (authentificationRequest.callFinished != true) {
					yield return null;
				}
				Debug.Log("Authentifiction finished");
				this.oauthCode = authentificationRequest.code;			
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

		// See http://answers.unity3d.com/questions/792342/how-to-validate-ssl-certificates-when-using-httpwe.html
		private static bool RemoteCertificateValidationCallback(
			object sender,
			X509Certificate certificate,
			X509Chain chain,
			SslPolicyErrors sslPolicyErrors)
		{
			Debug.Log ("[Warning] Using unsafe certification");
			// Debug.Log (sender);
			// Debug.Log (certificate);
			// Debug.Log (chain);
			// Debug.Log (sslPolicyErrors);
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
			//Debug.Log("Authenticating...");

			// SSL might fail
			// See http://stackoverflow.com/questions/16270347/mono-https-exception-the-authentication-or-decryption-has-failed
			int attempts = 3;
			while (attempts > 0) {
				attempts--;
				try {
					sslStream.AuthenticateAsClient (host);//, new X509Certificate2Collection (), SslProtocols.Tls, true);
					break;
				} catch (Exception e){
					Debug.Log ("ssl error");
					if (attempts <= 0) {
						throw;
					}
				}
			}

			//Debug.Log("Authent done");
			while (!sslStream.IsAuthenticated) {
				//Debug.Log("Not yet authenticated...");
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
			//Debug.Log ("Request:");
			//Debug.Log(requestData);
			result = "";
			String lastLine = null;
			while (true) {
				if (!client.Connected) {
					//Debug.Log ("Disconnected");
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

						//Debug.Log ("Received: >" + line + "<");
						result = result + line;
					} else {
						break;
					}
				}
				//Debug.Log ("No data ...");
				yield return null;

			}
			//Debug.Log (data);

			//Debug.Log("Closing...");
			sslStream.Close();
			client.Close();
			//Debug.Log("Closed.");

			callFinished = true;
			yield return null;
		}
	}
}

