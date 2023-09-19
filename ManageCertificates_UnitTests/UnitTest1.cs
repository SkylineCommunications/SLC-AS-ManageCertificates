namespace ManageCertificates_UnitTests
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Security.Cryptography.X509Certificates;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Org.BouncyCastle.Asn1.Nist;
	using Org.BouncyCastle.Asn1.Pkcs;
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

	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void BouncyCastleRootCACreation()
		{
			var issuer = "CN=rootCA,OU=opensearchcluster,O=OpenSearch,C=BE";
			var rootCAPath = "C:\\Skyline DataMiner\\Documents\\DMA_COMMON_DOCUMENTS\\Certificates\\CertificateAuthorities\\testcluster\\rootCA.p12";
			var rootCACrtPath = "C:\\Skyline DataMiner\\Documents\\DMA_COMMON_DOCUMENTS\\Certificates\\CertificateAuthorities\\testcluster\\rootCA.crt";
			string password = "1234567890"; // Set your keystore password here

			var randomGenerator = new CryptoApiRandomGenerator();
			var random = new SecureRandom(randomGenerator);

			// Generate a new key pair for the Root CA
			RsaKeyPairGenerator generator = new RsaKeyPairGenerator();
			generator.Init(new KeyGenerationParameters(new SecureRandom(), 4096));
			var rsaKeyPair = generator.GenerateKeyPair();

			// Create an X.509 certificate generator for the Root CA
			X509V3CertificateGenerator certGenerator = new X509V3CertificateGenerator();
			certGenerator.SetSerialNumber(BigInteger.ProbablePrime(120, new Random()));
			certGenerator.SetIssuerDN(new X509Name(issuer)); // Issuer's distinguished name
			certGenerator.SetSubjectDN(new X509Name(issuer)); // Subject's distinguished name
			certGenerator.SetNotBefore(DateTime.UtcNow.Date); // Certificate validity start date
			certGenerator.SetNotAfter(DateTime.UtcNow.Date.AddYears(10)); // Certificate validity end date
			certGenerator.SetPublicKey(rsaKeyPair.Public); // Public key of the Root CA

			// Self-sign the Root CA certificate
			certGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(true)); // Mark as CA
			certGenerator.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.KeyCertSign)); // Key usage
			certGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, true, new ExtendedKeyUsage(new[] { KeyPurposeID.IdKPClientAuth, KeyPurposeID.IdKPServerAuth })); // Extended key usage

			// Generate the Root CA certificate
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
			store.SetCertificateEntry("rootCA", certEntry);
			store.SetKeyEntry("rootCA", new AsymmetricKeyEntry(rsaKeyPair.Private), new[] { certEntry });

			// Save the keystore to a file (p12 format)
			using (FileStream stream = new FileStream(rootCAPath, FileMode.Create, FileAccess.ReadWrite))
			{
				store.Save(stream, passwordChars, new SecureRandom());
			}
		}

		[TestMethod]
		public void BouncyCastleSignedCertificateCreation()
		{
			var nodeName = "10.11.13.1";

			var issuer = "CN=rootCA,OU=opensearchcluster,O=OpenSearch,C=BE";
			var subject = $"CN={nodeName},OU=opensearchcluster,O=OpenSearch,C=BE";
			var rootCAPath = "C:\\Skyline DataMiner\\Documents\\DMA_COMMON_DOCUMENTS\\Certificates\\CertificateAuthorities\\testcluster\\rootCA.p12";
			var nodeCrtPath = $"C:\\Skyline DataMiner\\Documents\\DMA_COMMON_DOCUMENTS\\Certificates\\CertificateAuthorities\\testcluster\\{nodeName}.crt_signed";
			var nodeP12Path = $"C:\\Skyline DataMiner\\Documents\\DMA_COMMON_DOCUMENTS\\Certificates\\CertificateAuthorities\\testcluster\\{nodeName}-node-keystore.p12";
			var password = "0123456789";

			var randomGenerator = new CryptoApiRandomGenerator();
			var random = new SecureRandom(randomGenerator);

			// Load the Root CA certificate and private key
			Pkcs12Store rootCAStore = new Pkcs12Store(new FileStream(rootCAPath, FileMode.Open, FileAccess.Read), "1234567890".ToCharArray());
			X509CertificateEntry rootCACertificateEntry = rootCAStore.GetCertificate("rootCA");
			var rootCACertificate = rootCACertificateEntry.Certificate;
			AsymmetricKeyParameter rootCAPrivateKey = rootCAStore.GetKey("rootCA").Key;
			var rootCAKeyEntry = new AsymmetricKeyEntry(rootCAPrivateKey);

			// Generate a new key pair for the entity
			RsaKeyPairGenerator generator = new RsaKeyPairGenerator();
			generator.Init(new KeyGenerationParameters(new SecureRandom(), 4096));
			var rsaKeyPair = generator.GenerateKeyPair();

			// Create a Certificate Signing Request (CSR) for the entity
			X509V3CertificateGenerator csrGenerator = new X509V3CertificateGenerator();
			csrGenerator.SetSerialNumber(BigInteger.ProbablePrime(120, new Random()));
			csrGenerator.SetIssuerDN(rootCACertificate.SubjectDN); // Issuer (Root CA)
			csrGenerator.SetSubjectDN(new X509Name(subject)); // Subject (Entity)
			csrGenerator.SetNotBefore(DateTime.UtcNow.Date); // Certificate validity start date
			csrGenerator.SetNotAfter(DateTime.UtcNow.Date.AddYears(1)); // Certificate validity end date
			csrGenerator.SetPublicKey(rsaKeyPair.Public); // Public key of the entity

			// Add any extensions you need (e.g., BasicConstraints, KeyUsage, ExtendedKeyUsage)
			csrGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(false)); // Not a CA certificate

			// Create the CSR
			Pkcs10CertificationRequest csr = new Pkcs10CertificationRequest(new Asn1SignatureFactory("SHA256WITHRSA", rsaKeyPair.Private, random), new X509Name(subject), rsaKeyPair.Public, null);

			// Sign the CSR with the Root CA's private key to generate a signed certificate
			ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", rootCAPrivateKey, random);
			var certificate = csrGenerator.Generate(signatureFactory);

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
			store.SetCertificateEntry(nodeName, certEntry);
			store.SetKeyEntry(nodeName, new AsymmetricKeyEntry(rsaKeyPair.Private), new[] { certEntry });

			// Save the keystore to a file (p12 format)
			using (FileStream stream = new FileStream(nodeP12Path, FileMode.Create, FileAccess.ReadWrite))
			{
				store.Save(stream, passwordChars, new SecureRandom());
			}
		}

		[TestMethod]
		public void BouncyCastleJKSCreation()
		{
			var nodeName = "10.11.13.1";

			var issuer = "CN=rootCA,OU=opensearchcluster,O=OpenSearch,C=BE";
			var subject = $"CN={nodeName},OU=opensearchcluster,O=OpenSearch,C=BE";
			var rootCAPath = "C:\\Skyline DataMiner\\Documents\\DMA_COMMON_DOCUMENTS\\Certificates\\CertificateAuthorities\\testcluster\\rootCA.p12";
			var nodeCrtPath = $"C:\\Skyline DataMiner\\Documents\\DMA_COMMON_DOCUMENTS\\Certificates\\CertificateAuthorities\\testcluster\\{nodeName}.crt";
			var nodeP12Path = $"C:\\Skyline DataMiner\\Documents\\DMA_COMMON_DOCUMENTS\\Certificates\\CertificateAuthorities\\testcluster\\{nodeName}-node-keystore.p12";

			var randomGenerator = new CryptoApiRandomGenerator();
			var random = new SecureRandom(randomGenerator);

			// Load the Root CA certificate and private key
			Pkcs12Store rootCAStore = new Pkcs12Store(new FileStream(rootCAPath, FileMode.Open, FileAccess.Read), "1234567890".ToCharArray());
			X509CertificateEntry rootCACertificateEntry = rootCAStore.GetCertificate("rootCA");
			var rootCACertificate = rootCACertificateEntry.Certificate;
			AsymmetricKeyParameter rootCAPrivateKey = rootCAStore.GetKey("rootCA").Key;
			var rootCAKeyEntry = new AsymmetricKeyEntry(rootCAPrivateKey);

			// Generate a new key pair for the entity
			RsaKeyPairGenerator generator = new RsaKeyPairGenerator();
			generator.Init(new KeyGenerationParameters(new SecureRandom(), 4096));
			var rsaKeyPair = generator.GenerateKeyPair();

			// Create a Certificate Signing Request (CSR) for the entity
			X509V3CertificateGenerator csrGenerator = new X509V3CertificateGenerator();
			csrGenerator.SetSerialNumber(BigInteger.ProbablePrime(120, new Random()));
			csrGenerator.SetIssuerDN(rootCACertificate.SubjectDN); // Issuer (Root CA)
			csrGenerator.SetSubjectDN(new X509Name(subject)); // Subject (Entity)
			csrGenerator.SetNotBefore(DateTime.UtcNow.Date); // Certificate validity start date
			csrGenerator.SetNotAfter(DateTime.UtcNow.Date.AddYears(1)); // Certificate validity end date
			csrGenerator.SetPublicKey(rsaKeyPair.Public); // Public key of the entity

			// Add any extensions you need (e.g., BasicConstraints, KeyUsage, ExtendedKeyUsage)
			csrGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(false)); // Not a CA certificate

			// Create the CSR
			Pkcs10CertificationRequest csr = new Pkcs10CertificationRequest(new Asn1SignatureFactory("SHA256WITHRSA", rsaKeyPair.Private, random), new X509Name(subject), rsaKeyPair.Public, null);

			// Sign the CSR with the Root CA's private key to generate a signed certificate
			ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", rootCAPrivateKey, random);
			var certificate = csrGenerator.Generate(signatureFactory);

			// Create a PKCS#12 keystore
			var store2 = new Pkcs12Store();
			Pkcs12StoreBuilder storeBuilder = new Pkcs12StoreBuilder();
			storeBuilder.SetCertAlgorithm(PkcsObjectIdentifiers.PbeWithSha1AndDesCbc);
			storeBuilder.SetKeyAlgorithm(NistObjectIdentifiers.IdAes256Cbc, PkcsObjectIdentifiers.IdHmacWithSha256);
			Pkcs12Store store = storeBuilder.Build(); //new Pkcs12Store();

			// Add the RootCA
			store.SetCertificateEntry("rootCA", rootCACertificateEntry);
			//store.SetKeyEntry("rootCA", rootCAKeyEntry, new[] { rootCACertificateEntry });

			// Add the signed certificate
			X509CertificateEntry certEntry = new X509CertificateEntry(certificate);
			AsymmetricKeyEntry keyEntry = new AsymmetricKeyEntry(rsaKeyPair.Private);
			store.SetCertificateEntry(nodeName, certEntry);
			store.SetKeyEntry(nodeName, keyEntry, new[] { certEntry });

			// Export the rootCA cert to a crt file
			using (var stream = new StreamWriter(nodeCrtPath))
			{
				stream.WriteLine("-----BEGIN CERTIFICATE-----");
				var data = Convert.ToBase64String(certificate.GetEncoded(), Base64FormattingOptions.InsertLineBreaks);
				stream.WriteLine(data);
				stream.WriteLine("-----END CERTIFICATE-----");
			}

			// Save the PKCS#12 keystore to a P12 file (entity.p12)
			string keystorePassword = "0123456789"; // Set your keystore password here
			char[] passwordChars = keystorePassword.ToCharArray();
			using (FileStream stream = new FileStream(nodeP12Path, FileMode.Create, FileAccess.ReadWrite))
			{
				store.Save(stream, passwordChars, new SecureRandom());
			}
		}

		[TestMethod]
		public void RootCACreation()
		{
			X509Certificate2 certificate;
			byte[] privateKey;
			var distinguishedName = "CN=rootCA,OU=opensearchcluster,O=OpenSearch,C=BE";
			var destination = "C:\\Skyline DataMiner\\Documents\\DMA_COMMON_DOCUMENTS\\Certificates\\CertificateAuthorities";
			var path = $"{destination}\\opensearchcluster\\rootCA.crt";

			var this_folder = destination + "\\opensearchcluster";
			Directory.CreateDirectory(this_folder);

			using (RSACryptoServiceProvider csp = new RSACryptoServiceProvider(4096))
			{
				var request = new CertificateRequest(distinguishedName, csp, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				certificate = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));

				privateKey = csp.ExportCspBlob(true);
			}

			using (var stream = new StreamWriter($"{destination}\\rootCA.crt"))
			{
				stream.WriteLine("-----BEGIN CERTIFICATE-----");
				var data = Convert.ToBase64String(certificate.GetPublicKey(), Base64FormattingOptions.InsertLineBreaks);
				stream.WriteLine(data);
				stream.WriteLine("-----END CERTIFICATE-----");
			}

			using (var stream = new StreamWriter($"{destination}\\rootCA.p12"))
			{
				stream.WriteLine("-----BEGIN CERTIFICATE-----");
				var data = Convert.ToBase64String(certificate.Export(X509ContentType.Pkcs12, "123"), Base64FormattingOptions.InsertLineBreaks);
				stream.WriteLine(data);
				stream.WriteLine("-----END CERTIFICATE-----");
			}

			using (var stream = new StreamWriter($"{destination}\\rootCA.key"))
			{
				stream.WriteLine("-----BEGIN PRIVATE KEY-----");
				var data = Convert.ToBase64String(privateKey, Base64FormattingOptions.InsertLineBreaks);
				stream.WriteLine(data);
				stream.WriteLine("-----END PRIVATE KEY-----");
			}
		}

		[TestMethod]
		public void CreateCertificate()
		{
			var rootCAPath = "C:\\Skyline DataMiner\\Documents\\DMA_COMMON_DOCUMENTS\\Certificates\\CertificateAuthorities\\testcluster\\rootCA.p12";

			var rootCA = GetCertFromPath(rootCAPath, "123");

			var distinguishedName = "CN=10.11.13.2,OU=opensearchcluster,O=OpenSearch,C=BE";

			string hex = rootCA.SerialNumber;
			byte[] bytes = new byte[hex.Length / 2];

			X509Certificate2 certificate;
			for (int i = 0; i < hex.Length; i += 2)
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

			using (RSACryptoServiceProvider csp = new RSACryptoServiceProvider(4096))
			{
				var request = new CertificateRequest(distinguishedName, csp, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				certificate = request.Create(rootCA, new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), rootCA.NotAfter, bytes);
			}

			using (var stream = new StreamWriter($"C:\\Skyline DataMiner\\Documents\\DMA_COMMON_DOCUMENTS\\Certificates\\CertificateAuthorities\\testcluster\\10.11.13.2-node-keystore-copy.p12"))
			{
				stream.WriteLine("-----BEGIN CERTIFICATE-----");
				var data = Convert.ToBase64String(certificate.Export(X509ContentType.Pkcs12, "0123456789"), Base64FormattingOptions.InsertLineBreaks);
				stream.WriteLine(data);
				stream.WriteLine("-----END CERTIFICATE-----");
			}
		}

		[TestMethod]
		public void GetCertificates()
		{
			string scFolderPath = @"C:\Skyline DataMiner\Documents\DMA_COMMON_DOCUMENTS\Certificates\SignedCertificates";
			var certificates = new Dictionary<string, ICertificate>();
			foreach (string folder in Directory.GetDirectories(scFolderPath))
			{
				var folderName = folder.Substring(folder.LastIndexOf("\\") + 1);
				try
				{
					var p12 = Directory.GetFiles(folder).First(x => x.EndsWith($"{folderName}.p12"));
					var crt = Directory.GetFiles(folder).First(x => x.EndsWith($"{folderName}.crt"));
					ICertificate cert = CertificatesFactory.GetCertificate(crt, p12);
					certificates[folder] = cert;
				}
				catch
				{
					// do nothing
				}
			}
		}

		[TestMethod]
		public void GetCertInfo()
		{
			string folderPath = "C:\\Skyline DataMiner\\Documents\\DMA_COMMON_DOCUMENTS\\Certificates\\SignedCertificates";
			var certificates = new Dictionary<string, ICertificate>();
			foreach (string folder in Directory.GetDirectories(folderPath))
			{
				foreach (string inner_folder in Directory.GetDirectories(folder))
				{
					var p12 = Directory.GetFiles(inner_folder).First(x => x.EndsWith(".p12"));
					var crt = Directory.GetFiles(inner_folder).First(x => x.EndsWith(".crt"));
					ICertificate cert = CertificatesFactory.GetCertificate(crt, p12);

					certificates[inner_folder] = cert;
				}
			}

			foreach (var cert in certificates)
			{
				var dn = cert.Value.CertificateInfo.DistinguishedName;
				var expiry = cert.Value.CertificateFile.NotAfter;
				var issuer = cert.Value.Issuer;
			}
		}

		[TestMethod]
		public void TestPassword()
		{
			var p12Path = "C:\\Skyline DataMiner\\Documents\\DMA_COMMON_DOCUMENTS\\Certificates\\SignedCertificates\\10.11.13.1\\opensearchcluster\\10.11.13.1-node-keystore.p12";

			TryCertificatePassword(p12Path, "012345");
		}

		[TestMethod]
		public void TrustCert()
		{
			var rootCAPath = "C:\\Skyline DataMiner\\Documents\\DMA_COMMON_DOCUMENTS\\Certificates\\CertificateAuthorities\\opensearchcluster";
			var crt = Directory.GetFiles(rootCAPath).First(x => x.EndsWith(".crt"));

			var cert = new X509Certificate2(crt);
			using (X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
			{
				store.Open(OpenFlags.ReadWrite);
				store.Add(cert);
			}
		}

		private X509Certificate2 GetCertFromPath(string path, string password)
		{
			var str = File.ReadAllText(path);
			var start = str.IndexOf(Environment.NewLine);
			var end = str.LastIndexOf(Environment.NewLine + "-----END");
			var trimmed = str.Substring(start + 2, end - (start + 2));
			var p12Data = Convert.FromBase64String(trimmed);
			var p12 = new X509Certificate2(p12Data, password);

			return p12;
		}

		public static bool TryCertificatePassword(string p12Path, string password)
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
	}
}