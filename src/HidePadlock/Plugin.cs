using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;

namespace HidePadlock
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Hide Padlock";

        private const string CommandName = "/padlock";

        private const string CommandName2 = "//padlock";

        private DalamudPluginInterface PluginInterface { get; init; }
        private GameGui GameGui { get; init; }
        private PluginUI PluginUi { get; init; }
        private Framework Framework { get; set; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }
        private static int MsBuilder { get; set; }
        private static int CurrentMs { get; set; }
        private unsafe AtkUnitBase* Addon { get; set; } = null;
        private unsafe AtkResNode* Padlock { get; set; } = null;

        private const uint Padlock_NodeId = 21;

        private const uint UpdateOnNumOfMs = 1000;

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] GameGui gameGui,
            [RequiredVersion("1.0")] Framework framework,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            PluginInterface = pluginInterface;
            GameGui = gameGui;
            Framework = framework;
            CommandManager = commandManager;

            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            PluginUi = new PluginUI(Configuration, this);

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Toggles the visibility of the padlock."
            });

            CommandManager.AddHandler(CommandName2, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open settings"
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            Framework.Update += FrameworkOnOnUpdateEvent;
        }

        private void FrameworkOnOnUpdateEvent(Framework framework)
        {
            int previousMs = CurrentMs;
            CurrentMs = DateTime.Now.Millisecond;
            //PluginLog.LogDebug($"Current ms: {CurrentMs}");

            int delayInMsBetweenTicks = CurrentMs - previousMs;
            if (CurrentMs < previousMs) delayInMsBetweenTicks = 999 + CurrentMs - previousMs; // DateTime.Millisecond max is 999
            //PluginLog.LogDebug($"Delay between ticks: {delayInMsBetweenTicks} ms");

            MsBuilder += delayInMsBetweenTicks;

            if (MsBuilder < 0)
            {
                PluginLog.LogDebug("--------------------------------------------------------------------");
                PluginLog.LogDebug("MsBuilder was less than zero, this should not happen, resetting to 0");
                PluginLog.LogDebug("--------------------------------------------------------------------");
                MsBuilder = 0;
            }

            if (MsBuilder >= UpdateOnNumOfMs) // Update padlock settings from config each 1000 millisecond i.e update settings from config on each second.
            {
                //PluginLog.LogDebug($"-------------------------------------------------------");
                //PluginLog.LogDebug($"If you see this message then {MsBuilder} ms has passed.");
                //PluginLog.LogDebug($"-------------------------------------------------------");
                UpdateSettings();
                MsBuilder = 0;
            }
        }

        private unsafe void UpdateSettings() 
        {
            if (GrabAddon())
            {
                if (Padlock->IsVisible != Configuration.Lock_isVisible)
                {
                    Padlock->ToggleVisibility(Configuration.Lock_isVisible);
                    PluginLog.LogDebug("Grabbed Lock_isVisible from config");
                }
                if (Padlock->Color.A != (byte)(255 * Configuration.Lock_Opacity))
                {
                    Padlock->Color.A = (byte)(255 * Configuration.Lock_Opacity);
                    PluginLog.LogDebug("Grabbed Lock_Opacity from config");
                }
            }
        }

        private unsafe bool GrabAddon()
        {
            if (Addon == null)
            {
                Addon = (AtkUnitBase*)GameGui.GetAddonByName("_ActionBar", 1);
            }

            if (Addon != null)
            {
                if (Padlock == null)
                {
                    Padlock = Addon->GetNodeById(Padlock_NodeId);
                }

                if (Padlock != null)
                {
                    return true;
                }
            }
            return false;
        }

        public unsafe void Lock_isVisible()
        {
            if (GrabAddon())
            {
                Padlock->ToggleVisibility(Configuration.Lock_isVisible = !Configuration.Lock_isVisible);
                PluginLog.LogDebug($"Addon _ActionBar, NodeId: {Padlock_NodeId}, aka Padlock, IsVisible: {Configuration.Lock_isVisible}");
            }
        }

        public unsafe void Lock_Opacity()
        {
            if (GrabAddon())
            {
                Padlock->Color.A = (byte)(255 * Configuration.Lock_Opacity);
                PluginLog.LogDebug($"Addon _ActionBar, NodeId: {Padlock_NodeId}, aka Padlock, Opacity: {Configuration.Lock_Opacity}");
            }
        }

        private unsafe void Lock_Restore()
        {
            if (GrabAddon())
            {
                Padlock->ToggleVisibility(true);
                Padlock->Color.A = 255;
                PluginLog.LogDebug($"Addon _ActionBar, NodeId: {Padlock_NodeId}, aka Padlock, restored to default values");
            }
        }

        public void Dispose()
        {
            PluginUi.Dispose();
            Framework.Update -= FrameworkOnOnUpdateEvent;
            Lock_Restore();
            CommandManager.RemoveHandler(CommandName);
            CommandManager.RemoveHandler(CommandName2);
        }

        private void OnCommand(string command, string args)
        {
            if (command == "/padlock")
            {
                Lock_isVisible();
            }
            else if (command == "//padlock")
            {
                PluginUi.SettingsVisible = true;
            }
        }

        private void DrawUI()
        {
            PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            PluginUi.SettingsVisible = true;
        }
    }
}
