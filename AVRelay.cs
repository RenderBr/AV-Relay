﻿using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Terraria;
using Terraria.UI.Chat;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace AVRelay
{
    [ApiVersion(2, 1)]
    public class AVRelay : TerrariaPlugin
    {
        /// </summary>
        public override string Name => "AVRelay";

        /// <summary>
        /// The version of the plugin in its current state.
        /// </summary>
        public override Version Version => new(1, 0, 1);

        /// <summary>
        /// The author(s) of the plugin.
        /// </summary>
        public override string Author => "Average";

        /// <summary>
        /// A short, one-line, description of the plugin's purpose.
        /// </summary>
        public override string Description => "A simple & easy Discord relay plugin for TShock V5!";

        /// <summary>
        /// The plugin's constructor
        /// Set your plugin's order (optional) and any other constructor logic here
        /// </summary>
        public AVRelay(Main game) : base(game) { }

        public static Config Config { get; set; }

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, GameInit);
            ServerApi.Hooks.NetGreetPlayer.Register(this, GreetPlayer);
            ServerApi.Hooks.ServerLeave.Register(this, ExitPlayer);
            PlayerHooks.PlayerChat += OnPlayerChat;

        }

        public static void RelayMessage(string username, string message) => TSPlayer.All.SendMessage($"[c/5539CC:Discord>] {username}: {message}", Color.White);

        private void OnPlayerChat(PlayerChatEventArgs e)
        {
            var input = "";
            foreach (var b in ChatManager.ParseMessage(e.Player.Group.Prefix, Color.White).ToList())
            {
                input += b.Text;
            }
            input = ChatManager.ParseMessage(input, Color.White)[0].Text;

            AVDiscord.UserChat(e.Player, e.RawText, input);
        }

        private void ExitPlayer(LeaveEventArgs args) => AVDiscord.UserLeft(TShock.Players[args.Who]);

        private void GreetPlayer(GreetPlayerEventArgs args) => AVDiscord.UserJoined(TShock.Players[args.Who]);

        private void GameInit(EventArgs args)
        {
            Config = Config.Read();
            Connect(new string[0]);
        }

        public static Task Connect(string[] args) => new AVDiscord().MainAsync();

    }
}