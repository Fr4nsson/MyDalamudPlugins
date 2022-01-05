using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HidePadlock
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Hide Padlock";

        private const string CommandName = "/padlock";

        private const string CommandName2 = "//padlock";

        private DalamudPluginInterface PluginInterface { get; init; }
        private GameGui GameGui { get; init; }
        private ChatGui ChatGui { get; init; }
        private PluginUI PluginUi { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] GameGui gameGui,
            [RequiredVersion("1.0")] ChatGui chatGui,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            PluginInterface = pluginInterface;
            GameGui = gameGui;
            ChatGui = chatGui;
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

            OnLoad();
        }

        public unsafe void OnLoad()
        {
            var addon = (AtkUnitBase*)GameGui.GetAddonByName("_ActionBar", 1);
            if (addon != null)
            {
                var padlock = addon->GetNodeById(20);
                if (padlock != null)
                {
                    padlock->ToggleVisibility(Configuration.Lock_isVisible);
                    padlock->Color.A = (byte)(255 * Configuration.Lock_Opacity);
                }
            }
        }

        public unsafe void ToggleLock(bool showLock)
        {
            var addon = (AtkUnitBase*)GameGui.GetAddonByName("_ActionBar", 1);
            uint nodeId = 21; // NodeId for the padlock

            if (addon != null)
            {
                var padlock = addon->GetNodeById(nodeId);
                if (padlock != null)
                {
                    if (showLock)
                    {
                        padlock->ToggleVisibility(Configuration.Lock_isVisible = !Configuration.Lock_isVisible);
                        PluginLog.Log($"Addon _ActionBar, Node[7], NodeId: {nodeId}, aka Padlock, IsVisible: {Configuration.Lock_isVisible}");
                    }
                    else
                    {
                        padlock->Color.A = (byte)(255 * Configuration.Lock_Opacity);
                        PluginLog.Log($"Addon _ActionBar, Node[7], NodeId: {nodeId}, aka Padlock, Opacity: {Configuration.Lock_Opacity}");
                    }
                }
            }
        }

        public void Dispose()
        {
            PluginUi.Dispose();
            CommandManager.RemoveHandler(CommandName);
            CommandManager.RemoveHandler(CommandName2);
        }

        private void OnCommand(string command, string args)
        {
            if (command == "/padlock")
            {
                ToggleLock(true);
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
