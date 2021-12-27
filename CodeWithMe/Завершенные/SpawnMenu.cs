using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("spawnMenu", "AhigaO#4485", "1.0.0")]
    internal class spawnMenu : RustPlugin
    {
        #region Commands

        [ChatCommand("menu")]
        private void cmdChatmenu(BasePlayer player, string command, string[] args)
        {
            ShowUIMain(player);
        }

        #endregion

        #region Static

        private const string Layer = "UI_spawnMenu";
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
                RectTransform = {AnchorMin = "0 0 ", AnchorMax = "1 1"},
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

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.35 0.92", AnchorMax = "0.65 1"},
                Text =
                {
                    Text = "<color=#B6B8AA>SELECT SPAWNABLE ENTITY</color>", Font = "robotocondensed-bold.ttf", FontSize = 24, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer);
            //InputBG
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.41 0.845", AnchorMax = "0.59 0.905"},
                Image = {Color = "0 0 0 0.4"}
            }, Layer, Layer + ".InputBG");
            //LabelInput
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Text =
                {
                    Text = "Enter text to search", Font = "robotocondensed-regular.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 0.4"
                }
            }, Layer + ".InputBG");
            //Input
            container.Add(new CuiElement
            {
                Parent = Layer + ".InputBG",
                Components =
                {
                    new CuiInputFieldComponent
                    {
                        Align = TextAnchor.MiddleCenter, Font = "robotocondensed-regular.ttf", CharsLimit = 15, FontSize = 16,
                        Command = ""
                    },
                    new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "1 1"}
                }
            });

            #region ListPanel

            //ListPanel
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.05 0.1", AnchorMax = "0.95 0.82"},
                Image = {Color = "0 0 0 0"}
            }, Layer, Layer + ".ListPanel");

            var list = new List<string> {"TESTRIDABLEHORSE", "SCRAPTIRANSPORTHELICOPTER", "MINICOPTER.ENTITY", "HOTAIRBALLOON", "RHIB", "ROWBOAT", "BRADLEY GRATE", "HELI CRATE", "SUPPLY DROP", "FOODBOX", "GRATE TOOLS", "PUMPKIN COLLECTABLE", "CORN COLLECTABLE", "HEMP COLLECTABLE", "OIL BARREL", "CRATE NORMAL 2", "GRATE NORMAL", "DRIFTWOOD 1", "DRIFTWOOD 2", "DRIFTWOOD 3", "DRIFTWOOD 4", "DRIFTWOOD 5", "DEAD LOG A", "DEAD LOG B", "DEAD LOG C", "DEAD LOG A", "DEAD LOG C", "GRATE NORMAL", "DRIFTWOOD 1", "DRIFTWOOD 2", "DRIFTWOOD 3", "DRIFTWOOD 4", "DRIFTWOOD 5", "DEAD LOG A", "DEAD LOG B", "DEAD LOG C", "DEAD LOG A", "DEAD LOG C", "TESTRIDABLEHORSE", "SCRAPTIRANSPORTHELICOPTER", "MINICOPTER.ENTITY", "HOTAIRBALLOON", "RHIB", "ROWBOAT", "BRADLEY GRATE", "HELI CRATE", "SUPPLY DROP", "FOODBOX", "GRATE TOOLS", "PUMPKIN COLLECTABLE", "CORN COLLECTABLE", "HEMP COLLECTABLE", "OIL BARREL", "CRATE NORMAL 2", "GRATE NORMAL", "DRIFTWOOD 1", "DRIFTWOOD 2", "DRIFTWOOD 3", "DRIFTWOOD 4", "DRIFTWOOD 5", "DEAD LOG A", "DEAD LOG B", "DEAD LOG C", "DEAD LOG A", "DEAD LOG C", "GRATE NORMAL", "DRIFTWOOD 1", "DRIFTWOOD 2", "DRIFTWOOD 3", "DRIFTWOOD 4", "DRIFTWOOD 5", "DEAD LOG A", "DEAD LOG B", "DEAD LOG C", "DEAD LOG A", "DEAD LOG C", "TESTRIDABLEHORSE", "SCRAPTIRANSPORTHELICOPTER", "MINICOPTER.ENTITY", "HOTAIRBALLOON", "RHIB", "ROWBOAT", "BRADLEY GRATE", "HELI CRATE", "SUPPLY DROP", "FOODBOX", "GRATE TOOLS", "PUMPKIN COLLECTABLE", "CORN COLLECTABLE", "HEMP COLLECTABLE", "OIL BARREL", "CRATE NORMAL 2", "GRATE NORMAL", "DRIFTWOOD 1", "DRIFTWOOD 2", "DRIFTWOOD 3", "DRIFTWOOD 4", "DRIFTWOOD 5", "DEAD LOG A", "DEAD LOG B", "DEAD LOG C", "DEAD LOG A", "DEAD LOG C", "GRATE NORMAL", "DRIFTWOOD 1", "DRIFTWOOD 2", "DRIFTWOOD 3", "DRIFTWOOD 4", "DRIFTWOOD 5", "DEAD LOG A", "DEAD LOG B", "DEAD LOG C", "DEAD LOG A", "DEAD LOG C", "DRIFTWOOD 1", "DRIFTWOOD 2", "DRIFTWOOD 3", "DRIFTWOOD 4", "DRIFTWOOD 5", "DEAD LOG A", "DEAD LOG B", "DEAD LOG C", "DEAD LOG A", "DEAD LOG C", "DRIFTWOOD 1", "DRIFTWOOD 1"};
            var x = 0f;
            var y = 1f;
            var i = 0;
            foreach (var item in list)
            {
                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = $"{x} {y - 0.01f / 0.18f + 0.005f}", AnchorMax = $"{x + 0.01f / 0.07f - 0.002f} {y}"},
                    Button = {Color = "0 0 0 0.7", Command = ""},
                    Text =
                    {
                        Text = $"{item}", Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".ListPanel");
                i++;
                x += 0.01f / 0.07f;
                if (i != 7) continue;
                y -= 0.01f / 0.18f;
                x = 0f;
                i = 0;
            }

            #endregion

            //FooterPanel
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 0.08"},
                Image = {Color = "0 0 0 0.95"}
            }, Layer, Layer + ".FooterPanel");
            //FooterText
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Text =
                {
                    Text = "Showing items 126 of 131 items", Font = "robotocondensed-regular.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 0.4"
                }
            }, Layer + ".FooterPanel");

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