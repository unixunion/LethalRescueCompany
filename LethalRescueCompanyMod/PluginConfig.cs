using BepInEx.Configuration;

namespace LethalRescueCompanyPlugin;

public class PluginConfig
{
    private readonly ConfigFile configFile;

    public PluginConfig(ConfigFile cfg)
    {
        configFile = cfg;
    }

    public bool debug { get; set; }

    private T ConfigEntry<T>(string section, string key, T defaultVal, string description)
    {
        return configFile.Bind(section, key, defaultVal, description).Value;
    }

    public void InitBindings()
    {
        debug = ConfigEntry("Developer", "Debug", true, "Developer Super Powers and verbose logging");
    }
}