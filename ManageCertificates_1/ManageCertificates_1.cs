namespace ManageCertificates_1
{
	using System;
	using System.IO;

	using ManageCertificates_1.CertificatesOverview;
	using ManageCertificates_1.Models;
	using ManageCertificates_1.View;

	using ManageInstallPackages_1.CreateWindow;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private InteractiveController controller;

		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			// engine.ShowUI();
			engine.FindInteractiveClient("Launching Certificate Manager", 100, "user:" + engine.UserLoginName, AutomationScriptAttachOptions.AttachImmediately);
			controller = new InteractiveController(engine);
			engine.Timeout = new TimeSpan(1, 0, 0);
			CertificateClusterModel model = GetCertInfoModelFromInput(engine.GetScriptParam("Input").Value);

			try
			{
				Directory.CreateDirectory(CommonActions.CaFolderPath);
				Directory.CreateDirectory(CommonActions.ScFolderPath);
				ManageCertificateView manageCertificateView = new ManageCertificateView(engine);
				ManageCertificateController manageCertificateController = new ManageCertificateController(engine, manageCertificateView);
				ManageCertificateAuthorityView manageCertificateAuthorityView = new ManageCertificateAuthorityView(engine);
				ManageCertificateAuthorityController manageCertificateAuthorityController = new ManageCertificateAuthorityController(engine, manageCertificateAuthorityView);
				CertificateManagerMenuView certificateManagerMenuView = new CertificateManagerMenuView(engine);
				CertificateManagerMenuController certificateManagerMenuController = new CertificateManagerMenuController(engine, certificateManagerMenuView);

				CreateCertificateAuthorityView createCertificateAuthorityView = new CreateCertificateAuthorityView(engine);
				CreateCertificateAuthorityController createCertificateAuthorityController = new CreateCertificateAuthorityController(engine, createCertificateAuthorityView, model);
				CreateCertificateView createCertificateView = new CreateCertificateView(engine);
				CreateCertificateController createCertificateController = new CreateCertificateController(engine, createCertificateView, model);

				UploadView uploadView = new UploadView(engine);
				UploadController uploadController = new UploadController(engine, uploadView);

				certificateManagerMenuController.Initialize();

				certificateManagerMenuController.Finish += (sender, args) =>
				{
					engine.ExitSuccess("Manage Packages Completed.");
				};

				certificateManagerMenuController.Upload += (sender, args) =>
				{
					uploadController.Initialize();
					controller.ShowDialog(uploadView);
				};

				certificateManagerMenuController.ManageCertificateAuthority += (sender, args) =>
				{
					manageCertificateAuthorityController.Initialize();
					controller.ShowDialog(manageCertificateAuthorityView);
				};

				certificateManagerMenuController.ManageSignedCertificate += (sender, args) =>
				{
					manageCertificateController.Initialize();
					controller.ShowDialog(manageCertificateView);
				};

				manageCertificateAuthorityController.Finish += (sender, args) =>
				{
					certificateManagerMenuController.Initialize();
					controller.ShowDialog(certificateManagerMenuView);
				};

				manageCertificateAuthorityController.Create += (sender, args) =>
				{
					createCertificateAuthorityController.Initialize();
					controller.ShowDialog(createCertificateAuthorityView);
				};

				createCertificateAuthorityController.Finish += (sender, args) =>
				{
					manageCertificateAuthorityController.Initialize();
					controller.ShowDialog(manageCertificateAuthorityView);
				};

				manageCertificateController.Finish += (sender, args) =>
				{
					certificateManagerMenuController.Initialize();
					controller.ShowDialog(certificateManagerMenuView);
				};

				manageCertificateController.Create += (sender, args) =>
				{
					createCertificateController.Initialize();
					controller.ShowDialog(createCertificateView);
				};

				createCertificateController.Finish += (sender, args) =>
				{
					manageCertificateController.Initialize();
					controller.ShowDialog(manageCertificateView);
				};

				TrustCAHelpView trustCAHelpView = new TrustCAHelpView(engine);

				manageCertificateAuthorityController.Trust += (sender, args) =>
				{
					controller.ShowDialog(trustCAHelpView);
				};

				trustCAHelpView.Okay += (sender, args) =>
				{
					manageCertificateController.Initialize();
					controller.ShowDialog(manageCertificateAuthorityView);
				};

				uploadController.Finish += (sender, args) =>
				{
					certificateManagerMenuController.Initialize();
					controller.ShowDialog(certificateManagerMenuView);
				};

				controller.Run(certificateManagerMenuView);
			}
			catch (ScriptAbortException ex)
			{
				if (ex.Message.Contains("ExitFail"))
				{
					HandleknownException(engine, ex);
				}
				else
				{
					// Do nothing as it's an exitsuccess event
				}
			}
			catch (Exception ex)
			{
				HandleUnknownException(engine, ex);
			}
			finally
			{
				engine.AddScriptOutput("status", "success");
			}
		}

		private void ManageCertificateController_Finish(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		private CertificateClusterModel GetCertInfoModelFromInput(string input)
		{
			try
			{
				var model = JsonConvert.DeserializeObject<CertificateClusterModel>(input);

				return model;
			}
			catch (Exception)
			{
				// Do nothing
			}

			return new CertificateClusterModel(string.Empty, string.Empty, string.Empty, string.Empty);
		}

		private void HandleUnknownException(IEngine engine, Exception ex)
		{
			var message = "ERR| An unexpected error occurred, please contact skyline and provide the following information: \n" + ex;
			try
			{
				controller.Run(new ErrorView(engine, ex));
			}
			catch (Exception ex_two)
			{
				engine.GenerateInformation("ERR| Unable to show error message window: " + ex_two);
			}

			engine.GenerateInformation(message);
		}

		private void HandleknownException(IEngine engine, Exception ex)
		{
			var message = "ERR| Script has been canceled because of the following error: \n" + ex;
			try
			{
				controller.Run(new ErrorView(engine, ex));
			}
			catch (Exception ex_two)
			{
				engine.GenerateInformation("ERR| Unable to show error message window: " + ex_two);
			}

			engine.GenerateInformation(message);
		}
	}
}