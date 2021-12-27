using System;
using System.Collections.Generic;
using ConVar;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Christmas", "AhigaO#4485", "1.0.0")]
    internal class Christmas : RustPlugin
    {
        #region Static

        private const string Layer = "UI_Christmas";
        private int ImageLibraryCheck = 0;
        private Configuration _config;
        private Dictionary<ulong, Data> data;
        [PluginReference] private Plugin ImageLibrary;

        private class CItem
        {
            [JsonProperty(PropertyName = "Shortname")]
            public string shortname;

            [JsonProperty(PropertyName = "Amount")]
            public int amount;

            [JsonProperty(PropertyName = "SkinID")]
            public ulong skinID;

            [JsonProperty(PropertyName = "Chance of drop")]
            public int dropChance;
        }
        
        private class Gift
        {
            [JsonProperty(PropertyName = "Minimum number of items to be given out")]
            public int minAmount;

            [JsonProperty(PropertyName = "Maximum number of items to be given out")]
            public int maxAmount;

            [JsonProperty(PropertyName = "Item list", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<CItem> items;
        }
        
        private class Behavior
        {
            [JsonProperty(PropertyName = "Behavior name")]
            public string name;

            [JsonProperty(PropertyName = "The minimum number of behavior points to achieve this behavior")]
            public int behaviorAmount;

            [JsonProperty(PropertyName = "List of gifts for this behavior with chance", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<int, Gift> gifts;
        }
        
        #endregion

        #region Config
        
        private class Configuration
        {
            [JsonProperty(PropertyName = "SteamID for icon in chat messages")]
            public ulong iconID = 0;

            [JsonProperty(PropertyName = "Behaviors", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<Behavior> Behaviors = new List<Behavior>
            {
                new Behavior
                {
                    behaviorAmount = 0,
                    name = "UNSATISFACTORY",
                    gifts = new Dictionary<int, Gift>
                    {
                        [50] = new Gift
                        {
                            minAmount = 2,
                            maxAmount = 3,
                            items = new List<CItem>
                            {
                                
                            }
                        },
                        [100] = new Gift
                        {
                            minAmount = 1,
                            maxAmount = 2,
                            items = new List<CItem>
                            {
                                
                            }
                        }
                    }
                },
                new Behavior
                {
                    behaviorAmount = 500,
                    name = "SATISFACTORY",
                    gifts = new Dictionary<int, Gift>
                    {
                        [50] = new Gift
                        {
                            
                        },
                        [100] = new Gift
                        {
                            
                        }
                    }
                },
                new Behavior
                {
                    behaviorAmount = 1000,
                    name = "SANTA'S FAVORITE",
                    gifts = new Dictionary<int, Gift>
                    {
                        [50] = new Gift
                        {
                            
                        },
                        [100] = new Gift
                        {
                            
                        }
                    }
                }
            };

            [JsonProperty(PropertyName = "Behavior score for object destruction", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<string, float> behaviorForDestroy = new Dictionary<string, float>
            {
                ["assets/bundled/prefabs/autospawn/resource/loot/loot-barrel-1.prefab"] = 0.5f,
                ["assets/bundled/prefabs/autospawn/resource/loot/loot-barrel-2.prefab"] = 0.5f,
                ["assets/bundled/prefabs/radtown/loot_barrel_1.prefab"] = 1f,
                ["assets/bundled/prefabs/radtown/loot_barrel_2.prefab"] = 0.5f,
                ["assets/bundled/prefabs/radtown/oil_barrel.prefab"] = 1f,
                ["assets/rust.ai/agents/bear/bear.prefab"] = 1f,
                ["assets/rust.ai/agents/boar/boar.prefab"] = 1f,
                ["assets/rust.ai/agents/stag/stag.prefab"] = 1f,
                ["assets/rust.ai/agents/stag/wolf.prefab"] = 1f,
            };

            [JsonProperty(PropertyName = "Behaivor Points per player kill")]
            public float playerScore = 20f;   
            
            [JsonProperty(PropertyName = "Behavior points per chat message")]
            public float behaviorForMessage = 0.5f;

            [JsonProperty(PropertyName = "Behavior points for the presence of certain words in the chat", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<string, float> specialWords = new Dictionary<string, float>
            {
                ["dick"] = -0.5f,
                ["bitch"] = -0.5f,
                ["fuck"] = -1f,
                ["prick"] = -0.5f,
                ["bastard"] = -1f,
                ["bellend "] = -0.5f,
                ["cunt"] = -0.5f,
                ["prick"] = -0.5f,
                ["balls"] = -0.5f,
                ["shit"] = -1f,
                ["fucker"] = -0.5f,
                ["asshole"] = -1f,
                ["slave"] = -5f,
                ["nigger"] = -5f,
                ["best server"] = 5f,
                ["sorry"] = 1f,
                ["good"] = 0.5f,
                ["well done"] = 1f,
                ["cool"] = 0.5f,
                ["pretty"] = 0.5f,
                ["cute"] = 0.5f,
            };

            [JsonProperty(PropertyName = "Behavior Points for Learning a Recipe")]
            public float behaviorForStudy = 1.5f;

            [JsonProperty(PropertyName = "Behavior points per broken stone / ore / wood / corpse")]
            public float behaviorForDispenser = 4;

            [JsonProperty(PropertyName = "Behavior points for trading with NPC")]
            public float behaviorForTrading = 0.5f;

            [JsonProperty(PropertyName = "Behavior points for the letter to santa")]
            public float forLetterToSanta = 400;
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
            public float behaviorScore = 0;
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
            if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/bXFQXjk.jpg")) ImageLibrary.Call("AddImage", "https://i.imgur.com/bXFQXjk.jpg", "https://i.imgur.com/bXFQXjk.jpg");
        }

        private void Unload()
        {
            for (var i = 0; i < BasePlayer.activePlayerList.Count; i++) CuiHelper.DestroyUi(BasePlayer.activePlayerList[i], Layer);
            SaveData();
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null || data.ContainsKey(player.userID)) return;
            data.Add(player.userID, new Data());
        }
        
        private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || info == null) return;
            var player = info.InitiatorPlayer;
            if (player == null || !data.ContainsKey(player.userID) || !_config.behaviorForDestroy.ContainsKey(entity.PrefabName)) return;
            data[player.userID].behaviorScore += _config.behaviorForDestroy[entity.PrefabName];
        }

        #endregion

        #region Commands

        [ChatCommand("menu")]
        private void cmdChatmenu(BasePlayer player, string command, string[] args)
        {
            ShowUIMain(player);
        }

        #endregion

        #region Functions
       

        #endregion

        #region UI

        private void ShowUIMenu(BasePlayer player)
        {
            var container = new CuiElementContainer();

           container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0 0 0 1"} 
            }, Layer, Layer + ".Main");
           container.Add(new CuiElement
           {
               Parent = Layer + ".Main",
               Components =
               {
                   new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/bXFQXjk.jpg")},
                   new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "1 1"}
               }
           });
           container.Add(new CuiButton
           {
               RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
               Button = {Color = "0 0 0 0", Close = Layer},
               Text =
               {
                   Text = "", Font = "robotocondensed-bold.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                   Color = "1 1 1 1"
               }
           }, Layer);
            CuiHelper.DestroyUi(player, Layer + ".Main");
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
            }, "Overlay",Layer);
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Button = {Color = "0 0 0 0", Close = Layer},
                Text =
                {
                    Text = "", Font = "robotocondensed-bold.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                    Color = "1 1 1 1"
                }
            }, Layer);
            

            CuiHelper.DestroyUi(player, Layer);
            CuiHelper.AddUi(player, container);
            
            ShowUIMenu(player);            
        }

        #endregion

        #region Language

        private void SendMessage(BasePlayer player, string name, params object[] args) => Player.Message(player, GetMsg(player.UserIDString, name, args), _config.iconID);
        private string GetMsg(string player, string msg, params object[] args) => string.Format(lang.GetMessage(msg, this, player), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {

            }, this);
        }

        #endregion
    }
}