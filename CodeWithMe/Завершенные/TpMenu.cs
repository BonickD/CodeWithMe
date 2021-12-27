using System;
using System.Collections.Generic;
using System.Linq;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("TpMenu", "CASHR", "1.0.0")]
    internal class TpMenu : RustPlugin
    {
        #region Static

        private const string Layer = "UI_TpMenu";
        private int ImageLibraryCheck = 0;
        private Configuration _config;
        [PluginReference] private Plugin ImageLibrary, NTeleportation;

        private class BaseRect
        {
            public string anchorMin;
            public string anchorMax;
            public string url;
        }
        
        private class TeleportRect: BaseRect
        {
            public string btnAnchorMin;
            public string btnAnchorMax;
        }

        private class PlayerRect : BaseRect
        {
            public string btnYesAnchorMin;
            public string btnYesAnchorMax;
            public string btnNoAnchorMin;
            public string btnNoAnchorMax;
        }

   
        
        private List<PlayerRect> PositionsPlayer = new List<PlayerRect>
        {
            new PlayerRect
            {
                anchorMin = "0.622 0.394",
                anchorMax = "0.765 0.633",
                url = "https://i.imgur.com/7ua2Xiy.png",
                
                btnYesAnchorMin = "0.668 0.547",
                btnYesAnchorMax = "0.682 0.572",
                btnNoAnchorMin = "0.667 0.457",
                btnNoAnchorMax = "0.681 0.482",  
            },
            new PlayerRect
            {
                anchorMin = "0.564 0.11",
                anchorMax = "0.728 0.402",
                url = "https://i.imgur.com/v5fdIll.png",
                
                btnYesAnchorMin = "0.634 0.315",
                btnYesAnchorMax = "0.648 0.34",
                btnNoAnchorMin = "0.598 0.253",
                btnNoAnchorMax = "0.612 0.278"  
            },
            new PlayerRect
            {
                anchorMin = "0.566 0.635",
                anchorMax = "0.73 0.925",
                url = "https://i.imgur.com/r16UfAs.png",
                
                btnYesAnchorMin = "0.601 0.76",
                btnYesAnchorMax = "0.615 0.784",
                btnNoAnchorMin = "0.636 0.695",
                btnNoAnchorMax = "0.649 0.719",  
            }
        };
        
        private List<TeleportRect> PositionsPoint = new List<TeleportRect>
        {
            new TeleportRect
            {
                anchorMin = "0.233 0.385",
                anchorMax = "0.379 0.624",
                url = "https://i.imgur.com/JjZi0jb.png",
                btnAnchorMin = "0.346 0.489",
                btnAnchorMax = "0.367 0.53"
            },
            new TeleportRect
            {
                anchorMin = "0.269 0.632",
                anchorMax = "0.433 0.922",
                url = "https://i.imgur.com/ZvluzCP.png",
                btnAnchorMin = "0.387 0.671",
                btnAnchorMax = "0.407 0.715"
            },
            new TeleportRect
            {
                anchorMin = "0.274 0.106",
                anchorMax = "0.437 0.4",
                url = "https://i.imgur.com/J5qBrRG.png",   
                btnAnchorMin = "0.386 0.322",
                btnAnchorMax = "0.411 0.359"
            },
            new TeleportRect
            {
                anchorMin = "0.437 0.046",
                anchorMax = "0.571 0.302",
                url = "https://i.imgur.com/3757hI7.png",   
                btnAnchorMin = "0.489 0.757",
                btnAnchorMax = "0.511 0.794"
            },
            new TeleportRect
            {
                anchorMin = "0.436 0.738",
                anchorMax = "0.57 0.993",
                url = "https://i.imgur.com/yUcjjzZ.png",    
                btnAnchorMin = "0.489 0.244",
                btnAnchorMax = "0.511 0.281"
            },
        };
        
 
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

            if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/UQXpRWF.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/UQXpRWF.png", "https://i.imgur.com/UQXpRWF.png");
            if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/yUcjjzZ.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/yUcjjzZ.png", "https://i.imgur.com/yUcjjzZ.png");
            if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/ZvluzCP.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/ZvluzCP.png", "https://i.imgur.com/ZvluzCP.png");
            if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/JjZi0jb.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/JjZi0jb.png", "https://i.imgur.com/JjZi0jb.png");
            if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/J5qBrRG.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/J5qBrRG.png", "https://i.imgur.com/J5qBrRG.png");
            if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/3757hI7.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/3757hI7.png", "https://i.imgur.com/3757hI7.png");
            if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/7ua2Xiy.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/7ua2Xiy.png", "https://i.imgur.com/7ua2Xiy.png");
        }

        private void Unload()
        {
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++) CuiHelper.DestroyUi(BasePlayer.activePlayerList[i], Layer);
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

        private void ShowUICricle(BasePlayer player)
        {
            var container = new CuiElementContainer();
           // var homes = NTeleportation.Call<List<string>>("API_GetHomes", player) ?? new List<string>();
           var homes = new List<string>
           {
               "abobs",
               "abobs",
               "abobs",
               "abobs",
               "abobs",
           };
            var i = 0;
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0 0 0 0"}
            }, Layer, Layer + ".BgCircle");

       
            container.Add(new CuiElement
            {
                Parent = Layer + ".BgCircle",
                Name = Layer + ".BgCircleMenu",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/UQXpRWF.png")},
                    new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "1 1"}
                }
            });
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Button = {Color = "0 0 0 0", Close = Layer},
                Text = {Text = ""}
            }, Layer + ".BgCircle");

            foreach (var pos in homes.Take(5))
            {
                var check = PositionsPoint[i];
                
                container.Add(new CuiElement
                {
                    Parent = Layer + ".BgCircleMenu",
                    Components =
                    {
                        new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", check.url)},
                        new CuiRectTransformComponent {AnchorMin = check.anchorMin, AnchorMax = check.anchorMax}
                    }
                });
                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = check.anchorMin, AnchorMax = check.anchorMax},
                    Text =
                    {
                        Text = "ABOBA", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer + ".BgCircleMenu");
                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = check.btnAnchorMin, AnchorMax = check.btnAnchorMax},
                    Button = {Color = "0 0 0 0", Command = $"chat.say /home {pos}"},
                    Text = {Text = ""}
                }, Layer + ".BgCircleMenu");
                i++;
            }
            // NTeleportation.Call<bool>("API_HavePendingRequest", player
            if (true)
            {
               //var name = NTeleportation.Call<string>("API_GetPendingRequestName", player);
                var pending = PositionsPlayer[0];
                
                container.Add(new CuiElement
                {
                    Parent = Layer + ".BgCircleMenu",
                    Components =
                    {
                        new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", pending.url)},
                        new CuiRectTransformComponent {AnchorMin = pending.anchorMin, AnchorMax = pending.anchorMax}
                    }
                });

                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = pending.btnYesAnchorMin, AnchorMax = pending.btnYesAnchorMax},
                    Button = {Color = "1 1 1 0", Command = "chat.say /tpa"},
                    Text = {Text = ""}
                }, Layer + ".BgCircleMenu");

                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = pending.btnNoAnchorMin, AnchorMax = pending.btnNoAnchorMax},
                    Button = {Color = "1 1 1 0", Command = "chat.say /tpc"},
                    Text = {Text = ""}
                }, Layer + ".BgCircleMenu");
            }

            CuiHelper.DestroyUi(player, Layer + ".BgCircle");
            CuiHelper.AddUi(player, container);

        }

        private void ShowUIMain(BasePlayer player)
        {
            var container = new CuiElementContainer();

            container.Add(new CuiPanel
            {
                CursorEnabled = true,
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0 0 0 0"}
            }, "Overlay", Layer);
            
            CuiHelper.DestroyUi(player, Layer);
            CuiHelper.AddUi(player, container);
            
            
            ShowUICricle(player);

        }

        #endregion
    }
}