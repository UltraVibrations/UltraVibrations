using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;

namespace UltraVibrations.UI.Components;

public static class ConnectedIndicator
{
    private static readonly Vector4 ColorDeviceActive = new(0, 1, 1, 1);
    private static readonly Vector4 ColorDeviceDisconnected = new(1, 0, 0, 1);

    public static void Draw(bool connected)
    {
        using var style = ImRaii.PushColor(ImGuiCol.Text, connected ? ColorDeviceActive : ColorDeviceDisconnected);
        ImGui.Bullet();
        style.Pop();
    }
}
