﻿namespace ManageCertificates_1.CertificatesOverview
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.Certificates;

	internal class ManageCertificateController
	{
		private readonly ManageCertificateView view;
		private readonly IEngine engine;
		private readonly string folderPath;
		private Dictionary<string, ICertificate> certificates;

		public ManageCertificateController(IEngine engine, ManageCertificateView view, string folderPath)
		{
			this.view = view;
			this.engine = engine;
			this.folderPath = folderPath;
			view.FinishButton.Pressed += OnNextButtonPressed;
			view.DeleteButton.Pressed += OnDeleteButtonPressed;
			view.CreateButton.Pressed += OnCreateButtonPressed;
		}

		internal event EventHandler<EventArgs> Finish;

		internal event EventHandler<EventArgs> Create;

		public void Initialize()
		{
			certificates = new Dictionary<string, ICertificate>();
			foreach (string folder in Directory.GetDirectories(folderPath))
			{
				var folderName = folder.Substring(folder.LastIndexOf("\\") + 1);
				try
				{
					var p12 = Directory.GetFiles(folder).First(x => x.EndsWith($"{folderName}.p12"));
					var crt = Directory.GetFiles(folder).First(x => x.EndsWith($"{folderName}.crt"));
					ICertificate cert = CertificatesFactory.GetCertificate(crt, p12);
					certificates[folder] = cert;
				}
				catch
				{
					// do nothing
				}
			}

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
	}
}