Noco Virtual reality client.
Unity library for noco.tv connection 


Licence
==========
Main code
----------------
MIT license

Third party components
----------------
OVR/ and OvrAvatar/ content provided and licensed by Oculus, and stored here for convenience.
To find and download the latest versions, please see https://developer.oculus.com/downloads/unity/


Assets
--------------
If unspecified, all assets are the property of Noco and used in iNoco with their agreement. You have to contact Noco (noco.tv) before reusing them, and before any new release using these assets.


Status
===========
The library implementation can: 
- handle authentication (unsafely: Noco SSL certificates won't be checked, so do not use on an untrusted network)
- fetch access token and use refresh tokens
- fetch and cache latest shows (hardcoded page value for now)
- fetch a show url (hardcoded quality values for now)

The sample scene includes:
- uses environment variables for login (NOCO_CLIENTID, NOCO_CLIENTSECRET, NOCO_USER, NOCO_PASSWORD). Note that Unity should be launched from the command line in some cases for the env. variables to be properly found
- fetch latest shows
- play the last one on a plane



Details
----------
* WWW and UnityWebRequest do not handle properly http redirection. I had to handle them manually with a TcpClient. By doing it this way, the certificates collection is unfortunatly not loaded by Unity, and so I had to bypass certificates check, leading to an unsafe SSL connection.

