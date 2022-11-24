using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TShockAPI;
using TerrariaApi.Server;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;

namespace AVRelay
{
    public class CommandHandler
    {
        public static async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            AVDiscord._client.MessageReceived += HandleCommandAsync;

            // Here we discover all of the command modules in the entry 
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the 
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.
            await AVDiscord._commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: null);
        }

        private static async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasStringPrefix(AVRelay.Config.CommandPrefix, ref argPos) ||
                message.HasMentionPrefix(AVDiscord._client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(AVDiscord._client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await AVDiscord._commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null);
        }

        public class InfoModule : ModuleBase<SocketCommandContext>
        {
            [Command("who")]
            [Summary("Returns a list of all players.")]
            public async Task WhoAsync()
            {
                var playerList = TShock.Utils.GetActivePlayerCount();
                await Context.Channel.SendMessageAsync($"{playerList}");
            }

        }
    }
}
