using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PC_Creator_Modloader
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private bool firstLoadCompleted = false;

		public static MainWindow instance = null;

		public ContentControl mainContentControl = null;

		public HomePage homePage = new HomePage();

		public SettingsPage settingsPage = new SettingsPage();

		public MainConfig mainConfig = new MainConfig();

		private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
		{
			if (this.firstLoadCompleted == false)
			{
				this.mainContentControl = (ContentControl)this.FindName("MainContentControl");

				this.mainContentControl.Content = this.homePage;

				this.Closing += this.OnMainWindowClosing;

				this.firstLoadCompleted = true;
			}
		}

		private void OnMainWindowClosing(object sender, CancelEventArgs e)
		{
			this.mainConfig.Save();
		}

		public MainWindow()
		{
			instance = this;

			InitializeComponent();

			this.mainConfig.Load();
		}
	}
}
