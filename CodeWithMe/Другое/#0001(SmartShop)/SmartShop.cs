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
    [Info("SmartShop", "NO_NAME", "1.0.0")]
    internal class SmartShop : RustPlugin
    {
        #region Static

        private const string Layer = "UI_SMARTSHOP_LAYER";
        private int ImageLibraryCheck = 0;
        private Configuration _config;
        private Data data;
        [PluginReference] private Plugin ImageLibrary, Economics;
        private CuiRectTransformComponent resolutionBox = new CuiRectTransformComponent{AnchorMin = "0.213 0.459", AnchorMax = "0.787 0.956"};
        private CuiRectTransformComponent resolutionRectangle = new CuiRectTransformComponent{AnchorMin = "0.059 0.459", AnchorMax = "0.941 0.956"};
        #endregion

        #region Config

        private class Configuration
        {
            [JsonProperty(PropertyName = "General settings")]
            public BaseSettings Settings = new BaseSettings
            {
                smartAnalyseBuy = true,
                smartAnalyseSell = true,
                maxMarkUp = 15,
                minTrades = 100,
                useEffects = true,
                rankSystem = true,
                economics = new EconomicSettings
                {
                    useEconomics = false,
                    name = "Economics",
                    balance = "Balance",
                    deposit = "Deposit",
                    withdraw = "Withdraw"
                }
            };

            [JsonProperty(PropertyName = "Donator discounts", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<string, int> discounts = new Dictionary<string, int>
            {
                ["smartshop.vip"] = 10,
                ["smartshop.premium"] = 15
            };

            [JsonProperty(PropertyName = "Ranks", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<int, Rank> Ranks = new Dictionary<int, Rank>
            {
                [1] = new Rank
                {
                    needTrades = 0,
                    discount = 0
                },
                [2] = new Rank
                {
                    needTrades = 125,
                    discount = 5
                },
                [3] = new Rank
                {
                    needTrades = 250,
                    discount = 10
                },
                [4] = new Rank
                {
                    needTrades = 500,
                    discount = 15
                },
                [5] = new Rank
                {
                    needTrades = 1000,
                    discount = 20
                }
            };

            [JsonProperty(PropertyName = "Shop", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<ShopCategory> shop = new List<ShopCategory>
            {
                new ShopCategory
                {
                    name = "WEAPONS",
                    permission = "",
                    products = new List<Product>
                    {
                        new Product
                        {
                            displayName = "Assault Rifle",
                            id = 0,
                            useBackground = true,
                            resolution = true,
                            url = "",
                            type = true,
                            price = 1500,
                            productItem = new SItem
                            {
                                shortname = "rifle.ak",
                                amount = 1,
                                skin = 0
                            },
                            commands = new List<string>()
                        },
                        new Product
                        {
                            displayName = "LR-300 Assault Rifle",
                            id = 1,
                            useBackground = true,
                            resolution = true,
                            url = "",
                            type = true,
                            price = 1000,
                            productItem = new SItem
                            {
                                shortname = "rifle.lr300",
                                amount = 1,
                                skin = 0
                            },
                            commands = new List<string>()
                        }
                    }
                },
                new ShopCategory
                {
                    name = "CLOTHING",
                    permission = "",
                    products = new List<Product>
                    {
                        new Product
                        {
                            displayName = "Metal Facemask",
                            id = 2,
                            useBackground = true,
                            resolution = true,
                            url = "",
                            type = true,
                            price = 1200,
                            productItem = new SItem
                            {
                                shortname = "metal.facemask",
                                amount = 1,
                                skin = 0
                            },
                            commands = new List<string>()
                        },
                        new Product
                        {
                            displayName = "Metal Chest Plate",
                            id = 3,
                            useBackground = true,
                            resolution = true,
                            url = "",
                            type = true,
                            price = 1000,
                            productItem = new SItem
                            {
                                shortname = "metal.plate.torso",
                                amount = 1,
                                skin = 0
                            },
                            commands = new List<string>()
                        }
                    }
                },
                new ShopCategory
                {
                    name = "TOOLS",
                    permission = "",
                    products = new List<Product>
                    {
                        new Product
                        {
                            displayName = "Jackhammer",
                            id = 4,
                            useBackground = true,
                            resolution = true,
                            url = "",
                            type = true,
                            price = 300,
                            productItem = new SItem
                            {
                                shortname = "jackhammer",
                                amount = 1,
                                skin = 0
                            },
                            commands = new List<string>()
                        },
                        new Product
                        {
                            displayName = "Chainsaw",
                            id = 5,
                            useBackground = true,
                            resolution = true,
                            url = "",
                            type = true,
                            price = 250,
                            productItem = new SItem
                            {
                                shortname = "chainsaw",
                                amount = 1,
                                skin = 0
                            },
                            commands = new List<string>()
                        }
                    }
                }
            };
        }
        
        private class ShopCategory
        {
            [JsonProperty(PropertyName = "Category name")]
            public string name;

            [JsonProperty(PropertyName = "Permission for open this category(if empty then public category)")]
            public string permission;

            [JsonProperty(PropertyName = "List of products", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<Product> products;
        }
        
        private class Product
        {
            [JsonProperty(PropertyName = "Display name")]
            public string displayName;

            [JsonProperty(PropertyName = "Product ID(Must be unique)")]
            public int id;

            [JsonProperty(PropertyName = "Use BackGround for image")]
            public bool useBackground;
            
            [JsonProperty(PropertyName = "Image(URL)(If the product is an item, you can leave it blank)")]
            public string url;

            [JsonProperty(PropertyName = "Image resolution is 1:1 or 4:3")]
            public bool resolution;
            
            [JsonProperty(PropertyName = "Product is Item or Command(true | false)")] 
            public bool type;

            [JsonProperty(PropertyName = "Price")]
            public int price;
            
            [JsonProperty(PropertyName = "Item")]
            public SItem productItem;

            [JsonProperty(PropertyName = "Purchase Commands(%STEAMID% replacing to player steamID)", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<string> commands;
        }
        
        private class BaseSettings
        {
            [JsonProperty(PropertyName = "Enable Smart Price Increase (BUY)")]
            public bool smartAnalyseBuy;

            [JsonProperty(PropertyName = "Enable smart price reduction (SALE)")]
            public bool smartAnalyseSell;

            [JsonProperty(PropertyName = "Maximum markup")]
            public int maxMarkUp;

            [JsonProperty(PropertyName = "The minimum number of trades to activate the smart price system")]
            public int minTrades;
            
            [JsonProperty(PropertyName = "Use the buy effect of vending machines")]
            public bool useEffects;

            [JsonProperty(PropertyName = "Use the consumer ranking system(Discounts)")]
            public bool rankSystem;

            [JsonProperty(PropertyName = "Economics Settings")]
            public EconomicSettings economics;
        }
        
        private class Rank
        {
            [JsonProperty(PropertyName = "How many trades do you need to make to get a rank")] 
            public int needTrades;
            
            [JsonProperty(PropertyName = "Discount")]
            public int discount;
        }
        
        private class EconomicSettings
        {
            [JsonProperty(PropertyName = "Use side economics?(If true will using your economics with your settings, else will using built-in economics)")]
            public bool useEconomics;
            
            [JsonProperty(PropertyName = "Name of plugin")]
            public string name;

            [JsonProperty(PropertyName = "Name of Balance hook")]
            public string balance;

            [JsonProperty(PropertyName = "Name of Deposit hook")]
            public string deposit;

            [JsonProperty(PropertyName = "Name of Withdraw hook")]
            public string withdraw;
        }
        
        private class SItem
        {
            [JsonProperty(PropertyName = "Item shortname")]
            public string shortname;

            [JsonProperty(PropertyName = "Item amount")]
            public int amount;

            [JsonProperty(PropertyName = "Item skinid")]
            public int skin;
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
            public bool dataFilled = false;
            public Dictionary<ulong, PlayerData> players = new Dictionary<ulong, PlayerData>();
            public SmartAnalyse analyse = new SmartAnalyse();
        }
        
        private class SmartAnalyse
        {
            public int tradesAmount = 0;
            public Dictionary<int, int> trades = new Dictionary<int, int>();
        }
        
        private class PlayerData
        {
            public int balance = 0;
            public int trades = 0;
            public int rank = 1;
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
            if (!data.dataFilled)
            {
                foreach (var check in _config.discounts) if(!permission.PermissionExists(check.Key)) permission.RegisterPermission(check.Key, this);
                var analyse = data.analyse.trades;
                for (var i = 0; i < _config.shop.Count; i++)
                {
                    var category = _config.shop[i];
                    if (!string.IsNullOrEmpty(category.permission) && !permission.PermissionExists(category.permission)) permission.RegisterPermission(category.permission, this);
                    for (var a = 0; a < category.products.Count; a++)
                    {
                        var item = category.products[a];
                        if (!analyse.ContainsKey(item.id)) analyse.Add(item.id, 0);
                        if (string.IsNullOrEmpty(item.url) || ImageLibrary.Call<bool>("HasImage", item.url)) continue;
                        var url = item.url;
                        ImageLibrary.Call("AddImage", url, url);
                    }
                }
                data.dataFilled = true;
            }

            if (_config.Settings.economics.useEconomics)
            {
                Economics = plugins.Find(_config.Settings.economics.name);
                if (Economics == null)
                {
                    PrintError($"----------ANOTHER ECONOMICS - ON----------\n----------{_config.Settings.economics.name} NOT FOUND----------\nUNLOAD!");
                    Interface.Oxide.UnloadPlugin(Name);
                    return;
                }
            }
            
            for (var i = 0; i < BasePlayer.activePlayerList.Count; i++) OnPlayerConnected(BasePlayer.activePlayerList[i]);
            ShowUIMain(BasePlayer.activePlayerList.FirstOrDefault());
        }

        private void Unload()
        {
            for (var i = 0; i < BasePlayer.activePlayerList.Count; i++) CuiHelper.DestroyUi(BasePlayer.activePlayerList[i], Layer + ".bg");
            SaveData();
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null || data.players.ContainsKey(player.userID)) return;
            data.players.Add(player.userID, new PlayerData());
        }

        #endregion

        #region Commands

        [ChatCommand("shop")]
        private void cmdChatshop(BasePlayer player, string command, string[] args)
        {
            ShowUIMain(player);
        }

        [ConsoleCommand("UI_SHOP")]
        private void cmdConsoleUI_SHOP(ConsoleSystem.Arg arg)
        {
            if (arg?.Args == null || arg.Args.Length < 1) return;
            var player = arg.Player();
            switch (arg.Args[0])
            {
                case "find":
                    var nameOfItem = arg.Args.Length > 2 ? string.Join(" ", arg.Args.Skip(2)) : "";
                    ShowUICategoriesBlock(player, -1);
                    ShowUIMainBlock(player, -1, int.Parse(arg.Args[1]), nameOfItem.ToLower());
                    ShowUIFindBlock(player, nameOfItem);
                    break;
                case "open":
                    var category = int.Parse(arg.Args[1]);
                    ShowUICategoriesBlock(player, category);
                    ShowUIMainBlock(player, category);
                    break;
            }
        }

        #endregion

        #region Functions

        private int GetBalance(ulong id) => _config.Settings.economics.useEconomics ? Convert.ToInt32(Economics.Call<double>(_config.Settings.economics.balance, id)) : data.players[id].balance;
        private void Deposit(ulong id, int amount)
        {
            if (_config.Settings.economics.useEconomics) Economics.Call(_config.Settings.economics.deposit, (double) amount);
            else data.players[id].balance += amount;
        }

        private void Withdraw(ulong id, int amount)
        {
            if (_config.Settings.economics.useEconomics) Economics.Call(_config.Settings.economics.withdraw, (double) amount);
            else data.players[id].balance -= amount;
        }

        #endregion

        #region UI

        private void ShowUIMainBlock(BasePlayer player, int category = 0, int page = 0, string find = "")
        {
            int x = 0, y = 0;
            var shop = new List<Product>();
            if (category > -1) shop = _config.shop[category].products;
            else
            {
                foreach (var check in _config.shop)
                {
                    if (!string.IsNullOrEmpty(check.permission) && !permission.UserHasPermission(player.UserIDString, check.permission)) continue;
                    foreach (var prudct in check.products)
                        if (prudct.displayName.ToLower().Contains(find))
                            shop.Add(prudct);
                }
            }

            var container = new CuiElementContainer();
            var balance = GetBalance(player.userID);
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.298 0.139", AnchorMax = "0.863 0.881"},
                Image = {Color = "0.07 0.1 0.14 0.97"}
            }, Layer + ".bg", Layer + "mainBlock");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0.05 0.05 0.05 1", Sprite = "assets/content/ui/ui.background.transparent.linear.psd"}
            }, Layer + "mainBlock");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0.05 0.05 0.05 0.6"}
            }, Layer + "mainBlock");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.01 0.927", AnchorMax = "0.15 0.996"},
                Text =
                {
                    Text = $"Результатов: {shop.Count}", Font = "robotocondensed-regular.ttf", FontSize = 13, Align = TextAnchor.MiddleLeft,
                    Color = "0.55 0.54 0.53 1.00"
                }
            }, Layer + "mainBlock");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0 0.927", AnchorMax = "1 1"},
                Text =
                {
                    Text = "ВАШ УМНЫЙ МАГАЗИН", Font = "robotocondensed-bold.ttf", FontSize = 25, Align = TextAnchor.MiddleCenter,
                    Color = "0.7 0.7 0.7 1.00"
                }
            }, Layer + "mainBlock");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.01 0.927", AnchorMax = "0.98 0.996"},
                Text =
                {
                    Text = $"Баланс: {balance}", Font = "robotocondensed-regular.ttf", FontSize = 13, Align = TextAnchor.MiddleRight,
                    Color = "0.55 0.54 0.53 1.00"
                }
            }, Layer + "mainBlock");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.008 0.008", AnchorMax = "1 0.927"},
                Image = {Color = "0 0 0 0"}
            }, Layer + "mainBlock", Layer + "mainBlock" + ".visual");

            for (var i = 15 * page; i < 15 * (page + 1); i++)
            {
                if (shop.Count <= i) break;
                var product = shop[i];
                var resolution = product.resolution;

                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = $"{0.2 * x} {0.69 - 0.3333 * y}", AnchorMax = $"{0.185 + 0.2 * x} {1 - 0.3333 * y}"},
                    Image = {Color = "0.07 0.1 0.14 0.95"}
                }, Layer + "mainBlock" + ".visual", Layer + "mainBlock" + ".visual" + x + y);

                if (product.useBackground)
                {
                    container.Add(new CuiPanel
                    {
                        RectTransform = {AnchorMin = "0.059 0.459", AnchorMax = "0.941 0.956"},
                        Image = {Color = "0.26 0.26 0.26 1.00"}
                    }, Layer + "mainBlock" + ".visual" + x + y);

                    container.Add(new CuiPanel
                    {
                        RectTransform = {AnchorMin = "0.059 0.459", AnchorMax = "0.941 0.956"},
                        Image = {Color = "0.5 0.5 0.5 0.6", Sprite = "assets/content/ui/ui.background.transparent.linear.psd"}
                    }, Layer + "mainBlock" + ".visual" + x + y);
                }

                container.Add(new CuiElement
                {
                    Parent = Layer + "mainBlock" + ".visual" + x + y,
                    Components =
                    {
                        new CuiRawImageComponent {Png = ImageLibrary.Call<string>("GetImage", string.IsNullOrEmpty(product.url) ? product.productItem.shortname : product.url)},
                        resolution ? resolutionBox : resolutionRectangle
                    }
                });

                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0 0.3", AnchorMax = "1 0.421"},
                    Text =
                    {
                        Text = product.displayName, Font = "robotocondensed-bold.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                        Color = "0.73 0.73 0.73 1.00"
                    }
                }, Layer + "mainBlock" + ".visual" + x + y);

                container.Add(new CuiPanel
                {
                    RectTransform = {AnchorMin = "0.059 0.038", AnchorMax = "0.941 0.163"},
                    Image = {Color = "0.31 0.26 0.18 1.00"}
                }, Layer + "mainBlock" + ".visual" + x + y);

                container.Add(new CuiLabel
                {
                    RectTransform = {AnchorMin = "0 0.163", AnchorMax = "1 0.31"},
                    Text =
                    {
                        Text = $"КУПЛЕНО: {data.analyse.trades[product.id]}", Font = "robotocondensed-regular.ttf", FontSize = 10, Align = TextAnchor.MiddleCenter,
                        Color = "0.42 0.42 0.42 1.00"
                    }
                }, Layer + "mainBlock" + ".visual" + x + y);

                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = "0.059 0.038", AnchorMax = "0.941 0.163"},
                    Button = {Color = "0.74 0.28 0.00 0.9", Command = "", Sprite = "assets/content/ui/ui.background.transparent.linear.psd"},
                    Text =
                    {
                        Text = $"{product.price}", Font = "robotocondensed-bold.ttf", FontSize = 15, Align = TextAnchor.MiddleCenter,
                        Color = "1.00 0.95 0.62 1.00"
                    }
                }, Layer + "mainBlock" + ".visual" + x + y, Layer + "mainBlock" + ".visual" + x + y + ".buyBtn");

                x++;
                if (x != 5) continue;
                x = 0;
                y++;
            }

            CuiHelper.DestroyUi(player, Layer + "mainBlock");
            CuiHelper.AddUi(player, container);
        }


        private void ShowUICategoriesBlock(BasePlayer player, int category = 0)
        {
            var container = new CuiElementContainer();
            var y = 0;

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.138 0.139", AnchorMax = "0.294 0.774"},
                Image = {Color = "0.07 0.1 0.14 1.00"}
            }, Layer + ".bg", Layer + "categoriesBlock");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0.945", AnchorMax = "1 1", OffsetMax = "-1 0"},
                Image = {Color = "0 0 0 1"}
            }, Layer + "categoriesBlock");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0.05 0.05 0.05 1", Sprite = "assets/content/ui/ui.background.transparent.linear.psd"}
            }, Layer + "categoriesBlock");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.04 0.945", AnchorMax = "1 1"},
                Text =
                {
                    Text = "КАТЕГОРИИ", Font = "robotocondensed-bold.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "1 1 1 1"
                }
            }, Layer + "categoriesBlock");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0.055", AnchorMax = "0.99 0.923"},
                Image = {Color = "0 0 0 0"}
            }, Layer + "categoriesBlock", Layer + "categoriesBlock" + ".visual");

            foreach (var check in _config.shop)
            {
                var thisCategory = y == category;
                container.Add(new CuiButton
                {
                    RectTransform = {AnchorMin = $"0.08 {0.9375 - 0.0625 * y}", AnchorMax = $"1 {1 - 0.0625 * y}"},
                    Button = {Color = "0 0 0 0", Command = $"UI_SHOP open {y}"},
                    Text =
                    {
                        Text = check.name, Font = thisCategory ? "robotocondensed-bold.ttf" : "robotocondensed-regular.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                        Color = !string.IsNullOrEmpty(check.permission) && permission.UserHasPermission(player.UserIDString, check.permission) ? "0.96 0.93 0.61 1.00" : thisCategory ? "1 1 1 1" : "0.57 0.57 0.57 1.00"
                    }
                }, Layer + "categoriesBlock" + ".visual");
                y++;
            }

            CuiHelper.DestroyUi(player, Layer + "categoriesBlock");
            CuiHelper.AddUi(player, container);
        }

        private void ShowUIFindBlock(BasePlayer player, string find = "")
        {
            if (string.IsNullOrEmpty(find)) find = "Введите ключевые слова...";
            var container = new CuiElementContainer();

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.138 0.784", AnchorMax = "0.294 0.881"},
                Image = {Color = "0.08 0.09 0.10 1"}
            }, Layer + ".bg", Layer + "findBlock");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0.658", AnchorMax = "1 1", OffsetMax = "-1 -1"},
                Image = {Color = "0.12 0.13 0.15 1.00"}
            }, Layer + "findBlock");
            
            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "0.9 0.98"},
                Image = {Color = "0.5 0.5 0.5 0.2", Sprite = "assets/content/ui/ui.background.transparent.linearltr.tga"}
            }, Layer + "findBlock");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.045 0.658", AnchorMax = "1 0.959"},
                Text =
                {
                    Text = "ПОИСК", Font = "robotocondensed-bold.ttf", FontSize = 15, Align = TextAnchor.MiddleLeft,
                    Color = "1 1 1 1"
                }
            }, Layer + "findBlock");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0.05 0.205", AnchorMax = "0.95 0.534"},
                Image = {Color = "0.10 0.10 0.10 1.00"}
            }, Layer + "findBlock", Layer + "findBlock" + ".field");

            container.Add(new CuiLabel
            {
                RectTransform = {AnchorMin = "0.05 0", AnchorMax = "1 1"},
                Text =
                {
                    Text = find, Font = "robotocondensed-bold.ttf", FontSize = 11, Align = TextAnchor.MiddleLeft,
                    Color = "0.36 0.36 0.36 1.00"
                }
            }, Layer + "findBlock" + ".field");

            container.Add(new CuiElement
            {
                Parent = Layer + "findBlock" + ".field",
                Components =
                {
                    new CuiInputFieldComponent
                    {
                        Align = TextAnchor.MiddleLeft, CharsLimit = 30, FontSize = 13,
                        Command = "UI_SHOP find 0"
                    },
                    new CuiRectTransformComponent {AnchorMin = "0.05 0", AnchorMax = "1 1"}
                }
            });
            
            Outline(ref container, Layer + "findBlock" + ".field", "0.25 0.25 0.25 1.00", "1.3");

            CuiHelper.DestroyUi(player, Layer + "findBlock");
            CuiHelper.AddUi(player, container);
        }

        private void ShowUIMain(BasePlayer player)
        {
            var container = new CuiElementContainer();

            container.Add(new CuiPanel
            {
                CursorEnabled = true,
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0 0 0 0.85", Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat"}
            }, "Overlay", Layer + ".bg");

            container.Add(new CuiPanel
            {
                RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                Image = {Color = "0.06 0.40 0.40 1.00", Sprite = "assets/content/ui/overlay_freezing.png"}
            }, Layer + ".bg");

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
            
            ShowUIFindBlock(player);
            ShowUICategoriesBlock(player);
            ShowUIMainBlock(player);
        }

        private void Outline(ref CuiElementContainer container, string parent, string color = "1 1 1 1", string size = "2.5")
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