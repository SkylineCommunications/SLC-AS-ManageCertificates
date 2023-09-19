namespace ManageCertificates_1
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	using ManageCertificates_1.Models;

	using Org.BouncyCastle.Asn1.X509;
	using Org.BouncyCastle.Crypto;
	using Org.BouncyCastle.Crypto.Generators;
	using Org.BouncyCastle.Crypto.Operators;
	using Org.BouncyCastle.Crypto.Prng;
	using Org.BouncyCastle.Math;
	using Org.BouncyCastle.Pkcs;
	using Org.BouncyCastle.Security;
	using Org.BouncyCastle.X509;

	using Skyline.DataMiner.Utils.Certificates;

	internal static class CommonActions
	{
		internal static void CreateSelfSignedCertificateAuthority(string destination, CertificateModel certificateModel)
		{
			var issuer = certificateModel.DistinguishedName;
			var rootCAPath = $"{destination}\\{certificateModel.CommonName}.p12";
			var rootCACrtPath = $"{destination}\\{certificateModel.CommonName}.crt";
			string password = certificateModel.Password; // Set your keystore password here

			var randomGenerator = new CryptoApiRandomGenerator();
			var random = new SecureRandom(randomGenerator);

			// Generate a new key pair for the Root CA
			RsaKeyPairGenerator generator = new RsaKeyPairGenerator();
			generator.Init(new KeyGenerationParameters(new SecureRandom(), certificateModel.KeySize.Value));
			var rsaKeyPair = generator.GenerateKeyPair();

			// Create an X.509 certificate generator for the Root CA
			X509V3CertificateGenerator certGenerator = new X509V3CertificateGenerator();
			certGenerator.SetSerialNumber(BigInteger.ProbablePrime(120, new Random()));
			certGenerator.SetIssuerDN(new X509Name(issuer)); // Issuer's distinguished name
			certGenerator.SetSubjectDN(new X509Name(issuer)); // Subject's distinguished name
			certGenerator.SetNotBefore(DateTime.UtcNow.Date); // Certificate validity start date
			certGenerator.SetNotAfter(DateTime.UtcNow.Date.AddDays(certificateModel.Validity.Value)); // Certificate validity end date
			certGenerator.SetPublicKey(rsaKeyPair.Public); // Public key of the Root CA

			// Mark as certificate authority
			certGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(true)); // Mark as CA
			certGenerator.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.KeyCertSign)); // Key usage
			certGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, true, new ExtendedKeyUsage(new[] { KeyPurposeID.IdKPClientAuth, KeyPurposeID.IdKPServerAuth })); // Extended key usage

			// Self-sign
			ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", rsaKeyPair.Private, random);
			var certificate = certGenerator.Generate(signatureFactory);

			// Export the rootCA cert to a crt file
			using (var stream = new StreamWriter(rootCACrtPath))
			{
				stream.WriteLine("-----BEGIN CERTIFICATE-----");
				var data = Convert.ToBase64String(certificate.GetEncoded(), Base64FormattingOptions.InsertLineBreaks);
				stream.WriteLine(data);
				stream.WriteLine("-----END CERTIFICATE-----");
			}

			// Create a PKCS#12 keystore for the Root CA
			Pkcs12Store store = new Pkcs12Store();
			char[] passwordChars = password.ToCharArray();
			X509CertificateEntry certEntry = new X509CertificateEntry(certificate);
			store.SetCertificateEntry(certificateModel.CommonName, certEntry);
			store.SetKeyEntry(certificateModel.CommonName, new AsymmetricKeyEntry(rsaKeyPair.Private), new[] { certEntry });

			// Save the keystore to a file (p12 format)
			using (FileStream stream = new FileStream(rootCAPath, FileMode.Create, FileAccess.ReadWrite))
			{
				store.Save(stream, passwordChars, new SecureRandom());
			}
		}

		internal static void CreateCertificate(string destination, CertificateModel certificateModel, CertificateIssuer certificateIssuer = null)
		{
			var subject = certificateModel.DistinguishedName;

			var nodeCrtPath = $"{destination}\\{certificateModel.CertificateName}.crt";
			var nodeP12Path = $"{destination}\\{certificateModel.CertificateName}.p12";
			var password = certificateModel.Password;

			var randomGenerator = new CryptoApiRandomGenerator();
			var random = new SecureRandom(randomGenerator);

			Org.BouncyCastle.X509.X509Certificate certificate;

			// Generate a new key pair for the entity
			RsaKeyPairGenerator generator = new RsaKeyPairGenerator();
			generator.Init(new KeyGenerationParameters(new SecureRandom(), certificateModel.KeySize.Value));
			var rsaKeyPair = generator.GenerateKeyPair();

			// Create a Certificate Signing Request (CSR) for the entity
			X509V3CertificateGenerator csrGenerator = new X509V3CertificateGenerator();
			csrGenerator.SetSerialNumber(BigInteger.ProbablePrime(120, new Random()));
			csrGenerator.SetSubjectDN(new X509Name(subject)); // Subject (Entity)
			csrGenerator.SetNotBefore(DateTime.UtcNow.Date); // Certificate validity start date
			csrGenerator.SetNotAfter(DateTime.UtcNow.Date.AddDays(certificateModel.Validity.Value)); // Certificate validity end date
			csrGenerator.SetPublicKey(rsaKeyPair.Public); // Public key of the entity

			// Add any extensions you need (e.g., BasicConstraints, KeyUsage, ExtendedKeyUsage)
			csrGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(false)); // Not a CA certificate

			if (!string.IsNullOrEmpty(certificateModel.IPAddress))
			{
				csrGenerator.AddExtension(X509Extensions.SubjectAlternativeName, false, new GeneralNames(new GeneralName(GeneralName.IPAddress, certificateModel.IPAddress)));
			}

			if (certificateModel.DNSNames.Any())
			{
				foreach (var dnsName in certificateModel.DNSNames)
				{
					csrGenerator.AddExtension(X509Extensions.SubjectAlternativeName, false, new GeneralNames(new GeneralName(GeneralName.DnsName, dnsName)));
				}
			}

			// If Issuer provided
			if (certificateIssuer != null)
			{
				var rootCA = certificateIssuer.Certificate;
				var rootCAPath = rootCA.P12Path;
				var rootCAName = GetFileName(rootCAPath);

				// Load the Root CA certificate and private key
				Pkcs12Store rootCAStore = new Pkcs12Store(new FileStream(rootCAPath, FileMode.Open, FileAccess.Read), certificateIssuer.Password.ToCharArray());
				X509CertificateEntry rootCACertificateEntry = rootCAStore.GetCertificate(rootCAName);
				var rootCACertificate = rootCACertificateEntry.Certificate;
				AsymmetricKeyParameter rootCAPrivateKey = rootCAStore.GetKey(rootCAName).Key;
				var rootCAKeyEntry = new AsymmetricKeyEntry(rootCAPrivateKey);

				csrGenerator.SetIssuerDN(rootCACertificate.SubjectDN); // Issuer (Root CA)

				// Sign the CSR with the Root CA's private key to generate a signed certificate
				ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", rootCAPrivateKey, random);
				certificate = csrGenerator.Generate(signatureFactory);
			}
			else
			{
				csrGenerator.SetIssuerDN(new X509Name(subject)); // Issuer (Root CA)

				// Sign the CSR with the Root CA's private key to generate a signed certificate
				ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", rsaKeyPair.Private, random);
				certificate = csrGenerator.Generate(signatureFactory);
			}

			// Export the cert to a crt file
			using (var stream = new StreamWriter(nodeCrtPath))
			{
				stream.WriteLine("-----BEGIN CERTIFICATE-----");
				var data = Convert.ToBase64String(certificate.GetEncoded(), Base64FormattingOptions.InsertLineBreaks);
				stream.WriteLine(data);
				stream.WriteLine("-----END CERTIFICATE-----");
			}

			// Create a PKCS#12 keystore for the Root CA
			Pkcs12Store store = new Pkcs12Store();
			char[] passwordChars = password.ToCharArray();
			X509CertificateEntry certEntry = new X509CertificateEntry(certificate);
			store.SetCertificateEntry(certificateModel.CommonName, certEntry);
			store.SetKeyEntry(certificateModel.CommonName, new AsymmetricKeyEntry(rsaKeyPair.Private), new[] { certEntry });

			// Save the keystore to a file (p12 format)
			using (FileStream stream = new FileStream(nodeP12Path, FileMode.Create, FileAccess.ReadWrite))
			{
				store.Save(stream, passwordChars, new SecureRandom());
			}
		}

		internal static Dictionary<string, ICertificate> GetRootCertificates(string folderPath)
		{
			var certificates = new Dictionary<string, ICertificate>();
			foreach (string folder in Directory.GetDirectories(folderPath))
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
	}
}