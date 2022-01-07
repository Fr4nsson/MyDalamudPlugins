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

        private unsafe AtkUnitBase* Addon { get; set; } = null;
        private unsafe AtkResNode* Padlock { get; set; } = null;

        private const uint Padlock_NodeId = 21;


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

        public unsafe bool GrabAddon()
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

        public unsafe void OnLoad()
        {
            if (GrabAddon())
            {
                Padlock->ToggleVisibility(Configuration.Lock_isVisible);
                Padlock->Color.A = (byte)(255 * Configuration.Lock_Opacity);
            }
        }

        public unsafe void Lock_isVisible()
        {
            if (GrabAddon())
            {
                Padlock->ToggleVisibility(Configuration.Lock_isVisible = !Configuration.Lock_isVisible);
                PluginLog.Log($"Addon _ActionBar, NodeId: {Padlock_NodeId}, aka Padlock, IsVisible: {Configuration.Lock_isVisible}");
            }
        }

        public unsafe void Lock_Opacity()
        {
            if (GrabAddon())
            {
                Padlock->Color.A = (byte)(255 * Configuration.Lock_Opacity);
                PluginLog.Log($"Addon _ActionBar, NodeId: {Padlock_NodeId}, aka Padlock, Opacity: {Configuration.Lock_Opacity}");
            }
        }

        public unsafe void Lock_Restore()
        {
            if (GrabAddon())
            {
                Padlock->ToggleVisibility(true);
                Padlock->Color.A = 255;
                PluginLog.Log($"Addon _ActionBar, NodeId: {Padlock_NodeId}, aka Padlock, Restored to default");
            }
        }

        public void Dispose()
        {
            Lock_Restore();
            PluginUi.Dispose();
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
