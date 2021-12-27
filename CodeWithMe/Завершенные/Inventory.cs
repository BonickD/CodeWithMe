using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Inventory", "AhigaO#4485", "1.0.0")]
    internal class Inventory : RustPlugin
    {
        #region Commands

        [ChatCommand("inv")]
        private void cmdChatNAME(BasePlayer player, string command, string[] args)
        {
            ShowUIMain(player);
        }
            
        #endregion

        #region Static

        private const string Layer = "UI_Inventory";
        private int ImageLibraryCheck = 0;
        private Configuration _config;
        private Data data;
        [PluginReference] private Plugin ImageLibrary;

        private class FItem
        {
            public int amount;
            public string shortname;
            public ulong skin;
        }

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
            public List<FItem> inventory = new List<FItem>();
        }

        private void LoadData()
        {
            if (Interface.Oxide.DataFileSystem.ExistsDatafile($"{Name}/data"))
                data = Interface.Oxide.DataFileSystem.ReadObject<Data>(
                    $"{Name}/data");
            else data = new Data();
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
        }

        #endregion

        #region Functions

        #endregion

        #region UI

        private void ShowUIInventory(BasePlayer player, int page = 0)
        {
            var container = new CuiElementContainer();
            var inventory = data.inventory;
            int posX = 4, posY = -25;
            var length = inventory.Count;
            var items = player.inventory.AllItems();

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-403 -226", OffsetMax = "-157 261"},
                Image = {Color = "0 0 0 0.98"}
            }, Layer + ".bg", Layer + ".Inventory");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMax = "246 25"},
                Text =
                {
                    Text = "ИНВЕНТАРЬ", Font = "robotocondensed-regular.ttf", FontSize = 20, Align = TextAnchor.MiddleLeft,
                    Color = "0.82 0.82 0.87 1.00"
                }
            }, Layer + ".Inventory");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -21", OffsetMax = "246 0"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".Inventory");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "8 -21", OffsetMax = "246 0"},
                Text =
                {
                    Text = "ХРАНИЛИЩЕ", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".Inventory");

            if (page > 0)
                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = "1 1", AnchorMax = "1 1", OffsetMin = "-42 -42", OffsetMax = "-21 -21"},
                    Button = {Color = "0 0 0 0", Command = $"UI_INVENTORY ui {page - 1}"},
                    Text =
                    {
                        Text = "<", Font = "robotocondensed-regular.ttf", FontSize = 18, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".Inventory");

            if (length - 15 * (page + 1) > 0)
                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = "1 1", AnchorMax = "1 1", OffsetMin = "-21 -21"},
                    Button = {Color = "0 0 0 0", Command = $"UI_INVENTORY ui {page + 1}"},
                    Text =
                    {
                        Text = ">", Font = "robotocondensed-regular.ttf", FontSize = 18, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".Inventory");

            for (var i = 15 * page; i < 15 * (page + 1); i++)
            {
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"{posX} {posY - 46}", OffsetMax = $"{posX + 46} {posY}"},
                    Image = {Color = i < length ? "0.25 0.25 0.25 1.00" : "0.11 0.11 0.12 0.9"}
                }, Layer + ".Inventory", Layer + ".Inventory" + posX + posY);

                if (i < length)
                {
                    var check = inventory[i];
                    var baseInfo = check;
                    container.Add(new CuiElement
                    {
                        Parent = Layer + ".Inventory" + posX + posY,
                        Components =
                        {
                            new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", baseInfo.shortname)},
                            new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "6 6", OffsetMax = "40 40"}
                        }
                    });

                    container.Add(new CuiLabel
                    {
                        RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "0 2", OffsetMax = "44 46"},
                        Text =
                        {
                            Text = baseInfo.amount.ToString(), Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.LowerRight,
                            Color = "0.65 0.65 0.64 1.00"
                        }
                    }, Layer + ".Inventory" + posX + posY);

                    container.Add(new CuiButton
                    {
                        RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMax = "46 46"},
                        Button = {Color = "0 0 0 0", Command = $"UI_INVENTORY get {i} {page}"},
                        Text = {Text = ""}
                    }, Layer + ".Inventory" + posX + posY);
                }

                posX += 48;
                if (posX < 242) continue;
                posX = 4;
                posY -= 48;
            }

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -192", OffsetMax = "246 -171"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".Inventory");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "8 -192", OffsetMax = "246 -171"},
                Text =
                {
                    Text = "ВАШИ ПРЕДМЕТЫ", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".Inventory");

            length = items.Length;
            posX = 4;
            posY = -196;

            for (var i = 0; i < 30; i++)
            {
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"{posX} {posY - 46}", OffsetMax = $"{posX + 46} {posY}"},
                    Image = {Color = i < length ? "0.25 0.25 0.25 1.00" : "0.11 0.11 0.12 0.9"}
                }, Layer + ".Inventory", Layer + ".Inventory" + posX + posY);

                if (i < length)
                {
                    var item = items[i];
                    container.Add(new CuiElement
                    {
                        Parent = Layer + ".Inventory" + posX + posY,
                        Components =
                        {
                            new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", item.info.shortname)},
                            new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "6 6", OffsetMax = "40 40"}
                        }
                    });

                    container.Add(new CuiLabel
                    {
                        RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "0 2", OffsetMax = "44 46"},
                        Text =
                        {
                            Text = item.amount.ToString(), Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.LowerRight,
                            Color = "0.65 0.65 0.64 1.00"
                        }
                    }, Layer + ".Inventory" + posX + posY);

                    container.Add(new CuiButton
                    {
                        RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMax = "46 46"},
                        Button = {Color = "0 0 0 0", Command = $"UI_INVENTORY move {item.uid} {page}"},
                        Text = {Text = ""}
                    }, Layer + ".Inventory" + posX + posY);
                }

                posX += 48;
                if (posX < 242) continue;
                posX = 4;
                posY -= 48;
            }

            CuiHelper.DestroyUi(player, Layer + ".Inventory");
            CuiHelper.AddUi(player, container);
        }

        private void ShowUICars(BasePlayer player)
        {
            var container = new CuiElementContainer();
            var dict = new Dictionary<string, List<string>>
            {
                ["2Module"] = new List<string>
                {
                    "Car1",
                    "Car2"
                },
                ["3Module"] = new List<string>
                {
                    "Car1",
                    "Car2",
                    "Car3",
                    "Car4",
                    "Car5",
                    "Car6",
                },
                ["4Module"] = new List<string>
                {
                    "Car1",
                    "Car2",
                    "Car3",
                    "Car4",
                    "Car5",
                    "Car6",
                    "Car7",
                    "Car8",
                    "Car9",
                    "Car10",
                    "Car11",
                    "Car12",
                    "Car13",
                }
            };
            var posX = 0;
            var posY = -110;
            foreach (var check in dict)
            {
                //MainPanel
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"5 {posY}",OffsetMax = $"546 {posY + 63}"},
                    Image = {Color = "0.67 0.67 0.70 0.75"}
                }, Layer+".Technology", Layer + ".CarTechnologyPanel" + check);
                //ModelPanel
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "5 5", OffsetMax = "58 58"},
                    Image = {Color = "0.25 0.25 0.25 1.00"}
                }, Layer + ".CarTechnologyPanel" + check, Layer + ".CarModelPanel" + check);
                //ModelPanelImg
                container.Add(new CuiElement
                {
                    Parent = Layer + ".CarModelPanel" + check,
                    Components =
                    {
                        new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", ""), Color = true ? "0.92 0.49 0.24 1.00" : "1 1 1 1"}, // КАРТИНОЧКА //
                        new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "0 0", OffsetMax = "53 53"}
                    }
                });                
                //DividerPanel
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = $"66 5", OffsetMax = $"69 58"},
                    Image = {Color = "1 1 1 1"}
                }, Layer + ".CarTechnologyPanel" + check);
                
                posX = 77;
                for (var i = 0; i < check.Value.Count; i++)
                {
                    //TechnologyItem
                    container.Add(new CuiPanel
                    {
                        RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = $"{posX} 5", OffsetMax = $"{posX +  53} 58"},
                        Image = {Color = "0.25 0.25 0.25 1.00"}
                    },  Layer + ".CarTechnologyPanel"+  check, Layer + ".CarTechnologyItem" + i);
                    //TechnologyItemImg
                    container.Add(new CuiElement
                    {
                        Parent = Layer + ".CarTechnologyItem" + i,
                        Components =
                        {
                            new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", ""), Color = true ? "0.92 0.49 0.24 1.00" : "1 1 1 1"}, // КАРТИНОЧКА //
                            new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "0 0", OffsetMax = "53 53"}
                        }
                    });
                    posX += 58;
                    if (check.Value.Count <= 8 || i != 7) continue;
                    posX = 77;
                    //MainPanel
                    container.Add(new CuiPanel
                    {
                        RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"5 {posY - 63}", OffsetMax = $"546 {posY}"},
                        Image = {Color = "0.67 0.67 0.70 0.75"}
                    }, Layer + ".Technology", Layer + ".CarTechnologyPanel" + check);
                    //DividerPanel
                    container.Add(new CuiPanel
                    {
                        RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = $"66 5", OffsetMax = $"69 58"},
                        Image = {Color = "1 1 1 1"}
                    }, Layer + ".CarTechnologyPanel" + check);

                }
                posY -= 68;
            }

            CuiHelper.DestroyUi(player, Layer);
            CuiHelper.AddUi(player, container);
        }

        private void ShowUIAmmo(BasePlayer player)
        {
            var container = new CuiElementContainer();
            var list = new List<string>
            {
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
                "someItem",
            };
            var posX = 5f;
            var posY = -110f;
            for (var i = 0; i < list.Count; i++)
            {
                //AmmoPanel
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"{posX} {posY}", OffsetMax = $"{posX-63.25} {posY-63.25}"},
                    Image = {Color = "0 0 0 0.7"}
                }, Layer + ".Technology", Layer + ".AmmoPanel");
                //AmmoTechnologyItem
                container.Add(new CuiElement
                {
                    Parent = Layer + ".AmmoPanel",
                    Components =
                    {
                        new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "")},
                        new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "0 0", OffsetMax = "63.25 63.25"}
                    }
                });
                posX += 68.25f;
                if (posX > 500)
                {
                    posX = 5;
                    posY -= 68.25f;
                }
            }
           
            
            CuiHelper.DestroyUi(player, Layer);
            CuiHelper.AddUi(player, container);
        }
        
        private void ShowUITech(BasePlayer player)
        {
            var container = new CuiElementContainer();
            var posX = 0;
            var posY = -42;
            
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-147 -226", OffsetMax = "404 261"},
                Image = {Color = "0 0 0 0.98"}
            }, Layer + ".bg", Layer + ".Technology");
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -21", OffsetMax = "551 0"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".Technology");
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "8 -21", OffsetMax = "551 0"},
                Text =
                {
                    Text = "RESEARCH", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".Technology");
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -42", OffsetMax = "551 -21"},
                Image = {Color = "0.22 0.21 0.22 1.00"}
            }, Layer + ".Technology");

            for (var i = 0; i < 2; i++)
            {
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"{posX} {posY}", OffsetMax = $"{posX + 22} {posY + 21}"},
                    Image = {Color = "0.25 0.25 0.25 1.00"}
                }, Layer + ".Technology", Layer + ".Technology" + posX + posY);
                container.Add(new CuiElement
                {
                    Parent = Layer + ".Technology" + posX + posY,
                    Name = Layer + ".TechonolgyImage" + posX + posY,
                    Components =
                    {
                        new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", ""), Color = true ? "0.92 0.49 0.24 1.00" : "1 1 1 1"}, // КАРТИНОЧКА //
                        new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMin = "0 0", OffsetMax = "22 21"}
                    }
                });
                if (true) // ТИПА ЧЕК НА НЕ НАЖАТУЮ КНОПКУ
                    container.Add(new CuiButton
                    {
                        RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "", OffsetMax = "22 22"},
                        Button = {Color = "0 0 0 0", Command = ""}, // КОМАНДА КОТОРОЙ У МЕНЯ НЕТ :((((
                        Text =
                        {
                            Text = "", Font = "robotocondensed-bold.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                            Color = "1 1 1 1"
                        }
                    }, Layer + ".TechonolgyImage" + posX + posY);
                posX += 22;
            }
           
            CuiHelper.DestroyUi(player, Layer + ".Technology");
            CuiHelper.AddUi(player, container);
        }

        private void ShowUIMain(BasePlayer player)
        {
            var container = new CuiElementContainer();

            container.Add(new CuiPanel
            {
                CursorEnabled = true,
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0 0 0 0.75", Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat"}
            }, "Overlay", Layer + ".bg");

            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Button = {Color = "0 0 0 0", Close = Layer + ".bg"},
                Text =
                {
                    Text = "", Font = "robotocondensed-bold.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");

            CuiHelper.DestroyUi(player, Layer + ".bg");
            CuiHelper.AddUi(player, container);
            ShowUIInventory(player);
            ShowUITech(player);
            //ShowUICars(player);
            //ShowUIAmmo(player);
        }

        #endregion
    }
}