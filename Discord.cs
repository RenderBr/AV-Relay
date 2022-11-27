using Discord;
using Discord.API;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Google.Protobuf.WellKnownTypes;
using IL.Terraria.DataStructures;
using IL.Terraria.Localization;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using On.Terraria.Localization;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.UI.Chat;
using TerrariaApi.Server;
using TShockAPI;
using static System.Net.Mime.MediaTypeNames;

namespace AVRelay
{
    public class AVDiscord
    {
        public static DiscordSocketClient _client;
        public static CommandService _commands;
        public static ulong channelID = ulong.Parse(AVRelay.Config.channelId);
        public static IMessageChannel channel;
        public async Task MainAsync()
        {
            if (AVRelay.Config.EnableDiscord)
            {
                var config = new DiscordSocketConfig()
                {
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
                    
                };

                _client = new DiscordSocketClient(config);
                _client.Log += Log;
                _client.Ready += clientReady;
                _client.SlashCommandExecuted += SlashCommandHandler;
                _client.MessageReceived += discordChat;

                var token = AVRelay.Config.Token;
                channel = _client.GetChannel(channelID) as IMessageChannel;
                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();
                await _client.SetActivityAsync(new Game($" with {TShock.Utils.GetActivePlayerCount()}/{Main.maxNetPlayers} active players!", ActivityType.Playing));
                await channel.SendMessageAsync(":green_circle: **This server is now online!**");

                // Block this task until the program is closed.
                await Task.Delay(-1);


            }

        }

        private async Task discordChat(SocketMessage arg)
        {
            if (arg is not SocketUserMessage userMessage)
                return;

            if (arg.Channel == channel) {
                if (userMessage.Author is not SocketGuildUser user)
                    return;

                if (user.IsBot || user.IsWebhook)
                    return;

                AVRelay.RelayMessage(arg.Author.Username, arg.CleanContent);
                return;
            }
            else
            {
                return;
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand arg)
        {
            if (arg.CommandName == "who")
            {
                var players = "```";
                var fullmsg = "";
      

                for(int i = 0; i < TShock.Utils.GetActivePlayerCount(); i++)
                {
                    if(i+1 == TShock.Utils.GetActivePlayerCount())
                    {
                        players += TShock.Players[i].Name + "```";
                        break;
                    }
                    players += TShock.Players[i].Name + ", ";


                }

                fullmsg += "There are currently: **" + TShock.Utils.GetActivePlayerCount() + " / " + Main.maxNetPlayers + "** users online!";
                fullmsg += "\n" + players;
                await arg.RespondAsync(fullmsg);
            }

            if (arg.CommandName == "cmd")
            {
                var cmd = arg.Data.Options.First().Value;
                var runningUser = (SocketGuildUser)arg.User;

                if (runningUser.Roles.Any(x => x.Permissions.Administrator) == false)
                {
                    await arg.RespondAsync("You are not a manager!");

                }

                Commands.HandleCommand(TSPlayer.Server, (string)arg.Data.Options.First().Value);
                await arg.RespondAsync("Command executed!");
            }


            if (arg.CommandName == "joinserver")
            {
                var cmd = arg.Data.Options.First().Value;
                var runningUser = (SocketGuildUser)arg.User;

                await arg.RespondAsync($"You can join the server with the following IP: {AVRelay.Config.serverIp} Port: {AVRelay.Config.serverPort}");
            }

        }

        public async Task clientReady()
        {
            List<ApplicationCommandProperties> applicationCommandProperties = new();
            try
            {
                var onlinePlayers = new SlashCommandBuilder();
                onlinePlayers.WithName("who");
                onlinePlayers.WithDescription("Get a list of players online!");
                applicationCommandProperties.Add(onlinePlayers.Build());

                var serverInfo = new SlashCommandBuilder();
                serverInfo.WithName("joinserver");
                serverInfo.WithDescription("Retrieves the server info to connect.");
                applicationCommandProperties.Add(serverInfo.Build());


                var runCommand = new SlashCommandBuilder();
                runCommand.WithName("cmd");
                runCommand.WithDescription("Run a command! (MANAGER ONLY)");
                runCommand.AddOption("command", ApplicationCommandOptionType.String, "Enter a command to run, along with its arguments");
                applicationCommandProperties.Add(runCommand.Build());



                await _client.BulkOverwriteGlobalApplicationCommandsAsync(applicationCommandProperties.ToArray());
            }
            catch(HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }





        }

        public static async void UserJoined(TSPlayer player)
        {
            channel = _client.GetChannel(channelID) as IMessageChannel;
            await _client.SetGameAsync($" with {TShock.Utils.GetActivePlayerCount()}/{Main.maxNetPlayers} active players!");
            var embedded = new EmbedBuilder
            {
                // Embed property can be set within object initializer
                Title = $"**{AVRelay.Config.serverName}** - {TShock.Utils.GetActivePlayerCount()}/{Main.maxNetPlayers} players",
                Description = $"{player.Name} has joined the game!",
                Color = Color.Green

            };



            await channel.SendMessageAsync(embed: embedded.Build());

        }

        public static async void UserChat(TSPlayer player, string message, string input)
        {
            channel = _client.GetChannel(channelID) as IMessageChannel;

            var embedded = new EmbedBuilder
            {
                // Embed property can be set within object initializer
                Description = $"**{AVRelay.Config.serverName}** **{input}** {player.Name}: {message}",
                Color = AVRelay.Config.rgbServerColor
                
            };


            await channel.SendMessageAsync(embed: embedded.Build());
        }

        public static async void UserLeft(TSPlayer player)
        {
            if(player == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(player.Name))
            {
                return;
            }

            channel = _client.GetChannel(channelID) as IMessageChannel;
            await _client.SetGameAsync($" with {TShock.Utils.GetActivePlayerCount()}/{Main.maxNetPlayers} active players!");


            var embedded = new EmbedBuilder
            {
                // Embed property can be set within object initializer
                Title = $"**{AVRelay.Config.serverName}** - {TShock.Utils.GetActivePlayerCount()}/{Main.maxNetPlayers} players",
                Description = $"{player.Name} has left the game!",
                Color = Color.Red

            };



            await channel.SendMessageAsync(embed: embedded.Build()) ;


        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

    }
}
