using ImGuiNET;
using System;

namespace HidePadlock
{
    class PluginUI : IDisposable
    {
        private readonly Configuration Configuration;

        private readonly Plugin Plugin;

        private bool visible = false;

        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        private bool settingsVisible = false;

        public bool SettingsVisible
        {
            get { return settingsVisible; }
            set { settingsVisible = value; }
        }

        public PluginUI(Configuration configuration, Plugin plugin)
        {
            Configuration = configuration;
            Plugin = plugin;
        }

        public void Dispose()
        {

        }

        public void Draw()
        {
            DrawSettingsWindow();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            if (ImGui.Begin("HidePadlock - Settings", ref settingsVisible, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                bool refBool = Configuration.Lock_isVisible;
                if (ImGui.Checkbox("Show Padlock", ref refBool))
                {
                    Plugin.ToggleLock(true);
                    Configuration.Lock_isVisible = refBool;
                    Configuration.Save();
                }

                if (Configuration.Lock_isVisible)
                {
                    float refFloat = Configuration.Lock_Opacity;
                    float minValue = 0.1f;
                    float maxValue = 1.0f;
                    if (ImGui.DragFloat("Opacity", ref refFloat, .005f, minValue, maxValue, "%.1f"))
                    {
                        Configuration.Lock_Opacity = refFloat;
                        Plugin.ToggleLock(false);
                        Configuration.Save();
                    }
                }
            }
            ImGui.End();
        }
    }
}
