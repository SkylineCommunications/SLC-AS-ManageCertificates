namespace ManageInstallPackages_1.CreateWindow
{
	using System;
	using System.IO;
	using System.Text.RegularExpressions;

	using ManageCertificates_1;
	using ManageCertificates_1.View;

	using Skyline.DataMiner.Automation;

	internal class UploadController
	{
		private readonly IEngine engine;
		private readonly Regex usernameRegex = new Regex("[A-Za-z0-9]+", RegexOptions.IgnoreCase);
		private readonly UploadView view;

		public UploadController(IEngine engine, UploadView view)
		{
			this.view = view;
			this.engine = engine;
			view.FinishButton.Pressed += OnFinishButtonPressed;
			view.CrtUploadButton.Pressed += OnCrtUploadButtonPressed;
			view.P12UploadButton.Pressed += OnP12UploadButtonPressed;
			view.ResetButton.Pressed += OnResetButtonPressed;
		}

		internal event EventHandler<EventArgs> Finish;

		public void Initialize()
		{
			view.Initialize();
		}

		public void OnCrtUploadButtonPressed(object sender, EventArgs e)
		{
			if (!Validate())
			{
				return;
			}

			view.ToggleP12Section();
			view.CrtUploadButton.IsEnabled = false;

			var certName = view.CertName.Text;
			var path = view.CertificateAuthority.IsChecked ? CommonActions.CaFolderPath + "\\" + certName : CommonActions.ScFolderPath + "\\" + certName;

			UploadCrt(path);
		}

		public void OnFinishButtonPressed(object sender, EventArgs e)
		{
			Finish?.Invoke(this, EventArgs.Empty);
		}

		public void OnP12UploadButtonPressed(object sender, EventArgs e)
		{
			view.FinishButton.IsEnabled = true;
			view.P12UploadButton.IsEnabled = false;

			var certName = view.CertName.Text;
			var path = view.CertificateAuthority.IsChecked ? CommonActions.CaFolderPath + "\\" + certName : CommonActions.ScFolderPath + "\\" + certName;

			UploadP12(path);
		}

		public void OnResetButtonPressed(object sender, EventArgs e)
		{
			view.Initialize();
		}

		public void UploadCrt(string path)
		{
			try
			{
				view.CrtFileSelector.CopyUploadedFiles(path);
				view.FeedBackField.Text = "crt file uploaded.";
			}
			catch (Exception e)
			{
				view.FeedBackField.Text = $"Upload crt failed. {e}";
				view.ResetButton.IsEnabled = true;
			}
		}

		public void UploadP12(string path)
		{
			try
			{
				view.P12FileSelector.CopyUploadedFiles(path);
				view.FeedBackField.Text = "p12 file uploaded.";
			}
			catch (Exception e)
			{
				view.FeedBackField.Text = $"Upload p12 failed. {e}";
				view.ResetButton.IsEnabled = true;
			}
		}

		public bool Validate()
		{
			var certName = view.CertName.Text;
			var path = view.CertificateAuthority.IsChecked ? CommonActions.CaFolderPath + "\\" + certName : CommonActions.ScFolderPath + "\\" + certName;

			if (Directory.Exists(path))
			{
				view.FeedBackField.Text = "Certificate already exists, choose another name and try again";
				view.ResetButton.IsEnabled = true;
				return false;
			}

			if (!usernameRegex.IsMatch(certName))
			{
				view.FeedBackField.Text = "Certificate name should be only alphanumeric. Try again.";
				view.ResetButton.IsEnabled = true;
				return false;
			}

			Directory.CreateDirectory(path);
			view.CertName.IsEnabled = false;
			view.CertificateAuthority.IsEnabled = false;

			return true;
		}
	}
}