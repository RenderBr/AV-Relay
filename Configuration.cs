﻿using Discord;
using Newtonsoft.Json;
using System;
using System.IO;
using TShockAPI;

namespace AVRelay
{
    public class Config
    {

        [JsonProperty(Order = 40)]
        public bool EnableDiscord { get; set; } = true;
        [JsonProperty(Order = 50)]
        public string Token { get; set; } = "TOKEN";
        public string channelId { get; set; } = "CHANNEL ID";

        public Color rgbServerColor { get; set; } = Color.Blue;

        public string serverName = "Freebuild";
        public string serverIp { get; set; } = "127.0.0.1";
        public string serverPort { get; set; } = "7777";

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
                Config config = new();

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