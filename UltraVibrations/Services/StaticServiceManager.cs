using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface.DragDrop;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using OtterGui.Classes;
using OtterGui.Log;
using OtterGui.Services;
using UltraVibrations.UI;
using UltraVibrations.UI.Tabs;

namespace UltraVibrations.Services;

public static class StaticServiceManager
{
    public static ServiceManager CreateProvider(Plugin plugin, IDalamudPluginInterface pi, Logger log)
    {
        var services = new ServiceManager(log)
                       .AddDalamudServices(pi)
                       .AddExistingService(log)
                       .AddExistingService(plugin)
                       .AddInterop()
                       .AddConfiguration()
                       .AddUi();

        services.AddIServices(typeof(Plugin).Assembly);

        services.AddSingleton<DeviceManagerService>();
        services.AddSingleton<TriggerManagerService>();
        services.AddSingleton<ChannelService>();
        services.AddSingleton<ButtplugService>();

        services.CreateProvider();
        return services;
    }

    private static ServiceManager AddDalamudServices(this ServiceManager services, IDalamudPluginInterface pi)
        => services.AddExistingService(pi)
                   .AddExistingService(pi.UiBuilder)
                   .AddDalamudService<ICommandManager>(pi)
                   .AddDalamudService<IDataManager>(pi)
                   .AddDalamudService<IClientState>(pi)
                   .AddDalamudService<IChatGui>(pi)
                   .AddDalamudService<IFramework>(pi)
                   .AddDalamudService<ICondition>(pi)
                   .AddDalamudService<ITargetManager>(pi)
                   .AddDalamudService<IObjectTable>(pi)
                   .AddDalamudService<ITitleScreenMenu>(pi)
                   .AddDalamudService<IGameGui>(pi)
                   .AddDalamudService<IKeyState>(pi)
                   .AddDalamudService<ISigScanner>(pi)
                   .AddDalamudService<IDragDropManager>(pi)
                   .AddDalamudService<ITextureProvider>(pi)
                   .AddDalamudService<ITextureSubstitutionProvider>(pi)
                   .AddDalamudService<IGameInteropProvider>(pi)
                   .AddDalamudService<IPluginLog>(pi)
                   .AddDalamudService<INotificationManager>(pi);

    private static ServiceManager AddInterop(this ServiceManager services)
        => services.AddSingleton<FrameworkManager>();

    private static ServiceManager AddConfiguration(this ServiceManager services)
        => services.AddSingleton<Configuration>();

    private static ServiceManager AddUi(this ServiceManager services)
        => services.AddSingleton<UltraVibrationsWindowSystem>()
                   .AddSingleton<MainWindow>()
                   .AddSingleton<CollectionsTab>()
                   .AddSingleton<DevicesTab>()
                   .AddSingleton<PatternsTab>()
                   .AddSingleton<SettingsTab>()
                   .AddSingleton<TriggersTab>();
}
