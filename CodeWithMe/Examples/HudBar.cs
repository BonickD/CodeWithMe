using System;
 using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
 using Oxide.Core.Plugins;
 using Oxide.Game.Rust.Cui;
 using UnityEngine;

 namespace Oxide.Plugins
{
    [Info("HudBar", "CASHR#6906", "1.0.0")]
    internal class HudBar : RustPlugin
    {
        #region Static

        [PluginReference] private Plugin ImageLibrary;
        private static HudBar _ins;
        private Configuration _config;
        private bool AIR;
        private bool PATROOL;

        #endregion

        #region Config

        private class Configuration
        {
            [JsonProperty("Compas Image")] public string Compas = "https://i.imgur.com/dG5nOOJ.png";
            [JsonProperty("Sleep Image")] public string Sleep = "http://i.imgur.com/XIIZkqD.png";
            [JsonProperty("Online Image")] public string Online = "https://i.imgur.com/n9EYIWi.png";
            [JsonProperty("Cargo Plane Image")] public string Air = "http://i.imgur.com/dble6vf.png";
            [JsonProperty("Helicopter Image")] public string Helicopter = "http://i.imgur.com/hTTyTTx.png";
            [JsonProperty("Coordinate Image")] public string Coordinate = "http://i.imgur.com/Kr1pQ5b.png";
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

        #region OxideHooks

        private void OnServerInitialized()
        {
            _ins = this;
            LoadConfig();
            ImageLibrary.Call("AddImage", _config.Sleep, _config.Sleep);
            ImageLibrary.Call("AddImage", _config.Compas, _config.Compas);
            ImageLibrary.Call("AddImage", _config.Coordinate, _config.Coordinate);
            ImageLibrary.Call("AddImage", _config.Air, _config.Air);
            ImageLibrary.Call("AddImage", _config.Helicopter, _config.Helicopter);
            ImageLibrary.Call("AddImage", _config.Online, _config.Online);
            foreach (var entity in BaseNetworkable.serverEntities.Where(p => p is CargoPlane  || p is BaseHelicopter))
            {
                if (entity is CargoPlane)
                    AIR = true;
                if (entity is BaseHelicopter)
                    PATROOL = true;
             
            }
            foreach (var check in BasePlayer.activePlayerList)
            {
                OnPlayerConnected(check);
            }
        }
        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;
            player.gameObject.AddComponent<HudPlayer>();
        }
        private void Unload()
        {
            _ins = null;
            var obj = UnityEngine.Object.FindObjectsOfType<HudPlayer>();
            foreach (var check in obj)
            {
                check?.Kill();
                UnityEngine.Object.Destroy(check); 
            }
        }
        private void OnEntitySpawned(BaseNetworkable entity)
        {
            if (entity is CargoPlane)
            {
                AIR = true;
                return;
            }
            if (entity is BaseHelicopter)
            {
                PATROOL = true;
            }
        }

        private void OnEntityKill(BaseNetworkable entity)
        {
            if (entity is CargoPlane)
            {
                AIR = false;
                return;
            }
            if (entity is BaseHelicopter)
            {
                PATROOL = false;
            }
        }
        #endregion

        [ChatCommand("hud")]
        private void cmdChathud(BasePlayer player, string command, string[] args)
        {
            if (args == null || args.Length == 0)
            {
                player.ChatMessage("USE /hud on/off");
                return;
            }

            switch (args[0])
            {
                case"on":
                    if (player.GetComponent<HudPlayer>() != null) return;
                    player.gameObject.AddComponent<HudPlayer>();
                    break;
                case "off":
                    if (player.GetComponent<HudPlayer>() == null) return;
                    player.GetComponent<HudPlayer>().Kill();
                    break;
                
            }
        }
        private class HudPlayer : FacepunchBehaviour
        {
            private BasePlayer player;
            private string Layer = "HUDBAR.MAINPANEL";
            private Configuration config;
            private Plugin Library;
            private Dictionary<string, string> CompassDirections = new Dictionary<string, string>
            {
                {"n", "N"},
                {"ne", "NE"},
                {"e", "EA"},
                {"se", "SE"},
                {"s", "S"},
                {"sw", "SW"},
                {"w", "W"},
                {"nw", "NW"},
            };
            private void Awake()
            {
                player = GetComponent<BasePlayer>();
                config = _ins._config;
                Library = _ins.ImageLibrary;
             ShowPanel();   
            }

            private string GetDirection()
            {
                var PCurrent = player.eyes.rotation.eulerAngles;
                string str = $"{PCurrent.y.ToString("0")}\u00B0";
               
                    if (PCurrent.y > 337.5 || PCurrent.y < 22.5)
                        str = CompassDirections["n"];
                    else if (PCurrent.y > 22.5 && PCurrent.y < 67.5)
                        str = CompassDirections["ne"];
                    else if (PCurrent.y > 67.5 && PCurrent.y < 112.5)
                        str = CompassDirections["e"];
                    else if (PCurrent.y > 112.5 && PCurrent.y < 157.5)
                        str = CompassDirections["se"];
                    else if (PCurrent.y > 157.5 && PCurrent.y < 202.5)
                        str = CompassDirections["s"];
                    else if (PCurrent.y > 202.5 && PCurrent.y < 247.5)
                        str = CompassDirections["sw"];
                    else if (PCurrent.y > 247.5 && PCurrent.y < 292.5)
                        str = CompassDirections["w"];
                    else if (PCurrent.y > 292.5 && PCurrent.y < 337.5)
                        str = CompassDirections["nw"];
                

                return str;
            }

            private string GetImage(string imageName) => Library.Call<string>("GetImage", imageName);
            private void ShowPanel()
            {
                var container = new CuiElementContainer();

                container.Add(new CuiPanel
                {
                    CursorEnabled = false,
                    RectTransform =
                        {AnchorMin = "0.5 1", AnchorMax = "0.5 1", OffsetMin = "-180 -50", OffsetMax = "180 -32"},
                    Image = {Color = "0 0 0 0.7"}
                }, "Overlay", Layer);
                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = "0.85 0", AnchorMax = "1 0.98"},
                    Button = {Color = "1 1 1 0"},
                    Text =
                    {
                        Text = "", Align = TextAnchor.MiddleCenter, Font = "robotocondensed-regular.ttf", FontSize = 18
                    }
                }, Layer, Layer + ".COMPAS");
                container.Add(new CuiElement
                {
                    Parent = Layer + ".COMPAS",
                    Components =
                    {
                        new CuiRawImageComponent
                            {Png =GetImage(config.Compas), Color = "1 1 1 1"},
                        new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "0.4 1"}
                    }
                });
                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = "0.7 0", AnchorMax = "0.85 0.98"},
                    Button = {Color = "1 1 1 0"},
                    Text =
                    {
                        Text = "", Align = TextAnchor.MiddleCenter, Font = "robotocondensed-regular.ttf", FontSize = 18
                    }
                }, Layer, Layer + ".SLEEP");
                 container.Add(new CuiElement
                {
                    Parent = Layer + ".SLEEP",
                    Components =
                    {
                        new CuiRawImageComponent
                            {Png =GetImage(config.Sleep), Color = "1.00 1.00 0.00 1.00"},
                        new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "0.4 1"}
                    }
                });
                  container.Add(new CuiButton
                 {
                    RectTransform = {AnchorMin = "0.55 0", AnchorMax = "0.7 0.98"},
                    Button = {Color = "1 1 1 0"},
                    Text =
                    {
                        Text = "", Align = TextAnchor.MiddleCenter, Font = "robotocondensed-regular.ttf", FontSize = 18
                    }
                }, Layer, Layer + ".ONLINE");
                 container.Add(new CuiElement
                {
                    Parent = Layer + ".ONLINE",
                    Components =
                    {
                        new CuiRawImageComponent
                            {Png =GetImage(config.Online), Color = "0.13 0.91 0.05 1.00"},
                        new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "0.4 1"}
                    }
                });
                 container.Add(new CuiButton
                 {
                    RectTransform = {AnchorMin = "0.4 0", AnchorMax = "0.55 0.98"},
                    Button = {Color = "1 1 1 0"},
                    Text =
                    {
                        Text = "", Align = TextAnchor.MiddleCenter, Font = "robotocondensed-regular.ttf", FontSize = 18
                    }
                }, Layer, Layer + ".TIME");
                  container.Add(new CuiButton
                 {
                    RectTransform = {AnchorMin = "0.25 0", AnchorMax = "0.4 0.98"},
                    Button = {Color = "1 1 1 0"},
                    Text =
                    {
                        Text = "", Align = TextAnchor.MiddleCenter, Font = "robotocondensed-regular.ttf", FontSize = 18
                    }
                }, Layer, Layer + ".EVENT");
                 container.Add(new CuiButton 
                 {
                    RectTransform = {AnchorMin = "0 0", AnchorMax = "0.25 0.98"},
                    Button = {Color = "1 1 1 0"},
                    Text =
                    {
                        Text = "", Align = TextAnchor.MiddleCenter, Font = "robotocondensed-regular.ttf", FontSize = 18
                    }
                }, Layer, Layer + ".COORD");
                     container.Add(new CuiElement
                {
                    Parent = Layer + ".COORD",
                    Components =
                    {
                        new CuiRawImageComponent
                            {Png =GetImage(config.Coordinate), Color = "1 1 1 1"},
                        new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "0.27 0.95"}
                    }
                });
                CuiHelper.DestroyUi(player, Layer);
                CuiHelper.AddUi(player, container);
                UpdateValue();
                InvokeRepeating(UpdateValue, 1f,1f);
            }

            private void UpdateValue()
            {
                var container = new CuiElementContainer();
                container.Add(new CuiElement
                {
                    Parent = Layer + ".COMPAS",
                    Name = Layer + ".COMPAS" + ".TEXT",
                    Components =
                    {
                        new CuiTextComponent
                        {
                            Text = GetDirection(), FontSize = 15, Font = "robotocondensed-bold.ttf",
                            Align = TextAnchor.MiddleCenter
                        },
                        new CuiRectTransformComponent {AnchorMin = "0.4 0", AnchorMax = "1 1"},
                        new CuiOutlineComponent {Color = "0 0 0 0", Distance = "0.5 -0.5"}
                    }
                });
                container.Add(new CuiElement
                {
                    Parent = Layer + ".SLEEP",
                    Name = Layer + ".SLEEP" + ".TEXT",
                    Components =
                    {
                        new CuiTextComponent
                        {
                            Text = BasePlayer.sleepingPlayerList.Count.ToString(), Color = "1.00 1.00 0.00 1.00",FontSize = 15,
                            Font = "robotocondensed-bold.ttf",
                            Align = TextAnchor.MiddleCenter
                        },
                        new CuiRectTransformComponent {AnchorMin = "0.4 0", AnchorMax = "1 1"},
                        new CuiOutlineComponent {Color = "0 0 0 0", Distance = "0.5 -0.5"}
                    }
                });
                container.Add(new CuiElement
                {
                    Parent = Layer + ".ONLINE",
                    Name = Layer + ".ONLINE" + ".TEXT",
                    Components =
                    {
                        new CuiTextComponent
                        {
                            Text = $"<color=#20e80e>{BasePlayer.activePlayerList.Count}</color>", FontSize = 15,
                            Font = "robotocondensed-bold.ttf",
                            Align = TextAnchor.MiddleCenter
                        },
                        new CuiRectTransformComponent {AnchorMin = "0.4 0", AnchorMax = "1 1"},
                        new CuiOutlineComponent {Color = "0 0 0 0", Distance = "0.5 -0.5"}
                    }
                });
                container.Add(new CuiElement
                {
                    Parent = Layer + ".EVENT",
                    Name = Layer + ".EVENT" + ".AIR",
                    Components =
                    {
                        new CuiRawImageComponent
                            {Png = GetImage(config.Air), Color = _ins.AIR ? "0 1 0 1" : "1 1 1 1"},
                        new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "0.45 0.95"}
                    }
                });
                container.Add(new CuiElement
                {
                    Parent = Layer + ".EVENT",
                    Name = Layer + ".EVENT" + ".HELI",
                    Components =
                    {
                        new CuiRawImageComponent
                            {Png = GetImage(config.Helicopter), Color = _ins.PATROOL ? "0 1 0 1" : "1 1 1 1"},
                        new CuiRectTransformComponent {AnchorMin = "0.56 0", AnchorMax = "0.99 1"}
                    }
                });
                container.Add(new CuiElement
                {
                    Parent = Layer + ".COORD",
                    Name = Layer + ".COORD" + ".TEXT",
                    Components =
                    {
                        new CuiTextComponent
                        {
                            Text = GetCoord(), FontSize = 10, Font = "robotocondensed-bold.ttf",
                            Align = TextAnchor.MiddleCenter
                        },
                        new CuiRectTransformComponent {AnchorMin = "0.22 0", AnchorMax = "1 1"},
                        new CuiOutlineComponent {Color = "0 0 0 0", Distance = "0.5 -0.5"}
                    }
                });
                container.Add(new CuiElement
                {
                    Parent = Layer + ".TIME",
                    Name = Layer + ".TIME" + ".TEXT",
                    Components =
                    {
                        new CuiTextComponent
                        {
                            Text = $"{TOD_Sky.Instance.Cycle.DateTime:HH:mm}", FontSize = 12, Font = "robotocondensed-bold.ttf",
                            Align = TextAnchor.MiddleCenter
                        },
                        new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "1 1"},
                        new CuiOutlineComponent {Color = "0 0 0 0", Distance = "0.5 -0.5"}
                    }
                });
                CuiHelper.DestroyUi(player, Layer + ".COMPAS" + ".TEXT");
                CuiHelper.DestroyUi(player, Layer + ".SLEEP" + ".TEXT");
                CuiHelper.DestroyUi(player, Layer + ".COORD" + ".TEXT");
                 CuiHelper.DestroyUi(player, Layer + ".TIME" + ".TEXT");
                CuiHelper.DestroyUi(player, Layer + ".ONLINE" + ".TEXT");
                CuiHelper.DestroyUi(player, Layer + ".EVENT" + ".AIR");
                CuiHelper.DestroyUi(player, Layer + ".EVENT" + ".HELI");
                CuiHelper.AddUi(player, container);
            }

            private string GetCoord()
            {
                var position = player.transform.position;
                return $"X: {position.x:0} | Z: {position.z:0}";
            }
            public void Kill()
            {
                CuiHelper.DestroyUi(player, Layer);
                CancelInvoke(UpdateValue);
                Destroy(this);
            }
        }
    }
}