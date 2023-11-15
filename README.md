# ManageCertificates

## About

Certificates manager automation script for creating, uploading and managing certificate authorities and signed certificates.

## Usage

The script can be executed with an empty json or with pre-specified certificate information, for example:

```cs
{{
	""commonName"":""myNode"",
	""organization"":""Skyline Communications"",
	""organizationalUnit"":""Singapore Office"",
	""country"":""BE"",
	""validity"":3650}}
}}

```
Once executed, the following functions are possible from the script:
* Create a certificate authority (creates a public key as .crt and a private key as .p12)
* Create a signed certificate from a specified certificate authority
* Delete any stored certificate authority or signed certificate
* Upload certificate authorities and/or signed certificates

Certificates are stored on the DMS at the path '/Skyline DataMiner/Documents/DMA_COMMON_DOCUMENTS/Certificates'.


## Running the script from another Automation Script

```cs
using Skyline.DataMiner.Automation;

/// <summary>
/// DataMiner Script Class.
/// </summary>
public class Script
{
	/// <summary>
	/// The Script entry point.
	/// </summary>
	/// <param name="engine">Link with SLAutomation process.</param>
	public void Run(Engine engine)
	{
					// Call subscript
			// Prepare a subscript
			SubScriptOptions subScript = Engine.PrepareSubScript("ManageCertificates");

			// Prepare Input param
			string input = $@"
{{
	""commonName"":""{model.HostName}"",
	""organization"":""Skyline Communications"",
	""organizationalUnit"":""{model.ClusterName}"",
	""country"":""BE"",
	""validity"":3650}}
}}
";

			subScript.SelectScriptParam("Input", input);

			// Launch the script
			subScript.StartScript();
	}
}
```

Requires [Skyline.DataMiner.Utils.Certificates](https://github.com/SkylineCommunications/Skyline.DataMiner.Utils.Certificates).
