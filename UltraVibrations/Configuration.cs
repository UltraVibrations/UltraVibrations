using System;
using System.IO;
using Dalamud.Configuration;
using Newtonsoft.Json;
using UltraVibrations.Services;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace UltraVibrations
{
    [Serializable]
    public class Configuration : IPluginConfiguration, ISavable
    {
        [JsonIgnore]
        private readonly SaveService saveService;

        public int Version { get; set; } = 0;
        public string ButtplugServerUrl { get; set; } = "ws://localhost:12345/";
        public int ButtplugServerReconnectDelay { get; set; } = 1000;
        public int ButtplugServerReconnectAttempts { get; set; } = 4;
        public int ButtplugServerScanDuration { get; set; } = 10000;

        public Configuration(SaveService saveService)
        {
            this.saveService = saveService;
            
            Load();
        }

        public void Load()
        {
            if (File.Exists(saveService.FileNames.ConfigFile))
            {
                try
                {
                    var text = File.ReadAllText(saveService.FileNames.ConfigFile);
                    JsonConvert.PopulateObject(
                        text,
                        this,
                        new JsonSerializerSettings { Error = HandleDeserializationError, }
                    );
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error($"Error loading Configuration:\n{ex}");
                }
            }

            return;

            void HandleDeserializationError(object? sender, ErrorEventArgs errorArgs)
            {
                Plugin.Log.Error(
                    $"Error parsing Configuration at {errorArgs.ErrorContext.Path}, using default or migrating:\n{errorArgs.ErrorContext.Error}"
                );
                errorArgs.ErrorContext.Handled = true;
            }
        }

        public string ToFilename(FilenameService fileNames) => fileNames.ConfigFile;

        public void Save(StreamWriter writer) => saveService.QueueSave(this);
    }
}
