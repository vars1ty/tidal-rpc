using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DiscordRPC;

namespace TidalRPC
{
    /// <summary>
    /// Main entry point structure.
    /// </summary>
    internal struct Program
    {
        #region Variables
        /// <summary>
        /// A single <see cref="DiscordRpcClient"/> instance which we can re-use every time.
        /// </summary>
        private static readonly DiscordRpcClient client = new DiscordRpcClient(application);

        /// <summary>
        /// Static readonly strings for the RPC.
        /// </summary>
        private static readonly string[] staticRPC = {"tidal", "TIDAL RPC made by Shaw."};

        /// <summary>
        /// The application id.
        /// </summary>
        private const string application = "777323572188282900";

        /// <summary>
        /// TIDAL's main process name.
        /// </summary>
        private const string processLookup = "TIDAL";

        /// <summary>
        /// Delay in ms until the RPC updates again.
        /// </summary>
        private const ushort delay = 4000;

        /// <summary>
        /// The character to split.
        /// </summary>
        private const char split = '-';
        #endregion

        /// <summary>
        /// Main entry point.
        /// </summary>
        public static async Task Main()
        {
            // Update the RPC every 4s in a new thread.
            new Thread(() =>
            {
                while (true)
                {
                    var processes = Process.GetProcessesByName(processLookup);
                    for (ushort i = 0; i < processes.Length; i++)
                    {
                        var title = processes[i].MainWindowTitle;
                        if (title.Length <= 3) continue; // Too short, probably a different instance of TIDAL (thread).
                        var content = title.Split(split);
                        Setup(content[0], content[1]);
                    }

                    Thread.Sleep(delay);
                }
            }).Start();
            await Task.Delay(-1);
            client.Dispose();
        }

        /// <summary>
        /// Setup/Update the RPC.
        /// </summary>
        /// <param name="song">The song's name.</param>
        /// <param name="artist">The artist's name.</param>
        private static void Setup(string song, string artist) =>
            new Thread(() =>
            {
                if (!client.IsInitialized)
                {
                    client.OnReady += (sender, msg) =>
                        Console.WriteLine($"Connected to Discord as {msg.User.Username}!");
                    client.Initialize();
                    return;
                }

                var array = staticRPC;
                client.SetPresence(new RichPresence
                {
                    Details = song,
                    State = artist,
                    Assets = new Assets {LargeImageKey = array[0], LargeImageText = array[1]}
                });
                Console.ReadKey();
                client.Dispose();
            }).Start();
    }
}