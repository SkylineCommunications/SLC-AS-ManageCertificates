namespace ManageCertificates_1.CertificatesOverview
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Security.Cryptography.X509Certificates;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.Certificates;

	internal class ManageCertificateAuthorityController
	{
		private readonly ManageCertificateAuthorityView view;
		private readonly IEngine engine;
		private readonly string folderPath;
		private Dictionary<string, ICertificate> certificates;

		public ManageCertificateAuthorityController(IEngine engine, ManageCertificateAuthorityView view, string folderPath)
		{
			this.view = view;
			this.engine = engine;
			this.folderPath = folderPath;
			view.FinishButton.Pressed += OnNextButtonPressed;
			view.DeleteButton.Pressed += OnDeleteButtonPressed;
			view.CreateButton.Pressed += OnCreateButtonPressed;
			view.TrustButton.Pressed += OnTrustButtonPressed;
		}

		internal event EventHandler<EventArgs> Finish;

		internal event EventHandler<EventArgs> Create;

		public void Initialize()
		{
			certificates = CommonActions.GetRootCertificates(folderPath);

			view.Initialize(certificates);
		}

		private void OnCreateButtonPressed(object sender, EventArgs e)
		{
			Create?.Invoke(this, EventArgs.Empty);
		}

		private void OnDeleteButtonPressed(object sender, EventArgs e)
		{
			var folders = view.GetSelectedCertificates();
			foreach (var folder in folders)
			{
				engine.GenerateInformation($"Deleting {folder}");
				var dir = new DirectoryInfo(folder);
				dir.Delete(true);
			}

			Initialize();
		}

		private void OnNextButtonPressed(object sender, EventArgs e)
		{
			Finish?.Invoke(this, EventArgs.Empty);
		}

		private void OnTrustButtonPressed(object sender, EventArgs e)
		{
			var folders = view.GetSelectedCertificates();

			foreach (var folder in folders)
			{
				var crt = Directory.GetFiles(folder).First(x => x.EndsWith(".crt"));

				var cert = new X509Certificate2(crt);
				using (X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
				{
					store.Open(OpenFlags.ReadWrite);
					store.Add(cert);
				}
			}
		}
	}
}