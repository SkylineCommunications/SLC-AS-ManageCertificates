namespace ManageInstallPackages_1.CreateWindow
{
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.Certificates;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	internal class CreateCertificateView : Dialog
	{
		private const int TextBoxWidth = 320;

		public CreateCertificateView(IEngine engine) : base(engine)
		{
			Title = "Create Cert";
			Width = 400;
			SetColumnWidth(0, 110);
			SetColumnWidth(1, 110);
			SetColumnWidth(2, 110);

			FinishButton = new Button("Finish");
			CreateButton = new Button("Create");
			CertificateAuthorities = new DropDown(new[] { "None" }) { Width = TextBoxWidth };
			Feedback = new Label { IsVisible = false };
		}

		public DropDown CertificateAuthorities { get; set; }

		public Button FinishButton { get; set; }

		public Button CreateButton { get; set; }

		public TextBox CertificateName { get; set; }

		public TextBox CommonName { get; set; }

		public TextBox OrganizationalUnit { get; set; }

		public TextBox Organization { get; set; }

		public TextBox Country { get; set; }

		public TextBox Validity { get; set; }

		public TextBox KeySize { get; set; }

		public TextBox IPAddress { get; set; }

		public TextBox DNSNames { get; set; }

		public PasswordBox Password { get; set; }

		public PasswordBox CAPassword { get; set; }

		internal Label Feedback { get; set; }

		public void Initialize(Dictionary<string, ICertificate> certificateAuthorities)
		{
			Clear();
			SetFeedback(string.Empty);
			int row = 0;

			if (certificateAuthorities.Any())
			{
				CertificateAuthorities.Options = certificateAuthorities.Keys.Prepend("None");
			}

			AddWidget(GetHeader("Choose a Certificate Authority"), row++, 0, 1, 3);
			AddWidget(CertificateAuthorities, row++, 0, 1, 3);

			CAPassword = new PasswordBox(true) { Width = TextBoxWidth };
			AddWidget(GetHeader("CA Password"), row++, 0, 1, 3);
			AddWidget(CAPassword, row++, 0, 1, 3);

			CertificateName = GetTextBox();
			AddWidget(GetHeader("Certificate Name (***.crt)"), row++, 0, 1, 3);
			AddWidget(CertificateName, row++, 0, 1, 3);

			CommonName = GetTextBox();
			AddWidget(GetHeader("Common Name"), row++, 0, 1, 3);
			AddWidget(CommonName, row++, 0, 1, 3);

			OrganizationalUnit = GetTextBox();
			AddWidget(GetHeader("Organizational Unit"), row++, 0, 1, 3);
			AddWidget(OrganizationalUnit, row++, 0, 1, 3);

			Organization = GetTextBox();
			AddWidget(GetHeader("Organization"), row++, 0, 1, 3);
			AddWidget(Organization, row++, 0, 1, 3);

			Country = GetTextBox();
			AddWidget(GetHeader("Country"), row++, 0, 1, 3);
			AddWidget(Country, row++, 0, 1, 3);

			Validity = GetTextBox();
			AddWidget(GetHeader("Validity"), row++, 0, 1, 3);
			AddWidget(Validity, row++, 0, 1, 3);

			KeySize = GetTextBox();
			AddWidget(GetHeader("Key Size"), row++, 0, 1, 3);
			AddWidget(KeySize, row++, 0, 1, 3);

			Password = new PasswordBox(true) { Width = TextBoxWidth };
			AddWidget(GetHeader("Password for this Certificate"), row++, 0, 1, 3);
			AddWidget(Password, row++, 0, 1, 3);

			AddWidget(GetHeader("---Extensions---"), row++, 0, 1, 3);
			IPAddress = GetTextBox();
			AddWidget(GetHeader("IP Address (optional)"), row++, 0, 1, 3);
			AddWidget(IPAddress, row++, 0, 1, 3);

			DNSNames = GetTextBox();
			AddWidget(GetHeader("DNS Names (space separated, optional)"), row++, 0, 1, 3);
			AddWidget(DNSNames, row++, 0, 1, 3);

			AddWidget(CreateButton, row++, 2);
			AddWidget(FinishButton, row++, 2);
			AddWidget(Feedback, row, 0, 1, 3);
		}

		public void SetFeedback(string message)
		{
			Feedback.IsVisible = true;
			Feedback.Text = message;
		}

		private TextBox GetTextBox()
		{
			TextBox textBox = new TextBox
			{
				Width = TextBoxWidth,
			};

			return textBox;
		}

		private Label GetHeader(string s)
		{
			var label = new Label(s)
			{
				Style = TextStyle.Bold,
			};

			return label;
		}
	}
}