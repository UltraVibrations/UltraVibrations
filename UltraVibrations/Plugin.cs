using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using OtterGui.Log;
using OtterGui.Services;
using UltraVibrations.Services;
using UltraVibrations.UI;

namespace UltraVibrations
{
    public sealed class Plugin : IDalamudPlugin
    {
        public static readonly Logger Log = new();
        private readonly ServiceManager serviceManager;
        private readonly UltraVibrationsWindowSystem? windowSystem;
        private readonly Configuration config;

        public Plugin(IDalamudPluginInterface pluginInterface)
        {
            serviceManager = StaticServiceManager.CreateProvider(this, pluginInterface, Log);
            serviceManager.EnsureRequiredServices();

            // Load config
            config = serviceManager.GetService<Configuration>();

            windowSystem = serviceManager.GetService<UltraVibrationsWindowSystem>();

            if (windowSystem == null)
            {
                Log.Error("WindowSystem was not created.");
            }

            var commandManager = serviceManager.GetService<ICommandManager>();

            commandManager?.AddHandler("/uv", new CommandInfo(MainCommand)
            {
                HelpMessage = "UltraVibrations command handler."
            });

            Log.Debug("UltraVibrations initialized.");
            windowSystem?.Toggle();
        }

        private void MainCommand(string command, string args)
        {
            windowSystem?.Toggle();
        }

        public void Dispose()
        {
            serviceManager.Dispose();
        }
    }
}
