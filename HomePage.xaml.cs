using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PC_Creator_Modloader
{
	/// <summary>
	/// Interaction logic for HomePage.xaml
	/// </summary>
	public partial class HomePage : UserControl
	{
		private Button settingsButton = null;

		private void OnHomePageLoaded(object sender, RoutedEventArgs e)
		{
			this.settingsButton = (Button)this.FindName("SettingsButton");

			this.settingsButton.Click += this.OnSettingsButtonClick;
		}

		private void OnSettingsButtonClick(object sender, RoutedEventArgs e)
		{
			MainWindow.instance.mainContentControl.Content = MainWindow.instance.settingsPage;
		}

		public HomePage()
		{
			InitializeComponent();
		}
	}
}
