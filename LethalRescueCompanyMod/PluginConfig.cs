using BepInEx.Configuration;

namespace LethalRescueCompanyPlugin
{
    public class PluginConfig
    {
        readonly ConfigFile configFile;

        public bool debug { get; set; }

        public PluginConfig(ConfigFile cfg)
        {
            configFile = cfg;
        }

        private T ConfigEntry<T>(string section, string key, T defaultVal, string description)
        {
            return configFile.Bind(section, key, defaultVal, description).Value;
        }

        public void InitBindings()
        {
            debug = ConfigEntry("Developer", "Debug", true, "Developer Super Powers and verbose logging");

        }

    }
}