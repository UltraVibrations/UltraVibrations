using System.IO;
using Dalamud.Plugin;
using OtterGui.Services;

namespace UltraVibrations.Services;

public class FilenameService(IDalamudPluginInterface pluginInterface): IService
{
    public readonly string ConfigDirectory = pluginInterface.ConfigDirectory.FullName;
    public readonly string ConfigFile = pluginInterface.ConfigFile.FullName;
    public readonly string DeviceConfigsDirectory = Path.Join(pluginInterface.ConfigDirectory.FullName, "devices");
    public readonly string TriggerConfigsDirectory = Path.Join(pluginInterface.ConfigDirectory.FullName, "triggers");
}
