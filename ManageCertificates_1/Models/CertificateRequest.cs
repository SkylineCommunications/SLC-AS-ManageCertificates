// Ignore Spelling: Dns

namespace ManageCertificates_1.Models
{
	using System;

	using Skyline.DataMiner.Utils.Certificates;

	internal class CertificateRequest
	{
		public enum KeySizes
		{
			/// <summary>
			/// 2048 bits RSA certificates are the norm to be used until 2030.
			/// </summary>
			_2048 = 2048,

			/// <summary>
			/// 3072 bits RSA certificates should be used for security beyond 2030.
			/// </summary>
			_3072 = 3072,
		}

		/// <summary>
		/// Gets or sets the DNS entries that can be used as alternative names for the certificate (OPTIONAL).
		/// </summary>
		public string[] DnsNames { get; set; }

		/// <summary>
		/// Gets or sets the IP addresses that can be used as alternative names for the certificate (OPTIONAL).
		/// </summary>
		public string[] IPAddresses { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the certificate is allowed to sign other certificates.
		/// </summary>
		public bool IsCertificateAuthority { get; set; }

		/// <summary>
		/// Gets or sets the common name of the issuer that will sign the certificate.
		/// </summary>
		public string Issuer { get; set; }

		/// <summary>
		/// Gets or sets the password for the issuers PKCS12 file.
		/// </summary>
		public string IssuerPassword { get; set; }

		/// <summary>
		/// Gets or sets the key size that has to be used.
		/// </summary>
		public int KeySize { get; set; }

		/// <summary>
		/// Gets or sets the password of the PKCS12 file.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Gets or sets the subject (DN) of the certificate.
		/// </summary>
		public DistinguishedName Subject { get; set; }

		/// <summary>
		/// Gets or sets the date from which the certificate is valid.
		/// </summary>
		public DateTime ValidFrom { get; set; }

		/// <summary>
		/// Gets or sets the date until the certificate is valid.
		/// </summary>
		public DateTime ValidUntil { get; set; }
	}
}