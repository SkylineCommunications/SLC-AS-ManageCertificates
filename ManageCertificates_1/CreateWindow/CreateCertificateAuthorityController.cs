namespace ManageInstallPackages_1.CreateWindow
{
	using System;
	using System.IO;
	using System.Text.RegularExpressions;

	using ManageCertificates_1;
	using ManageCertificates_1.Models;

	using Skyline.DataMiner.Automation;

	internal class CreateCertificateAuthorityController
	{
		private readonly CreateCertificateAuthorityView view;
		private readonly IEngine engine;
		private readonly string folderPath;
		private readonly CertificateClusterModel model;
		private readonly Regex passwordRegex = new Regex("[A-Za-z0-9]{6,}", RegexOptions.IgnoreCase);

		public CreateCertificateAuthorityController(IEngine engine, CreateCertificateAuthorityView view, string folderPath, CertificateClusterModel model)
		{
			this.view = view;
			this.engine = engine;
			this.model = model;
			this.folderPath = folderPath;
			view.FinishButton.Pressed += OnFinishButtonPressed;
			view.CreateButton.Pressed += OnCreateButtonPressed;
		}

		internal event EventHandler<EventArgs> Finish;

		public void Initialize()
		{
			view.Initialize();

			FillInputs();
		}

		public void OnFinishButtonPressed(object sender, EventArgs e)
		{
			Finish?.Invoke(this, EventArgs.Empty);
		}

		public void OnCreateButtonPressed(object sender, EventArgs e)
		{
			var commonName = view.CommonName.Text;
			var password = view.Password.Password;

			if (Directory.Exists(folderPath + $"\\{commonName}"))
			{
				view.SetFeedback("CA already exists, please delete the existing one before creating a new one.");
				return;
			}

			if (!passwordRegex.IsMatch(password))
			{
				view.SetFeedback("Password should be alphanumeric and contain at least 6 characters.");
				return;
			}

			UpdateModel();


			var certificateInfo = new CertificateModel(commonName, model, password);

			bool success = CreateRootCA(certificateInfo);

			if (success)
			{
				view.SetFeedback("CA successfully created.");
			}
			else
			{
				view.SetFeedback("Failed to create CA.");
			}
		}

		private bool CreateRootCA(CertificateModel certificateModel)
		{
			try
			{
				var this_folder = folderPath + $"\\{certificateModel.CommonName}";
				Directory.CreateDirectory(this_folder);

				CommonActions.CreateSelfSignedCertificateAuthority(this_folder, certificateModel);

				return true;
			}
			catch
			{
				return false;
			}
		}

		private void FillInputs()
		{
			view.OrganizationalUnit.Text = string.IsNullOrEmpty(model.OrganizationalUnit) ? string.Empty : model.OrganizationalUnit;
			view.Organization.Text = string.IsNullOrEmpty(model.Organization) ? string.Empty : model.Organization;
			view.Country.Text = string.IsNullOrEmpty(model.Country) ? string.Empty : model.Country;
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