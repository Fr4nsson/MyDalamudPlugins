using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace HidePadlock
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; }
        public bool Lock_isVisible { get; set; } = true;
        public float Lock_Opacity { get; set; } = 1;

        [NonSerialized]
        private DalamudPluginInterface? PluginInterface;

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
