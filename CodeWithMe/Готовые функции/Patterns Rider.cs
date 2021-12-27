using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("abc", "SA", "1.0.0")]
    internal class abc : RustPlugin
    {
        #region Static

        private const string Layer = "UI_";
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

            if (!ImageLibrary.Call<bool>("HasImage", "")) ImageLibrary.Call("AddImage", "", "");
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


        #endregion

        #region Functions



        #endregion

        #region UI


        #endregion

        #region CreateCommands

        [ChatCommand("")]
        private void cmdChat(BasePlayer player, string command, string[] args)
        {

        }

        [ConsoleCommand("")]
        private void cmdConsole(ConsoleSystem.Arg arg)
        {
            if (arg?.Args == null || arg.Args.Length < 1) return;
            var player = arg.Player();
        }

        #endregion

        #region CreateUI

        private void ShowUIMain(BasePlayer player)
        {
            var container = new CuiElementContainer();

            container.Add(new CuiPanel
            {
                CursorEnabled = true,
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0 0 0 0", Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat"}
            }, "Overlay", Layer + ".bg");
            
            CuiHelper.DestroyUi(player, Layer + ".bg");
            CuiHelper.AddUi(player, container);
        }
        
        private void ShowUI(BasePlayer player)
        {
            var container = new CuiElementContainer();

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "", AnchorMax = ""},
                Image = {Color = "0 0 0 0"}
            }, Layer + ".bg", Layer);

            CuiHelper.DestroyUi(player, Layer);
            CuiHelper.AddUi(player, container);

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "", AnchorMax = ""},
                Image = {Color = ""}
            }, Layer);

            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "", AnchorMax = ""},
                Button = {Color = "0 0 0 0", Command = ""},
                Text =
                {
                    Text = "", Font = "robotocondensed-bold.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer);

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "", AnchorMax = ""},
                Text =
                {
                    Text = "", Font = "robotocondensed-bold.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer);

            container.Add(new CuiElement
            {
                Parent = Layer,
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "")},
                    new CuiRectTransformComponent {AnchorMin = "", AnchorMax = ""}
                }
            });
            
            container.Add(new CuiElement
            {
                Parent = Layer,
                Components =
                {
                    new CuiInputFieldComponent
                    {
                        Align = TextAnchor.MiddleCenter, CharsLimit = 8, FontSize = 15,
                        Command = ""
                    },
                    new CuiRectTransformComponent {AnchorMin = "", AnchorMax = ""}
                }
            });
        }

        #endregion

        #region Language

        private string GetMsg(string player, string msg, string[] args) =>
            string.Format(lang.GetMessage(msg, this, player), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["MY_PROFILE"] = "#####"
            }, this);

        }

        #endregion
        
        private string Font1 = "daubmark.ttf";
        private string Font2 = "droidsansmono.ttf";
        private string Font3 = "robotocondensed-bold.ttf";
        private string Font4 = "robotocondensed-regular.ttf";
        private string Green = "0.38 0.46 0.25 0.88";
        private string Red = "0.69 0.22 0.15 1.00";

    }
}


