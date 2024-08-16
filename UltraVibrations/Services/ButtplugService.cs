using System;
using System.Threading.Tasks;
using Buttplug.Client;
using Buttplug.Client.Connectors.WebsocketConnector;
using Buttplug.Core;

namespace UltraVibrations.Services;

public class ButtplugService : IDisposable
{
    private readonly ButtplugClient client = new("UltraVibrations");
    private readonly Configuration configuration;
    private readonly DeviceManagerService deviceManagerService;

    public event EventHandler<int> OnRequestedConnect;
    public event EventHandler OnRequestedDisconnect;

    public event EventHandler OnRequestedScan;

    public bool IsConnected => client.Connected;
    public bool IsScanning { get; private set; }
    private string? statusMessage;

    public string StatusMessage
    {
        get
        {
            if (statusMessage != null)
            {
                return statusMessage;
            }

            var scanning = IsScanning ? " - Scanning" : "";
            return IsConnected ? "Connected" + scanning : "Disconnected";
        }
    }

    public ButtplugService(Configuration configuration, DeviceManagerService deviceManagerService)
    {
        this.configuration = configuration;
        this.deviceManagerService = deviceManagerService;
        client.DeviceAdded += DeviceAdded;
        client.DeviceRemoved += DeviceRemoved;
        client.ScanningFinished += HandleScanCompleted;
        client.ServerDisconnect += HandleServerDisconnect;
        client.PingTimeout += HandlePingTimeout;
        client.ErrorReceived += HandleErrorReceived;

        OnRequestedConnect += HandleRequestedConnected;
        OnRequestedDisconnect += HandleRequestedDisconnect;

        OnRequestedScan += HandleRequestedScan;
    }

    public ButtplugClientDevice? GetDevice(uint index)
    {
        return Array.Find(client.Devices, (d) => d.Index == index);
    }

    private void HandleErrorReceived(object? sender, ButtplugExceptionEventArgs e)
    {
        Plugin.Log.Error($"Error received: {e.Exception.Message}");
    }

    private void HandlePingTimeout(object? sender, EventArgs e)
    {
        Plugin.Log.Error("Ping timeout.");
    }

    private void HandleServerDisconnect(object? sender, EventArgs e)
    {
        Plugin.Log.Error("Server disconnected.");
        desiredConnected = false;
    }

    public void Dispose()
    {
        client.DeviceAdded -= DeviceAdded;
        client.DeviceRemoved -= DeviceRemoved;
        client.Dispose();
    }

    private void HandleRequestedConnected(object? sender, int retryCount)
    {
        if (retryCount > configuration.ButtplugServerReconnectAttempts)
        {
            Plugin.Log.Error("Failed to connect to the Buttplug server.");
            return;
        }

        Task.Run(async () => await Connect(retryCount));
    }

    private void HandleRequestedDisconnect(object? sender, EventArgs e)
    {
        Task.Run(async () => await Disconnect());
    }

    private void HandleRequestedScan(object? sender, EventArgs e)
    {
        Task.Run(async () => await Scan());
    }

    private bool desiredConnected;

    public bool DesiredConnected
    {
        get => desiredConnected;
        set
        {
            if (value == desiredConnected)
            {
                return;
            }

            if (value)
            {
                OnRequestedConnect.Invoke(this, 0);
            }
            else
            {
                OnRequestedDisconnect.Invoke(this, EventArgs.Empty);
            }

            desiredConnected = value;
        }
    }

    public async Task Connect(int retryCount = 0)
    {
        if (!desiredConnected)
        {
            statusMessage = null;
            return;
        }
        
        if (client.Connected)
        {
            Plugin.Log.Error("Already connected to the Buttplug server.");
            return;
        }

        try
        {
            var uri = new Uri(configuration.ButtplugServerUrl);
            var connector = new ButtplugWebsocketConnector(uri);
            statusMessage = "Connecting...";
            await client.ConnectAsync(connector);
            if (!client.Connected)
            {
                Plugin.Log.Debug("Failed to connect.");
                Retry();
                return;
            }

            Plugin.Log.Debug("Connected. Triggering scan.");
            OnRequestedScan?.Invoke(this, EventArgs.Empty);
        }
        catch (ButtplugException e)
        {
            Plugin.Log.Error($"Buttplug connection failed: {e.Message}");
            Retry();
            return;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"Failed to connect: {e.Message}");
            Retry();
            return;
        }

        return;

        async void Retry()
        {
            if (retryCount >= configuration.ButtplugServerReconnectAttempts)
            {
                Plugin.Log.Error("Failed to connect to the Buttplug server.");
                desiredConnected = false;
                statusMessage = null;
                return;
            }

            var expBackoff = (int)(Math.Pow(2, retryCount) * configuration.ButtplugServerReconnectDelay);
            statusMessage = $"Failed to connect. Retrying in {expBackoff / 1000}s (Retry {retryCount + 1})...";
            Plugin.Log.Warning(
                $"Failed to connect to the Buttplug server. Retrying in {expBackoff}ms (Retry {retryCount + 1})..."
            );
            await Task.Delay(expBackoff);
            OnRequestedConnect.Invoke(this, retryCount + 1);
        }
    }

    public async Task Disconnect()
    {
        if (client.Connected)
        {
            Plugin.Log.Debug("Disconnecting from the Buttplug server.");
            await client.DisconnectAsync();
            deviceManagerService.MarkDisconnected(null);
        }
    }

    public void RequestScan()
    {
        OnRequestedScan?.Invoke(this, EventArgs.Empty);
    }

    private async Task Scan()
    {
        if (!IsScanning)
        {
            Plugin.Log.Debug("Scanning for devices.");
            await client.StartScanningAsync();
            IsScanning = true;
            await Task.Delay(configuration.ButtplugServerScanDuration);
            await StopScanning();
        }
    }

    private async Task StopScanning()
    {
        if (IsScanning)
        {
            Plugin.Log.Debug("Stopping scan.");
            await client.StopScanningAsync();
            IsScanning = false;
        }
    }

    private void DeviceAdded(object? sender, DeviceAddedEventArgs args)
    {
        Plugin.Log.Information($"Device added: {args.Device.Name}");
        deviceManagerService.UpdateDevices(client.Devices);
    }

    private void DeviceRemoved(object? sender, DeviceRemovedEventArgs args)
    {
        Plugin.Log.Information($"Device removed: {args.Device.Name}");
        deviceManagerService.MarkDisconnected(args.Device.Index);
    }

    private static void HandleScanCompleted(object? sender, EventArgs e)
    {
        Plugin.Log.Debug("Scan completed.");
    }
}
