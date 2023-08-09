using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace AVRelay
{
    public class AVDiscord
    {
        private static DiscordSocketClient _client;
        private static ulong _channelID;
        private static IMessageChannel _channel;

        public async Task MainAsync()
        {
            if (!AVRelay.Config.EnableDiscord)
                return;

            _channelID = ulong.Parse(AVRelay.Config.channelId);

            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(config);
            _client.Log += Log;
            _client.Ready += ClientReady;
            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.MessageReceived += DiscordChat;

            var token = AVRelay.Config.Token;
            _channel = _client.GetChannel(_channelID) as IMessageChannel;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await _client.SetActivityAsync(new Game($" with {TShock.Utils.GetActivePlayerCount()}/{Main.maxNetPlayers} active players!", ActivityType.Playing));
            await _channel.SendMessageAsync(":green_circle: **This server is now online!**");

            await Task.Delay(-1);
        }

        private async Task DiscordChat(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage userMessage))
                return;

            if (arg.Channel != _channel)
                return;

            if (!(arg.Author is SocketGuildUser user) || user.IsBot || user.IsWebhook)
                return;

            AVRelay.RelayMessage(arg.Author.Username, arg.CleanContent);
        }

        private async Task SlashCommandHandler(SocketSlashCommand arg)
        {
            switch (arg.CommandName)
            {
                case "who":
                    await WhoCommand(arg);
                    break;
                case "cmd":
                    await CommandCommand(arg);
                    break;
                case "joinserver":
                    await JoinServerCommand(arg);
                    break;
            }
        }

        private async Task WhoCommand(SocketSlashCommand arg)
        {
            var players = new List<string>();
            for (int i = 0; i < TShock.Utils.GetActivePlayerCount(); i++)
                players.Add(TShock.Players[i].Name);

            var response = new StringBuilder();
            response.AppendLine($"There are currently: **{TShock.Utils.GetActivePlayerCount()} / {Main.maxNetPlayers}** users online!");
            response.AppendLine(string.Join(", ", players));

            await arg.RespondAsync(response.ToString());
        }

        private async Task CommandCommand(SocketSlashCommand arg)
        {
            var cmd = arg.Data.Options.First().Value;
            var runningUser = (SocketGuildUser)arg.User;

            if (runningUser.Roles.Any(x => x.Permissions.Administrator) == false)
                await arg.RespondAsync("You are not a manager!");
            else
            {
                Commands.HandleCommand(TSPlayer.Server, (string)arg.Data.Options.First().Value);
                await arg.RespondAsync("Command executed!");
            }
        }

        private async Task JoinServerCommand(SocketSlashCommand arg)
        {
            await arg.RespondAsync($"You can join the server with the following IP: {AVRelay.Config.serverIp} Port: {AVRelay.Config.serverPort}");
        }

        private async Task ClientReady()
        {
            var applicationCommandProperties = new List<ApplicationCommandProperties>
            {
                new SlashCommandBuilder().WithName("who").WithDescription("Get a list of players online!").Build(),
                new SlashCommandBuilder().WithName("joinserver").WithDescription("Retrieves the server info to connect.").Build(),
                new SlashCommandBuilder().WithName("cmd").WithDescription("Run a command! (MANAGER ONLY)")
                    .AddOption("command", ApplicationCommandOptionType.String, "Enter a command to run, along with its arguments").Build()
            };

            try
            {
                await _client.BulkOverwriteGlobalApplicationCommandsAsync(applicationCommandProperties.ToArray());
            }
            catch (HttpException exception)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(exception.Errors, Newtonsoft.Json.Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        public static async void UserJoined(TSPlayer player)
        {
            _channel = _client.GetChannel(_channelID) as IMessageChannel;
            await _client.SetGameAsync($" with {TShock.Utils.GetActivePlayerCount()}/{Main.maxNetPlayers} active players!");

            var embedded = new EmbedBuilder
            {
                Title = $"**{AVRelay.Config.serverName}** - {TShock.Utils.GetActivePlayerCount()}/{Main.maxNetPlayers} players",
                Description = $"{player.Name} has joined the game!",
                Color = Color.Green
            };

            await _channel.SendMessageAsync(embed: embedded.Build());
        }

        public static async void UserChat(TSPlayer player, string message, string input)
        {
            _channel = _client.GetChannel(_channelID) as IMessageChannel;

            var embedded = new EmbedBuilder
            {
                Description = $"**{AVRelay.Config.serverName}** **{input}** {player.Name}: {message}",
                Color = AVRelay.Config.rgbServerColor
            };

            await _channel.SendMessageAsync(embed: embedded.Build());
        }

        public static async void UserLeft(TSPlayer player)
        {
            if (player == null || string.IsNullOrEmpty(player.Name))
                return;

            _channel = _client.GetChannel(_channelID) as IMessageChannel;
            await _client.SetGameAsync($" with {TShock.Utils.GetActivePlayerCount()}/{Main.maxNetPlayers} active players!");

            var embedded = new EmbedBuilder
            {
                Title = $"**{AVRelay.Config.serverName}** - {TShock.Utils.GetActivePlayerCount()}/{Main.maxNetPlayers} players",
                Description = $"{player.Name} has left the game!",
                Color = Color.Red
            };

            await _channel.SendMessageAsync(embed: embedded.Build());
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
