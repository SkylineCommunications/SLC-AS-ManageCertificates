namespace ManageCertificates_1.CertificatesOverview
{
	using System;

	using Skyline.DataMiner.Automation;

	internal class CertificateManagerMenuController
	{
		private readonly CertificateManagerMenuView view;
		private readonly IEngine engine;

		public CertificateManagerMenuController(IEngine engine, CertificateManagerMenuView view)
		{
			this.view = view;
			this.engine = engine;
			this.view.FinishButton.Pressed += OnNextButtonPressed;
			this.view.CertificateAuthorityButton.Pressed += OnCAButtonPressed;
			this.view.SignedCertificateButton.Pressed += OnSCButtonPressed;
		}

		internal event EventHandler<EventArgs> ManageCertificateAuthority;

		internal event EventHandler<EventArgs> ManageSignedCertificate;

		internal event EventHandler<EventArgs> Finish;

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
	}
}