namespace ManageCertificates_1.View
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class UploadView : Dialog
	{
		public UploadView(IEngine engine) : base(engine)
		{
			this.Title = "Upload a certificate";
			this.Header = new Label("Please follow the instructions below:");

			this.CrtUploadButton = new Button("Upload crt");
			this.P12UploadButton = new Button("Upload p12");

			this.FinishButton = new Button("Finish");

			this.ResetButton = new Button("Reset");

			Width = 500;
			Height = 700;
			SetColumnWidth(0, 110);
			SetColumnWidth(1, 110);
			SetColumnWidth(2, 110);
			SetColumnWidth(3, 110);
		}

		public Label Header { get; set; }

		public TextBox CertName { get; set; }

		public Button FinishButton { get; set; }

		public Button ResetButton { get; set; }

		public FileSelector CrtFileSelector { get; set; }

		public FileSelector P12FileSelector { get; set; }

		public Button CrtUploadButton { get; set; }

		public Button P12UploadButton { get; set; }

		public Section CrtSection { get; set; }

		public Section P12Section { get; set; }

		public CheckBox CertificateAuthority { get; set; }

		public Label FeedBackField { get; set; }

		public void Initialize()
		{
			Clear();
			
			if (CrtSection != null)
			{
				CrtSection.Clear();
			}

			if (P12Section != null)
			{
				P12Section.Clear();
			}

			CrtUploadButton.IsEnabled = true;

			int row = 0;

			this.CertName = new TextBox()
			{
				Width = 250,
			};

			this.CertificateAuthority = new CheckBox("Is Certificate Authority?");

			this.FeedBackField = new Label(string.Empty)
			{
				Style = TextStyle.Bold,
			};

			this.FinishButton.IsEnabled = false;
			this.ResetButton.IsEnabled = false;

			this.CrtSection = GetCrtSection();
			this.P12Section = GetP12Section();
			ToggleP12Section();

			this.AddWidget(Header, row++, 0, 1, 3);

			this.AddWidget(CertificateAuthority, row++, 0, 1, 3);

			this.AddWidget(new Label("Input a certificate name"), row++, 0 ,1, 3);
			this.AddWidget(CertName, row++, 0, 1, 3);

			this.AddSection(CrtSection, row++, 0);
			row += CrtSection.RowCount;

			this.AddSection(P12Section, row++, 0);
			row += P12Section.RowCount;

			this.AddWidget(FeedBackField, row++, 0, 1, 3);
			this.AddWidget(FinishButton, row++, 3, 1, 1);
			this.AddWidget(ResetButton, row, 3, 1, 1);
		}

		public Section GetCrtSection()
		{
			Section section = new Section();
			int row = 0;

			this.CrtFileSelector = new FileSelector();

			section.AddWidget(new Label("Select .crt file to upload:"), row++, 0, 1, 3);
			section.AddWidget(CrtFileSelector, row, 0, 1, 2);
			section.AddWidget(CrtUploadButton, row++, 3, 1, 1);

			return section;
		}

		public Section GetP12Section()
		{
			Section section = new Section();
			int row = 0;

			this.P12FileSelector = new FileSelector();

			section.AddWidget(new Label("Select .p12 file to upload:"), row++, 0, 1, 3);
			section.AddWidget(P12FileSelector, row, 0, 1, 2);
			section.AddWidget(P12UploadButton, row++, 3, 1, 1);

			return section;
		}

		public void ToggleP12Section()
		{
			P12Section.IsEnabled = !P12Section.IsEnabled;
			P12Section.IsVisible = !P12Section.IsVisible;
		}
	}
}