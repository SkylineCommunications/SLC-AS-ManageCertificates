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
		private Dictionary<string, ICertificate> _certAuthorities;

		public ManageCertificateAuthorityController(IEngine engine, ManageCertificateAuthorityView view)
		{
			this.view = view;
			this.engine = engine;
			view.FinishButton.Pressed += OnNextButtonPressed;
			view.DeleteButton.Pressed += OnDeleteButtonPressed;
			view.CreateButton.Pressed += OnCreateButtonPressed;
			view.TrustButton.Pressed += OnTrustButtonPressed;
		}

		internal event EventHandler<EventArgs> Finish;

		internal event EventHandler<EventArgs> Create;

		internal event EventHandler<EventArgs> Trust;

		private Dictionary<string, ICertificate> CertAuthorities
		{
			get
			{
				_certAuthorities = _certAuthorities ?? CommonActions.GetCertificateAuthorities();
				return _certAuthorities;
			}
		}

		public void Initialize()
		{
			_certAuthorities = null;
			view.Initialize(CertAuthorities);
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
			//var folders = view.GetSelectedCertificates();
			//X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
			//store.Open(OpenFlags.ReadWrite);

			//foreach (var folder in folders)
			//{
			//	var crt = Directory.GetFiles(folder).First(x => x.EndsWith(".crt"));

			//	var cert = new X509Certificate2(crt);
			//	store.Add(cert);
			//}

			//store.Close();

			Trust?.Invoke(this, EventArgs.Empty);
		}
	}
}