namespace ManageCertificates_1.View
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class TrustCAHelpView : Dialog
	{
		public TrustCAHelpView(IEngine engine) : base(engine)
		{
			this.Title = "Manually Trust a Certificate Authority";
			this.Header = new Label("Please follow the instructions below:");
			this.HelpText = new TextBox();
			this.HelpText.IsMultiline = true;
			this.HelpText.Height = 320;
			this.HelpText.Width = 880;
			this.HelpText.Text =
				"NOTE: You are now adding a Certificate as a Trusted Root Certificate Authority. Only do this if the certificate comes from a recognized source and is trustable." + Environment.NewLine +
				"1. Navigate to C:\\Skyline DataMiner\\Documents\\DMA_COMMON_DOCUMENTS\\Certificates\\CertificateAuthorities on the DataMiner server." + Environment.NewLine +
				"2. Find the .crt file of the Certificate Authority (located in a folder with the same name)." + Environment.NewLine +
				"3. Double click the .crt file and click 'Install Certificate...'." + Environment.NewLine +
				"4. Select 'Local Machine' and press 'Next'." + Environment.NewLine +
				"5. Select 'Place all certificates in the following store' and click 'Browse'." + Environment.NewLine +
				"6. Select 'Trusted Root Certification Authorities' folder and press 'OK'. Then press 'Next'." + Environment.NewLine +
				"7. Press 'Finish'.";

			this.OkButton = new Button("Confirm");

			this.AddWidget(Header, 0, 0);
			this.AddWidget(HelpText, 1, 0);
			this.AddWidget(OkButton, 2, 0);

			OkButton.Pressed += (s, e) => Okay?.Invoke(this, EventArgs.Empty);
		}

		internal event EventHandler<EventArgs> Okay;

		public Label Header { get; set; }

		public TextBox HelpText { get; set; }

		public Button OkButton { get; set; }
	}
}