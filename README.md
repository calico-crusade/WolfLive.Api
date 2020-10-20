# WolfLive.Api
An API for connecting to and creating bots for wolf.live in C#

## Installation
You can view a more indepth tutorial on [the wiki](https://github.com/calico-crusade/WolfLive.Api/wiki)!

You can get the project via nuget: `PM> Install-Package WolfLive.Api.Commands`

Then to create a connection to the server and add some commands you can do:
```CSharp
using System;
using System.Threading.Tasks;
using WolfLive.Api;
using WolfLive.Api.Commands;

namespace MyBot 
{
    public class Program
    {
        private static IWolfClient _client;

        public static async Task Main(string[] args)
        {
            string email = "email@example.com",
                   password = "S0me P4$$W0RD";

            _client = new WolfClient()
                .SetupCommands()
                .WithCommandSet(c => 
                {
                    c.AddCommands<MyCommands>()
                     .AddCommands<MyStaticCommands>()
                     .WithPrefix("!");
                })
                .WithSerilog()
                .Done();

            _client.OnConnected += (_) => Console.WriteLine("Connected to wolf.live!");

            var result = await _client.Login(email, password);

            Console.WriteLine($"Login {(result ? "success!" : "failed!")}");

            await Task.Delay(-1);
        }
    }

    public class MyCommands : WolfContext
    {
        [Command("test")]
        public async Task TestCommand(string message)
        {
            await this.Reply("Hello there! I saw your " + message);
        }

        [Command("dm only"), PrivateOnly]
        public async Task PrivateOnlyCommand(string message)
        {
            await context.Reply($"This command only works in PM / DM! But I see your: {message}");
        }
    }

    public class MyStaticCommands
    {
        [Command("grp only"), GroupOnly]
        public static async Task GroupOnlyCommand(StaticContext context, string message)
        {
            await context.Reply($"This command only works in groups! But I see your: {message}");
        }

        
        [Command("grp only"), GroupOnly, Role("Admin")]
        public static async Task AdminOnlyCommand(StaticContext context, string message)
        {
            await context.Reply($"This command only works in groups and if they calling user is an Admin! But I see your: {message}");
        }
    }
}
```

## Comments, questions, concerns?
You can either open an issue here, [Contact me on wolf](https://wolf.live/u/43681734), or message me on discord: `Cardboard#0026`