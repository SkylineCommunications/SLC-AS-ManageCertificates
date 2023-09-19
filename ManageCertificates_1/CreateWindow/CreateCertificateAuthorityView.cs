namespace ManageInstallPackages_1.CreateWindow
{
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	internal class CreateCertificateAuthorityView : Dialog
	{
		private const int TextBoxWidth = 320;
		private const int TextBoxHeight = 100;

		public CreateCertificateAuthorityView(IEngine engine) : base(engine)
		{
			Title = "Create CA";
			Width = 400;
			Height = 450;
			SetColumnWidth(0, 110);
			SetColumnWidth(1, 110);
			SetColumnWidth(2, 110);

			FinishButton = new Button("Finish");
			CreateButton = new Button("Create");
			Feedback = new Label() { IsVisible = false };
		}

		public Button FinishButton { get; set; }

		public Button CreateButton { get; set; }

		public TextBox CommonName { get; set; }

		public TextBox OrganizationalUnit { get; set; }

		public TextBox Organization { get; set; }

		public TextBox Country { get; set; }

		public TextBox Validity { get; set; }

		public TextBox KeySize { get; set; }

		public PasswordBox Password { get; set; }

		internal Label Feedback { get; set; }

		public void Initialize()
		{
			Clear();
			SetFeedback(string.Empty);
			int row = 0;

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
			AddWidget(GetHeader("Set a Password"), row++, 0, 1, 3);
			AddWidget(Password, row++, 0, 1, 3);

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