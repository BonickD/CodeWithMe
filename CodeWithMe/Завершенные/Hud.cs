using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("hud", "AhigaO#4485", "1.0.0")]
    internal class hud : RustPlugin
    {
        #region Commands

        [ChatCommand("menu")]
        private void cmdChatmenu(BasePlayer player, string command, string[] args)
        {
            ShowUIMain(player);
        }

        #endregion

        #region Static

        private const string Layer = "UI_hud";
        private int ImageLibraryCheck = 0;
        private Configuration _config;
        private Dictionary<ulong, Data> data;
        [PluginReference] private Plugin ImageLibrary;

        #endregion

        #region Config

        private class Configuration
        {
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _config = Config.ReadObject<Configuration>();
                if (_config == null) throw new Exception();
                SaveConfig();
            }
            catch
            {
                PrintError("Your configuration file contains an error. Using default configuration values.");
                LoadDefaultConfig();
            }
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config);
        }

        protected override void LoadDefaultConfig()
        {
            _config = new Configuration();
        }

        #endregion

        #region Data

        private class Data
        {
        }

        private void LoadData()
        {
            if (Interface.Oxide.DataFileSystem.ExistsDatafile($"{Name}/data"))
                data = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<ulong, Data>>(
                    $"{Name}/data");
            else data = new Dictionary<ulong, Data>();
            Interface.Oxide.DataFileSystem.WriteObject($"{Name}/data", data);
        }

        private void OnServerSave()
        {
            SaveData();
        }

        private void SaveData()
        {
            if (data != null) Interface.Oxide.DataFileSystem.WriteObject($"{Name}/data", data);
        }

        #endregion

        #region OxideHooks

        private void OnServerInitialized()
        {
            if (!ImageLibrary)
            {
                if (ImageLibraryCheck == 3)
                {
                    PrintError("ImageLibrary not found!Unloading");
                    Interface.Oxide.UnloadPlugin(Name);
                    return;
                }

                timer.In(1, () =>
                {
                    ImageLibraryCheck++;
                    OnServerInitialized();
                });
                return;
            }

            LoadData();
        }

        private void Unload()
        {
            for (var i = 0; i < BasePlayer.activePlayerList.Count; i++) CuiHelper.DestroyUi(BasePlayer.activePlayerList[i], Layer + ".bg");
            SaveData();
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null || data.ContainsKey(player.userID)) return;
            data.Add(player.userID, new Data());
        }

        #endregion

        #region Functions

        #endregion

        #region UI

        private void ShowUI(BasePlayer player)
        {
            var container = new CuiElementContainer();

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0 0 0 0"}
            }, Layer + ".bg", Layer);

            //Close
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Button = {Color = "0 0.00 0.00 0", Close = Layer + ".bg"},
                Text =
                {
                    Text = ""
                }
            }, Layer);
            //Close X
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.948 0.907", AnchorMax = "0.99 0.98"},
                Button = {Color = "0 0 0 0", Close = Layer + ".bg"},
                Text =
                {
                    Text = "×", Font = "robotocondensed-regular.ttf", FontSize = 46, Align = TextAnchor.MiddleCenter,
                    Color = "0.56 0.58 0.64 1.00"
                }
            }, Layer, Layer + ".buttonClose");

            Outline(ref container, Layer + ".buttonClose", "0.56 0.58 0.64 1.00");

            //BuildingMenu
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.25 0.872", AnchorMax = "0.752 1"},
                Text =
                {
                    Text = "<color=#B6B8AA>BUILDING MENU</color>", Font = "robotocondensed-bold.ttf", FontSize = 36, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");

            #region FirstPanel

            //FirstPanel//
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.25 0.52", AnchorMax = "0.75 0.87"},
                Image = {Color = "0 0 0 0"}
            }, Layer, Layer + ".firstPanel");

            //Build Mode
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0.85", AnchorMax = "0.2 1"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Build Mode", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".firstPanel");

            //Resources Needed
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0.68", AnchorMax = "0.2 0.83"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Resources Needed", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".firstPanel");
            //Building Stability
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0.51", AnchorMax = "0.2 0.66"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Building Stability", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".firstPanel");
            //Claim Visibility
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0.34", AnchorMax = "0.2 0.49"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Claim Visibility", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".firstPanel");
            //Clear Something
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0.17", AnchorMax = "0.2 0.32"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Clear Something", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".firstPanel");
            //Select Spawnable
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "0.2 0.15"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Select Spawnable", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".firstPanel");
            // Crosshair Visibility
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.21 0.17", AnchorMax = "0.435 0.32"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Crosshair Visibility", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".firstPanel");
            // Set Grade
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.21 0", AnchorMax = "0.435 0.15"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Set Grade", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".firstPanel");
            // Twig
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.44 0", AnchorMax = "0.55 0.15"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Twig", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".firstPanel");
            // Wood
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.555 0", AnchorMax = "0.665 0.15"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Wood", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".firstPanel");
            // Stone
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.67 0", AnchorMax = "0.78 0.15"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Stone", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".firstPanel");
            // Metal
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.785 0", AnchorMax = "0.895 0.15"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Metal", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".firstPanel");
            // Armor
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.9 0", AnchorMax = "1 0.15"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Armor", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".firstPanel");
            // subPanelFirstPanel
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.21 0.34", AnchorMax = "1 1"},
                Image = {Color = "0 0 0 0.9"}
            }, Layer + ".firstPanel", Layer + ".subPanelFirstPanel");

            //Save
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.785 0.17", AnchorMax = "0.895 0.32"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Save", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".firstPanel");
            //Load
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.9 0.17", AnchorMax = "1 0.32"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Load", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".firstPanel");
            // subPanelFirstPanelTitle
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0.8", AnchorMax = "0.998 0.997"},
                Image = {Color = "0 0 0 0.8"}
            }, Layer + ".subPanelFirstPanel", Layer + ".subPanelFirstPanelTitle");
            // Title
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Text =
                {
                    Text = "BUILD MODE HELP", Font = "robotocondensed-bold.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".subPanelFirstPanelTitle");
            // Rows
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.01 0.05", AnchorMax = "1 0.75"},
                Text =
                {
                    Text = "Sprint + Reload - Remove a building block or enity\nSprint + Use -Spawn an entity from the spawnable list", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.UpperLeft,
                    Color = "1 1 1 1"
                }
            }, Layer + ".subPanelFirstPanel");
            //<FirstPanel>//

            #endregion

            #region SecondPanel

            //SecondPanel
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.25 0.13", AnchorMax = "0.75 0.48"},
                Image = {Color = "0 0 0 0"}
            }, Layer, Layer + ".secondPanel");
            //GTFO Mode
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0.85", AnchorMax = "0.2 1"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "GTFO Mode", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".secondPanel");

            //Building Protection
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0.68", AnchorMax = "0.2 0.83"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Building Protection", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".secondPanel");
            //Player Protection
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0.51", AnchorMax = "0.2 0.66"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Player Protection", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".secondPanel");
            //Weapon Durability
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0.34", AnchorMax = "0.2 0.49"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Weapon Durability", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".secondPanel");
            //Noclip (Fly)
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0.17", AnchorMax = "0.2 0.32"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Noclip (Fly)", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".secondPanel");
            //Auto Respawn Here
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "0.2 0.15"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Auto Respawn Here", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".secondPanel");
            // subPanelSecondPanel
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.21 0.17", AnchorMax = "1 1"},
                Image = {Color = "0 0 0 0.9"}
            }, Layer + ".secondPanel", Layer + ".subPanelSecondPanel");
            // subPanelSecondPanelTitle
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0.84", AnchorMax = "0.998 1"},
                Image = {Color = "0 0 0 0.8"}
            }, Layer + ".subPanelSecondPanel", Layer + ".subPanelSecondPanelTitle");
            // Title
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Text =
                {
                    Text = "BUILD MODE HELP", Font = "robotocondensed-bold.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".subPanelSecondPanelTitle");
            // Row-1
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.01 0.2", AnchorMax = "1 0.78"},
                Text =
                {
                    Text = "This is a creative server, everyone has unlimited resources and is protected by default. You can build without resources, craft anything for free or use the F1 menu to spawn items.", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.UpperLeft,
                    Color = "1 1 1 1"
                }
            }, Layer + ".subPanelSecondPanel");
            // Under-Row
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.01 0.05", AnchorMax = "1 0.15"},
                Text =
                {
                    Text = "Whitelist players to PVP without disabling god mode with /pvp in chat", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.LowerLeft,
                    Color = "1 1 1 1"
                }
            }, Layer + ".subPanelSecondPanel");
            // Set Time
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.795 0", AnchorMax = "0.895 0.15"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Set Time", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".secondPanel");
            // Share
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.9 0", AnchorMax = "1 0.15"},
                Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                Text =
                {
                    Text = "Share", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".secondPanel");

            #endregion

            #region ThirdPanel

            //ThirdPanel
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.25 0.01", AnchorMax = "0.75 0.05"},
                Image = {Color = "0 0 0 0"}
            }, Layer, Layer + ".thirdPanel");
            var list = new List<string> {"Cave1", "Cave2", "Cave3", "Cave4", "Cave5"};
            var x = 0.005 - (0.1 * list.Count + 0.005 * list.Count - 1) / 2;

            foreach (var item in list)
            {
                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = $"{x} 0", AnchorMax = $"{x + 0.1} 0.978"},
                    Button = {Color = "0.04 0.00 0.00 0.8", Command = ""},
                    Text =
                    {
                        Text = $"{item}", Font = "robotocondensed-bold.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".thirdPanel");
                x += 0.1 + 0.005;
            }

            #endregion


            CuiHelper.DestroyUi(player, Layer);
            CuiHelper.AddUi(player, container);
        }

        private void ShowUIMain(BasePlayer player)
        {
            var container = new CuiElementContainer();

            container.Add(new CuiPanel
            {
                CursorEnabled = true,
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0 0 0 0.8", Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat"}
            }, "Overlay", Layer + ".bg");

            CuiHelper.DestroyUi(player, Layer + ".bg");
            CuiHelper.AddUi(player, container);
            ShowUI(player);
        }

        private void Outline(ref CuiElementContainer container, string parent, string color = "1 1 1 1", string size = "3")
        {
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 0", OffsetMin = "0 0", OffsetMax = $"0 {size}"},
                Image = {Color = color}
            }, parent);
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "1 1", OffsetMin = $"0 -{size}", OffsetMax = "0 0"},
                Image = {Color = color}
            }, parent);
            container.Add(new CuiPanel
            {
                RectTransform =
                    {AnchorMin = "0 0", AnchorMax = "0 1", OffsetMin = $"0 {size}", OffsetMax = $"{size} -{size}"},
                Image = {Color = color}
            }, parent);
            container.Add(new CuiPanel
            {
                RectTransform =
                    {AnchorMin = "1 0", AnchorMax = "1 1", OffsetMin = $"-{size} {size}", OffsetMax = $"0 -{size}"},
                Image = {Color = color}
            }, parent);
        }

        #endregion
    }
}