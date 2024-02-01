namespace ManageInstallPackages_1.CreateWindow
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;

	using ManageCertificates_1;
	using ManageCertificates_1.Models;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.Certificates;

	internal class CreateCertificateAuthorityController
	{
		private readonly IEngine engine;
		private readonly CertificateClusterModel model;
		private readonly Regex passwordRegex = new Regex("[A-Za-z0-9]{6,}", RegexOptions.IgnoreCase);
		private readonly CreateCertificateAuthorityView view;
		private Dictionary<string, ICertificate> certAuthorities;

		public CreateCertificateAuthorityController(IEngine engine, CreateCertificateAuthorityView view, CertificateClusterModel model)
		{
			this.view = view;
			this.engine = engine;
			this.model = model;
			view.FinishButton.Pressed += OnFinishButtonPressed;
			view.CreateButton.Pressed += OnCreateButtonPressed;
			view.CertificateAuthorities.Changed += OnCertificateAuthorityChanged;
		}

		internal event EventHandler<EventArgs> Finish;

		public void Initialize()
		{
			certAuthorities = CommonActions.GetCertificateAuthorities().ToDictionary(x => x.Value.Subject.CommonName, x => x.Value);
			view.Initialize(certAuthorities);
			FillInputs();
		}

		public void OnCertificateAuthorityChanged(object sender, EventArgs e)
		{
			FillInputs();
		}

		public void OnCreateButtonPressed(object sender, EventArgs e)
		{
			UpdateModel();
			var certRequest = new CertificateRequest
			{
				IsCertificateAuthority = true,
				Subject = DistinguishedName.GetDistinguishedName(
					view.CommonName.Text,
					view.Organization.Text,
					view.OrganizationalUnit.Text,
					view.Country.Text),
				DnsNames = null,
				IPAddresses = null,
				KeySize = Convert.ToInt32(view.KeySize.Text),
				Password = view.Password.Password,
				ValidFrom = DateTime.Now,
				ValidUntil = DateTime.Now.AddDays(Convert.ToInt32(view.Validity.Text)),
			};

			if (!passwordRegex.IsMatch(certRequest.Password))
			{
				view.SetFeedback("Password should be alphanumeric and contain at least 6 characters.");
				return;
			}

			if (!view.CertificateAuthorities.Selected.Equals("None"))
			{
				certRequest.Issuer = view.CertificateAuthorities.Selected;
				certRequest.IssuerPassword = view.CAPassword.Password;
			}

			try
			{
				CommonActions.CreateCertificate(certRequest, certAuthorities);
				view.SetFeedback("CA successfully created.");
			}
			catch (Exception ex)
			{
				engine.GenerateInformation("Exception: " + ex);
				view.SetFeedback(ex.Message);
			}
		}

		public void OnFinishButtonPressed(object sender, EventArgs e)
		{
			Finish?.Invoke(this, EventArgs.Empty);
		}

		private void FillInputs()
		{
			var rootCA = view.CertificateAuthorities.Selected;
			if (rootCA != "None")
			{
				view.CAPassword.IsEnabled = true;
				view.OrganizationalUnit.Text = certAuthorities[rootCA].Subject.OrganizationalUnitName;
				view.Organization.Text = certAuthorities[rootCA].Subject.OrganizationName;
				view.Country.Text = certAuthorities[rootCA].Subject.CountryName;
			}
			else
			{
				view.CAPassword.IsEnabled = false;
				view.OrganizationalUnit.Text = string.IsNullOrEmpty(model.OrganizationalUnit) ? string.Empty : model.OrganizationalUnit;
				view.Organization.Text = string.IsNullOrEmpty(model.Organization) ? string.Empty : model.Organization;
				view.Country.Text = string.IsNullOrEmpty(model.Country) ? string.Empty : model.Country;
			}

			view.Validity.Text = model.Validity == 0 ? string.Empty : model.Validity.ToString();
			view.KeySize.Text = model.KeySize == 0 ? string.Empty : model.KeySize.ToString();
		}

		private void UpdateModel()
		{
			model.OrganizationalUnit = view.OrganizationalUnit.Text;
			model.Organization = view.Organization.Text;
			model.Country = view.Country.Text;
			model.Validity = Int32.Parse(view.Validity.Text);
			model.KeySize = Int32.Parse(view.KeySize.Text);
		}
	}
}