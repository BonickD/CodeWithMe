using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("HudRanked", "AhigaO#4485", "1.0.0")]
    internal class HudRanked : RustPlugin
    {
        #region Static

        private const string Layer = "UI_HudRanked";
        private int ImageLibraryCheck = 0;
        private Configuration _config;
        [PluginReference] private Plugin ImageLibrary;

        private static HudRanked _ins;
        int max = 0;

        private Dictionary<ulong, PlayerHud> _players = new Dictionary<ulong, PlayerHud>();
        private TOD_Sky sky;

        #endregion

        #region Config

        private class Configuration
        {
            [JsonProperty(PropertyName = "Ссылка на панель")]
            public string url = "https://i.imgur.com/3gVUU3i.png";

            [JsonProperty(PropertyName = "Команда для кнопки Инфо")]
            public string comandInfo = "";

            [JsonProperty(PropertyName = "Команда для кнопки Магазин")]
            public string comandShop = "";

            [JsonProperty(PropertyName = "Команда для кнопки Награды")]
            public string comandReward = "";
            
            [JsonProperty(PropertyName = "Команда для кнопки Магазин в менюшке")]
            public string comandStore = "";

            [JsonProperty(PropertyName = "Команда для кнопки Меню")]
            public string comandMenu = "";

            [JsonProperty(PropertyName = "Команда для кнопки Статистика")]
            public string comandStatistics = "";
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


        #region Hook

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;
            _players.Add(player.userID, player.gameObject.AddComponent<PlayerHud>());
            var current = BasePlayer.activePlayerList.Count;
            var sleepers = BasePlayer.sleepingPlayerList.Count;
            foreach (var check in _players) check.Value.ShowUIPlayerOnline(current, max, sleepers);
        }

        private void OnPlayerSleep(BasePlayer player)
        {
            var current = BasePlayer.activePlayerList.Count;
            var sleepers = BasePlayer.sleepingPlayerList.Count;
            foreach (var check in _players) check.Value.ShowUIPlayerOnline(current, max, sleepers);
        }

        private void OnPlayerSleepEnded(BasePlayer player)
        {
            var current = BasePlayer.activePlayerList.Count;
            var sleepers = BasePlayer.sleepingPlayerList.Count;
            foreach (var check in _players) check.Value.ShowUIPlayerOnline(current, max, sleepers);
        }

        private void OnPlayerDisconnected(BasePlayer player)
        {
            _players[player.userID].Kill();
            _players.Remove(player.userID);
        }

        private void Unload()
        {
            foreach (var check in BasePlayer.activePlayerList.ToArray()) OnPlayerDisconnected(check);
            _ins = null;
        }

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

            _ins = this;
            sky = TOD_Sky.Instance;
            max = ConVar.Server.maxplayers;
            foreach (var check in BasePlayer.activePlayerList) OnPlayerConnected(check);
            if (!ImageLibrary.Call<bool>("HasImage", _config.url)) ImageLibrary.Call("AddImage", _config.url, _config.url);
        }

        #endregion

        #region Functions

        private class PlayerHud : FacepunchBehaviour
        {
            private BasePlayer _player;

            private void Awake()
            {
                _player = GetComponent<BasePlayer>();
                ShowUIMain();
                InvokeRepeating(ShowUIInfo, 0, 1);
            }

            #region UI

            public void ShowUIPlayerOnline(int current, int max, int sleepers)
            {
                var container = new CuiElementContainer();


                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0.249 0.404", AnchorMax = "0.501 0.6027"},
                    Text =
                    {
                        Text = $"ОНЛАЙН: {current}/{max}", Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleLeft,
                        Color = "0.63 0.75 0.82 1.00"
                    }
                }, Layer, Layer + ".Online");
                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0.526 0.404", AnchorMax = "0.742 0.6027"},
                    Text =
                    {
                        Text = $"СПЯТ: {sleepers}", Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleLeft,
                        Color = "0.63 0.75 0.82 1.00"
                    }
                }, Layer, Layer + ".Sleep");

                CuiHelper.DestroyUi(_player, Layer + ".Online");
                CuiHelper.DestroyUi(_player, Layer + ".Sleep");
                CuiHelper.AddUi(_player, container);
            }

            private void ShowUIInfo()
            {
                var container = new CuiElementContainer();

                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0.766 0.404", AnchorMax = "0.969 0.6027"},
                    Text =
                    {
                        Text = $"ВРЕМЯ  {_ins.sky.Cycle.DateTime.ToShortTimeString()}", Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleLeft,
                        Color = "0.63 0.75 0.82 1.00"
                    }
                }, Layer, Layer + ".Time");

                CuiHelper.DestroyUi(_player, Layer + ".Time");
                CuiHelper.AddUi(_player, container);
            }

            private void ShowUIMain()
            {
                var container = new CuiElementContainer();

                container.Add(new CuiElement
                {
                    Parent = "Overlay",
                    Name = Layer,
                    Components =
                    {
                        new CuiRawImageComponent {Png = _ins.ImageLibrary.Call<string>("GetImage", _ins._config.url)},
                        new CuiRectTransformComponent {AnchorMin = "0.002 0.879", AnchorMax = "0.245 0.988"}
                    }
                });

                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = "0.249 0.173", AnchorMax = "0.474 0.394"},
                    Button = {Color = "0 0 0 0", Command = _ins._config.comandInfo},
                    Text =
                    {
                        Text = "", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                        Color = ""
                    }
                }, Layer);
                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = "0.497 0.173", AnchorMax = "0.721 0.394"},
                    Button = {Color = "0 0 0 0", Command = _ins._config.comandShop},
                    Text =
                    {
                        Text = "", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                        Color = ""
                    }
                }, Layer);
                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = "0.744 0.173", AnchorMax = "0.971 0.394"},
                    Button = {Color = "0 0 0 0", Command = _ins._config.comandReward},
                    Text =
                    {
                        Text = "", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                        Color = ""
                    }
                }, Layer);
                
                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = "0.726 0.644", AnchorMax = "0.798 0.952"},
                    Button = {Color = "0 0 0 0", Command = _ins._config.comandStore},
                    Text =
                    {
                        Text = "", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                        Color = ""
                    }
                }, Layer);
                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = "0.82 0.644", AnchorMax = "0.892 0.952"},
                    Button = {Color = "0 0 0 0", Command = _ins._config.comandMenu},
                    Text =
                    {
                        Text = "", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                        Color = ""
                    }
                }, Layer);
                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = "0.915 0.644", AnchorMax = "0.987 0.952"},
                    Button = {Color = "0 0 0 0", Command = _ins._config.comandStatistics},
                    Text =
                    {
                        Text = "", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                        Color = ""
                    }
                }, Layer);


                CuiHelper.DestroyUi(_player, Layer);
                CuiHelper.AddUi(_player, container);
            }

            #endregion

            public void Kill()
            {
                CancelInvoke(ShowUIInfo);
                Destroy(this);
            }

        }

        #endregion


    }
}