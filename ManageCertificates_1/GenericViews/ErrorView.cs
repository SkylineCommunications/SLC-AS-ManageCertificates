namespace ManageCertificates_1.View
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	public class ErrorView : Dialog
	{
		public ErrorView(IEngine engine, Exception exception) : base(engine)
		{
			this.Title = "Unexpected error";
			this.ContactSkyline = new Label("Please contact skyline and provide the following information:");
			this.ErrorText = new TextBox();
			this.ErrorText.IsMultiline = true;
			this.ErrorText.Height = 320;
			this.ErrorText.Width = 500;
			this.ErrorText.Text = exception.ToString();
			this.OkButton = new Button("Confirm");

			this.AddWidget(ContactSkyline, 0, 0);
			this.AddWidget(ErrorText, 1, 0);
			this.AddWidget(OkButton, 2, 0);

			OkButton.Pressed += (s, e) => engine.ExitSuccess(exception.ToString());
		}

		public Label ContactSkyline { get; set; }

		public TextBox ErrorText { get; set; }

		public Button OkButton { get; set; }
	}
}