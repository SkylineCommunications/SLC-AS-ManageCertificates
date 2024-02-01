namespace ManageCertificates_1
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.ConstrainedExecution;
	using System.Security.Cryptography.X509Certificates;

	using ManageCertificates_1.Models;
	using Newtonsoft.Json.Linq;

	using Org.BouncyCastle.Asn1.X509;
	using Org.BouncyCastle.Crypto;
	using Org.BouncyCastle.Crypto.Generators;
	using Org.BouncyCastle.Crypto.Operators;
	using Org.BouncyCastle.Crypto.Prng;
	using Org.BouncyCastle.Math;
	using Org.BouncyCastle.Pkcs;
	using Org.BouncyCastle.Security;
	using Org.BouncyCastle.Utilities;
	using Org.BouncyCastle.X509;

	using Skyline.DataMiner.Utils.Certificates;

	internal static class CommonActions
	{
		internal const string CaFolderPath = @"C:\Skyline DataMiner\Documents\DMA_COMMON_DOCUMENTS\Certificates\CertificateAuthorities";
		internal const string ScFolderPath = @"C:\Skyline DataMiner\Documents\DMA_COMMON_DOCUMENTS\Certificates\SignedCertificates";

		internal static void CreateCertificate(Models.CertificateRequest request, Dictionary<string, ICertificate> certAuthorities)
		{
			var certFolder = request.IsCertificateAuthority ?
				$"{CaFolderPath}\\{request.Subject.CommonName}" :
				$"{ScFolderPath}\\{request.Subject.CommonName}";
			if (Directory.Exists(certFolder))
			{
				throw new ArgumentException($"The directory {certFolder} already exists.");
			}

			// Generate a new key pair for the certificate
			var randomGenerator = new CryptoApiRandomGenerator();
			var random = new SecureRandom(randomGenerator);
			RsaKeyPairGenerator generator = new RsaKeyPairGenerator();
			generator.Init(new KeyGenerationParameters(new SecureRandom(), request.KeySize));
			var rsaKeyPair = generator.GenerateKeyPair();

			// Create an X.509 certificate generator
			X509V3CertificateGenerator certGenerator = new X509V3CertificateGenerator();
			certGenerator.SetSerialNumber(BigInteger.ProbablePrime(120, new Random()));
			certGenerator.SetSubjectDN(new X509Name(request.Subject.Value)); // Subject's distinguished name
			certGenerator.SetNotBefore(request.ValidFrom.Date); // Certificate validity start date
			certGenerator.SetNotAfter(request.ValidUntil.Date); // Certificate validity end date
			certGenerator.SetPublicKey(rsaKeyPair.Public); // Public key of the Root CA

			if (request.IsCertificateAuthority)
			{
				// Mark as certificate authority
				certGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(true)); // Mark as CA
				certGenerator.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.KeyCertSign)); // Key usage
				certGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, true, new ExtendedKeyUsage(KeyPurposeID.IdKPClientAuth, KeyPurposeID.IdKPServerAuth)); // Extended key usage
			}
			else
			{
				// Add any extensions you need (e.g., BasicConstraints, KeyUsage, ExtendedKeyUsage)
				certGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(false)); // Not a CA certificate
				if (request.IPAddresses != null && request.IPAddresses.Any())
				{
					foreach (var ip in request.IPAddresses)
					{
						certGenerator.AddExtension(X509Extensions.SubjectAlternativeName, false, new GeneralNames(new GeneralName(GeneralName.IPAddress, ip)));
					}
				}

				if (request.IPAddresses != null && request.DnsNames.Any())
				{
					foreach (var dnsName in request.DnsNames)
					{
						certGenerator.AddExtension(X509Extensions.SubjectAlternativeName, false, new GeneralNames(new GeneralName(GeneralName.DnsName, dnsName)));
					}
				}
			}

			// Sign Certificate
			Org.BouncyCastle.X509.X509Certificate certificate;
			if (string.IsNullOrWhiteSpace(request.Issuer))
			{
				// Self Signed
				certGenerator.SetIssuerDN(new X509Name(request.Subject.Value));
				ISignatureFactory selfSignedFactory = new Asn1SignatureFactory("SHA256WITHRSA", rsaKeyPair.Private, random);
				certificate = certGenerator.Generate(selfSignedFactory);
			}
			else
			{
				if (!certAuthorities.ContainsKey(request.Issuer))
				{
					throw new ArgumentException($"The issuer {request.Issuer} can not be found");
				}

				var certAuthority = certAuthorities[request.Issuer];
				if (!TryCertificatePassword(certAuthority.P12Path, request.IssuerPassword))
				{
					throw new ArgumentException("Password provided for CA p12 file is wrong.");
				}

				// Load the CA certificate and private key
				Pkcs12Store rootCAStore = new Pkcs12Store(new FileStream(certAuthority.P12Path, FileMode.Open, FileAccess.Read), request.IssuerPassword.ToCharArray());
				X509CertificateEntry rootCACertificateEntry = rootCAStore.GetCertificate(certAuthority.Subject.CommonName);
				var rootCACertificate = rootCACertificateEntry.Certificate;
				AsymmetricKeyParameter rootCAPrivateKey = rootCAStore.GetKey(certAuthority.Subject.CommonName).Key;
				certGenerator.SetIssuerDN(rootCACertificate.SubjectDN); // Issuer (Root CA)

				// Sign the CSR with the Root CA's private key to generate a signed certificate
				ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", rootCAPrivateKey, random);
				certificate = certGenerator.Generate(signatureFactory);
			}

			// Export the cert to a crt file
			Directory.CreateDirectory(certFolder);
			var keyStorePath = $"{certFolder}\\{request.Subject.CommonName}.p12";
			var crtPath = $"{certFolder}\\{request.Subject.CommonName}.crt";
			using (var stream = new StreamWriter(crtPath))
			{
				stream.WriteLine("-----BEGIN CERTIFICATE-----");
				var data = Convert.ToBase64String(certificate.GetEncoded(), Base64FormattingOptions.InsertLineBreaks);
				stream.WriteLine(data);
				stream.WriteLine("-----END CERTIFICATE-----");
			}

			// Create a PKCS#12 keystore
			Pkcs12Store store = new Pkcs12Store();
			char[] passwordChars = request.Password.ToCharArray();
			X509CertificateEntry certEntry = new X509CertificateEntry(certificate);
			store.SetCertificateEntry(request.Subject.CommonName, certEntry);
			store.SetKeyEntry(request.Subject.CommonName, new AsymmetricKeyEntry(rsaKeyPair.Private), new[] { certEntry });

			// Save the keystore to a file (p12 format)
			using (FileStream stream = new FileStream(keyStorePath, FileMode.Create, FileAccess.ReadWrite))
			{
				store.Save(stream, passwordChars, new SecureRandom());
			}
		}

		/// <summary>
		/// Get the certificates from a specific folder.
		/// </summary>
		/// <returns>A dictionary with as key the folder location and as value the ICertificate object.</returns>
		internal static Dictionary<string, ICertificate> GetCertificateAuthorities()
		{
			var certificates = new Dictionary<string, ICertificate>();
			foreach (string folder in Directory.GetDirectories(CaFolderPath))
			{
				var crt = Directory.GetFiles(folder).First(x => x.EndsWith(".crt"));
				var p12 = Directory.GetFiles(folder).First(x => x.EndsWith(".p12"));
				ICertificate ca = CertificatesFactory.GetCertificate(crt, p12);

				certificates[folder] = ca;
			}

			return certificates;
		}

		internal static bool TryCertificatePassword(string p12Path, string password)
		{
			try
			{
				Pkcs12Store certStore = new Pkcs12Store(new FileStream(p12Path, FileMode.Open, FileAccess.Read), password.ToCharArray());
				return true;
			}
			catch (IOException)
			{
				return false;
			}
		}

		internal static string GetFileName(string s)
		{
			var startIndex = s.LastIndexOf('\\') + 1;
			var endIndex = s.LastIndexOf(".");

			return s.Substring(startIndex, endIndex - startIndex);
		}

		internal static string GetFolderName(string s)
		{
			var startIndex = s.LastIndexOf('\\') + 1;

			return s.Substring(startIndex);
		}
	}
}