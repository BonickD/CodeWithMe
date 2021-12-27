using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using TinyJSON;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Factory", "CASHR", "1.0.0")]
    internal class Factory : RustPlugin
    {
        #region Static

        private static Factory _;
        private const string Layer = "UI_Factory";
        private int ImageLibraryCheck = 0;
        private Configuration _config;
        private Data _data;
        [PluginReference] private Plugin ImageLibrary;

        private BasePlayer bot;
        private Dictionary<ulong, FactoryPlayer> players = new Dictionary<ulong, FactoryPlayer>();
        private Dictionary<ulong, Dictionary<string, Order>> creatingOrder = new Dictionary<ulong, Dictionary<string, Order>>();
        private Dictionary<string, FactoryCraft> factories = new Dictionary<string, FactoryCraft>();
        
        private enum Owner
        {
            Personal,
            Team,
            Public
        }
        
        private class FItem
        {
            public string shortname;
            public int amount;
            public ulong skin;
        }
        
        private class Order
        {
            public List<ItemCraft> crafts = new List<ItemCraft>();
            public ulong owner;
            public int time = 0;
            public int waitTime = 0;
            public int startTime = 0;
            public Owner type = Owner.Personal;
        }
        
        private class ItemCraft
        {
            public FItem result;
            public int time;
            public List<FItem> cost = new List<FItem>();
        }
        
        #endregion

        #region Config

        private class Configuration
        {
            [JsonProperty(PropertyName = "Предметы, которые можно скрафтить без фабрики", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<string> canCraft = new List<string>
            {
                "rifle.ak"
            };

            [JsonProperty(PropertyName = "Список предметов, которые можно переместить в инвентарь завода", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<string> canBeInFactory = new List<string>
            {
                "wood"
            };
            
            [JsonProperty(PropertyName = "Настройка бота")]
            public FactoryBot botSettings = new FactoryBot();

            [JsonProperty(PropertyName = "Крафты", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<string, CraftCategory> crafts = new Dictionary<string, CraftCategory>
            {
                ["Оружие"] = new CraftCategory
                {
                    icon = "https://i.imgur.com/Fni4Z7S.png",
                    items = new List<ItemCraft>
                    {
                        new ItemCraft
                        {
                            result = new FItem
                            {
                                shortname = "pistol.semiauto",
                                amount = 1,
                                skin = 0
                            },
                            time = 20,
                            cost = new List<FItem>
                            {
                                new FItem
                                {
                                    shortname = "metalpipe",
                                    amount = 1,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "semibody",
                                    amount = 1,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "metal.refined",
                                    amount = 4,
                                    skin = 0
                                }
                            }
                        }
                    }
                },
                ["Взрывчатка"] = new CraftCategory
                {
                    icon = "https://i.imgur.com/AP4m2yn.png",
                    items = new List<ItemCraft>
                    {
                        new ItemCraft
                        {
                            result = new FItem
                            {
                                shortname = "explosive.timed",
                                amount = 1,
                                skin = 0
                            },
                            time = 20,
                            cost = new List<FItem>
                            {
                                new FItem
                                {
                                    shortname = "explosives",
                                    amount = 20,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "cloth",
                                    amount = 5,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "techparts",
                                    amount = 2,
                                    skin = 0
                                }
                            }
                        }   
                    }
                },
                ["Одежда"] = new CraftCategory
                {
                    icon = "https://i.imgur.com/J9WwYCe.png",
                    items = new List<ItemCraft>
                    {
                        new ItemCraft
                        {
                            result = new FItem
                            {
                                shortname = "explosive.timed",
                                amount = 1,
                                skin = 0
                            },
                            time = 20,
                            cost = new List<FItem>
                            {
                                new FItem
                                {
                                    shortname = "explosives",
                                    amount = 20,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "cloth",
                                    amount = 5,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "techparts",
                                    amount = 2,
                                    skin = 0
                                }
                            }
                        }   
                    }
                },
                ["Медикаменты"] = new CraftCategory
                {
                    icon = "https://i.imgur.com/7ap38Lq.png",
                    items = new List<ItemCraft>
                    {
                        new ItemCraft
                        {
                            result = new FItem
                            {
                                shortname = "explosive.timed",
                                amount = 1,
                                skin = 0
                            },
                            time = 20,
                            cost = new List<FItem>
                            {
                                new FItem
                                {
                                    shortname = "explosives",
                                    amount = 20,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "cloth",
                                    amount = 5,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "techparts",
                                    amount = 2,
                                    skin = 0
                                }
                            }
                        }   
                    }
                },
                ["Инструменты"] = new CraftCategory
                {
                    icon = "https://i.imgur.com/NblTi3T.png",
                    items = new List<ItemCraft>
                    {
                        new ItemCraft
                        {
                            result = new FItem
                            {
                                shortname = "explosive.timed",
                                amount = 1,
                                skin = 0
                            },
                            time = 20,
                            cost = new List<FItem>
                            {
                                new FItem
                                {
                                    shortname = "explosives",
                                    amount = 20,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "cloth",
                                    amount = 5,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "techparts",
                                    amount = 2,
                                    skin = 0
                                }
                            }
                        }   
                    }
                },
                ["Взрывчатка"] = new CraftCategory
                {
                    icon = "https://i.imgur.com/AP4m2yn.png",
                    items = new List<ItemCraft>
                    {
                        new ItemCraft
                        {
                            result = new FItem
                            {
                                shortname = "explosive.timed",
                                amount = 1,
                                skin = 0
                            },
                            time = 20,
                            cost = new List<FItem>
                            {
                                new FItem
                                {
                                    shortname = "explosives",
                                    amount = 20,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "cloth",
                                    amount = 5,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "techparts",
                                    amount = 2,
                                    skin = 0
                                }
                            }
                        }   
                    }
                },
                ["Компоненты"] = new CraftCategory
                {
                    icon = "https://i.imgur.com/oEc17Bp.png",
                    items = new List<ItemCraft>
                    {
                        new ItemCraft
                        {
                            result = new FItem
                            {
                                shortname = "explosive.timed",
                                amount = 1,
                                skin = 0
                            },
                            time = 20,
                            cost = new List<FItem>
                            {
                                new FItem
                                {
                                    shortname = "explosives",
                                    amount = 20,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "cloth",
                                    amount = 5,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "techparts",
                                    amount = 2,
                                    skin = 0
                                }
                            }
                        }   
                    }
                },
                ["Структуры"] = new CraftCategory
                {
                    icon = "https://i.imgur.com/t1FZcn3.png",
                    items = new List<ItemCraft>
                    {
                        new ItemCraft
                        {
                            result = new FItem
                            {
                                shortname = "explosive.timed",
                                amount = 1,
                                skin = 0
                            },
                            time = 20,
                            cost = new List<FItem>
                            {
                                new FItem
                                {
                                    shortname = "explosives",
                                    amount = 20,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "cloth",
                                    amount = 5,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "techparts",
                                    amount = 2,
                                    skin = 0
                                }
                            }
                        }   
                    }
                },
                ["Переработка"] = new CraftCategory
                {
                    icon = "https://i.imgur.com/S5MzBUd.png",
                    items = new List<ItemCraft>
                    {
                        new ItemCraft
                        {
                            result = new FItem
                            {
                                shortname = "explosive.timed",
                                amount = 1,
                                skin = 0
                            },
                            time = 20,
                            cost = new List<FItem>
                            {
                                new FItem
                                {
                                    shortname = "explosives",
                                    amount = 20,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "cloth",
                                    amount = 5,
                                    skin = 0
                                },
                                new FItem
                                {
                                    shortname = "techparts",
                                    amount = 2,
                                    skin = 0
                                }
                            }
                        }   
                    }
                }
            };
        }
        
        private class CraftCategory
        {
            [JsonProperty(PropertyName = "Ссылка на иконку")]
            public string icon;

            [JsonProperty(PropertyName = "Список крафтов", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<ItemCraft> items;
        }

        private class FactoryBot
        {
            [JsonProperty(PropertyName = "Имя бота")]
            public string botName = "ГЛАВА ФАБРИКИ";

            [JsonProperty(PropertyName = "Место спавна бота")]
            public Vector3 spawnPos = new Vector3(0,0,0);

            [JsonProperty(PropertyName = "Направление бота")]
            public Quater rotation;

            [JsonProperty(PropertyName = "Предметы, надетые на бота", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<string> items = new List<string>
            {
                "hazmatsuit"
            };
        }
        
        private class Quater
        {
            public float x;
            public float y;
            public float z;
            public float w;
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
            public Dictionary<string, List<Order>> factoryData = new Dictionary<string, List<Order>>();
            public List<FItem> inventory = new List<FItem>();
        }

        private void LoadData()
        {
            if (Interface.Oxide.DataFileSystem.ExistsDatafile($"{Name}/data"))
                _data = Interface.Oxide.DataFileSystem.ReadObject<Data>(
                    $"{Name}/data");
            else _data = new Data();
            Interface.Oxide.DataFileSystem.WriteObject($"{Name}/data", _data);
        }

        private void OnServerSave()
        {
            SaveData();
        }

        private void SaveData()
        {
            if (_data != null) Interface.Oxide.DataFileSystem.WriteObject($"{Name}/data", _data);
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

            _ = this;
            LoadData();
            if (_data.factoryData.Count == 0) 
                foreach (var check in _config.crafts)
                {
                    if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/LgLLwhv.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/LgLLwhv.png", "https://i.imgur.com/LgLLwhv.png");
                    if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/nHSedyg.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/nHSedyg.png", "https://i.imgur.com/nHSedyg.png");
                    if (!ImageLibrary.Call<bool>("HasImage", "https://i.imgur.com/QVtAvvL.png")) ImageLibrary.Call("AddImage", "https://i.imgur.com/QVtAvvL.png", "https://i.imgur.com/QVtAvvL.png");
                    if (!ImageLibrary.Call<bool>("HasImage", check.Value.icon)) ImageLibrary.Call("AddImage", check.Value.icon, check.Value.icon);
                    _data.factoryData.Add(check.Key, new List<Order>());
                }
            timer.In(1f, SpawnBot);
            timer.In(2f, () =>
            {
                foreach (var check in _data.factoryData) CreateFactory(check.Value, check.Key);
            });
            timer.In(3f, () =>
            {
                for (var i = 0; i < BasePlayer.activePlayerList.Count; i++) OnPlayerConnected(BasePlayer.activePlayerList[i]);
            });
        }
        
        private object CanCraft(PlayerBlueprints playerBlueprints, ItemDefinition itemDefinition, int skinItemId)
        {
            if (playerBlueprints == null || itemDefinition == null || _config.canCraft.Contains(itemDefinition.shortname)) return null;
            return false;
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;
            LoadOrders(player);
            players.Add(player.userID, player.gameObject.AddComponent<FactoryPlayer>());
        }

        private void OnPlayerDisconnected(BasePlayer player)
        {
            if (player == null) return;
            players[player.userID].Kill();
            players.Remove(player.userID);
        }

        private void Unload()
        {
            for (var i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var check = BasePlayer.activePlayerList[i];
                OnPlayerDisconnected(check);
                CuiHelper.DestroyUi(check, Layer + ".bg");
            }
            bot?.Kill();
            foreach (var check in factories.ToArray()) check.Value.Kill();
            SaveData();
            _ = null;
            bot = null;
        }

        private object CanLootEntity(BasePlayer player, StorageContainer container)
        {
            if (player == null || container?._name != "FACTORY") return null;
            ShowUIMain(player);
            ShowUIInventory(player);
            ShowUICraftCategory(player, _config.crafts.First().Key);
            ShowUIQueuePersonal(player);
            players[player.userID].UpdateState(Owner.Personal, 0);
            return false;
        }
        
        #endregion

        #region Commands

        [ChatCommand("setbot")]
        private void cmdChatsetbot(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            var rot = player.eyes.rotation;
            var botSettings = _config.botSettings;
            botSettings.spawnPos = player.transform.position;
            botSettings.rotation = new Quater{x = rot.x,y = rot.y,z = rot.z,w = rot.w};
            bot?.Kill();
            SaveConfig();
            SpawnBot();
        }

        [ConsoleCommand("UI_FACTORY")]
        private void cmdConsoleUI_CATEGORY(ConsoleSystem.Arg args)
        {
            if (args?.Args == null || args.Args.Length < 1) return;
            var player = args.Player();
            var arg = args.Args;
            switch (arg[0])
            {
                case "open":
                    ShowUICraftCategory(player, arg[1]);
                    break;
                case "addcraft":
                    var c = arg[1];
                    var o = creatingOrder[player.userID][c];
                    if (o.crafts.Count >= 10 || factories[c].GetPlayerOrder(player.userID) != null) return;
                    var r = _config.crafts[c].items[args.GetInt(2)];
                    o.crafts.Add(r);
                    o.startTime += r.time;
                    o.time += r.time;
                    ShowUIQueuePersonal(player);
                    players[player.userID].UpdateState(Owner.Personal, 0);
                    break;
                case "queue":
                    var page = arg.Length > 2 ? args.GetInt(2) : 0;
                    switch (arg[1])
                    {
                        case "0":
                            ShowUIQueuePersonal(player);
                            players[player.userID].UpdateState(Owner.Personal, 0);
                            break;
                        case "1":
                            ShowUITeamQueue(player, page);
                            players[player.userID].UpdateState(Owner.Team, page);
                            break;
                        case "2":
                            ShowUIPublicQueue(player, page);
                            players[player.userID].UpdateState(Owner.Public, page);
                            break;
                    }
                    break;
                case "order":
                    var category = arg[1];
                    var order = creatingOrder[player.userID][category];
                    switch (arg[2])
                    {
                        case "take":
                            var list1 = new List<FItem>();
                            foreach (var check in order.crafts) list1.Add(new FItem{shortname = check.result.shortname, amount = check.result.amount, skin = check.result.skin});
                            if (!HaveSlots(player, list1))
                            {
                                Player.Message(player, "У меня не хватит места в инвентаре, что бы забрать заказ", player.userID);
                                return;
                            }
                            foreach (var check in order.crafts)
                            {  
                                var max = ItemManager.FindItemDefinition(check.result.shortname).stackable;
                                var amount = check.result.amount;
                                while (amount >= max)
                                {
                                    amount -= max;
                                    player.GiveItem(CreateItem(new FItem{shortname = check.result.shortname, amount = max,skin = check.result.skin}));
                                }
                                if (amount > 0) player.GiveItem(CreateItem(new FItem {shortname = check.result.shortname, amount = amount, skin = check.result.skin}));
                            }
                            factories[category].RemoveOrder(order.owner);
                            creatingOrder[order.owner][category] = new Order{owner = order.owner};
                            ShowUIInventory(player);
                            switch (arg[3])
                            {
                                case "0":
                                    ShowUIQueuePersonal(player);
                                    players[player.userID].UpdateState(Owner.Personal, 0);
                                    break;
                                case "1":
                                    ShowUITeamQueue(player, args.GetInt(4));
                                    players[player.userID].UpdateState(Owner.Team, args.GetInt(4));
                                    break;
                                case "2":
                                    ShowUIPublicQueue(player, args.GetInt(4));
                                    players[player.userID].UpdateState(Owner.Public, args.GetInt(4));
                                    break;
                            }
                            break;
                        case "start":
                            var list = new List<FItem>();
                            foreach (var check in order.crafts) foreach (var cost in check.cost) list.Add(new FItem{shortname = cost.shortname, amount = cost.amount, skin = cost.skin});
                            ConsumeItems(player, list);
                            factories[category].AddOrder(order);
                            ShowUIInventory(player);
                            ShowUIQueuePersonal(player);
                            players[player.userID].UpdateState(Owner.Personal, 0);
                            break;
                        case "stop":
                            foreach (var check in order.crafts) foreach (var fitem in check.cost) MoveToFactory(new FItem{shortname = fitem.shortname, amount = fitem.amount, skin = fitem.skin});
                            factories[category].RemoveOrder(player.userID);
                            ShowUIInventory(player);
                            ShowUIQueuePersonal(player);
                            players[player.userID].UpdateState(Owner.Personal, 0);
                            break;
                        case "removeitem":
                            var craft = order.crafts[args.GetInt(3)];
                            order.crafts.Remove(craft);
                            order.startTime -= craft.time;
                            order.time -= craft.time;
                            ShowUIQueuePersonal(player);
                            players[player.userID].UpdateState(Owner.Personal, 0);
                            break;
                        case "type":
                            var type = arg[3] == "2" ? Owner.Public : arg[3] == "1" ? Owner.Team : Owner.Personal;
                            order.type = type;
                            factories[category].ChangeOrderType(player.userID, type);
                            switch (arg[4])
                            {
                                case "0":
                                    ShowUIQueuePersonal(player);
                                    players[player.userID].UpdateState(Owner.Personal, 0);
                                    break;
                                case "1":
                                    ShowUITeamQueue(player, args.GetInt(5));
                                    players[player.userID].UpdateState(Owner.Team, args.GetInt(5));
                                    break;
                                case "2":
                                    ShowUIPublicQueue(player, args.GetInt(5));
                                    players[player.userID].UpdateState(Owner.Public, args.GetInt(5));
                                    break;
                            }
                            break;
                        case "switchtype":
                            var y = args.GetInt(3);
                            var container = new CuiElementContainer();
                            container.Add(new CuiPanel
                            {
                                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "29 -55", OffsetMax = "80 -22"},
                                Image = {Color = "0 0 0 0"}
                            }, Layer + ".Queue" + y, Layer + ".Queue" + ".change");

                            container.Add(new CuiButton
                            {
                                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -11", OffsetMax = "51 0"},
                                Button = {Color = "0.26 0.26 0.26 1.00", Command = $"UI_FACTORY order {category} type 0 {arg[4]}", Close = Layer + ".Queue" + ".change"},
                                Text = {
                                    Text = "Personal", Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter, 
                                    Color = "0.78 0.78 0.78 1.00"
                                }
                            }, Layer + ".Queue" + ".change");
                            
                            container.Add(new CuiButton
                            {
                                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -22", OffsetMax = "51 -11"},
                                Button = {Color = "0.26 0.26 0.26 1.00", Command = $"UI_FACTORY order {category} type 1 {arg[4]}", Close = Layer + ".Queue" + ".change"},
                                Text = {
                                    Text = "Team", Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter, 
                                    Color = "0.78 0.78 0.78 1.00"
                                }
                            }, Layer + ".Queue" + ".change");
                            
                            container.Add(new CuiButton
                            {
                                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -33", OffsetMax = "51 -21"},
                                Button = {Color = "0.26 0.26 0.26 1.00", Command = $"UI_FACTORY order {category} type 2 {arg[4]}", Close = Layer + ".Queue" + ".change"},
                                Text = {
                                    Text = "Public", Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter, 
                                    Color = "0.78 0.78 0.78 1.00"
                                }
                            }, Layer + ".Queue" + ".change");
                            
                            CuiHelper.DestroyUi(player, Layer + ".Queue" + ".change");
                            CuiHelper.AddUi(player, container);
                            break;
                    }
                    break;
                case "close":
                    players[player.userID].Close();
                    break;
            }
        }
        
        [ConsoleCommand("UI_INVENTORY")]
        private void cmdConsoleUI_INVENTORY(ConsoleSystem.Arg args)
        {
            if (args?.Args == null || args.Args.Length < 1) return;
            var player = args.Player();
            var arg = args.Args;
            int page;
            var inventory = _data.inventory;
            switch (args.Args[0])
            {
                case "ui":
                    ShowUIInventory(player,args.GetInt(1));
                    break;
                case "move":
                    var item = player.inventory.AllItems().FirstOrDefault(x => x.uid == uint.Parse(arg[1]));
                    if (item == null) return;
                    if (!_config.canBeInFactory.Contains(item.info.shortname)) return;
                    page = args.GetInt(2);
                    MoveToFactory(item);
                    ShowUIInventory(player, page * 15 > inventory.Count ? page - 1 : page);
                    break;
                case "get":
                    page = args.GetInt(2);
                    var fItem = inventory[args.GetInt(1)];
                    player.GiveItem(CreateItem(fItem));
                    inventory.Remove(fItem);
                    ShowUIInventory(player, page * 15 > inventory.Count ? page - 1 : page);
                    break;
            }
        }
        
        #endregion

        #region Functions

        private bool HaveSlots(BasePlayer player, List<FItem> items)
        {
            var inv = player.inventory;
            var needSlots = 0;
            foreach (var check in items)
            {
                var max = ItemManager.FindItemDefinition(check.shortname).stackable;
                while (check.amount >= max)
                {
                    check.amount -= max;
                    needSlots++;
                }
                if (check.amount > 0) needSlots++;
            }
            return inv.containerMain.capacity - inv.containerMain.itemList.Count + inv.containerBelt.capacity - inv.containerBelt.itemList.Count >= needSlots;
        }

        private void ConsumeItems(BasePlayer player, List<FItem> items)
        {
            var fact = _data.inventory;
            var inv = player.inventory.AllItems();
            foreach (var cost in items.ToArray())
            {
                foreach (var item in fact.ToArray())
                {
                    if (item.shortname != cost.shortname) continue;
                    if (item.amount > cost.amount)
                    {
                        item.amount -= cost.amount;
                        cost.amount = 0;
                        break;
                    }

                    if (item.amount == cost.amount)
                    {
                        fact.Remove(item);
                        cost.amount = 0;
                        break;
                    }
                    cost.amount -= item.amount;
                    fact.Remove(item);
                }
                if (cost.amount <= 0) continue;
                foreach (var item in inv)
                {
                    if (item.info.shortname != cost.shortname) continue;
                    if (item.amount > cost.amount)
                    {
                        item.amount -= cost.amount;
                        cost.amount = 0;
                        item.MarkDirty();
                        break;
                    }

                    if (item.amount == cost.amount)
                    {
                        item.DoRemove();
                        cost.amount = 0;
                        break;
                    }
                    cost.amount -= item.amount;
                    item.DoRemove();
                }
            }
        }
        
        private void MoveToFactory(FItem item)
        {
            var max = ItemManager.FindItemDefinition(item.shortname).stackable;
            var inv = _data.inventory;
            var items = _data.inventory.Where(x => x.shortname == item.shortname).OrderBy(x => x.amount).ToList();
            if (items.Count == 0)
            {
                inv.Add(item);
                return;
            }
            
            for (var i = 0; i < items.Count; i++)
            {
                if (item.amount == 0) break;
                var check = inv[inv.IndexOf(items[i])];
                if (check.amount >= max) continue;
                var need = max - check.amount;
                if (need < item.amount)
                {
                    item.amount -= need;
                    check.amount += need;
                    continue;
                }

                check.amount += item.amount;
                item.amount = 0;
                break;
            }
            if (item.amount > 0) inv.Add(item); 
        }

        private void MoveToFactory(Item item)
        {
            var max = item.MaxStackable();
            var inv = _data.inventory;
            var items = _data.inventory.Where(x => x.shortname == item.info.shortname).OrderBy(x => x.amount).ToList();
            if (items.Count == 0)
            {
                inv.Add(CreateFItem(item));
                item.DoRemove();
                return;
            }
            
            for (var i = 0; i < items.Count; i++)
            {
                if (item.amount == 0) break;
                var check = inv[inv.IndexOf(items[i])];
                if (check.amount >= max) continue;
                var need = max - check.amount;
                if (need < item.amount)
                {
                    item.amount -= need;
                    check.amount += need;
                    continue;
                }

                check.amount += item.amount;
                item.amount = 0;
                break;
            }
            if (item.amount > 0) inv.Add(CreateFItem(item));
            item.DoRemove();
        }

        private void AddItemCraft(Order order, ItemCraft craft)
        {
            order.crafts.Add(craft);
            order.startTime += craft.time;
            order.time += craft.time;
        }
        
        private FItem CreateFItem(Item item) => new FItem {shortname = item.info.shortname, amount = item.amount, skin = item.skin};

        private Dictionary<string, int> GetItems(BasePlayer player)
        {
            var list = new Dictionary<string, int>();
            var items = player.inventory.AllItems();
            for (var i = 0; i < items.Length; i++)
            {
                var check = items[i];
                var sh = check.info.shortname;
                if (list.ContainsKey(sh)) list[sh] += check.amount;
                else list.Add(sh, check.amount);
            }

            for (var i = 0; i < _data.inventory.Count; i++)
            {
                var check = _data.inventory[i];
                var sh = check.shortname;
                if (list.ContainsKey(sh)) list[sh] += check.amount;
                else list.Add(sh, check.amount);
            }
            return list;
        }

        private Dictionary<string, int> GetCost(Order order)
        {
            var list = new Dictionary<string, int>();
            for (var index = 0; index < order.crafts.Count; index++)
            {
                var check = order.crafts[index];
                for (var i = 0; i < check.cost.Count; i++)
                {
                    var item = check.cost[i];
                    var sh = item.shortname;
                    if (list.ContainsKey(sh)) list[sh] += item.amount;
                    else list.Add(sh, item.amount);
                }
            }

            return list;
        }

        private static string TimeString(int seconds)
        {
            var span = TimeSpan.FromSeconds(seconds);
            var time = string.Empty;
            time += span.Hours > 9 ? $"{span.Hours}:" : $"0{span.Hours}:";
            time += span.Minutes > 9 ? $"{span.Minutes}:" : $"0{span.Minutes}:";
            time += span.Seconds > 9 ? $"{span.Seconds}" : $"0{span.Seconds}";
            return time;
        }

        private void LoadOrders(BasePlayer player)
        {
            if (!creatingOrder.ContainsKey(player.userID)) creatingOrder.Add(player.userID, new Dictionary<string, Order>());
            foreach (var check in factories) creatingOrder[player.userID].Add(check.Key, check.Value.GetPlayerOrder(player.userID) ?? new Order {owner = player.userID});
        }
        
        private void UpdateOrders(BasePlayer player)
        {
            foreach (var check in factories)
            {
                var order = check.Value.GetPlayerOrder(player.userID);
                if (order == null) continue;
                creatingOrder[player.userID][check.Key] = order;
            }
        }

        private void SpawnBot()
        {
            foreach (var check in UnityEngine.Object.FindObjectsOfType<BasePlayer>()) if (check?._name == "fact_bot") check.Kill();
            var botSettings = _config.botSettings;
            if (botSettings.spawnPos == Vector3.zero) return;
            bot = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab") as BasePlayer;
            if (bot == null) return;
            bot._name = "fact_bot";
            bot.transform.position = botSettings.spawnPos;
            bot.Spawn();
            bot.eyes.rotation = new Quaternion(botSettings.rotation.x, botSettings.rotation.y, botSettings.rotation.z,botSettings.rotation.w);
            bot.viewAngles = bot.eyes.rotation.eulerAngles;
            bot.ServerRotation = bot.eyes.rotation;
            bot.enableSaving = false;
            bot.displayName = botSettings.botName;
            var panel = GameManager.server.CreateEntity("assets/prefabs/deployable/quarry/fuelstorage.prefab") as StorageContainer;
            if (panel == null) return;
            panel.Spawn();
            panel.SetParent(bot);
            panel.transform.localPosition = new Vector3(0, 0, 0);
            panel._name = "FACTORY";
            for (var i = 0; i < botSettings.items.Count; i++) ItemManager.CreateByName(botSettings.items[i]).MoveToContainer(bot.inventory.containerWear);
            bot.SendNetworkUpdateImmediate();
        }

        private static Item CreateItem(FItem fItem) => ItemManager.CreateByName(fItem.shortname, fItem.amount, fItem.skin);

        private void CreateFactory(List<Order> data, string category)
        {
            var factory = new GameObject().AddComponent<FactoryCraft>();
            factory.InitializeFactory(data, category);
            factories.Add(category, factory);
        }

        private class FactoryPlayer : FacepunchBehaviour
        {
            private BasePlayer player;
            private int page = 0;
            private Owner type = Owner.Personal;
            private bool update = false;

            private void Awake()
            {
                player = GetComponent<BasePlayer>();
                InvokeRepeating(UpdateInfo, 0f, 1f);
            }

            public void UpdateState(Owner type, int page = 0)
            {
                update = true;
                this.type = type;
                this.page = page;
            }

            public void Close() => update = false;

            private void UpdateInfo()
            {
                if (!update) return;
                var container = new CuiElementContainer();
                var y = -45;
                var skipAmount = page * 8;
                var takeAmount = 8;
                switch (type)
                {
                    case Owner.Personal:
                        _.UpdateOrders(player);
                        var orders = _.creatingOrder[player.userID];

                        foreach (var check in orders)
                        {
                            var key = check.Key;
                            var value = check.Value;
                            var fact = _.factories[key];
                            var isCrafting = fact.GetPlayerOrder(player.userID) != null;
                            container.Add(new CuiLabel
                            {
                                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                                Text =
                                {
                                    Text = TimeString(value.time), Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                                    Color = "0.78 0.78 0.78 1.00"
                                }
                            }, Layer + ".Queue" + y + ".craftTimePanel", Layer + ".Queue" + y + ".craftTime");

                            container.Add(new CuiLabel
                            {
                                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                                Text =
                                {
                                    Text = TimeString(fact.GetTimeForOrder(player.userID)), Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                                    Color = "0.36 0.36 0.27 1.00"
                                }
                            }, Layer + ".Queue" + y + ".queueTimePanel", Layer + ".Queue" + y + ".queueTime");
                            
                            container.Add(new CuiPanel
                            {
                                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -7", OffsetMax = $"{124 * ((check.Value.startTime - check.Value.time) / (float)check.Value.startTime)} 0"},
                                Image = {Color = "0.95 0.95 0.95 1"}
                            }, Layer + ".Queue" + y + ".progressPanel", Layer + ".Queue" + y + ".progress");
                            
                            var take = isCrafting && value.time <= 0;
                            var stop = isCrafting && value.time > 0;
                
                            if (stop || take)
                            {
                                container.Add(new CuiPanel
                                {
                                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "266 -26", OffsetMax = "282 -10"},
                                    Image = {Color = "1 1 1 1", Sprite = stop ? "assets/icons/vote_down.png" : "assets/icons/vote_up.png"}
                                }, Layer + ".Queue" + y, Layer + ".Queue" + y + ".sprite");

                                container.Add(new CuiButton
                                {
                                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "263 -29", OffsetMax = "285 -7"},
                                    Button = {Color = "0 0 0 0", Command = stop ? $"UI_FACTORY order {key} stop" : $"UI_FACTORY order {key} take 0",Close = Layer + ".Queue" + y + ".button"},
                                    Text =
                                    {
                                        Text = "", Font = "robotocondensed-bold.ttf", FontSize = 18, Align = TextAnchor.MiddleCenter,
                                        Color = "1 1 1 1"
                                    }
                                }, Layer + ".Queue" + y, Layer + ".Queue" + y + ".button");
                                CuiHelper.DestroyUi(player, Layer + ".Queue" + y + ".sprite");
                                CuiHelper.DestroyUi(player, Layer + ".Queue" + y + ".button");
                            }
                           
                            CuiHelper.DestroyUi(player, Layer + ".Queue" + y + ".progress");
                            CuiHelper.DestroyUi(player, Layer + ".Queue" + y + ".craftTime");
                            CuiHelper.DestroyUi(player, Layer + ".Queue" + y + ".queueTime");
                            y -= 55;
                        }
                        break;
                    case Owner.Team:
                        var teamList = player.Team == null ? new List<ulong>() : player.Team.members;
                        var orders1 = new List<KeyValuePair<string, Order>>();
                        if (teamList.Count != 0)
                            foreach (var check in _.factories)
                            {
                                if (takeAmount == 0) break;
                                foreach (var order in check.Value.GetPlayerTeamOrders(teamList))
                                {
                                    if (skipAmount > 0)
                                    {
                                        skipAmount--;
                                        continue;
                                    }

                                    orders1.Add(new KeyValuePair<string, Order>(check.Key, order));
                                    takeAmount++;
                                    if (takeAmount == 0) break;
                                }
                            }
                        
                        foreach (var check in orders1)
                        {
                            var key = check.Key;
                            container.Add(new CuiLabel
                            {
                                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                                Text =
                                {
                                    Text = TimeString(check.Value.time), Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                                    Color = "0.78 0.78 0.78 1.00"
                                }
                            }, Layer + ".Queue" + y + ".craftTimePanel", Layer + ".Queue" + y + ".craftTime");

                            container.Add(new CuiLabel
                            {
                                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                                Text =
                                {
                                    Text = TimeString(_.factories[key].GetTimeForOrder(player.userID)), Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                                    Color = "0.36 0.36 0.27 1.00"
                                }
                            }, Layer + ".Queue" + y + ".queueTimePanel", Layer + ".Queue" + y + ".queueTime");
                            
                            container.Add(new CuiPanel
                            {
                                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -7", OffsetMax = $"{124 * ((check.Value.startTime - check.Value.time) / (float)check.Value.startTime)} 0"},
                                Image = {Color = "0.95 0.95 0.95 1"}
                            }, Layer + ".Queue" + y + ".progressPanel", Layer + ".Queue" + y + ".progress");
                            
                            if (check.Value.time <= 0)
                            {
                                container.Add(new CuiPanel
                                {
                                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "266 -26", OffsetMax = "282 -10"},
                                    Image = {Color = "1 1 1 1", Sprite = "assets/icons/vote_up.png"}
                                }, Layer + ".Queue" + y);

                                container.Add(new CuiButton
                                {
                                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "263 -29", OffsetMax = "285 -7"},
                                    Button = {Color = "0 0 0 0", Command = $"UI_FACTORY order {key} take 1",Close = Layer + ".Queue" + y + ".button"},
                                    Text =
                                    {
                                        Text = "", Font = "robotocondensed-bold.ttf", FontSize = 18, Align = TextAnchor.MiddleCenter,
                                        Color = "1 1 1 1"
                                    }
                                }, Layer + ".Queue" + y);
                                CuiHelper.DestroyUi(player, Layer + ".Queue" + y + ".sprite");
                                CuiHelper.DestroyUi(player, Layer + ".Queue" + y + ".button");
                            }
                            
                            CuiHelper.DestroyUi(player, Layer + ".Queue" + y + ".progress");
                            CuiHelper.DestroyUi(player, Layer + ".Queue" + y + ".craftTime");
                            CuiHelper.DestroyUi(player, Layer + ".Queue" + y + ".queueTime");
                            y -= 55;
                        }
                        break;
                    case Owner.Public:
                        var orders2 = new List<KeyValuePair<string, Order>>();
                            foreach (var check in _.factories)
                            {
                                if (takeAmount == 0) break;
                                foreach (var order in check.Value.GetPublicOrders())
                                {
                                    if (skipAmount > 0)
                                    {
                                        skipAmount--;
                                        continue;
                                    }

                                    orders2.Add(new KeyValuePair<string, Order>(check.Key, order));
                                    takeAmount++;
                                    if (takeAmount == 0) break;
                                }
                            }
                        
                        foreach (var check in orders2)
                        {
                            var key = check.Key;
                            container.Add(new CuiLabel
                            {
                                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                                Text =
                                {
                                    Text = TimeString(check.Value.time), Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                                    Color = "0.78 0.78 0.78 1.00"
                                }
                            }, Layer + ".Queue" + y + ".craftTimePanel", Layer + ".Queue" + y + ".craftTime");

                            container.Add(new CuiLabel
                            {
                                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                                Text =
                                {
                                    Text = TimeString(_.factories[key].GetTimeForOrder(player.userID)), Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                                    Color = "0.36 0.36 0.27 1.00"
                                }
                            }, Layer + ".Queue" + y + ".queueTimePanel", Layer + ".Queue" + y + ".queueTime");
                            
                            container.Add(new CuiPanel
                            {
                                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -7", OffsetMax = $"{124 * ((check.Value.startTime - check.Value.time) / (float)check.Value.startTime)} 0"},
                                Image = {Color = "0.95 0.95 0.95 1"}
                            }, Layer + ".Queue" + y + ".progressPanel", Layer + ".Queue" + y + ".progress");

                            if (check.Value.time <= 0)
                            {
                                container.Add(new CuiPanel
                                {
                                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "266 -26", OffsetMax = "282 -10"},
                                    Image = {Color = "1 1 1 1", Sprite = "assets/icons/vote_up.png"}
                                }, Layer + ".Queue" + y);

                                container.Add(new CuiButton
                                {
                                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "263 -29", OffsetMax = "285 -7"},
                                    Button = {Color = "0 0 0 0", Command = $"UI_FACTORY order {key} take 2",Close = Layer + ".Queue" + y + ".button"},
                                    Text =
                                    {
                                        Text = "", Font = "robotocondensed-bold.ttf", FontSize = 18, Align = TextAnchor.MiddleCenter,
                                        Color = "1 1 1 1"
                                    }
                                }, Layer + ".Queue" + y);
                                CuiHelper.DestroyUi(player, Layer + ".Queue" + y + ".sprite");
                                CuiHelper.DestroyUi(player, Layer + ".Queue" + y + ".button");
                            }
                           
                            CuiHelper.DestroyUi(player, Layer + ".Queue" + y + ".progress");
                            CuiHelper.DestroyUi(player, Layer + ".Queue" + y + ".craftTime");
                            CuiHelper.DestroyUi(player, Layer + ".Queue" + y + ".queueTime");
                            y -= 55;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                CuiHelper.AddUi(player, container);
            }

            public void Kill()
            {
                CancelInvoke(UpdateInfo);
                Destroy(this);
            }
        }

        private class FactoryCraft : FacepunchBehaviour
        {
            private List<Order> orders = new List<Order>();
            private string category = string.Empty;
            
            public void InitializeFactory(List<Order> list, string category)
            {
                orders = list;
                this.category = category;
                InvokeRepeating(Craft, 0f, 1f);
            }

            public List<Order> GetPublicOrders()
            {
                var list = new List<Order>();
                foreach (var check in orders) if (check.type == Owner.Public) list.Add(check);
                return list;
            }

            public List<Order> GetPlayerTeamOrders(List<ulong> members)
            {
                var list = new List<Order>();
                foreach (var check in orders) if (check.type == Owner.Team && members.Contains(check.owner)) list.Add(check);
                return list;
            }

            public void ChangeOrderType(ulong id, Owner type)
            {
                for (var i = 0; i < orders.Count; i++)
                {
                    if (orders[i].owner != id) continue;
                    orders[i].type = type;
                }
            }

            public int GetTimeForOrder(ulong id)
            {
                var time = 0;

                for (var i = 0; i < orders.Count; i++)
                {
                    var a = orders[i];
                    if (a.owner == id) break;
                    time += a.time;
                }
                
                return time;
            }

            public void RemoveOrder(ulong id)
            {
                for (var i = 0; i < orders.Count; i++)
                {
                    var order = orders[i];
                    if (order.owner != id) continue;
                    order.time = order.startTime;
                    orders.Remove(order);
                }
            }

            public Order GetPlayerOrder(ulong id)
            {
                for (var i = 0; i < orders.Count; i++)
                {
                    var a = orders[i];
                    if (a.owner == id) return a;
                }
                return null;
            }

            public void AddOrder(Order order)
            {
                if (!IsInvoking(Craft)) InvokeRepeating(Craft, 0f, 1f);
                orders.Add(order);
            }
            
            private void Craft()
            {
                for (var i = 0; i < orders.Count; i++)
                {
                    var order = orders[i];
                    if (order.time <= 0)
                    {
                        if (order.type == Owner.Public) continue;
                        order.waitTime++;
                        if (order.waitTime < 21600) continue;
                        order.type = Owner.Public;
                        continue;
                    }
                    order.time--;
                    return;
                }
                CancelInvoke(Craft);
            }

            private void Save() => _._data.factoryData[category] = orders;
            
            public void Kill()
            {
                CancelInvoke(Craft);
                Save();
                Destroy(gameObject);
            }
        }

        #endregion

        #region UI
        
        private void ShowUIQueuePersonal(BasePlayer player)
        {
            var crafts = _config.crafts;
            var y = -45;
            UpdateOrders(player);
            var orders = creatingOrder[player.userID];
            var container = new CuiElementContainer();
            var inventory = GetItems(player);
            
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "130 -226", OffsetMax = "428 261"},
                Image = {Color = "0 0 0 0.98"}
            }, Layer + ".bg", Layer + ".Queue");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMax = "299 25"},
                Text =
                {
                    Text = "ФАБРИКА", Font = "robotocondensed-regular.ttf", FontSize = 20, Align = TextAnchor.MiddleLeft,
                    Color = "0.82 0.82 0.87 1.00"
                }
            }, Layer + ".Queue");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -21", OffsetMax = "299 0"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".Queue");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "8 -21", OffsetMax = "299 0"},
                Text =
                {
                    Text = "ОЧЕРЕДИ ПРОИЗВОДСТВА", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".Queue");
            
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -42", OffsetMax = "299 -21"},
                Image = {Color = "0.21 0.21 0.21 1.00"}
            }, Layer + ".Queue");

            container.Add(new CuiElement
            {
                Parent = Layer + ".Queue",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/LgLLwhv.png"), Color = "0.92 0.49 0.24 1.00"},
                    new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -42", OffsetMax = "21 -21"}
                }
            });

            container.Add(new CuiElement
            {
                Parent = Layer + ".Queue",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/nHSedyg.png")},
                    new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "21 -42", OffsetMax = "42 -21"}
                }
            });

            container.Add(new CuiElement
            {
                Parent = Layer + ".Queue",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/QVtAvvL.png")},
                    new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "42 -42", OffsetMax = "63 -21"}
                }
            });

            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "21 -42", OffsetMax = "42 -21"},
                Button = {Color = "0 0 0 0", Command = "UI_FACTORY queue 1"},
                Text = {Text = ""}
            }, Layer + ".Queue");
            
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "42 -42", OffsetMax = "63 -21"},
                Button = {Color = "0 0 0 0", Command = "UI_FACTORY queue 2"},
                Text = {Text = ""}
            }, Layer + ".Queue");

            foreach (var check in orders)
            {
                var x = 65;
                var z = 253;
                var category = check.Key;
                var order = check.Value;
                var cost = GetCost(check.Value);
                var enough = 0;
                var fact = factories[category];
                var isCrafting = fact.GetPlayerOrder(player.userID) != null;
                
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"4 {y - 52}", OffsetMax = $"294 {y}"},
                    Image = {Color = "0.15 0.15 0.15 1.00"}
                }, Layer + ".Queue", Layer + ".Queue" + y);

                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -52", OffsetMax = "290 -35"},
                    Image = {Color = "0.26 0.26 0.26 1.00"}
                }, Layer + ".Queue" + y);

                container.Add(new CuiElement
                {
                    Parent = Layer + ".Queue" + y,
                    Components =
                    {
                        new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", crafts[category].icon)},
                        new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "5 -28", OffsetMax = "25 -8"}
                    }
                });

                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "29 -19", OffsetMax = "84 -6"},
                    Text =
                    {
                        Text = category, Font = "robotocondensed-regular.ttf", FontSize = 9, Align = TextAnchor.MiddleLeft,
                        Color = "0.78 0.78 0.78 1.00"
                    }
                }, Layer + ".Queue" + y);

                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "29 -33", OffsetMax = "80 -22"},
                    Button = {Color = "0.26 0.26 0.26 1.00", Command = $"UI_FACTORY order {check.Key} switchtype {y} 0"},
                    Text =
                    {
                        Text = order.type == Owner.Personal ? "Personal" : order.type == Owner.Team ? "Team" : "Public", Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                        Color = "0.78 0.78 0.78 1.00"
                    }
                }, Layer + ".Queue" + y);

                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "2 -50", OffsetMax = "44 -37"},
                    Image = {Color = "0.15 0.15 0.15 1.00"}
                }, Layer + ".Queue" + y, Layer + ".Queue" + y + ".craftTimePanel");

                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                    Text =
                    {
                        Text = TimeString(order.time), Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                        Color = "0.78 0.78 0.78 1.00"
                    }
                }, Layer + ".Queue" + y + ".craftTimePanel", Layer + ".Queue" + y + ".craftTime");
                
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "209 -34", OffsetMax = "253 -21"},
                    Image = {Color = "0 0 0 0"}
                }, Layer + ".Queue" + y, Layer + ".Queue" + y + ".queueTimePanel");

                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                    Text =
                    {
                        Text = TimeString(fact.GetTimeForOrder(player.userID)), Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                        Color = "0.36 0.36 0.27 1.00"
                    }
                }, Layer + ".Queue" + y + ".queueTimePanel", Layer + ".Queue" + y + ".queueTime");

                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "82 -31", OffsetMax = "206 -24"},
                    Image = {Color = "0.22 0.22 0.22 1.00"}
                }, Layer + ".Queue" + y,  Layer + ".Queue" + y + ".progressPanel");
                
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -7", OffsetMax = $"{124 * ((check.Value.startTime - check.Value.time) / (float)check.Value.startTime)} 0"},
                    Image = {Color = "0.95 0.95 0.95 1"}
                }, Layer + ".Queue" + y + ".progressPanel", Layer + ".Queue" + y + ".progress");

                for (var i = 0; i < order.crafts.Count; i++)
                {
                    var result = order.crafts[i];
                    var a = z - 16;
                    container.Add(new CuiElement
                    {
                        Parent = Layer + ".Queue" + y,
                        Components =
                        {
                            new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", result.result.shortname)},
                            new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"{a} -20", OffsetMax = $"{z} -3"}
                        }
                    });
                    
                    if (!isCrafting)
                        container.Add(new CuiButton
                        {
                            RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"{a} -20", OffsetMax = $"{z} -3"},
                            Button = {Color = "0 0 0 0", Command = $"UI_FACTORY order {check.Key} removeitem {i}",Close = Layer + ".Queue" + y},
                            Text = {Text = ""}
                        }, Layer + ".Queue" + y);

                    z -= 17;
                }

                foreach (var item in cost)
                {
                    var a = x + 13;
                    var b = x + 50;
                    var amount = inventory.ContainsKey(item.Key) ? inventory[item.Key] : 0;
                    container.Add(new CuiElement
                    {
                        Parent = Layer + ".Queue" + y,
                        Components =
                        {
                            new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", item.Key)},
                            new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"{x} -50", OffsetMax = $"{x + 13} -37"}
                        }
                    });

                    container.Add(new CuiPanel
                    {
                        RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"{a} -50", OffsetMax = $"{b} -37"},
                        Image = {Color = "0.15 0.15 0.15 1.00"}
                    }, Layer + ".Queue" + y);

                    container.Add(new CuiLabel
                    {
                        RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"{a} -50", OffsetMax = $"{b} -37"},
                        Text =
                        {
                            Text = $"{amount}/{item.Value}", Font = "robotocondensed-regular.ttf", FontSize = 8, Align = TextAnchor.MiddleCenter,
                            Color = "0.78 0.78 0.78 1.00"
                        }
                    }, Layer + ".Queue" + y);
                    if (amount >= item.Value) enough++;
                    x += 50;
                    if (x >= 215) break;
                }

                var take = isCrafting && order.time <= 0;
                var stop = isCrafting && order.time > 0;
                var start = !isCrafting && cost.Count > 0 && enough == cost.Count;
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "263 -29", OffsetMax = "285 -7"},
                    Image = {Color = "0.26 0.26 0.26 1.00"}
                }, Layer + ".Queue" + y);
                
                if (start || stop || take)
                {
                    container.Add(new CuiPanel
                    {
                        RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "266 -26", OffsetMax = "282 -10"},
                        Image = {Color = "1 1 1 1", Sprite = start ? "assets/icons/maximum.png" : stop ? "assets/icons/vote_down.png" : "assets/icons/vote_up.png"}
                    }, Layer + ".Queue" + y, Layer + ".Queue" + y + ".sprite");

                    container.Add(new CuiButton
                    {
                        RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "263 -29", OffsetMax = "285 -7"},
                        Button = {Color = "0 0 0 0",Close = Layer + ".Queue" + y + ".button", Command = start ? $"UI_FACTORY order {category} start" : stop ? $"UI_FACTORY order {category} stop" : $"UI_FACTORY order {category} take 0"},
                        Text =
                        {
                            Text = "", Font = "robotocondensed-bold.ttf", FontSize = 18, Align = TextAnchor.MiddleCenter,
                            Color = "1 1 1 1"
                        }
                    }, Layer + ".Queue" + y, Layer + ".Queue" + y + ".button");
                }
                
                y -= 55;
            }

            CuiHelper.DestroyUi(player, Layer + ".Queue");
            CuiHelper.AddUi(player, container);
        }
        
        private void ShowUIPublicQueue(BasePlayer player, int page)
        {
            var crafts = _config.crafts;
            var y = -45;
            var container = new CuiElementContainer();
            var skipAmount = page * 8;
            var takeAmount = 8;
            var orders = new List<KeyValuePair<string, Order>>();
                foreach (var check in factories)
                {
                    if (takeAmount == 0) break;
                    foreach (var order in check.Value.GetPublicOrders())
                    {
                        if (skipAmount > 0)
                        {
                            skipAmount--;
                            continue;
                        }

                        orders.Add(new KeyValuePair<string, Order>(check.Key, order));
                        takeAmount++;
                        if (takeAmount == 0) break;
                    }
                }
            
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "130 -226", OffsetMax = "428 261"},
                Image = {Color = "0 0 0 0.98"}
            }, Layer + ".bg", Layer + ".Queue");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMax = "299 25"},
                Text =
                {
                    Text = "ФАБРИКА", Font = "robotocondensed-regular.ttf", FontSize = 20, Align = TextAnchor.MiddleLeft,
                    Color = "0.82 0.82 0.87 1.00"
                }
            }, Layer + ".Queue");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -21", OffsetMax = "299 0"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".Queue");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "8 -21", OffsetMax = "299 0"},
                Text =
                {
                    Text = "ОЧЕРЕДИ ПРОИЗВОДСТВА", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".Queue");
            
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -42", OffsetMax = "299 -21"},
                Image = {Color = "0.21 0.21 0.21 1.00"}
            }, Layer + ".Queue");

            container.Add(new CuiElement
            {
                Parent = Layer + ".Queue",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/LgLLwhv.png")},
                    new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -42", OffsetMax = "21 -21"}
                }
            });

            container.Add(new CuiElement
            {
                Parent = Layer + ".Queue",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/nHSedyg.png")},
                    new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "21 -42", OffsetMax = "42 -21"}
                }
            });

            container.Add(new CuiElement
            {
                Parent = Layer + ".Queue",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/QVtAvvL.png"), Color = "0.92 0.49 0.24 1.00"},
                    new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "42 -42", OffsetMax = "63 -21"}
                }
            });

            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "21 -42", OffsetMax = "42 -21"},
                Button = {Color = "0 0 0 0", Command = "UI_FACTORY queue 1"},
                Text = {Text = ""}
            }, Layer + ".Queue");
            
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -42", OffsetMax = "21 -21"},
                Button = {Color = "0 0 0 0", Command = "UI_FACTORY queue 0"},
                Text = {Text = ""}
            }, Layer + ".Queue");

            foreach (var check in orders)
            {
                var x = 65;
                var z = 253;
                var category = check.Key;
                var order = check.Value;
                var isMy = order.owner == player.userID;
                
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"4 {y - 52}", OffsetMax = $"294 {y}"},
                    Image = {Color = "0.15 0.15 0.15 1.00"}
                }, Layer + ".Queue", Layer + ".Queue" + y);

                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -52", OffsetMax = "290 -35"},
                    Image = {Color = "0.26 0.26 0.26 1.00"}
                }, Layer + ".Queue" + y);

                container.Add(new CuiElement
                {
                    Parent = Layer + ".Queue" + y,
                    Components =
                    {
                        new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", crafts[category].icon)},
                        new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "5 -28", OffsetMax = "25 -8"}
                    }
                });

                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "29 -19", OffsetMax = "84 -6"},
                    Text =
                    {
                        Text = category, Font = "robotocondensed-regular.ttf", FontSize = 9, Align = TextAnchor.MiddleLeft,
                        Color = "0.78 0.78 0.78 1.00"
                    }
                }, Layer + ".Queue" + y);

                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "29 -33", OffsetMax = "80 -22"},
                    Button = {Color = "0.26 0.26 0.26 1.00", Command = isMy ? $"UI_FACTORY order {check.Key} switchtype {y} 0" : ""},
                    Text =
                    {
                        Text = "Public", Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                        Color = "0.78 0.78 0.78 1.00"
                    }
                }, Layer + ".Queue" + y);

                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "2 -50", OffsetMax = "44 -37"},
                    Image = {Color = "0.15 0.15 0.15 1.00"}
                }, Layer + ".Queue" + y, Layer + ".Queue" + y + ".craftTimePanel");

                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                    Text =
                    {
                        Text = TimeString(order.time), Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                        Color = "0.78 0.78 0.78 1.00"
                    }
                }, Layer + ".Queue" + y + ".craftTimePanel", Layer + ".Queue" + y + ".craftTime");
                
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "209 -34", OffsetMax = "253 -21"},
                    Image = {Color = "0 0 0 0"}
                }, Layer + ".Queue" + y, Layer + ".Queue" + y + ".queueTimePanel");

                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                    Text =
                    {
                        Text = TimeString(factories[category].GetTimeForOrder(player.userID)), Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                        Color = "0.36 0.36 0.27 1.00"
                    }
                }, Layer + ".Queue" + y + ".queueTimePanel", Layer + ".Queue" + y + ".queueTime");

                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "82 -31", OffsetMax = "206 -24"},
                    Image = {Color = "0.22 0.22 0.22 1.00"}
                }, Layer + ".Queue" + y,  Layer + ".Queue" + y + ".progressPanel");
                
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -7", OffsetMax = $"{124 * ((check.Value.startTime - check.Value.time) / (float)check.Value.startTime)} 0"},
                    Image = {Color = "0.95 0.95 0.95 1"}
                }, Layer + ".Queue" + y + ".progressPanel", Layer + ".Queue" + y + ".progress");

                foreach (var result in check.Value.crafts)
                {
                    var a = z - 16;
                    container.Add(new CuiElement
                    {
                        Parent = Layer + ".Queue" + y,
                        Components =
                        {
                            new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", result.result.shortname)},
                            new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"{a} -20", OffsetMax = $"{z} -3"}
                        }
                    });

                    z -= 17;
                }
             
                var take = order.time <= 0;

                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "263 -29", OffsetMax = "285 -7"},
                    Image = {Color = "0.26 0.26 0.26 1.00"}
                }, Layer + ".Queue" + y);
                
                if (take)
                {
                    container.Add(new CuiPanel
                    {
                        RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "266 -26", OffsetMax = "282 -10"},
                        Image = {Color = "1 1 1 1", Sprite = "assets/icons/vote_up.png"}
                    }, Layer + ".Queue" + y, Layer + ".Queue" + y + ".sprite");

                    container.Add(new CuiButton
                    {
                        RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "263 -29", OffsetMax = "285 -7"},
                        Button = {Color = "0 0 0 0", Command = $"UI_FACTORY order {category} take 2",Close = Layer + ".Queue" + y + ".button"},
                        Text =
                        {
                            Text = "", Font = "robotocondensed-bold.ttf", FontSize = 18, Align = TextAnchor.MiddleCenter,
                            Color = "1 1 1 1"
                        }
                    }, Layer + ".Queue" + y, Layer + ".Queue" + y + ".button");
                }

                y -= 55;
            }

            CuiHelper.DestroyUi(player, Layer + ".Queue");
            CuiHelper.AddUi(player, container);
        }

        private void ShowUITeamQueue(BasePlayer player, int page)
        {
            var crafts = _config.crafts;
            var y = -45;
            var container = new CuiElementContainer();
            var skipAmount = page * 8;
            var takeAmount = 8;
            var teamList = player.Team == null ? new List<ulong>() : player.Team.members;
            var orders = new List<KeyValuePair<string, Order>>();
            if (teamList.Count != 0)
                foreach (var check in factories)
                {
                    if (takeAmount == 0) break;
                    foreach (var order in check.Value.GetPlayerTeamOrders(teamList))
                    {
                        if (skipAmount > 0)
                        {
                            skipAmount--;
                            continue;
                        }

                        orders.Add(new KeyValuePair<string, Order>(check.Key, order));
                        takeAmount++;
                        if (takeAmount == 0) break;
                    }
                }
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "130 -226", OffsetMax = "428 261"},
                Image = {Color = "0 0 0 0.98"}
            }, Layer + ".bg", Layer + ".Queue");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMax = "299 25"},
                Text =
                {
                    Text = "ФАБРИКА", Font = "robotocondensed-regular.ttf", FontSize = 20, Align = TextAnchor.MiddleLeft,
                    Color = "0.82 0.82 0.87 1.00"
                }
            }, Layer + ".Queue");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -21", OffsetMax = "299 0"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".Queue");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "8 -21", OffsetMax = "299 0"},
                Text =
                {
                    Text = "ОЧЕРЕДИ ПРОИЗВОДСТВА", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".Queue");
            
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -42", OffsetMax = "299 -21"},
                Image = {Color = "0.21 0.21 0.21 1.00"}
            }, Layer + ".Queue");

            container.Add(new CuiElement
            {
                Parent = Layer + ".Queue",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/LgLLwhv.png")},
                    new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -42", OffsetMax = "21 -21"}
                }
            });

            container.Add(new CuiElement
            {
                Parent = Layer + ".Queue",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/nHSedyg.png"), Color = "0.92 0.49 0.24 1.00"},
                    new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "21 -42", OffsetMax = "42 -21"}
                }
            });

            container.Add(new CuiElement
            {
                Parent = Layer + ".Queue",
                Components =
                {
                    new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", "https://i.imgur.com/QVtAvvL.png")},
                    new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "42 -42", OffsetMax = "63 -21"}
                }
            });

            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -42", OffsetMax = "21 -21"},
                Button = {Color = "0 0 0 0", Command = "UI_FACTORY queue 0"},
                Text = {Text = ""}
            }, Layer + ".Queue");
            
            container.Add(new CuiButton
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "42 -42", OffsetMax = "63 -21"},
                Button = {Color = "0 0 0 0", Command = "UI_FACTORY queue 2"},
                Text = {Text = ""}
            }, Layer + ".Queue");

            foreach (var check in orders)
            {
                var z = 253;
                var category = check.Key;
                var order = check.Value;
                var isMy = check.Value.owner == player.userID;
                
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"4 {y - 52}", OffsetMax = $"294 {y}"},
                    Image = {Color = "0.15 0.15 0.15 1.00"}
                }, Layer + ".Queue", Layer + ".Queue" + y);

                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -52", OffsetMax = "290 -35"},
                    Image = {Color = "0.26 0.26 0.26 1.00"}
                }, Layer + ".Queue" + y);

                container.Add(new CuiElement
                {
                    Parent = Layer + ".Queue" + y,
                    Components =
                    {
                        new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", crafts[category].icon)},
                        new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "5 -28", OffsetMax = "25 -8"}
                    }
                });

                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "29 -19", OffsetMax = "84 -6"},
                    Text =
                    {
                        Text = category, Font = "robotocondensed-regular.ttf", FontSize = 9, Align = TextAnchor.MiddleLeft,
                        Color = "0.78 0.78 0.78 1.00"
                    }
                }, Layer + ".Queue" + y);

                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "29 -33", OffsetMax = "80 -22"},
                    Button = {Color = "0.26 0.26 0.26 1.00", Command = isMy ? $"UI_FACTORY order {check.Key} switchtype {y} 0" : ""},
                    Text =
                    {
                        Text = "Team", Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                        Color = "0.78 0.78 0.78 1.00"
                    }
                }, Layer + ".Queue" + y);

                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "2 -50", OffsetMax = "44 -37"},
                    Image = {Color = "0.15 0.15 0.15 1.00"}
                }, Layer + ".Queue" + y, Layer + ".Queue" + y + ".craftTimePanel");

                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                    Text =
                    {
                        Text = TimeString(order.time), Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                        Color = "0.78 0.78 0.78 1.00"
                    }
                }, Layer + ".Queue" + y + ".craftTimePanel", Layer + ".Queue" + y + ".craftTime");
                
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "209 -34", OffsetMax = "253 -21"},
                    Image = {Color = "0 0 0 0"}
                }, Layer + ".Queue" + y, Layer + ".Queue" + y + ".queueTimePanel");

                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                    Text =
                    {
                        Text = TimeString(factories[category].GetTimeForOrder(player.userID)), Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                        Color = "0.36 0.36 0.27 1.00"
                    }
                }, Layer + ".Queue" + y + ".queueTimePanel", Layer + ".Queue" + y + ".queueTime");

                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "82 -31", OffsetMax = "206 -24"},
                    Image = {Color = "0.22 0.22 0.22 1.00"}
                }, Layer + ".Queue" + y,  Layer + ".Queue" + y + ".progressPanel");
                
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -7", OffsetMax = $"{124 * ((check.Value.startTime - check.Value.time) / (float)check.Value.startTime)} 0"},
                    Image = {Color = "0.95 0.95 0.95 1"}
                }, Layer + ".Queue" + y + ".progressPanel", Layer + ".Queue" + y + ".progress");

                foreach (var result in check.Value.crafts)
                {
                    var a = z - 16;
                    container.Add(new CuiElement
                    {
                        Parent = Layer + ".Queue" + y,
                        Components =
                        {
                            new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", result.result.shortname)},
                            new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"{a} -20", OffsetMax = $"{z} -3"}
                        }
                    });
                    
                    z -= 17;
                }

                var take = order.time <= 0;

                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "263 -29", OffsetMax = "285 -7"},
                    Image = {Color = "0.26 0.26 0.26 1.00"}
                }, Layer + ".Queue" + y);
                
                if (take)
                {
                    container.Add(new CuiPanel
                    {
                        RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "266 -26", OffsetMax = "282 -10"},
                        Image = {Color = "1 1 1 1", Sprite = "assets/icons/vote_up.png"}
                    }, Layer + ".Queue" + y, Layer + ".Queue" + y + ".sprite");

                    container.Add(new CuiButton
                    {
                        RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "263 -29", OffsetMax = "285 -7"},
                        Button = {Color = "0 0 0 0",Close = Layer + ".Queue" + y + ".button", Command = $"UI_FACTORY order {category} take 1"},
                        Text =
                        {
                            Text = "", Font = "robotocondensed-bold.ttf", FontSize = 18, Align = TextAnchor.MiddleCenter,
                            Color = "1 1 1 1"
                        }
                    }, Layer + ".Queue" + y, Layer + ".Queue" + y + ".button");
                }
                
                y -= 55;
            }

            CuiHelper.DestroyUi(player, Layer + ".Queue");
            CuiHelper.AddUi(player, container);
        }

        private void ShowUICraftCategory(BasePlayer player, string category)
        {
            var catalog = _config.crafts;
            var container = new CuiElementContainer();
            var bluePrints = player.blueprints;
            int posX = 0, posY = -46;
            
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-175 -226", OffsetMax = "119 261"},
                Image = {Color = "0 0 0 0.98"}
            }, Layer + ".bg", Layer + ".CraftCategory");
            
            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMax = "294 25"},
                Text =
                {
                    Text = "ПРОИЗВОДСТВО", Font = "robotocondensed-regular.ttf", FontSize = 20, Align = TextAnchor.MiddleLeft,
                    Color = "0.82 0.82 0.87 1.00"
                }
            }, Layer + ".CraftCategory");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -21", OffsetMax = "294 0"},
                Image = {Color = "0.45 0.45 0.45 1.00"}
            }, Layer + ".CraftCategory");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "8 -21", OffsetMax = "294 0"},
                Text =
                {
                    Text = "РЕЦЕПТЫ", Font = "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "0.85 0.85 0.85 1.00"
                }
            }, Layer + ".CraftCategory");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -42", OffsetMax = "294 -21"},
                Image = {Color = "0.21 0.21 0.21 1.00"}
            }, Layer + ".CraftCategory");

            foreach (var check in catalog)
            {
                var thisCategory = check.Key == category;
                
                container.Add(new CuiElement
                {
                    Parent = Layer + ".CraftCategory",
                    Name = Layer + ".CraftCategory" + posX,
                    Components =
                    {
                        new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", check.Value.icon), Color = thisCategory ? "0.92 0.49 0.24 1.00" : "1 1 1 1"},
                        new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"{posX} -42", OffsetMax = $"{posX + 21} -21"}
                    }
                });
                
                if (!thisCategory)
                    container.Add(new CuiButton
                    {
                        RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMax = "21 21"},
                        Button = {Color = "0 0 0 0", Command = $"UI_FACTORY open {check.Key}"},
                        Text = {Text = ""}
                    }, Layer + ".CraftCategory" + posX);
                posX += 21;
            }
            posX = 4;
            for (var i = 0; i < catalog[category].items.Count; i++)
            {
                var item = catalog[category].items[i].result;
                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = $"{posX} {posY - 46}", OffsetMax = $"{posX + 46} {posY}"},
                    Image = {Color = "0.21 0.21 0.21 1.00"}
                }, Layer + ".CraftCategory", Layer + ".CraftCategory" + posX + posY);

                container.Add(new CuiElement
                {
                    Parent = Layer + ".CraftCategory" + posX + posY,
                    Components =
                    {
                        new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", item.shortname)},
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
                }, Layer + ".CraftCategory" + posX + posY);
                PrintWarning(item.shortname);
                if (bluePrints.HasUnlocked(ItemManager.FindItemDefinition(item.shortname)))
                    container.Add(new CuiButton
                    {
                        RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMax = "46 46"},
                        Button = {Color = "0 0 0 0", Command = $"UI_FACTORY addcraft {category} {i}"},
                        Text = {Text = ""}
                    }, Layer + ".CraftCategory" + posX + posY);
                else
                    container.Add(new CuiPanel
                    {
                        RectTransform = {AnchorMin = "0 0", AnchorMax = "0 0", OffsetMax = "46 46"},
                        Image = {Color = "0 0 0 0.8"}
                    }, Layer + ".CraftCategory" + posX + posY);
                    

                posX += 48;
                if (posX < 290) continue;
                posX = 4;
                posY -= 48;
            }

            CuiHelper.DestroyUi(player, Layer + ".CraftCategory");
            CuiHelper.AddUi(player, container);
        }
        
        private void ShowUIInventory(BasePlayer player, int page = 0)
        {
            var container = new CuiElementContainer();
            var inventory = _data.inventory;
            int posX = 4, posY = -25;
            var length = inventory.Count;
            var items = player.inventory.AllItems();
            
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-431 -226", OffsetMax = "-185 261"},
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
                    Image = {Color =  i < length ? "0.25 0.25 0.25 1.00" : "0.11 0.11 0.12 0.9"}
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
                    Image = {Color =  i < length ? "0.25 0.25 0.25 1.00" : "0.11 0.11 0.12 0.9"}
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
                Button = {Color = "0 0 0 0", Command = "UI_FACTORY close", Close = Layer + ".bg"},
                Text = {Text = ""}
            }, Layer + ".bg");

            CuiHelper.DestroyUi(player, Layer + ".bg");
            CuiHelper.AddUi(player, container);
        }
        
        #endregion
    }
}