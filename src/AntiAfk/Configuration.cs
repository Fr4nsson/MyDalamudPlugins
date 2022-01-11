using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace AntiAfk
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; }
        public int RndNumMin { get; set; } = 60;
        public int RndNumMax { get; set; } = 900;
        public bool VisualizeUpdates { get; set; } = false;
        public bool Debug { get; set; } = false;
        public bool Enable { get; set; } = true;

        [NonSerialized]
        private DalamudPluginInterface PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
        }

        public void Save()
        {
            PluginInterface!.SavePluginConfig(this);
        }
    }
}
