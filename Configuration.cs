using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace AVRelay
{
	public class Config
	{

		[JsonProperty(Order = 40)]
		public bool EnableDiscord { get; set; } = true;
		[JsonProperty(Order = 50)]
		public string CommandPrefix { get; set; } = "t!";
		[JsonProperty(Order = 60)]
		public bool ShowPoweredByMessageOnStartup { get; set; } = true;
		[JsonProperty(Order = 75)]
		public ulong OwnerUserId { get; set; } = 0;
		[JsonProperty(Order = 110)]
		public List<ulong> ManagerUserIds { get; set; } = new List<ulong>();
		[JsonProperty(Order = 120)]
		public List<ulong> AdminUserIds { get; set; } = new List<ulong>();
		[JsonProperty(Order = 130)]
		public string Token { get; set; } = "TOKEN";
		[JsonProperty(Order = 140)]
		public string RetryHelp { get; set; } = "Set NumberOfTimesToRetryConnectionAfterError to -1 to retry infinitely";
		[JsonProperty(Order = 150)]
		public int NumberOfTimesToRetryConnectionAfterError { get; set; } = 5;
		[JsonProperty(Order = 160)]
		public int SecondsToWaitBeforeRetryingAgain { get; set; } = 10;
		[JsonProperty(Order = 170)]
		public string FormatHelp1 { get; set; } = "You can insert any of these formatters to change how your message looks! (CASE SENSITIVE)";
		[JsonProperty(Order = 180)]
		public string FormatHelp2 { get; set; } = "%playername% = Player Name";
		[JsonProperty(Order = 190)]
		public string FormatHelp3 { get; set; } = "%worldname% = World Name";
		[JsonProperty(Order = 200)]
		public string FormatHelp4 { get; set; } = "%message% = Initial message content";
		[JsonProperty(Order = 210)]
		public string FormatHelp5 { get; set; } = "%bossname% = Name of boss being summoned (only for VanillaBossSpawned)";
		[JsonProperty(Order = 220)]
		public string FormatHelp6 { get; set; } = "%groupprefix% = Group prefix";
		[JsonProperty(Order = 230)]
		public string FormatHelp7 { get; set; } = "%groupsuffix% = Group suffix";

		[JsonProperty(Order = 240)]
		public static string PlayerChatFormat = ":speech_left: **%playername%:** %message%";
		[JsonProperty(Order = 250)]
		public static string PlayerLoggedInFormat = ":small_blue_diamond: **%playername%** joined the server.";
		[JsonProperty(Order = 260)]
		public static string PlayerLoggedOutFormat = ":small_orange_diamond: **%playername%** left the server.";
		[JsonProperty(Order = 270)]
		public static string WorldEventFormat = "**%message%**";
		[JsonProperty(Order = 280)]
		public static string ServerStartingFormat = ":small_blue_diamond: **%message%**";
		[JsonProperty(Order = 290)]
		public static string ServerStoppingFormat = ":small_orange_diamond: **%message%**";
		[JsonProperty(Order = 305)]
		public static string VanillaBossSpawned = ":anger: **%bossname% has awoken!**";

		public void Write()
		{
			string path = Path.Combine(TShock.SavePath, "AVRelay.json");
			File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
		}
		public static Config Read()
		{
			string filepath = Path.Combine(TShock.SavePath, "AVRelay.json");

			try
			{
				Config config = new Config();

				if (!File.Exists(filepath))
				{
					File.WriteAllText(filepath, JsonConvert.SerializeObject(config, Formatting.Indented));
				}
				config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(filepath));


				return config;
			}
			catch (Exception ex)
			{
				TShock.Log.ConsoleError(ex.ToString());
				return new Config();
			}
		}
	}
}