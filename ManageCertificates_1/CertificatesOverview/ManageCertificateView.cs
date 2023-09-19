namespace ManageCertificates_1.CertificatesOverview
{
	using System.Collections.Generic;

	using ManageCertificates_1.View;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.Certificates;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	internal class ManageCertificateView : Dialog
	{
		private readonly string[] headers = { "DistinguishedName", "Validity", "Issuer" };

		public ManageCertificateView(IEngine engine) : base(engine)
		{
			Title = "Manage Certificates";
			Width = 700;
			Height = 450;
			SetColumnWidth(0, 50);
			SetColumnWidth(1, 220);
			SetColumnWidth(2, 160);
			SetColumnWidth(3, 220);

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

			if (Certificates != null)
			{
				Certificates.Clear();
			}

			// First Column
			var tableRows = GetTableRows(certificates);
			Certificates.Initialize(tableRows);
			AddSection(Certificates, row, 0);
			row += Certificates.RowCount;

			AddWidget(DeleteButton, row, 2);
			AddWidget(CreateButton, row++, 3, 1, 1, HorizontalAlignment.Right);
			AddWidget(new Label(string.Empty), row++, 0);
			AddWidget(FinishButton, row, 3, 1, 1, HorizontalAlignment.Right);
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
					new Label(certificate.Value.CertificateInfo.DistinguishedName),
					new Label(certificate.Value.CertificateFile.NotAfter.ToString()),
					new Label(certificate.Value.Issuer),
				};
			}

			return tableRows;
		}
	}
}