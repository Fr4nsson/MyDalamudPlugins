using Dalamud.Utility;
using ImGuiNET;
using System;
using System.Numerics;

namespace OopsNoLalafells
{
    public class PluginUI
    {
        private static Vector4 WHAT_THE_HELL_ARE_YOU_DOING = new Vector4(1, 0, 0, 1);
        private readonly Plugin plugin;
        private bool enableExperimental;

        // had to do it, /No you did not
        //private bool changeSelf;
        //private bool changeSelfLaunched;
        //private bool changeSelfShowText;

        public PluginUI(Plugin plugin)
        {
            this.plugin = plugin;
        }

        public void Draw()
        {
            if (!this.plugin.SettingsVisible)
            {
                return;
            }

            bool settingsVisible = this.plugin.SettingsVisible;
            if (ImGui.Begin("Oops, No Lalafells!", ref settingsVisible, ImGuiWindowFlags.AlwaysAutoResize))
            {

                bool shouldChangeOthers = this.plugin.config.ShouldChangeOthers;
                ImGui.Checkbox("Change other players", ref shouldChangeOthers);

                if (shouldChangeOthers)
                {
                    bool onlyChangeLalafells = this.plugin.config.OnlyChangeLalafells;
                    ImGui.Checkbox("Only change lalafells", ref onlyChangeLalafells);

                    this.plugin.OnlyChangeLalafells(onlyChangeLalafells);
                }

                Race othersTargetRace = this.plugin.config.ChangeOthersTargetRace;

                if (shouldChangeOthers)
                {
                    if (ImGui.BeginCombo("Race", othersTargetRace.GetAttribute<Display>().Value))
                    {
                        foreach (Race race in Enum.GetValues(typeof(Race)))
                        {
                            ImGui.PushID((byte) race);
                            if (ImGui.Selectable(race.GetAttribute<Display>().Value, race == othersTargetRace))
                            {
                                othersTargetRace = race;
                            }

                            if (race == othersTargetRace)
                            {
                                ImGui.SetItemDefaultFocus();
                            }

                            ImGui.PopID();
                        }

                        ImGui.EndCombo();
                    }
                }

                this.plugin.UpdateOtherRace(othersTargetRace);

                this.plugin.ToggleOtherRace(shouldChangeOthers);

                //------------------------------------------------

                bool shouldChangeSelf = this.plugin.config.ChangeSelf;
                ImGui.Checkbox("Change self", ref shouldChangeSelf);

                this.plugin.ToggleChangeSelf(shouldChangeSelf);

                Race selfTargetRace = this.plugin.config.ChangeSelfTargetRace;

                if (shouldChangeSelf)
                {
                    if (ImGui.BeginCombo("Race Self", selfTargetRace.GetAttribute<Display>().Value))
                    {
                        foreach (Race race in Enum.GetValues(typeof(Race)))
                        {
                            ImGui.PushID((byte)race);
                            if (ImGui.Selectable(race.GetAttribute<Display>().Value, race == selfTargetRace))
                            {
                                selfTargetRace = race;
                            }

                            if (race == selfTargetRace)
                            {
                                ImGui.SetItemDefaultFocus();
                            }

                            ImGui.PopID();
                        }

                        ImGui.EndCombo();
                    }
                }

                this.plugin.UpdateSelfRace(selfTargetRace);

                if (enableExperimental)
                {
                    bool immersiveMode = this.plugin.config.ImmersiveMode;
                    ImGui.Checkbox("Immersive Mode", ref immersiveMode);
                    ImGui.Text("If Immersive Mode is enabled, \"Examine\" windows will also be modified.");

                    this.plugin.UpdateImmersiveMode(immersiveMode);
                }

                ImGui.Separator();

                ImGui.Checkbox("Enable Experimental Features", ref this.enableExperimental);
                if (enableExperimental)
                {
                    ImGui.Text("Experimental feature configuration will (intentionally) not persist,\n" +
                               "so you will need to open this settings menu to re-activate\n" +
                               "them if you disable the plugin or restart your game.");

                    ImGui.TextColored(WHAT_THE_HELL_ARE_YOU_DOING,
                        "Experimental features may crash your game, uncat your boy,\nor cause the Eighth Umbral Calamity. YOU HAVE BEEN WARNED!");

                    ImGui.Text(
                        "But seriously, if you do encounter any crashes, please report\nthem to someone on the internet with whatever details you can get.");
                }

                ImGui.End();
            }

            this.plugin.SettingsVisible = settingsVisible;
            this.plugin.SaveConfig();
        }
    }
}