using Microsoft.Win32;
using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO.Compression;
using System.Diagnostics;

namespace PC_Creator_Mod_Launcher
{
	/// <summary>
	/// Interaction logic for SettingsPage.xaml
	/// </summary>
	public partial class SettingsPage : UserControl
	{
		private bool firstLoadCompleted = false;

		private OpenFileDialog openFileDialog = new OpenFileDialog();

		private Button returnToHomePageButton = null;

		private Button openFileDialogButton = null;

		private TextBox gameExecutablePathTextBox = null;

		private Button setupBepInExButton = null;

		private Button removeBepInExButton = null;

		private WebClient webClient = new WebClient();

		private Uri bepInExLatestReleaseDownloadUri = null;

		private GitHubClient gitHubClient = new GitHubClient(
			new ProductHeaderValue("pc-creator-mod-launcher")
		);

		private Button updateBepInExButton = null;

		private void OnSettingsPageLoaded(object sender, RoutedEventArgs e)
		{
			if (this.firstLoadCompleted == false)
			{
				this.returnToHomePageButton = (Button)this.FindName("ReturnToHomePageButton");

				this.returnToHomePageButton.Click += this.OnReturnToHomePageButtonClick;

				this.openFileDialogButton = (Button)this.FindName("OpenFileDialogButton");

				this.openFileDialogButton.Click += this.OnOpenFileDialogButtonClick;

				this.gameExecutablePathTextBox = (TextBox)this.FindName("GameExecutablePathTextBox");

				this.gameExecutablePathTextBox.TextChanged += this.OnGameExecutablePathTextBoxTextChanged;

				this.setupBepInExButton = (Button)this.FindName("SetupBepInExButton");

				this.setupBepInExButton.Click += this.OnSetupBepInExButtonClick;

				this.removeBepInExButton = (Button)this.FindName("RemoveBepInExButton");

				this.removeBepInExButton.Click += this.OnRemoveBepInExButtonClick;

				this.updateBepInExButton = (Button)this.FindName("UpdateBepInExButton");

				this.updateBepInExButton.Click += this.OnUpdateBepInExButtonClick;

				this.firstLoadCompleted = true;
			}

			this.gameExecutablePathTextBox.Text = MainWindow.instance.mainConfig.gameExecutablePath;

			this.OnBepInExSetupStatusChanged();
		}

		private void UpdateSetupBepInExButtonStatus()
		{
			this.setupBepInExButton.IsEnabled = !this.IsBepInExAlreadySetup();

			if (this.setupBepInExButton.IsEnabled == true)
			{
				this.setupBepInExButton.Content = "Setup BepInEx";
			}
			else
			{
				this.setupBepInExButton.Content = "BepInEx has already been set up at this path.";
			}
		}

		private void UpdateRemoveBepInExButtonStatus()
		{
			this.removeBepInExButton.IsEnabled = this.IsBepInExAlreadySetup();

			if (this.removeBepInExButton.IsEnabled == true)
			{
				this.removeBepInExButton.Content = "Remove BepInEx";
			}
			else
			{
				this.removeBepInExButton.Content = "BepInEx has not yet been set up at this path.";
			}
		}

		private void OnReturnToHomePageButtonClick(object sender, RoutedEventArgs e)
		{
			MainWindow.instance.mainContentControl.Content = MainWindow.instance.homePage;
		}

		private void OnOpenFileDialogButtonClick(object sender, RoutedEventArgs e)
		{
			this.openFileDialog.ShowDialog();
		}

		private void OnOpenFileDialogFileOk(object sender, CancelEventArgs e)
		{
			this.gameExecutablePathTextBox.Text = this.openFileDialog.FileName;

			this.OnBepInExSetupStatusChanged();
		}

		private void OnGameExecutablePathTextBoxTextChanged(object sender, RoutedEventArgs e)
		{
			MainWindow.instance.mainConfig.gameExecutablePath = this.gameExecutablePathTextBox.Text;
		}

		private async void OnSetupBepInExButtonClick(object sender, RoutedEventArgs e)
		{
			this.setupBepInExButton.IsEnabled = false;

			this.setupBepInExButton.Content = "Setting up BepInEx. Please wait...";

			this.removeBepInExButton.IsEnabled = false;

			await this.UpdateBepInExLatestReleaseDownloadUri();

			string[] bepInExLatestReleaseDownloadUriStringComponents = this.bepInExLatestReleaseDownloadUri.OriginalString.Split('/');

			string bepInExLatestReleaseZippedFileFullPath = Path.Combine(
				MainWindow.instance.mainConfig.baseDirectoryPath,
				bepInExLatestReleaseDownloadUriStringComponents[bepInExLatestReleaseDownloadUriStringComponents.Length - 1]
			);

			this.webClient.DownloadFileAsync(
				this.bepInExLatestReleaseDownloadUri,
				bepInExLatestReleaseZippedFileFullPath
			);
		}

		private void OnRemoveBepInExButtonClick(object sender, RoutedEventArgs e)
		{
			this.removeBepInExButton.IsEnabled = false;

			this.removeBepInExButton.Content = "Removing BepInEx. Please wait...";

			string gameExecutableParentDirectoryAbsolutePath = new FileInfo(
				MainWindow.instance.mainConfig.gameExecutablePath
			).Directory.FullName;

			string bepInExFolderAbsolutePath = Path.Combine(gameExecutableParentDirectoryAbsolutePath, "BepInEx");

			string bepInExChangelogTxtFileAbsolutePath = Path.Combine(gameExecutableParentDirectoryAbsolutePath, "changelog.txt");

			string bepInExDoorstopConfigIniFileAbsolutePath = Path.Combine(gameExecutableParentDirectoryAbsolutePath, "doorstop_config.ini");

			string bepInExWinHttpDllFileAbsolutePath = Path.Combine(gameExecutableParentDirectoryAbsolutePath, "winhttp.dll");

			if (Directory.Exists(bepInExFolderAbsolutePath) == true)
			{
				Directory.Delete(bepInExFolderAbsolutePath, true);
			}

			if (File.Exists(bepInExChangelogTxtFileAbsolutePath) == true)
			{
				File.Delete(bepInExChangelogTxtFileAbsolutePath);
			}

			if (File.Exists(bepInExDoorstopConfigIniFileAbsolutePath) == true)
			{
				File.Delete(bepInExDoorstopConfigIniFileAbsolutePath);
			}

			if (File.Exists(bepInExWinHttpDllFileAbsolutePath) == true)
			{
				File.Delete(bepInExWinHttpDllFileAbsolutePath);
			}

			this.removeBepInExButton.IsEnabled = true;

			this.removeBepInExButton.Content = "Remove BepInEx";

			this.OnBepInExSetupStatusChanged();
		}

		private void OnWebClientDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
			string[] bepInExLatestReleaseDownloadUriStringComponents = this.bepInExLatestReleaseDownloadUri.OriginalString.Split('/');

			string bepInExLatestReleaseZippedFileFullPath = Path.Combine(
				MainWindow.instance.mainConfig.baseDirectoryPath,
				bepInExLatestReleaseDownloadUriStringComponents[bepInExLatestReleaseDownloadUriStringComponents.Length - 1]
			);

			string bepInExLatestReleaseUnzippedFolderFullPath = Path.Combine(
				MainWindow.instance.mainConfig.baseDirectoryPath,
				"unzipped_" + bepInExLatestReleaseDownloadUriStringComponents[bepInExLatestReleaseDownloadUriStringComponents.Length - 1]
			);

			FileInfo gameExecutableFileInfo = new FileInfo(
				MainWindow.instance.mainConfig.gameExecutablePath
			);

			Directory.CreateDirectory(bepInExLatestReleaseUnzippedFolderFullPath);

			ZipFile.ExtractToDirectory(
				bepInExLatestReleaseZippedFileFullPath,
				bepInExLatestReleaseUnzippedFolderFullPath
			);

			foreach(string folderFullPath in Directory.GetDirectories(bepInExLatestReleaseUnzippedFolderFullPath))
			{
				Directory.Move(
					folderFullPath,
					Path.Combine(
						gameExecutableFileInfo.Directory.FullName,
						Path.GetFileName(folderFullPath)
					)
				);
			}

			foreach(string fileFullPath in Directory.GetFiles(bepInExLatestReleaseUnzippedFolderFullPath))
			{
				File.Move(
					fileFullPath,
					Path.Combine(
						gameExecutableFileInfo.Directory.FullName,
						Path.GetFileName(fileFullPath)
					)
				);
			}

			File.Delete(bepInExLatestReleaseZippedFileFullPath);

			Directory.Delete(bepInExLatestReleaseUnzippedFolderFullPath);

			this.setupBepInExButton.IsEnabled = true;

			this.setupBepInExButton.Content = "Setup BepInEx";

			this.removeBepInExButton.IsEnabled = true;

			this.OnBepInExSetupStatusChanged();
		}

		private async Task UpdateBepInExLatestReleaseDownloadUri()
		{
			IReadOnlyList<Release> bepInExReleases = await this.gitHubClient.Repository.Release.GetAll(
				"BepInEx",
				"BepInEx"
			);

			foreach (Release bepInExRelease in bepInExReleases)
			{
				if (bepInExRelease.Prerelease == false)
				{
					foreach(ReleaseAsset releaseAsset in bepInExRelease.Assets)
					{
						if (releaseAsset.Name.Contains("x64") == true)
						{
							this.bepInExLatestReleaseDownloadUri = new Uri(
								releaseAsset.BrowserDownloadUrl
							);

							break;
						}
					}

					break;
				}
			}
		}

		private bool IsBepInExAlreadySetup()
		{
			string gameExecutableParentDirectoryAbsolutePath = new FileInfo(
				MainWindow.instance.mainConfig.gameExecutablePath
			).Directory.FullName;

			if (Directory.Exists(Path.Combine(gameExecutableParentDirectoryAbsolutePath, "BepInEx")) == false)
			{
				return false;
			}

			if (File.Exists(Path.Combine(gameExecutableParentDirectoryAbsolutePath, "changelog.txt")) == false)
			{
				return false;
			}

			if (File.Exists(Path.Combine(gameExecutableParentDirectoryAbsolutePath, "doorstop_config.ini")) == false)
			{
				return false;
			}

			if (File.Exists(Path.Combine(gameExecutableParentDirectoryAbsolutePath, "winhttp.dll")) == false)
			{
				return false;
			}

			return true;			
		}

		private void OnUpdateBepInExButtonClick(object sender, RoutedEventArgs e)
		{
			if (this.IsBepInExLatestStableVersionInstalled() == true)
			{
				return;
			}

			this.OnBepInExSetupStatusChanged();
		}

		private void UpdateBepInExUpdateButtonStatus()
		{
			this.updateBepInExButton.IsEnabled = this.IsBepInExAlreadySetup();

			if (this.updateBepInExButton.IsEnabled == true)
			{
				if (this.IsBepInExLatestStableVersionInstalled() == true)
				{
					this.updateBepInExButton.Content = "Your version of BepInEx is currently up to date.";
				}
				else
				{
					this.updateBepInExButton.Content = "Update BepInEx to latest stable version";
				}
			}
			else
			{
				this.updateBepInExButton.Content = "BepInEx has not yet been set up at this path.";
			}
		}

		private void OnBepInExSetupStatusChanged()
		{
			this.UpdateSetupBepInExButtonStatus();

			this.UpdateRemoveBepInExButtonStatus();

			this.UpdateBepInExUpdateButtonStatus();
		}

		private bool IsBepInExLatestStableVersionInstalled()
		{
			FileVersionInfo bepInExDllFileVersionInfo = FileVersionInfo.GetVersionInfo(
				Path.Combine(
					new FileInfo(MainWindow.instance.mainConfig.gameExecutablePath).Directory.FullName,
					"BepInEx",
					"core",
					"BepInEx.dll"
				)
			);

			MessageBox.Show(bepInExDllFileVersionInfo.FileVersion);

			return false;
		}

		public SettingsPage()
		{
			InitializeComponent();

			this.Loaded += this.OnSettingsPageLoaded;

			this.openFileDialog.Filter = "exe files (*.exe)|*.exe";

			this.openFileDialog.FileOk += this.OnOpenFileDialogFileOk;

			this.webClient.DownloadFileCompleted += this.OnWebClientDownloadFileCompleted;
		}
	}
}
