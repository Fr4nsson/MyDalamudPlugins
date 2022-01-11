using ImGuiNET;
using System;
using System.Numerics;

namespace AntiAfk
{
    class PluginUI : IDisposable
    {
        private readonly Configuration Configuration;
        private readonly Plugin Plugin;
        private readonly Vector4 V4Green = new(0, 1, 0, 1);
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

        public unsafe void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            if (ImGui.Begin($"AntiAfk - Settings", ref settingsVisible, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize))
            {
                bool refBool = false;
                int refInt = 0;
                float windowWidth = ImGui.GetWindowWidth();

                refBool = Configuration.Enable;
                if (ImGui.Checkbox("Enable Plugin", ref refBool))
                {
                    Configuration.Enable = refBool;
                    Configuration.Save();
                }

                if (Configuration.Enable)
                {
                    ImGui.SameLine();
                    refBool = Configuration.VisualizeUpdates;
                    if (ImGui.Checkbox("Visualize Updates", ref refBool))
                    {
                        Configuration.VisualizeUpdates = refBool;
                        Configuration.Save();
                    }

                    ImGui.SameLine();
                    refBool = Configuration.Debug;
                    if (ImGui.Checkbox("Debug", ref refBool))
                    {
                        Configuration.Debug = refBool;
                        Configuration.Save();
                    }

                    ImGui.SameLine(windowWidth / 1.238f);
                    ImGui.Text($"Time Afk: {TimeSpan.FromSeconds((int)(Plugin.Max(*Plugin.AfkTimer, *Plugin.AfkTimer2, *Plugin.AfkTimer3)))}");
                    ImGui.Separator();

                    ImGui.Text($"A keypress will be generated on a random time between:");
                    ImGui.Text($"{TimeSpan.FromSeconds(Configuration.RndNumMin)}");
                    ImGui.SameLine();
                    ImGui.Text($"and");
                    ImGui.SameLine();
                    ImGui.Text($"{TimeSpan.FromSeconds(Configuration.RndNumMax)}");

                    ImGui.NewLine();
                    int num_Min = 60;
                    int num_Max = 900;
                    refInt = Configuration.RndNumMin;
                    if (ImGui.SliderInt("##RndNumMin", ref refInt, num_Min, num_Max, $"{TimeSpan.FromSeconds(refInt)}"))
                    {
                        if (refInt > Configuration.RndNumMax)
                        {
                            refInt = Configuration.RndNumMax - 1;
                        }

                        if (refInt < num_Min || refInt > num_Max)
                        {
                            refInt = num_Min; // Tried to input a value that is not allowed, reset value to Min.
                        }

                        Configuration.RndNumMin = refInt;
                        Configuration.Save();
                    }
                    ImGui.SameLine();
                    refInt = Configuration.RndNumMax;
                    if (ImGui.SliderInt("##RndNumMax", ref refInt, num_Min, num_Max, $"{TimeSpan.FromSeconds(refInt)}"))
                    {
                        if (refInt < Configuration.RndNumMin)
                        {
                            refInt = Configuration.RndNumMin + 1;
                        }

                        if (refInt < num_Min || refInt > num_Max)
                        {
                            refInt = num_Max; // Tried to input a value that is not allowed, reset value to Max.
                        }

                        Configuration.RndNumMax = refInt;
                        Configuration.Save();
                    }

                    if (Configuration.VisualizeUpdates)
                    {
                        ImGui.Separator();
                        ImGui.NewLine();
                        ImGui.Text($"Time left until keypress");
                        ImGui.ProgressBar(*Plugin.AfkTimer / (float)Plugin.RandomWait, new Vector2(600, 40), "");
                        ImGui.SameLine();
                        if (Plugin.KeyPressed == true)
                        {
                            ImGui.TextColored(V4Green, "Keypress Sent!");
                        }
                        else
                        {
                            ImGui.Text($"{TimeSpan.FromSeconds((int)(Plugin.RandomWait - *Plugin.AfkTimer))}");
                        } 
                    }

                    if (Configuration.Debug)
                    {
                        ImGui.Separator();
                        ImGui.Text($"{TimeSpan.FromSeconds((int)(*Plugin.AfkTimer))} = Afk Timer 1");
                        ImGui.Text($"{TimeSpan.FromSeconds((int)(*Plugin.AfkTimer2))} = Afk Timer 2");
                        ImGui.Text($"{TimeSpan.FromSeconds((int)(*Plugin.AfkTimer3))} = Afk Timer 3");
                        ImGui.Text($"{TimeSpan.FromSeconds(Plugin.RandomWait)} = Randomly generated wait time");
                        ImGui.SameLine();
                        if (ImGui.SmallButton("New"))
                        {
                            Plugin.RandomWait = 1;
                        }
                    }
                }
            }
            ImGui.End();
        }
    }
}
