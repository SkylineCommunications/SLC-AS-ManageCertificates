namespace ManageCertificates_1.Models
{
	using System.ComponentModel;


	using Newtonsoft.Json;

	public class CertificateClusterModel
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CertificateClusterModel"/> class.
		/// </summary>
		/// <param name="commonName">The common name of the certificate.</param>
		/// <param name="organization">The organization of the certificate.</param>
		/// <param name="organizationalUnit">The organizational unit of the certificate.</param>
		/// <param name="country">The country of the certificate.</param>
		/// <param name="validity">The validity period of the certificate in days.</param>
		/// <param name="keySize">The key size of the certificate in bits.</param>
		public CertificateClusterModel(string commonName, string organization, string organizationalUnit, string country, int validity = 3650, int keySize = 4096)
		{
			CommonName = commonName;
			Organization = organization;
			OrganizationalUnit = organizationalUnit;
			Country = country;
			Validity = validity;
			KeySize = keySize;
		}

		/// <summary>
		/// Gets or sets the common name of the certificate.
		/// </summary>
		[JsonProperty("commonName", NullValueHandling = NullValueHandling.Ignore)]
		public string CommonName { get; set; }

		/// <summary>
		/// Gets or sets the organization of the certificate.
		/// </summary>
		[JsonProperty("organization", NullValueHandling = NullValueHandling.Ignore)]
		public string Organization { get; set; }

		/// <summary>
		/// Gets or sets the organizational unit of the certificate.
		/// </summary>
		[JsonProperty("organizationalUnit", NullValueHandling = NullValueHandling.Ignore)]
		public string OrganizationalUnit { get; set; }

		/// <summary>
		/// Gets or sets the country of the certificate.
		/// </summary>
		[JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
		public string Country { get; set; }

		/// <summary>
		/// Gets or sets the validity period of the certificate.
		/// </summary>
		[DefaultValue(3650)]
		[JsonProperty("validity", DefaultValueHandling = DefaultValueHandling.Populate)]
		public int Validity { get; set; }

		/// <summary>
		/// Gets or sets the key size of the certificate.
		/// </summary>
		[DefaultValue(4096)]
		[JsonProperty("keySize", DefaultValueHandling = DefaultValueHandling.Populate)]
		public int KeySize { get; set; }

		/// <summary>
		/// Deserialize the string to an instance of <see cref="CertificateClusterModel"/>.
		/// </summary>
		/// <param name="json">The serialized JSON string.</param>
		/// <returns>The package info.</returns>
		public static CertificateClusterModel Deserialize(string json)
		{
			return JsonConvert.DeserializeObject<CertificateClusterModel>(json);
		}

		/// <summary>
		/// Get the serialized string of the object.
		/// </summary>
		/// <returns>The serialized string.</returns>
		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}