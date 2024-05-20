using System;
using System.Collections.Generic;
using System.IO;
using Buttplug.Client;
using UltraVibrations.Devices;

namespace UltraVibrations.Services;

public class DeviceManagerService
{
    private readonly SaveService saveService;

    public DeviceManagerService(SaveService saveService)
    {
        this.saveService = saveService;
        Load();
    }

    public void Load()
    {
        Plugin.Log.Debug("Loading devices.");
        if (!Directory.Exists(saveService.FileNames.DeviceConfigsDirectory))
        {
            Directory.CreateDirectory(saveService.FileNames.DeviceConfigsDirectory);
        }

        var dirty = false;
        foreach (var fileName in Directory.EnumerateFiles(saveService.FileNames.DeviceConfigsDirectory))
        {
            Plugin.Log.Debug(fileName);
            if (!fileName.EndsWith(".json"))
            {
                continue;
            }

            var device = Device.FromFile(fileName, saveService);

            if (device != null)
            {
                Devices.Add(device);
                dirty = true;
            }
        }

        if (dirty)
        {
            OnDevicesChanged?.Invoke(this, Devices);
        }
    }

    public readonly List<Device> Devices = [];

    // Device Change EventHandler
    public EventHandler<IEnumerable<Device>>? OnDevicesChanged;

    public void ClearDevices()
    {
        Devices.Clear();
        OnDevicesChanged?.Invoke(this, Devices);
    }

    public Device? GetDevice(uint deviceId) => Devices.Find(d => d.Id == deviceId);

    public void RemoveDevice(uint id)
    {
        Devices.RemoveAll(d => d.Id == id);
        OnDevicesChanged?.Invoke(this, Devices);
    }

    public void MarkDisconnected(uint? id)
    {
        var dirty = false;
        foreach (var device in Devices)
        {
            if (id != null && device.Id != id)
            {
                continue;
            }

            device.AssumeConnected = false;
            dirty = true;
        }

        if (dirty)
        {
            OnDevicesChanged?.Invoke(this, Devices);
        }
    }

    public void UpdateDevices(IEnumerable<ButtplugClientDevice> devices)
    {
        foreach (var device in devices)
        {
            this.SetDevice(device);
        }
    }

    public void SetDevice(ButtplugClientDevice device)
    {
        var existingDevice = Devices.Find(d => d.Id == device.Index);
        if (existingDevice == null)
        {
            Devices.Add(Device.FromButtplugDevice(device, saveService));
            OnDevicesChanged?.Invoke(this, Devices);
            return;
        }

        existingDevice.UpdateFromButtplugDevice(device);
        OnDevicesChanged?.Invoke(this, Devices);
    }
}
