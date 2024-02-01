namespace ManageCertificates_1.CertificatesOverview
{
	using System;

	using Skyline.DataMiner.Automation;

	internal class CertificateManagerMenuController
	{
		private readonly CertificateManagerMenuView view;
#pragma warning disable S4487 // Unread "private" fields should be removed (for debugging purposes)
		private readonly IEngine engine;
#pragma warning restore S4487 // Unread "private" fields should be removed

		public CertificateManagerMenuController(IEngine engine, CertificateManagerMenuView view)
		{
			this.view = view;
			this.engine = engine;
			this.view.FinishButton.Pressed += OnNextButtonPressed;
			this.view.CertificateAuthorityButton.Pressed += OnCAButtonPressed;
			this.view.SignedCertificateButton.Pressed += OnSCButtonPressed;
			this.view.UploadCertificatesButton.Pressed += OnUploadButtonPressed;
		}

		internal event EventHandler<EventArgs> ManageCertificateAuthority;

		internal event EventHandler<EventArgs> ManageSignedCertificate;

		internal event EventHandler<EventArgs> Finish;

		internal event EventHandler<EventArgs> Upload;

		public void Initialize()
		{
			view.Initialize();
		}

		private void OnSCButtonPressed(object sender, EventArgs e)
		{
			ManageSignedCertificate?.Invoke(this, EventArgs.Empty);
		}

		private void OnCAButtonPressed(object sender, EventArgs e)
		{
			ManageCertificateAuthority?.Invoke(this, EventArgs.Empty);
		}

		private void OnNextButtonPressed(object sender, EventArgs e)
		{
			Finish?.Invoke(this, EventArgs.Empty);
		}

		private void OnUploadButtonPressed(object sender, EventArgs e)
		{
			Upload?.Invoke(this, EventArgs.Empty);
		}
	}
}