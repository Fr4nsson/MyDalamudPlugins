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
        
        private DalamudPluginInterface PluginInterface { get; init; }
        private GameGui GameGui { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] GameGui gameGui,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            PluginInterface = pluginInterface;
            GameGui = gameGui;
            CommandManager = commandManager;

            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Toggles the visibility of the padlock."
            });
        }

        public unsafe void ToggleLock()
        {
            bool isVisible = Configuration.ShowLock;
            var addon = (AtkUnitBase*)GameGui.GetAddonByName("_ActionBar", 1);
            uint nodeId = 20;

            if (addon != null)
            {
                var padlock = addon->GetNodeById(nodeId);
                if (padlock != null)
                {
                    isVisible = padlock->IsVisible;
                    isVisible = !isVisible;
                    padlock->ToggleVisibility(isVisible);
                }
            }

            PluginLog.Log($"Addon _ActionBar, Node[7], NodeId: {nodeId}, aka Padlock, IsVisible: {isVisible}");
            Configuration.ShowLock = isVisible;
            Configuration.Save();
        }

        public void Dispose()
        {
            CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            ToggleLock();
        }
    }
}
