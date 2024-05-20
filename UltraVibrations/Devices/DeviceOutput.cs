using System.Collections.Generic;
using Buttplug.Core.Messages;
using Newtonsoft.Json;

namespace UltraVibrations.Devices;

public class DeviceOutput
{
    public static DeviceOutput FromAttribute(uint deviceId, GenericDeviceMessageAttributes attribute)
    {
        return new DeviceOutput
        {
            DeviceId = deviceId,
            Id = attribute.Index,
            Type = attribute.ActuatorType,
            Description = attribute.FeatureDescriptor,
        };
    }

    public void UpdateFromAttribute(GenericDeviceMessageAttributes attribute)
    {
        Type = attribute.ActuatorType;
        Description = attribute.FeatureDescriptor;
    }

    public uint DeviceId;
    public uint Id;
    public ActuatorType Type;
    public string Description = "";

    public readonly List<string> Channels = [];

    public float MaxSpeed = 1.0f;
}
