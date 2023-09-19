namespace ManageInstallPackages_1.CreateWindow
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	using ManageCertificates_1;
	using ManageCertificates_1.Models;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.Certificates;

	internal class CreateCertificateController
	{
		private readonly CreateCertificateView view;
		private readonly IEngine engine;
		private readonly string caFolderPath;
		private readonly string scFolderPath;
		private readonly CertificateClusterModel model;
		private Dictionary<string, ICertificate> rootCertificates;

		public CreateCertificateController(IEngine engine, CreateCertificateView view, string caFolderPath, string scFolderPath, CertificateClusterModel model)
		{
			this.view = view;
			this.engine = engine;
			this.model = model;
			this.caFolderPath = caFolderPath;
			this.scFolderPath = scFolderPath;
			view.FinishButton.Pressed += OnFinishButtonPressed;
			view.CreateButton.Pressed += OnCreateButtonPressed;
		}

		internal event EventHandler<EventArgs> Finish;

		public void Initialize()
		{
			rootCertificates = CommonActions.GetRootCertificates(caFolderPath).ToDictionary(x => x.Value.CertificateInfo.DistinguishedName, x => x.Value);

			view.Initialize(rootCertificates);
			view.CertificateAuthorities.Changed += OnCertificateAuthorityChanged;

			FillInputs();
		}

		public void OnCertificateAuthorityChanged(object sender, EventArgs e)
		{
			FillInputs();
		}

		public void OnFinishButtonPressed(object sender, EventArgs e)
		{
			Finish?.Invoke(this, EventArgs.Empty);
		}

		public void OnCreateButtonPressed(object sender, EventArgs e)
		{
			if (Directory.Exists(scFolderPath + $"\\{view.CertificateName.Text}"))
			{
				view.SetFeedback("Cert already exists, please delete the existing one before creating a new one.");
				return;
			}

			var commonName = view.CommonName.Text;
			var organization = view.Organization.Text;
			var organizationalUnit = view.OrganizationalUnit.Text;
			var country = view.Country.Text;
			var validity = Convert.ToInt32(view.Validity.Text);
			var keySize = Convert.ToInt32(view.KeySize.Text);
			var password = view.Password.Password;
			var ipAddress = view.IPAddress.Text;
			var dnsNames = view.DNSNames.Text.Split(' ');
			var certificateName = view.CertificateName.Text;
			var certificateInfo = new CertificateModel(commonName, organization, organizationalUnit, country, validity, keySize, password, ipAddress, dnsNames, certificateName);

			if (view.CertificateAuthorities.Selected.Equals("None"))
			{
				CreateCert(certificateInfo);
			}
			else
			{
				var certificateIssuer = new CertificateIssuer(rootCertificates[view.CertificateAuthorities.Selected], view.CAPassword.Password);

				CreateCert(certificateInfo, certificateIssuer);
			}
		}

		private bool CreateCert(CertificateModel certificateModel)
		{
			try
			{
				var this_folder = scFolderPath + $"\\{view.CertificateName.Text}";
				Directory.CreateDirectory(this_folder);

				CommonActions.CreateCertificate(this_folder, certificateModel);

				view.SetFeedback("CA successfully created.");
				return true;
			}
			catch (Exception e)
			{
				engine.GenerateInformation("failed because of: " + e);
				view.SetFeedback("Failed to create CA.");
				return false;
			}
		}

		private bool CreateCert(CertificateModel certificateModel, CertificateIssuer certificateIssuer)
		{
			if (!CommonActions.TryCertificatePassword(certificateIssuer.Certificate.P12Path, certificateIssuer.Password))
			{
				view.SetFeedback("Password provided for CA p12 file is wrong.");
				return false;
			}

			try
			{
				var this_folder = scFolderPath + $"\\{view.CertificateName.Text}";
				Directory.CreateDirectory(this_folder);

				CommonActions.CreateCertificate(this_folder, certificateModel, certificateIssuer);

				view.SetFeedback("CA successfully created.");
				return true;
			}
			catch (Exception e)
			{
				engine.GenerateInformation("failed because of: " + e);
				view.SetFeedback("Failed to create CA.");
				return false;
			}
		}

		private void FillInputs()
		{
			var rootCA = view.CertificateAuthorities.Selected;

			if (rootCA != "None")
			{
				view.CAPassword.IsEnabled = true;
				view.OrganizationalUnit.Text = rootCertificates[rootCA].CertificateInfo.OrganizationalUnit;
				view.Organization.Text = rootCertificates[rootCA].CertificateInfo.Organization;
				view.Country.Text = rootCertificates[rootCA].CertificateInfo.Country;
			}
			else
			{
				view.CAPassword.IsEnabled = false;
				view.OrganizationalUnit.Text = string.Empty;
				view.Organization.Text = string.Empty;
				view.Country.Text = string.Empty;
			}

			view.CommonName.Text = model.CommonName;
			view.Validity.Text = model.Validity == 0 ? string.Empty : model.Validity.ToString();
			view.KeySize.Text = model.KeySize == 0 ? string.Empty : model.KeySize.ToString();
		}
	}
}