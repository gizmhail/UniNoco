Unity library for noco.tv connection 
Will be used in Noco Virtual reality client.

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

