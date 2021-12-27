using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("TableList", "AhigaO#4485", "1.0.0")]
    internal class TableList : RustPlugin
    {
        #region Static

        private const string Layer = "UI_TableList";
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

        protected override void SaveConfig() => Config.WriteObject(_config);

        protected override void LoadDefaultConfig() => _config = new Configuration();

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
            foreach (var check in BasePlayer.activePlayerList) CuiHelper.DestroyUi(check, Layer + ".bg");
            if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/Ssm35Zf.jpg")) ImageLibrary.Call("AddImage", "https://i.imgur.com/Ssm35Zf.jpg", "https://i.imgur.com/Ssm35Zf.jpg");
            if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/NtHSrgs.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/NtHSrgs.png", "https://i.imgur.com/NtHSrgs.png");
            if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/Fg2lEd9.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/Fg2lEd9.png", "https://i.imgur.com/Fg2lEd9.png");
            if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/nkoDZOH.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/nkoDZOH.png", "https://i.imgur.com/nkoDZOH.png");
            if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/TV18BIF.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/TV18BIF.png", "https://i.imgur.com/TV18BIF.png");
            if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/m8jznM5.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/m8jznM5.png", "https://i.imgur.com/m8jznM5.png");
            LoadData();
        }

        private void Unload()
        {
            SaveData();
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null || data.ContainsKey(player.userID)) return;
            data.Add(player.userID, new Data());
        }

        #endregion

        #region Commands

        [ChatCommand("t")]
        private void cmdChatmenu(BasePlayer player, string command, string[] args)
        {
            ShowUIMain(player);
        }

        #endregion

        #region Functions



        #endregion

        #region UI

        private void ShowUIMain(BasePlayer player)
        {
            var container = new CuiElementContainer();

            container.Add(new CuiPanel
            {   
                CursorEnabled = true,
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0 0 0 0"}
            }, "Overlay", Layer + ".bg");
            container.Add(new CuiElement
            {
                Parent = Layer + ".bg",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/Ssm35Zf.jpg"), Color = "1 1 1 0.8"},
                    new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "1 1"},
                }
            });
            
                
            CuiHelper.DestroyUi(player, Layer + ".bg");
            CuiHelper.AddUi(player, container);

            //ShowUITable(player);
            ShowUIClan(player);
        }

        private void ShowUITable(BasePlayer player)
        {
            var container = new CuiElementContainer();
            var maxRowCount = 14;
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.948 0.907", AnchorMax = "0.99 0.98"},
                Button = {Color = "0 0 0 0", Close = Layer + ".bg"},
                Text =
                {
                    Text = "×", Font = "robotocondensed-regular.ttf", FontSize = 46, Align = TextAnchor.MiddleCenter,
                    Color = "0.56 0.58 0.64 1.00"
                }
            }, Layer + ".bg", Layer + ".buttonClose");
            Outline(ref container, Layer + ".buttonClose", "0.56 0.58 0.64 1.00");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.171 0.762", AnchorMax = "0.26 0.826"},
                Text =
                {
                    Text = "Место", Font = "robotocondensed-bold.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.26 0.762", AnchorMax = "0.359 0.826"},
                Text =
                {
                    Text = "Клан", Font = "robotocondensed-bold.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.359 0.762", AnchorMax = "0.447 0.826"},
                Text =
                {
                    Text = "Рейд", Font = "robotocondensed-bold.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.447 0.762", AnchorMax = "0.544 0.826"},
                Text =
                {
                    Text = "У/с", Font = "robotocondensed-bold.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.544 0.762", AnchorMax = "0.637 0.826"},
                Text =
                {
                    Text = "Фарм", Font = "robotocondensed-bold.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.637 0.762", AnchorMax = "0.734 0.826"},
                Text =
                {
                    Text = "Ивент", Font = "robotocondensed-bold.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.734 0.762", AnchorMax = "0.827 0.826"},
                Text =
                {
                    Text = "Очки", Font = "robotocondensed-bold.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.171 0.762", AnchorMax = "0.827 0.7635"},
                Image = {Color = "1 1 1 0.6"}
            }, Layer + ".bg");
            container.Add(new CuiElement
            {
                Parent = Layer + ".bg",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/Fg2lEd9.png"), Color = "1 1 1 0.6"},
                    new CuiRectTransformComponent {AnchorMin = "0.171 0.762", AnchorMax = "0.827 0.806"}
                }
            });
            container.Add(new CuiElement
            {
                Parent = Layer + ".bg",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/NtHSrgs.png")},
                    new CuiRectTransformComponent {AnchorMin = "0.171 0.155", AnchorMax = "0.827 0.762"}
                }
            });
            
            var posY = 0.715f;
            var posX = 0.176f;
            for (int i = 0; i < maxRowCount; i++)
            {
                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = $"0.171 {posY}", AnchorMax = $"0.26 {posY + 0.043f}"},
                    Text =
                    {
                        Text = $"#{i}", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".bg");
                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = $"0.26 {posY}", AnchorMax = $"0.359 {posY + 0.043f}"},
                    Text =
                    {
                        Text = $"#{i}", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".bg");
                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = $"0.359 {posY}", AnchorMax = $"0.447 {posY + 0.043f}"},
                    Text =
                    {
                        Text = $"#{i}", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".bg");
                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = $"0.447 {posY}", AnchorMax = $"0.544 {posY + 0.043f}"},
                    Text =
                    {
                        Text = $"#{i}", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".bg");
                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = $"0.544 {posY}", AnchorMax = $"0.637 {posY + 0.043f}"},
                    Text =
                    {
                        Text = $"#{i}", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".bg");
                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = $"0.637 {posY}", AnchorMax = $"0.734 {posY + 0.043f}"},
                    Text =
                    {
                        Text = $"#{i}", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".bg"); 
                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = $"0.734 {posY}", AnchorMax = $"0.827 {posY + 0.043f}"},
                    Text =
                    {
                        Text = $"#{i}", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1" 
                    }
                }, Layer + ".bg");
                posY -= 0.043f;
            }
            
            container.Add(new CuiElement
            {
                Parent = Layer + ".bg",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/nkoDZOH.png")},
                    new CuiRectTransformComponent {AnchorMin = "0.467 0.105", AnchorMax = "0.477 0.14"}
                }
            });
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.467 0.105", AnchorMax = "0.477 0.14"},
                Button = {Color = "0 0 0 0"},
                Text =
                {
                    Text = "", Font = "robotocondensed-bold.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");
            container.Add(new CuiElement
            {
                Parent = Layer + ".bg",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/TV18BIF.png")},
                    new CuiRectTransformComponent {AnchorMin = "0.524 0.105", AnchorMax = "0.534 0.14"}
                }
            });
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.524 0.105", AnchorMax = "0.534 0.14"},
                Button = {Color = "0 0 0 0"},
                Text =
                {
                    Text = "", Font = "robotocondensed-bold.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");
            
            
            CuiHelper.DestroyUi(player, Layer);
            CuiHelper.AddUi(player, container);
        }
        private void ShowUIClan(BasePlayer player)
        {
            var container = new CuiElementContainer();
            var maxRowCount = 14;
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.948 0.907", AnchorMax = "0.99 0.98"},
                Button = {Color = "0 0 0 0", Close = Layer + ".bg"},
                Text =
                {
                    Text = "×", Font = "robotocondensed-regular.ttf", FontSize = 46, Align = TextAnchor.MiddleCenter,
                    Color = "0.56 0.58 0.64 1.00"
                }
            }, Layer + ".bg", Layer + ".buttonClose");
            Outline(ref container, Layer + ".buttonClose", "0.56 0.58 0.64 1.00");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.171 0.762", AnchorMax = "0.26 0.826"},
                Text =
                {
                    Text = "Игрок", Font = "robotocondensed-bold.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.26 0.762", AnchorMax = "0.359 0.826"},
                Text =
                {
                    Text = "Роль", Font = "robotocondensed-bold.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.359 0.762", AnchorMax = "0.447 0.826"},
                Text =
                {
                    Text = "Рейд", Font = "robotocondensed-bold.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.447 0.762", AnchorMax = "0.544 0.826"},
                Text =
                {
                    Text = "У/с", Font = "robotocondensed-bold.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.544 0.762", AnchorMax = "0.637 0.826"},
                Text =
                {
                    Text = "Фарм", Font = "robotocondensed-bold.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.637 0.762", AnchorMax = "0.734 0.826"},
                Text =
                {
                    Text = "Ивент", Font = "robotocondensed-bold.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.734 0.762", AnchorMax = "0.827 0.826"},
                Text =
                {
                    Text = "Очки", Font = "robotocondensed-bold.ttf", FontSize = 16, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.171 0.762", AnchorMax = "0.827 0.7635"},
                Image = {Color = "1 1 1 0.6"}
            }, Layer + ".bg");
            container.Add(new CuiElement
            {
                Parent = Layer + ".bg",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/Fg2lEd9.png"), Color = "1 1 1 0.6"},
                    new CuiRectTransformComponent {AnchorMin = "0.171 0.762", AnchorMax = "0.827 0.806"}
                }
            });
            container.Add(new CuiElement
            {
                Parent = Layer + ".bg",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/NtHSrgs.png")},
                    new CuiRectTransformComponent {AnchorMin = "0.171 0.155", AnchorMax = "0.827 0.762"}
                }
            });
            
            var posY = 0.719f;
            var btnPosY = 0.721;
            var posX = 0.176f;
            for (int i = 0; i < maxRowCount; i++)
            {
                container.Add(new CuiElement
                {
                    Parent = Layer + ".bg",
                    Components =
                    {
                        new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/m8jznM5.png")},
                        new CuiRectTransformComponent {AnchorMin = $"0.077 {btnPosY}", AnchorMax = $"0.164 {btnPosY + 0.036f}"}
                    }
                });
                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = $"0.077 {btnPosY}", AnchorMax = $"0.164 {btnPosY + 0.036f}"},
                    Button = {Color = "0 0 0 0"},
                    Text =
                    {
                        Text = "Кикнуть", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".bg");
                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = $"0.171 {posY}", AnchorMax = $"0.26 {posY + 0.043f}"},
                    Text =
                    {
                        Text = $"#{i}", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".bg");
                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = $"0.26 {posY}", AnchorMax = $"0.359 {posY + 0.043f}"},
                    Text =
                    {
                        Text = $"#{i}", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".bg");
                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = $"0.359 {posY}", AnchorMax = $"0.447 {posY + 0.043f}"},
                    Text =
                    {
                        Text = $"#{i}", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".bg");
                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = $"0.447 {posY}", AnchorMax = $"0.544 {posY + 0.043f}"},
                    Text =
                    {
                        Text = $"#{i}", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".bg");
                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = $"0.544 {posY}", AnchorMax = $"0.637 {posY + 0.043f}"},
                    Text =
                    {
                        Text = $"#{i}", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".bg");
                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = $"0.637 {posY}", AnchorMax = $"0.734 {posY + 0.043f}"},
                    Text =
                    {
                        Text = $"#{i}", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".bg"); 
                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = $"0.734 {posY}", AnchorMax = $"0.827 {posY + 0.043f}"},
                    Text =
                    {
                        Text = $"#{i}", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1" 
                    }
                }, Layer + ".bg");
                container.Add(new CuiElement
                {
                    Parent = Layer + ".bg",
                    Components =
                    {
                        new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/m8jznM5.png")},
                        new CuiRectTransformComponent {AnchorMin = $"0.835 {btnPosY}", AnchorMax = $"0.898 {btnPosY + 0.036f}"}
                    }
                });
                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = $"0.835 {btnPosY}", AnchorMax = $"0.898 {btnPosY + 0.036f}"},
                    Button = {Color = "0 0 0 0"},
                    Text =
                    {
                        Text = "Повыс.", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".bg");
                container.Add(new CuiElement
                {
                    Parent = Layer + ".bg",
                    Components =
                    {
                        new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/m8jznM5.png")},
                        new CuiRectTransformComponent {AnchorMin = $"0.902 {btnPosY}", AnchorMax = $"0.966 {btnPosY + 0.036f}"}
                    } 
                });
                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = $"0.902 {btnPosY}", AnchorMax = $"0.966 {btnPosY + 0.036f}"},
                    Button = {Color = "0 0 0 0"},
                    Text =
                    {
                        Text = "Пониз.", Font = "robotocondensed-regular.ttf", FontSize = 12, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".bg");
                btnPosY -= 0.04325f;
                posY -= 0.043f;
            }
            
            container.Add(new CuiElement
            {
                Parent = Layer + ".bg",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/nkoDZOH.png")},
                    new CuiRectTransformComponent {AnchorMin = "0.467 0.105", AnchorMax = "0.477 0.14"}
                }
            });
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.467 0.105", AnchorMax = "0.477 0.14"},
                Button = {Color = "0 0 0 0"},
                Text =
                {
                    Text = "", Font = "robotocondensed-bold.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");
            container.Add(new CuiElement
            {
                Parent = Layer + ".bg",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/TV18BIF.png")},
                    new CuiRectTransformComponent {AnchorMin = "0.524 0.105", AnchorMax = "0.534 0.14"}
                }
            });
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0.524 0.105", AnchorMax = "0.534 0.14"},
                Button = {Color = "0 0 0 0"},
                Text =
                {
                    Text = "", Font = "robotocondensed-bold.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer + ".bg");
            
            
            CuiHelper.DestroyUi(player, Layer);
            CuiHelper.AddUi(player, container);
        }

        private void Outline(ref CuiElementContainer container, string parent, string color = "1 1 1 1",
            string size = "3")
        {
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 0", OffsetMin = $"0 0", OffsetMax = $"0 {size}"},
                Image = {Color = color}
            }, parent);
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "1 1", OffsetMin = $"0 -{size}", OffsetMax = $"0 0"},
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