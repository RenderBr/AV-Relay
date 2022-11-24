using System.Collections.Generic;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using TerrariaApi.Server;
using TShockAPI;
using System;
using Terraria;
using AVRelay;
using System.Runtime.CompilerServices;
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
        public override Version Version => new Version(1, 0, 0);

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
        public AVRelay(Main game) : base(game)
        {

        }

        public static Config Config { get; set; }


        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, GameInit);
            ServerApi.Hooks.NetGreetPlayer.Register(this, GreetPlayer);
            ServerApi.Hooks.ServerLeave.Register(this, ExitPlayer);
            TShockAPI.Hooks.PlayerHooks.PlayerChat += OnPlayerChat;

        }

        private void OnPlayerChat(PlayerChatEventArgs e)
        {
            AVDiscord.UserChat(e.Player, e.RawText);
        }

        private void ExitPlayer(LeaveEventArgs args)
        {
            AVDiscord.UserLeft(TShock.Players[args.Who]);
        }

        private void GreetPlayer(GreetPlayerEventArgs args)
        {
            AVDiscord.UserJoined(TShock.Players[args.Who]);
        }

        private void GameInit(EventArgs args)
        {
            Config = Config.Read();
            Connect(new string[0]);
        }

        public static Task Connect(string[] args)
        {
            return new AVDiscord().MainAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //unhook
                //dispose child objects
                //set large objects to null
            }
            base.Dispose(disposing);
        }
    }
}