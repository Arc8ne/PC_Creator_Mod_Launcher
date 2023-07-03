using System;
using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace PC_Creator_Mod_Launcher
{
	public class MainConfig
	{
		private const string defaultMainConfigFileName = "pc_creator_mod_launcher_config.json";

		private const string defaultMainConfigFileRelativeFilePath = "/" + defaultMainConfigFileName;

		private string mainConfigFileAbsoluteFilePath = "";

		public string baseDirectoryPath = "";

		public string gameExecutablePath = "";

		public MainConfig()
		{
			this.baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;

			this.mainConfigFileAbsoluteFilePath = this.baseDirectoryPath + defaultMainConfigFileRelativeFilePath;
		}

		public void Save()
		{
			string currentProgramDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;

			File.WriteAllText(
				this.mainConfigFileAbsoluteFilePath,
				JsonConvert.SerializeObject(this)
			);
		}

		public void Load()
		{
			if (File.Exists(this.mainConfigFileAbsoluteFilePath) == true)
			{
				string mainConfigFileText = File.ReadAllText(this.mainConfigFileAbsoluteFilePath);
			
				MainConfig deserializedMainConfig = JsonConvert.DeserializeObject<MainConfig>(mainConfigFileText);

				this.gameExecutablePath = deserializedMainConfig.gameExecutablePath;
			}
			else
			{
				File.Create(this.mainConfigFileAbsoluteFilePath);
			}
		}
	}
}
