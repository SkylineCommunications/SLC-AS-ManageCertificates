namespace ManageCertificates_1.CertificatesOverview
{
	using System.Collections.Generic;

	using ManageCertificates_1.View;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.Certificates;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	internal class ManageCertificateView : Dialog
	{
		private readonly string[] headers = { "CN", "DN", "Validity", "Issuer" };

		public ManageCertificateView(IEngine engine) : base(engine)
		{
			Title = "Manage Certificates";
			Width = 900;
			SetColumnWidth(0, 50);
			SetColumnWidth(1, 160);
			SetColumnWidth(2, 260);
			SetColumnWidth(3, 160);
			SetColumnWidth(4, 260);

			FinishButton = new Button("Finish");
			Certificates = new TableSelection(engine, headers);
			DeleteButton = new Button("Delete");
			CreateButton = new Button("Create New Cert...") { Width = 180 };
		}

		public Button FinishButton { get; set; }

		public Button DeleteButton { get; set; }

		public Button CreateButton { get; set; }

		public TableSelection Certificates { get; set; }

		public void Initialize(Dictionary<string, ICertificate> certificates)
		{
			Clear();
			int row = 0;

			// First Column
			var tableRows = GetTableRows(certificates);
			Certificates.AddToDialog(this, tableRows, ref row);

			AddWidget(CreateButton, row++, 4, 1, 1, HorizontalAlignment.Right);
			AddWidget(new Label(string.Empty), row++, 0);
			AddWidget(DeleteButton, row, 1);
			AddWidget(FinishButton, row, 4, 1, 1, HorizontalAlignment.Right);
		}

		internal IEnumerable<string> GetSelectedCertificates()
		{
			return Certificates.Selected;
		}

		private Dictionary<string, Widget[]> GetTableRows(Dictionary<string, ICertificate> certificates)
		{
			Dictionary<string, Widget[]> tableRows = new Dictionary<string, Widget[]>();
			foreach (var certificate in certificates)
			{
				tableRows[certificate.Key] = new Widget[]
				{
					new Label(certificate.Value.Subject.CommonName){ Width = 150 },
					new Label(certificate.Value.Subject.Value){ Width = 250 },
					new Label(certificate.Value.CertificateFile.NotAfter.ToString("dd MMM yyyy")) { Width = 150 },
					new Label(certificate.Value.Subject == certificate.Value.Issuer ? "Self-Signed" : certificate.Value.Issuer.Value){ Width = 250 },
				};
			}

			return tableRows;
		}
	}
}