using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UltraVibrations.Services;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace UltraVibrations.Triggers;

public class Trigger : ISavable
{
    public Trigger(SaveService saveService)
    {
        this.saveService = saveService;
    }
    
    [JsonIgnore]
    private readonly SaveService saveService;

    public static Trigger? FromFile(string filePath, SaveService saveService)
    {
        if (File.Exists(filePath))
        {
            try
            {
                var text = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(text))
                {
                    return null;
                }

                var loadedDevice = new Trigger(saveService);
                JsonConvert.PopulateObject(
                    text,
                    loadedDevice,
                    new JsonSerializerSettings { Error = HandleDeserializationError }
                );

                return loadedDevice;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error loading Configuration:\n{ex}");
                return null;
            }
        }

        return null;

        void HandleDeserializationError(object? sender, ErrorEventArgs errorArgs)
        {
            Plugin.Log.Error(
                $"Error parsing Trigger Configuration at {errorArgs.ErrorContext.Path}:\n{errorArgs.ErrorContext.Error}"
            );
            errorArgs.ErrorContext.Handled = true;
        }
    }

    public int Version = 0;
    public string Id = Guid.NewGuid().ToString();
    public string Name = "";
    public bool Enabled = true;
    public TriggerType Type = TriggerType.None;
    
    public ChatTriggerSettings ChatSettings = new();
    
    public List<TriggerEffect> Effects = [];

    public string ToFilename(FilenameService fileNames) => Path.Join(fileNames.TriggerConfigsDirectory, $"{Id}.json");

    public void Save() => saveService.QueueSave(this);

    public void Save(StreamWriter writer)
    {
        using var jWriter = new JsonTextWriter(writer) { Formatting = Formatting.Indented };
        var serializer = new JsonSerializer();
        serializer.Serialize(jWriter, this);
    }
}
