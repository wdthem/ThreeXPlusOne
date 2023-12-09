using System.Text.Json;
using ThreeXPlusOne.Code;
using ThreeXPlusOne.Config;

string jsonFilePath = "settings.json";
string json = File.ReadAllText(jsonFilePath);

Settings? settings = JsonSerializer.Deserialize<Settings>(json) ?? throw new Exception("Invalid settings");

Process.Run(settings);