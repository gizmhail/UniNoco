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

		public NocoAPI (String clientId, String clientsecret, String redirectUri)
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
	}

	public class NocoOAuthAuthentificationRequest
	{
		public String result;
		public String redirection = null;

		public static bool CertificateValidationCallback(
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

		public IEnumerator Launch(String username, String password, String appName) {
			String host = "api.noco.tv";
			String authorizationUrl = "/1.1/OAuth2/authorize.php?response_type=code&client_id="+appName+"&state=STATE";
			String body = "username=" + username + "&password=" + password + "&login=1";
			int contentLength = System.Text.Encoding.UTF8.GetBytes(body).Length;

			TcpClient client = new TcpClient(host, 443);
			NetworkStream networkStream = client.GetStream();
			SslStream sslStream = new SslStream(networkStream
				,false
				,new RemoteCertificateValidationCallback(CertificateValidationCallback)
			);
			//Debug.Log("Authenticating...");
			sslStream.AuthenticateAsClient (host);//, new X509Certificate2Collection (), SslProtocols.Tls, true);
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

			yield return null;
		}
	}
}

