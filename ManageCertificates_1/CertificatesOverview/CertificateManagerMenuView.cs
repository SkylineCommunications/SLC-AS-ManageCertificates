namespace ManageCertificates_1.CertificatesOverview
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	internal class CertificateManagerMenuView : Dialog
	{
		public CertificateManagerMenuView(IEngine engine) : base(engine)
		{
			Title = "Manage Certificates";
			Width = 700;
			MinHeight = 600;
			SetColumnWidth(0, 50);
			SetColumnWidth(1, 110);
			SetColumnWidth(2, 110);
			SetColumnWidth(3, 110);
			SetColumnWidth(4, 110);
			SetColumnWidth(5, 180);

			FinishButton = new Button("Finish");
			CertificateAuthorityButton = new Button("Manage CAs") { Width = 180 };
			SignedCertificateButton = new Button("Manage Signed Certs") { Width = 180 };
			UploadCertificatesButton = new Button("Upload Certs") { Width = 180 };
		}

		public Button FinishButton { get; set; }

		public Button CertificateAuthorityButton { get; set; }

		public Button SignedCertificateButton { get; set; }

		public Button UploadCertificatesButton { get; set; }

		public void Initialize()
		{
			Clear();

			int row = 0;
			AddWidget(CertificateAuthorityButton, row++, 0, 1, 3);
			AddWidget(SignedCertificateButton, row++, 0, 1, 3);
			AddWidget(UploadCertificatesButton, row++, 0, 1, 3);
			AddWidget(FinishButton, row, 0, 1, 3);
		}
	}
}