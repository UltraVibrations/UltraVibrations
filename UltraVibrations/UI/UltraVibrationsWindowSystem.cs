using System;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using OtterGui.Log;

namespace UltraVibrations.UI;

public class UltraVibrationsWindowSystem : IDisposable
{
    private readonly UiBuilder uiBuilder;
    private readonly MainWindow mainWindow;
    private readonly Logger log;
    private readonly WindowSystem windowSystem;

    public UltraVibrationsWindowSystem(UiBuilder uiBuilder, MainWindow mainWindow, Logger log)
    {
        this.uiBuilder = uiBuilder;
        this.mainWindow = mainWindow;
        this.log = log;

        windowSystem = new WindowSystem("UltraVibrations");
        windowSystem.AddWindow(mainWindow);

        uiBuilder.Draw += windowSystem.Draw;
        uiBuilder.OpenConfigUi += mainWindow.Toggle;
        uiBuilder.OpenMainUi += mainWindow.Toggle;
    }

    public void Dispose()
    {
        uiBuilder.Draw -= windowSystem.Draw;
    }

    public void Toggle()
    {
        mainWindow.Toggle();
    }
}
