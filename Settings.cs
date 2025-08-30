using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Typo;

[JsonObject(MemberSerialization.OptIn)]
public class Settings
{
    [JsonProperty]
    public List<Layout> layouts { get; set; }= [];

    public Settings()
    {
    }
    
    // Serialize this Layout instance to a JSON file
    public void SaveToFile(string filePath)
    {
        var jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto 
        };
        jsonSettings.SerializationBinder = new KnownTypesBinder
        {
            KnownTypes = new List<Type>
            {
                typeof(KeyPressAction)
                // Add all IAction implementations here
            }
        };
        string json = JsonConvert.SerializeObject(this, jsonSettings);
        File.WriteAllText(filePath, json);
    }

    // Deserialize a JSON file into a Layout instance and initialize it
    public static Settings LoadFromFile(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var jsonSettings = new JsonSerializerSettings
        {
            // Case-insensitive property matching
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            TypeNameHandling = TypeNameHandling.Auto 
        };
        jsonSettings.SerializationBinder = new KnownTypesBinder
        {
            KnownTypes = new List<Type>
            {
                typeof(KeyPressAction)
                // Add all IAction implementations here
            }
        };
        
        var settings = JsonConvert.DeserializeObject<Settings>(json, jsonSettings);
        //settings.Initialize(); // Call post-deserialization logic
        return settings;
    }
    
    private static string GetBaseDirectory()
    {
        return Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
    }

    public static string GetSettingsPath()
    {
        return Path.Combine(GetBaseDirectory(), "settings.json");
    }

    public static string GetDllInjectionPath()
    {
        return Path.Combine(GetBaseDirectory(), "DllInjection.dll");
    }
    
    [OnDeserialized]
    internal void OnDeserialized(StreamingContext context)
    {
        Initialize();
    }
    
    public void Initialize()
    {
        foreach (var layout in layouts)
        {
            layout.Initialize();
        }
    }
    private class KnownTypesBinder : ISerializationBinder
    {
        public IList<Type> KnownTypes { get; set; }

        public Type BindToType(string assemblyName, string typeName)
        {
            return KnownTypes?.FirstOrDefault(t => t.FullName == typeName);
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.FullName;
        }
    }
}