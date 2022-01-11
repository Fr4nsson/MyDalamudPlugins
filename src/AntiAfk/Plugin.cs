using Dalamud.Game.Command;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;
using System.Linq;
using System.Threading;
using static AntiAfk.Native.Keypress;

namespace AntiAfk
{
    unsafe class Plugin : IDalamudPlugin
    {
        public string Name => "AntiAfk";
        private const string CommandName = "//afk";

        internal volatile bool Running = true;
        //long NextKeyPress = 0;
        IntPtr BaseAddress = IntPtr.Zero;
        public float* AfkTimer;
        public float* AfkTimer2;
        public float* AfkTimer3;
        public int RandomWait;
        public bool KeyPressed;

        public static PluginUI PluginUi { get; set; }
        public static Configuration Configuration { get; set; }

        delegate long UnkFunc(IntPtr a1, float a2);

        readonly Hook<UnkFunc> UnkFuncHook;

        public void Dispose()
        {
            if (!UnkFuncHook.IsDisposed)
            {
                if (UnkFuncHook.IsEnabled)
                {
                    UnkFuncHook.Disable();
                }
                UnkFuncHook.Dispose();
            }
            Running = false;
            Svc.CommandManager.RemoveHandler(CommandName);
            PluginUi.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            if (command == "//afk")
            {
                PluginUi.SettingsVisible = true;
            }
        }

        public Plugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Svc>();
            UnkFuncHook = new(Svc.SigScanner.ScanText("48 8B C4 48 89 58 18 48 89 70 20 55 57 41 55"), UnkFunc_Dtr);
            UnkFuncHook.Enable();

            Configuration = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(Svc.PluginInterface);

            PluginUi = new PluginUI(Configuration, this);

            Svc.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open settings"
            });

            Svc.PluginInterface.UiBuilder.Draw += DrawUI;
            Svc.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        private void DrawUI()
        {
            PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            PluginUi.SettingsVisible = true;
        }

        void BeginWork()
        {
            AfkTimer = (float*)(BaseAddress + 20);
            AfkTimer2 = (float*)(BaseAddress + 24);
            AfkTimer3 = (float*)(BaseAddress + 28);
            var rnd = new Random();
            RandomWait = rnd.Next(Configuration.RndNumMin, Configuration.RndNumMax);
            new Thread((ThreadStart)delegate
            {
                while (Running)
                {
                    if (Configuration.Enable)
                    {
                        try
                        {
                            KeyPressed = false;
                            //PluginLog.Debug($"Afk timers: {*AfkTimer}/{*AfkTimer2}/{*AfkTimer3}");
                            if (Max(*AfkTimer, *AfkTimer2, *AfkTimer3) > RandomWait) // Max(*AfkTimer, *AfkTimer2, *AfkTimer3) > 2f * 60f
                            {
                                if (Native.TryFindGameWindow(out var mwh))
                                {
                                    //PluginLog.Debug($"Afk timer before: {*AfkTimer}/{*AfkTimer2}/{*AfkTimer3}");
                                    //PluginLog.Debug($"Sending anti-afk keypress: {mwh:X16}");
                                    
                                    _ = new TickScheduler(delegate
                                    {
                                        KeyPressed = true;
                                        SendMessage(mwh, WM_KEYDOWN, (IntPtr)LControlKey, (IntPtr)0);
                                        _ = new TickScheduler(delegate
                                        {
                                            SendMessage(mwh, WM_KEYUP, (IntPtr)LControlKey, (IntPtr)0);
                                            KeyPressed = false;
                                            RandomWait = rnd.Next(Configuration.RndNumMin, Configuration.RndNumMax);
                                            //PluginLog.Debug($"Afk timer after: {*AfkTimer}/{*AfkTimer2}/{*AfkTimer3}");
                                        }, Svc.Framework, 200);
                                    }, Svc.Framework, 0);
                                }
                                else
                                {
                                    PluginLog.Error("Could not find game window");
                                }
                            }
                            Thread.Sleep(100); // 10000
                            //MaxAfk = Max(*AfkTimer, *AfkTimer2, *AfkTimer3);
                        }
                        catch (Exception e)
                        {
                            PluginLog.Error(e.Message + "\n" + e.StackTrace ?? "");
                        }
                    }
                    else
                    {
                        RandomWait = 1;
                    }
                }
                PluginLog.Debug("Thread has stopped");
            }).Start();
        }

        long UnkFunc_Dtr(IntPtr a1, float a2)
        {
            BaseAddress = a1;
            PluginLog.Information($"Obtained base address: {BaseAddress:X16}");
            _ = new TickScheduler(delegate
              {
                  if (!UnkFuncHook.IsDisposed)
                  {
                      if (UnkFuncHook.IsEnabled)
                      {
                          UnkFuncHook.Disable();
                      }
                      UnkFuncHook.Dispose();
                      PluginLog.Debug("Hook disposed");
                  }
                  BeginWork();
              }, Svc.Framework);
            return UnkFuncHook.Original(a1, a2);
        }

        public static float Max(params float[] values)
        {
            return values.Max();
        }
    }
}