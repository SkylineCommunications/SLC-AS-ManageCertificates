namespace ManageCertificates_1.Models
{
	using Skyline.DataMiner.Utils.Certificates;

	internal class CertificateIssuer
	{
		public CertificateIssuer(ICertificate certificate, string password)
		{
			Certificate = certificate;
			Password = password;
		}

		public ICertificate Certificate { get; set; }

		public string Password { get; set; }
	}
}