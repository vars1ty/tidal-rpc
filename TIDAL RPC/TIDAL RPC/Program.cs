using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DiscordRPC;

namespace TIDAL_RPC
{
    /// <summary>
    /// Main entry point structure.
    /// </summary>
    internal struct Program
    {
        #region Variables
        /// <summary>
        /// Static readonly strings for the RPC.
        /// </summary>
        private static readonly string[] staticRPC = {"tidal", "TIDAL RPC made by riya."};

        /// <summary>
        /// Static readonly <see cref="Assets"/> which will be used to update the RPC.
        /// </summary>
        private static readonly Assets staticAssets =
            new() {LargeImageKey = staticRPC[0], LargeImageText = staticRPC[1]};

        /// <summary>
        /// Single <see cref="RichPresence"/> instance.
        /// </summary>
        private static readonly RichPresence richPresenceInstance = new();

        /// <summary>
        /// A single <see cref="DiscordRpcClient"/> instance which we can re-use every time.
        /// </summary>
        private static readonly DiscordRpcClient client = new(application);

        /// <summary>
        /// The application id.
        /// </summary>
        private const string application = "796729599413059604";

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
            // Update the RPC every 4s.
            new Thread(Start).Start();
            // ! Never close, so set the delay to -1 (~inf).
            await Task.Delay(-1);
            // Dispose the client when the program exits.
            client.Dispose();
        }

        /// <summary>
        /// Start updating the RPC.
        /// </summary>
        private static void Start()
        {
            while (true)
            {
                var processes = Process.GetProcessesByName(processLookup);
                for (short i = 0; i < processes.Length; i++)
                {
                    var title = processes[i].MainWindowTitle;
                    if (title.Length <= 3) continue; // Too short, probably a different instance of TIDAL (thread).
                    var content = title.Split(split);
                    if (content.Length != 2)
                    {
                        // The size wasn't 2, break.
                        continue;
                    }

                    Setup(content[0], content[1]);
                }

                Thread.Sleep(delay);
            }
        }

        /// <summary>
        /// Setup/Update the RPC.
        /// </summary>
        /// <param name="song">The song's name.</param>
        /// <param name="artist">The artist's name.</param>
        private static void Setup(string song, string artist) =>
            new Thread(() =>
            {
                // ! If the client hasn't been initialized, return.
                // ? Plugin for colored one-line comments: Better Comments
                if (!client.IsInitialized)
                {
                    client.OnReady += (_, msg) => Console.WriteLine($"[+] Connected to Discord as {msg.User.Username}!",
                        Console.ForegroundColor = ConsoleColor.Red);
                    client.Initialize();
                    // Customize the RPC.
                    richPresenceInstance.Details = song;
                    richPresenceInstance.State = artist;
                    richPresenceInstance.Assets = staticAssets;
                    client.SetPresence(richPresenceInstance);
                    return;
                }

                // Customize the RPC.
                richPresenceInstance.Details = song;
                richPresenceInstance.State = artist;
                richPresenceInstance.Assets = staticAssets;
                client.SetPresence(richPresenceInstance);
            }).Start();
    }
}
