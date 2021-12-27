using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using Physics = UnityEngine.Physics;

namespace Oxide.Plugins
{
    [Info("SantaGifts", "AhigaO#4485", "1.0.0")]
    internal class SantaGifts : RustPlugin
    {
        #region Static

        private static SantaGifts _ins;
        private const string Layer = "UI_Christmas";
        private int ImageLibraryCheck = 0;
        private Configuration _config;
        private Data data;
        private Timer _timer;
        private Dictionary<ulong, SantasPlayer> _players = new Dictionary<ulong, SantasPlayer>();
        [PluginReference] private Plugin ImageLibrary;

        private class CItem
        {
            [JsonProperty(PropertyName = "Shortname")]
            public string shortname;

            [JsonProperty(PropertyName = "Amount")]
            public int amount;

            [JsonProperty(PropertyName = "SkinID")]
            public ulong skinID;
        }

        private class Gift
        {
            [JsonProperty(PropertyName = "Behavior name")]
            public string name;

            [JsonProperty(PropertyName = "Minimum number of items to be given out")]
            public int minAmount;

            [JsonProperty(PropertyName = "Maximum number of items to be given out")]
            public int maxAmount;

            [JsonProperty(PropertyName = "Item list", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<CItem> items;
        }

        private class Behavior
        {
            [JsonProperty(PropertyName = "The minimum number of behavior points to achieve this behavior")]
            public int behaviorAmount;

            [JsonProperty(PropertyName = "Name of behavior")]
            public string name;

            [JsonProperty(PropertyName = "List of gifts for this behavior with random drop chance", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<Gift> gifts;
        }

        #endregion

        #region Config

        private class Configuration
        {
            [JsonProperty(PropertyName = "Command for open UI")]
            public string command = "gifts";

            [JsonProperty(PropertyName = "SteamID for icon in chat messages")]
            public ulong iconID = 0;

            [JsonProperty(PropertyName = "The interval for giving out gifts from Santa.(In seconds)")]
            public int santaGiftsInterval = 86400;

            [JsonProperty(PropertyName = "Behaviors", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<Behavior> Behaviors = new List<Behavior>
            {
                new Behavior
                {
                    name = "<color=red>UNSATISFACTORY</color>",
                    behaviorAmount = 0,
                    gifts = new List<Gift>
                    {
                        new Gift
                        {
                            name = "COMMON GIFT",
                            minAmount = 3,
                            maxAmount = 6,
                            items = new List<CItem>
                            {
                                new CItem
                                {
                                    shortname = "coal",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "coal",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "coal",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "giantcandycanedecor",
                                    amount = 1,
                                    skinID = 0
                                }
                            }
                        },
                        new Gift
                        {
                            name = "UNCOMMON GIFT",
                            minAmount = 1,
                            maxAmount = 2,
                            items = new List<CItem>
                            {
                                new CItem
                                {
                                    shortname = "coal",
                                    amount = 1000,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "coal",
                                    amount = 1000,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "sulfur",
                                    amount = 1000,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "metal.fragments",
                                    amount = 1000,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "shotgun.waterpipe",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "ladder.wooden.wall",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "candycaneclub",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "bow.compound",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "door.double.hinged.metal",
                                    amount = 1,
                                    skinID = 0
                                },
                            }
                        }
                    }
                },
                new Behavior
                {
                    name = "<color=yellow>SATISFACTORY</color>",
                    behaviorAmount = 500,
                    gifts = new List<Gift>
                    {
                        new Gift
                        {
                            name = "RARE GIFT",
                            minAmount = 2,
                            maxAmount = 3,
                            items = new List<CItem>
                            {
                                new CItem
                                {
                                    shortname = "scrap",
                                    amount = 200,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "metal.fragments",
                                    amount = 1000,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "gunpowder",
                                    amount = 500,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "metalpipe",
                                    amount = 5,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "riflebody",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "smgbody",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "pistol.m92",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "grenade.f1",
                                    amount = 4,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "jackhammer",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "ammo.pistol",
                                    amount = 100,
                                    skinID = 0
                                },
                            }
                        },
                        new Gift
                        {
                            name = "EPIC GIFT",
                            minAmount = 2,
                            maxAmount = 3,
                            items = new List<CItem>
                            {
                                new CItem
                                {
                                    shortname = "scrap",
                                    amount = 500,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "lowgradefuel",
                                    amount = 300,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "metal.refined",
                                    amount = 100,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "ammo.pistol",
                                    amount = 100,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "ammo.pistol",
                                    amount = 28,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "workbench2",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "explosive.satchel",
                                    amount = 4,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "smg.thompson",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "oretea.advanced",
                                    amount = 3,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "wall.frame.garagedoor",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "supply.signal",
                                    amount = 100,
                                    skinID = 0
                                },
                            }
                        }
                    }
                },
                new Behavior
                {
                    name = "<color=green>SANTA'S FAVORITE</color>",
                    behaviorAmount = 1000,
                    gifts = new List<Gift>
                    {
                        new Gift
                        {
                            name = "LEGENDARY GIFT",
                            minAmount = 2,
                            maxAmount = 3,
                            items = new List<CItem>
                            {
                                new CItem
                                {
                                    shortname = "ammo.rifle",
                                    amount = 100,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "ammo.rifle",
                                    amount = 28,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "ammo.rifle.explosive",
                                    amount = 100,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "ammo.rifle.explosive",
                                    amount = 28,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "hazmatsuit",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "easter.silveregg",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "door.hinged.toptier",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "door.hinged.toptier",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "metal.refined",
                                    amount = 100,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "rifle.lr300",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "workbench3",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "syringe.medical",
                                    amount = 2,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "gunpowder",
                                    amount = 1000,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "gunpowder",
                                    amount = 1000,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "gunpowder",
                                    amount = 1000,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "gunpowder",
                                    amount = 1000,
                                    skinID = 0
                                },
                            }
                        },
                        new Gift
                        {
                            name = "SUPERIOR GIFT",
                            minAmount = 2,
                            maxAmount = 3,
                            items = new List<CItem>
                            {
                                new CItem
                                {
                                    shortname = "scrap",
                                    amount = 1000,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "explosives",
                                    amount = 100,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "explosive.timed",
                                    amount = 2,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "jackhammer",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "chainsaw",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "metal.refined",
                                    amount = 100,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "lmg.m249",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "weapon.mod.small.scope",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "weapon.mod.silencer",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "weapon.mod.lasersight",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "metal.facemask",
                                    amount = 1,
                                    skinID = 0
                                },
                                new CItem
                                {
                                    shortname = "metal.plate.torso",
                                    amount = 1,
                                    skinID = 0
                                },
                            }
                        }
                    }
                }
            };

            [JsonProperty(PropertyName = "Behaivor Points per player kill")]
            public float behaviorForPlayer = 20f;

            [JsonProperty(PropertyName = "Behaivor Points per NPC kill")]
            public float behaviorForNPC = 20f;

            [JsonProperty(PropertyName = "Behavior points per chat message")]
            public float behaviorForMessage = 0.1f;

            [JsonProperty(PropertyName = "Behavior Points for Learning a Recipe")]
            public float behaviorForStudy = 1.5f;

            [JsonProperty(PropertyName = "Behavior points per broken stone/ore/wood/corpse")]
            public float behaviorForDispenser = 4;

            [JsonProperty(PropertyName = "The interval for giving the player 1 behavior point for being on the server")]
            public int scoreInterval = 300;

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

        private class PlayerData
        {
            public float behaviorScore = 0;
            public Gift gift;
        }

        private class Data
        {
            public Dictionary<ulong, PlayerData> Players = new Dictionary<ulong, PlayerData>();
            public int currentTime = 0;
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

            if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/lU7o6aa.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/lU7o6aa.png", "https://i.imgur.com/lU7o6aa.png");
            if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/4mS1puC.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/4mS1puC.png", "https://i.imgur.com/4mS1puC.png");
            if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/t3a14Gh.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/t3a14Gh.png", "https://i.imgur.com/t3a14Gh.png");

            cmd.AddChatCommand(_config.command, this, nameof(ShowUI));
            _ins = this;
            LoadData();
            foreach (var check in BasePlayer.activePlayerList) OnPlayerConnected(check);
            _timer = timer.Every(1f, () =>
            {
                data.currentTime++;
                if (data.currentTime < _config.santaGiftsInterval) return;
                data.currentTime = 0;
                GenerateGifts();
            });
        }

        private void OnDispenserBonus(ResourceDispenser dispenser, BasePlayer player, Item item)
        {
            PlayerData _data;
            if (player == null || data.Players.TryGetValue(player.userID, out _data)) return;
            _data.behaviorScore += _config.behaviorForDispenser;
        }

        private void OnPlayerStudyBlueprint(BasePlayer player, Item item)
        {
            PlayerData _data;
            if (player == null || item == null || !data.Players.TryGetValue(player.userID, out _data)) return;
            _data.behaviorScore += _config.behaviorForStudy;
        }

        private void OnPlayerChat(BasePlayer player, string message, Chat.ChatChannel channel)
        {
            PlayerData _data;
            if (player == null || channel != Chat.ChatChannel.Global || !data.Players.TryGetValue(player.userID, out _data) || string.IsNullOrEmpty(message)) return;
            _data.behaviorScore += _config.behaviorForMessage;
        }

        private void Unload()
        {
            _timer.Destroy();
            foreach (var check in BasePlayer.activePlayerList.ToArray())
            {
                OnPlayerDisconnected(check);
                CuiHelper.DestroyUi(check, Layer);
            }

            SaveData();
            _ins = null;
        }

        private void OnPlayerDisconnected(BasePlayer player)
        {
            SantasPlayer comp;
            if (player == null || !_players.TryGetValue(player.userID, out comp)) return;
            comp?.Kill();
            _players.Remove(player.userID);
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;
            if (!data.Players.ContainsKey(player.userID)) data.Players.Add(player.userID, new PlayerData());
            _players.Add(player.userID, player.gameObject.AddComponent<SantasPlayer>());
        }

        private void OnEntityDeath(BasePlayer entity, HitInfo info)
        {
            if (entity == null) return;
            var player = info?.InitiatorPlayer;
            if (player == null) return;
            PlayerData _data;
            if (!data.Players.TryGetValue(player.userID, out _data)) return;
            _data.behaviorScore += entity.userID.IsSteamId() ? _config.behaviorForPlayer : _config.behaviorForNPC;
        }

        private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null) return;
            var player = info?.InitiatorPlayer;
            if (player == null) return;
            PlayerData _data;
            float score;
            if (!data.Players.TryGetValue(player.userID, out _data) || !_config.behaviorForDestroy.TryGetValue(entity.PrefabName, out score)) return;
            _data.behaviorScore += score;
        }

        #endregion

        #region Commands

        [ConsoleCommand("UI_GIFTS")]
        private void cmdConsoleUI_GIFTS(ConsoleSystem.Arg arg)
        {
            if (arg?.Args == null || arg.Args.Length < 1) return;
            var player = arg.Player();
            switch (arg.Args[0])
            {
                case "confirm":
                    var container = new CuiElementContainer();

                    container.Add(new CuiElement
                    {
                        Parent = Layer,
                        Name = Layer + ".confirm",
                        Components =
                        {
                            new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/t3a14Gh.png")},
                            new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "1 1"}
                        }
                    });

                    container.Add(new CuiButton
                    {
                        RectTransform = {AnchorMin = "0.41 0.31", AnchorMax = "0.59 0.403"},
                        Button = {Color = "0 0 0 0", Command = "UI_GIFTS takegift", Close = Layer},
                        Text = {Text = ""}
                    }, Layer + ".confirm");

                    container.Add(new CuiButton
                    {
                        RectTransform = {AnchorMin = "0.941 0.91", AnchorMax = "0.985 0.988"},
                        Button = {Color = "0 0 0 0", Command = "UI_GIFTS close", Close = Layer},
                        Text = {Text = ""}
                    }, Layer + ".confirm");

                    CuiHelper.DestroyUi(player, Layer + ".confirm");
                    CuiHelper.AddUi(player, container);
                    break;
                case "takegift":
                    var gift = data.Players[player.userID];
                    foreach (var check in gift.gift.items) player.GiveItem(ItemManager.CreateByName(check.shortname, check.amount, check.skinID));
                    SendMessage(player, "CM_TAKEGIFT", gift.gift.name);
                    gift.gift = null;
                    break;
                case "close":
                    _players[player.userID].inUI = false;
                    break;
            }
        }

        #endregion

        #region Functions

        private void GenerateGifts()
        {
            foreach (var check in data.Players)
            {
                var value = check.Value;
                if (value.gift != null) continue;
                var behavior = GetBehavior(value.behaviorScore);
                var gift = behavior.gifts.GetRandom();
                var playerGift = new Gift();
                playerGift.name = gift.name;
                playerGift.items = new List<CItem>();
                for (var i = 0; i < UnityEngine.Random.Range(gift.minAmount, gift.maxAmount + 1); i++)
                {
                    var item = gift.items.GetRandom();
                    playerGift.items.Add(new CItem {shortname = item.shortname, amount = item.amount, skinID = item.skinID});
                }

                check.Value.gift = playerGift;
            }
        }

        private Behavior GetBehavior(float behaviorScore)
        {
            var behavior = _config.Behaviors.First();
            for (var i = 0; i < _config.Behaviors.Count; i++)
                if (_config.Behaviors[i].behaviorAmount < behaviorScore)
                    behavior = _config.Behaviors[i];
            return behavior;
        }

        private class SantasPlayer : FacepunchBehaviour
        {
            private BasePlayer _player;
            private ulong id;
            public bool inUI = false;

            private void Awake()
            {
                _player = GetComponent<BasePlayer>();
                id = _player.userID;
                var interval = _ins._config.scoreInterval;
                InvokeRepeating(ScoreTimer, interval, interval);
                InvokeRepeating(UpdateUI, 0, 1f);
            }

            private void UpdateUI()
            {
                if (!inUI) return;
                var container = new CuiElementContainer();
                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0.143 0.031", AnchorMax = "0.332 0.126"},
                    Text =
                    {
                        Text = TimeString(_ins._config.santaGiftsInterval - _ins.data.currentTime), Font = "robotocondensed-bold.ttf", FontSize = 45, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer, Layer + ".timer");
                CuiHelper.DestroyUi(_player, Layer + ".timer");
                CuiHelper.AddUi(_player, container);
            }

            private void ScoreTimer() => _ins.data.Players[id].behaviorScore += 1;

            public void Kill()
            {
                CancelInvoke(UpdateUI);
                CancelInvoke(ScoreTimer);
                Destroy(this);
            }
        }

        #endregion

        #region UI

        private static string TimeString(int seconds)
        {
            var span = TimeSpan.FromSeconds(seconds);
            var time = string.Empty;
            time += span.Hours > 9 ? $"{span.Hours + span.Days * 24}:" : $"0{span.Hours + span.Days * 24}:";
            time += span.Minutes > 9 ? $"{span.Minutes}:" : $"0{span.Minutes}:";
            time += span.Seconds > 9 ? $"{span.Seconds}" : $"0{span.Seconds}";
            return time;
        }

        private void ShowUI(BasePlayer player)
        {
            var _data = data.Players[player.userID];
            var canTakeGift = _data.gift != null;
            var container = new CuiElementContainer();

            container.Add(new CuiPanel
            {
                CursorEnabled = true,
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0 0 0 0"}
            }, "Overlay", Layer);

            if (canTakeGift)
            {
                container.Add(new CuiElement
                {
                    Parent = Layer,
                    Components =
                    {
                        new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/4mS1puC.png")},
                        new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "1 1"}
                    }
                });

                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0.531 0.682", AnchorMax = "0.871 0.773"},
                    Text =
                    {
                        Text = GetMsg(player.UserIDString, "UI_BEHAVIOR") + GetBehavior(_data.behaviorScore).name, Font = "robotocondensed-bold.ttf", FontSize = 25, Align = TextAnchor.MiddleCenter,
                        Color = "0 0 0 1"
                    }
                }, Layer);

                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = "0 0", AnchorMax = "0.21 0.37"},
                    Button = {Color = "0 0 0 0", Command = "UI_GIFTS confirm"},
                    Text = {Text = ""}
                }, Layer);

                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = "0.941 0.91", AnchorMax = "0.985 0.988"},
                    Button = {Color = "0 0 0 0", Command = "UI_GIFTS close", Close = Layer},
                    Text =
                    {
                        Text = "", Font = "robotocondensed-bold.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer);
            }
            else
            {
                container.Add(new CuiElement
                {
                    Parent = Layer,
                    Components =
                    {
                        new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/lU7o6aa.png")},
                        new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "1 1"}
                    }
                });

                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0.531 0.682", AnchorMax = "0.871 0.773"},
                    Text =
                    {
                        Text = GetMsg(player.UserIDString, "UI_BEHAVIOR") + GetBehavior(_data.behaviorScore).name, Font = "robotocondensed-bold.ttf", FontSize = 25, Align = TextAnchor.MiddleCenter,
                        Color = "0 0 0 1"
                    }
                }, Layer);

                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0.143 0.031", AnchorMax = "0.332 0.126"},
                    Text =
                    {
                        Text = TimeString(_config.santaGiftsInterval - data.currentTime), Font = "robotocondensed-bold.ttf", FontSize = 45, Align = TextAnchor.MiddleCenter,
                        Color = "1 1 1 1"
                    }
                }, Layer, Layer + ".timer");

                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = "0.941 0.91", AnchorMax = "0.985 0.988"},
                    Button = {Color = "0 0 0 0", Command = "UI_GIFTS close", Close = Layer},
                    Text = {Text = ""}
                }, Layer);

                _players[player.userID].inUI = true;
            }


            CuiHelper.DestroyUi(player, Layer);
            CuiHelper.AddUi(player, container);
        }

        #endregion

        #region Language

        private void SendMessage(BasePlayer player, string name, params object[] args) => Player.Message(player, GetMsg(player.UserIDString, name, args), _config.iconID);
        private string GetMsg(string player, string msg, params object[] args) => string.Format(lang.GetMessage(msg, this, player), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["CM_TAKEGIFT"] = "Received a {0} gift from Santa",
                ["UI_BEHAVIOR"] = "YOUR BEHAVIOR "
            }, this);
        }

        #endregion
    }
}