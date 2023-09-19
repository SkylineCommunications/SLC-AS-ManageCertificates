namespace ManageCertificates_1.Models
{
	using System.Collections.Generic;

	public class CertificateModel
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CertificateModel"/> class.
		/// </summary>
		/// <param name="commonName">The common name of the certificate.</param>
		/// <param name="model">The certificate info model containing global information.</param>
		/// <param name="password">The password to access the p12 certificate.</param>
		public CertificateModel(string commonName, CertificateClusterModel model, string password)
		{
			CommonName = commonName;
			Organization = model.Organization;
			OrganizationalUnit = model.OrganizationalUnit;
			Country = model.Country;
			Validity = model.Validity;
			KeySize = model.KeySize;
			Password = password;
		}

		public CertificateModel(string commonName, string organization, string organizationalUnit, string country, int validity, int keySize, string password, string ipAddress, string[] dnsNames, string certificateName)
		{
			CommonName = commonName;
			Organization = organization;
			OrganizationalUnit = organizationalUnit;
			Country = country;
			Validity = validity;
			KeySize = keySize;
			Password = password;
			IPAddress = ipAddress;
			DNSNames = dnsNames;
			CertificateName = certificateName;
		}

		/// <summary>
		/// Gets or sets the common name of the certificate.
		/// </summary>
		public string CommonName { get; set; }

		/// <summary>
		/// Gets or sets the organization of the certificate.
		/// </summary>
		public string Organization { get; set; }

		/// <summary>
		/// Gets or sets the organizational unit of the certificate.
		/// </summary>
		public string OrganizationalUnit { get; set; }

		/// <summary>
		/// Gets or sets the country of the certificate.
		/// </summary>
		public string Country { get; set; }

		/// <summary>
		/// Gets or sets the validity period of the certificate.
		/// </summary>
		public int? Validity { get; set; }

		/// <summary>
		/// Gets or sets the key size of the certificate.
		/// </summary>
		public int? KeySize { get; set; }

		/// <summary>
		/// Gets or sets the password of the certificate.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Gets or sets the IP address of the certificate.
		/// </summary>
		public string IPAddress { get; set; }

		/// <summary>
		/// Gets or sets the DNS names of the certificate.
		/// </summary>
		public string[] DNSNames { get; set; }

		/// <summary>
		/// Gets or sets the certificate name, e.g. ***.crt.
		/// </summary>
		public string CertificateName { get; set; }

		/// <summary>
		/// Gets the distinguished name of the certificate.
		/// </summary>
		public string DistinguishedName
		{
			get
			{
				var builder = new List<string>();

				builder.Add($"CN={CommonName}");

				if (!string.IsNullOrEmpty(OrganizationalUnit))
				{
					builder.Add($"OU={OrganizationalUnit}");
				}

				if (!string.IsNullOrEmpty(Organization))
				{
					builder.Add($"O={Organization}");
				}

				if (!string.IsNullOrEmpty(Country))
				{
					builder.Add($"C={Country}");
				}

				return string.Join(",", builder);
			}
		}
	}
}