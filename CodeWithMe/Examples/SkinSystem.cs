using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("SkinSystem", "CASHR", "1.0.0")]
    internal class SkinSystem : RustPlugin
    {
        #region Static

        [PluginReference] private Plugin ImageLibrary;
        private Dictionary<ulong, Dictionary<string, ulong>> data;
        private PluginConfig _config;
        public class CategoriesItems 
        {
            public Dictionary<string, ulong> ItemsList = new Dictionary<string, ulong>();
        }
        public bool AddImage(string shortname, string name, ulong skin = 0) => (bool) ImageLibrary.Call("AddImage", shortname, name);
        public Dictionary<string, CategoriesItems> CategiriesList = new Dictionary<string, CategoriesItems>();
        
        private static string perm = "skinsystem.use";
        private string GetImage(string image) => (string) ImageLibrary?.Call("GetImage", image);
       // public string GetImageSkin(string shortname, ulong skin = 0)=>(string) ImageLibrary?.Call("GetImage", shortname, skin);
        private string GetImageSkin(string shortname, ulong skin = 0)
        {
            if (skin > 0)
                if (ImageLibrary.Call<bool>("HasImage", shortname, skin) == false &&
                    ImageLibrary.Call<Dictionary<string, object>>("GetSkinInfo", shortname, skin) == null)
                {
                    webrequest.Enqueue("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/",
                        $"key=FFC21913B1C6EE85A83057A59961CFFF&itemcount=1&publishedfileids%5B0%5D={skin}",
                        (code, response) =>
                        {
                            if (code != 200 || response == null)
                            {
                                PrintError(
                                    $"Image failed to download! Code HTTP error: {code} - Image Name: {shortname} - Image skinID: {skin} - Response: {response}");
                                return;
                            }

                            var sr = JsonConvert.DeserializeObject<SteampoweredResult>(response);
                            if (sr == null || !(sr is SteampoweredResult) || sr.response.result == 0 ||
                                sr.response.resultcount == 0)
                            {
                                PrintError(
                                    $"Image failed to download! Error: Parse JSON response - Image Name: {shortname} - Image skinID: {skin} - Response: {response}");
                                return;
                            }

                            foreach (var publishedfiled in sr.response
                                .publishedfiledetails)
                                ImageLibrary.Call("AddImage", publishedfiled.preview_url, shortname, skin);
                        }, this, RequestMethod.POST);
                    return ImageLibrary.Call<string>("GetImage", "LOADING");
                }

            return (string) ImageLibrary?.Call("GetImage", shortname, skin);
        }
        private class SteampoweredResult
        {
            public Response response;

            public class Response
            {
                [JsonProperty("publishedfiledetails")] public List<PublishedFiled> publishedfiledetails;
                [JsonProperty("result")] public int result;

                [JsonProperty("resultcount")] public int resultcount;

                public class PublishedFiled
                {
                    [JsonProperty("ban_reason")] public string ban_reason;

                    [JsonProperty("banned")] public int banned;

                    [JsonProperty("consumer_app_id")] public int consumer_app_id;

                    [JsonProperty("creator")] public string creator;

                    [JsonProperty("creator_app_id")] public int creator_app_id;

                    [JsonProperty("description")] public string description;

                    [JsonProperty("favorited")] public int favorited;

                    [JsonProperty("file_size")] public int file_size;

                    [JsonProperty("filename")] public string filename;

                    [JsonProperty("hcontent_preview")] public string hcontent_preview;

                    [JsonProperty("lifetime_favorited")] public int lifetime_favorited;

                    [JsonProperty("lifetime_subscriptions")]
                    public int lifetime_subscriptions;

                    [JsonProperty("preview_url")] public string preview_url;
                    [JsonProperty("publishedfileid")] public ulong publishedfileid;

                    [JsonProperty("result")] public int result;

                    [JsonProperty("subscriptions")] public int subscriptions;

                    [JsonProperty("tags")] public List<Tag> tags;

                    [JsonProperty("time_created")] public int time_created;

                    [JsonProperty("time_updated")] public int time_updated;

                    [JsonProperty("title")] public string title;

                    [JsonProperty("views")] public int views;

                    [JsonProperty("visibility")] public int visibility;

                    public class Tag
                    {
                        [JsonProperty("tag")] public string tag;
                    }
                }
            }
        }

        private void UpdateSkin()
        {
            var ItemsListed = ItemManager.itemList;
            CategiriesList = new Dictionary<string, CategoriesItems>()
            {
                ["Attire"] = new CategoriesItems()
                {
                    ItemsList = ItemsListed
                        .Where(x => x.category.ToString() == "Attire" &&
                                    GetImageSkins(x.shortname).Count > 1)
                        .ToDictionary(p => p.shortname, p => ulong.Parse("0"))
                },
                ["Weapon"] = new CategoriesItems()
                {
                    ItemsList = ItemsListed
                        .Where(x => x.category.ToString() == "Weapon" &&
                                    GetImageSkins(x.shortname).Count > 1 && !x.shortname.Contains(".mod"))
                        .ToDictionary(p => p.shortname, p => ulong.Parse("0"))
                },
            };
            List<string> IgnoreList = new List<string>();
            foreach (var check in CategiriesList.ToArray())
            {
                foreach (var item in check.Value.ItemsList.ToArray())
                {
                    if (!_config.IgnoreList.Contains(item.Key))
                    {
                        check.Value.ItemsList.Remove(item.Key);
                    }
                }
                
            }
/*
            for (int i = 0; i < IgnoreList.Count; i++)
            {
                var item = IgnoreList[i];
                if (!CategiriesList["Attire"].ItemsList.ContainsKey(item))
                {
                    CategiriesList["Attire"].ItemsList.Remove(item);
                }
                if (!CategiriesList["Weapon"].ItemsList.ContainsKey(item))
                {
                    CategiriesList["Weapon"].ItemsList.Remove(item);
                }
            }*/
        }
        public List<ulong> GetImageSkins(string shortname)
        {
            var list = ImageLibrary.Call("GetImageList", shortname) as List<ulong>;
            return list;
        }
 
        #endregion

        #region Config

        public class PluginConfig
        {
            [JsonProperty("List of items that should not be displayed(shortname)")]
            public List<string> IgnoreList;
            
            public Dictionary<string, List<ulong>> DopItems = new Dictionary<string, List<ulong>>
            {
                ["rifle.ak"] = new List<ulong>
                {
                    1557610898,
                    1560983948,
                    2263358920,
                    2201078717
                }
            };

            internal class DefaultSettings
            {
                [JsonProperty("List of items for default players")]
                public Dictionary<string, Dictionary<string, List<ulong>>> DefaultSkins;
            }
            [JsonProperty("List of added skins")]
            public DefaultSettings DefaultSkin;
        }

        protected override void LoadDefaultConfig()
        {
            _config = new PluginConfig
            {
                IgnoreList = new List<string>()
                {
                    "metal.facemask",
                    "metal.plate.torso",
                    "roadsign.gloves",
                    "roadsign.kilt",
                    "shoes.boots",
                    "pants",
                    "rifle.ak",
                    "rifle.m39",
                    "rifle.l96",
                    "rifle.lr300",
                    "smg.mp5",
                    "pistol.m92",
                    "shotgun.spas12",
                    "lmg.m249",
                    "hoodie"
                },
               DefaultSkin = new PluginConfig.DefaultSettings()
               {
                   DefaultSkins = new Dictionary<string, Dictionary<string, List<ulong>>>()
                   {
                       ["Attire"] = new Dictionary<string, List<ulong>>()
                       {
                           ["hoodie"] = new List<ulong>()
                           {
                               0, 1
                           }
                       },
                       ["Weapon"] = new Dictionary<string, List<ulong>>()
                       {
                           ["rifle.ak"] = new List<ulong>()
                           {
                               0, 1
                           }
                       }
                   }
               }
            }; 
        }  

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config = Config.ReadObject<PluginConfig>();
        }
 
        protected override void SaveConfig()
        { 
            Config.WriteObject(_config);
        } 

        #endregion
 
        #region OxideHooks

        private void Init()
        {
            LoadConfig();
            LoadData();
        }

        private void OnServerInitialized()
        {
            AddImage("https://i.imgur.com/NAXqf60.png", "ClanSkins_Update");
            AddImage("https://i.imgur.com/4waRRlv.png", "ClanSkins_Close");
            AddImage("https://i.imgur.com/d78SFco.png", "ClanSkins_Return");
		//	ImageLibrary?.Call("AddSkin", _config.SkinList);
			UpdateSkin();	            
            permission.RegisterPermission(perm, this);
            foreach (var check in BasePlayer.activePlayerList)
            {
                OnPlayerConnected(check);
            }
            
        }
 
        private void Unload()
        {
            SaveData();
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;
            if (data.ContainsKey(player.userID)) return;
            data.Add(player.userID, new Dictionary<string, ulong>());
        }

        void OnItemAddedToContainer(ItemContainer container, Item item)
        {
            if (container == null || item == null) return;
            var player = container?.playerOwner;
            if (player == null) return;
            if (!permission.UserHasPermission(player.UserIDString, perm)) return;
            if (!data.ContainsKey(player.userID))
            {
                OnPlayerConnected(player);
            }
            if (!data[player.userID].ContainsKey(item.info.shortname)) return;
            var skin = data[player.userID][item.info.shortname];
            item.skin = skin;
			if(item.GetHeldEntity() == null) return;
            item.GetHeldEntity().skinID = skin;
            item.GetHeldEntity().SendNetworkUpdate();
        }
        void OnActiveItemChanged(BasePlayer player, Item oldItem, Item newItem)
        {
            if (player == null) return;
            if (newItem == null) return;
            if (!permission.UserHasPermission(player.UserIDString, perm)) return;
            if (!data.ContainsKey(player.userID))
            {
                OnPlayerConnected(player);
            }
            if (!data[player.userID].ContainsKey(newItem.info.shortname)) return;
            var skin = data[player.userID][newItem.info.shortname];
            newItem.skin = skin;
			if(newItem.GetHeldEntity() == null) return;
            newItem.GetHeldEntity().skinID = skin;
            newItem.GetHeldEntity().SendNetworkUpdate();
        }
       


        #endregion

        #region Data

        private void LoadData()
        {
            if (Interface.Oxide.DataFileSystem.ExistsDatafile($"{Name}/PlayerData"))
                data = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<ulong, Dictionary<string, ulong>>>(
            $"{Name}/PlayerData");
            else data = new Dictionary<ulong, Dictionary<string, ulong>>();
            Interface.Oxide.DataFileSystem.WriteObject($"{Name}/PlayerData", data);
        }

        private void OnServerSave()
        {
            SaveData();
        }

        private void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject($"{Name}/PlayerData", data);
        }

        #endregion

        #region Function



        #endregion

        #region Interface

        private void CreateMainMenu(BasePlayer player, string categiries = "Attire", int page = 0)
        {
            CuiHelper.DestroyUi(player, "clanSkins_skinlist");
            var elements = new CuiElementContainer();
            elements.Add(new CuiElement
            {
                Name = "clanSkins_skinlist", Parent = "Overlay",
                Components =
                {
                    new CuiImageComponent
                        {Color = "0 0 0 0.85", Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat"},
                    new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "1 1"},
                    new CuiNeedsCursorComponent { }
                }
            });
            elements.Add(new CuiElement()
            {
                Name = "clanSkins_mainClose", Parent = $"clanSkins_skinlist",
                Components =
                {
                    new CuiRawImageComponent {Png = GetImage("ClanSkins_Close")},
                    new CuiRectTransformComponent {AnchorMin = $"0.925 0.88", AnchorMax = "0.98 0.98"},
                }
            });
            elements.Add(
                new CuiButton
                {
                    RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                    Button =
                    {
                        Color = "1 1 1 0", Close = "clanSkins_skinlist",
                        Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat"
                    },
                    Text =
                    {
                        Text = "", Align = TextAnchor.MiddleCenter, Font = "robotocondensed-bold.ttf", FontSize = 24
                    },
                }, "clanSkins_mainClose");
            elements.Add(new CuiElement
            {
                Name = "clanSkins_main", Parent = "clanSkins_skinlist",
                Components =
                {
                    new CuiImageComponent {Color = "0 0 0 0"},
                    new CuiRectTransformComponent {AnchorMin = "0.17 0.1546297", AnchorMax = "0.83 0.8935185"},
                }
            });
            elements.Add(new CuiElement
            {
                Name = "clanSkins_main200", Parent = "clanSkins_main",
                Components =
                {
                    new CuiRawImageComponent
                        {Color = "0 0 0 0.85", Sprite = "assets/content/ui/ui.background.tile.psd"},
                    new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "1 1.1"}
                }
            });
            elements.Add(new CuiElement
            {
                Parent = "clanSkins_main200",
                Components =
                {
                    new CuiTextComponent
                    {
                        Text = "", Color = "1 0.9294118 0.8666667 1",
                        Font = "robotocondensed-bold.ttf", Align = TextAnchor.MiddleCenter
                    },
                    new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "1 1"},
                }
            });
            elements.Add(new CuiElement()
            {
                Name = "clanSkins_mainUpdate", Parent = $"clanSkins_main200",
                Components =
                {
                    new CuiRawImageComponent {Png = GetImage("ClanSkins_Update"), Color = "1 1 1 0.3"},
                    new CuiRectTransformComponent {AnchorMin = $"0.925 0", AnchorMax = "0.99 1"},
                }
            });
            elements.Add(
                new CuiButton
                {
                    RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                    Button =
                    {
                        Color = "1 1 1 0", Command = $"ClanSkins_Update {categiries} {page}",
                        Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat"
                    },
                    Text =
                    {
                        Text = "", Align = TextAnchor.MiddleCenter, Font = "robotocondensed-bold.ttf", FontSize = 24
                    },
                }, "clanSkins_mainUpdate");
            CuiHelper.AddUi(player, elements);
            CreateCategoriesItems(player, categiries);
        }

        private void CreateCategoriesItems(BasePlayer player, string categiries, int page = 0)
        {
            var elements = new CuiElementContainer();
            CuiHelper.DestroyUi(player, "clanSkins_main2");
            CuiHelper.DestroyUi(player, "clanSkins_main3");
            elements.Add(new CuiElement
            {
                Name = "clanSkins_main2", Parent = "clanSkins_skinlist",
                Components =
                {
                    new CuiImageComponent {Color = "0 0 0 0.4"},
                    new CuiRectTransformComponent {AnchorMin = "0.17 0.1546297", AnchorMax = "0.83 0.8935185"},
                }
            });
            elements.Add(new CuiElement
            {
                Name = "clanSkins_CategoriesItems", Parent = "clanSkins_main2",
                Components =
                {
                    new CuiImageComponent
                    {
                        Color = "0 0 0 0.4",
                        Sprite = "assets/content/ui/ui.background.tile.psd"
                    },
                    new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "1 1"},
                }
            });
            var ItemList = CategiriesList[categiries].ItemsList;
            var poses = GetPositions(7, 5, 0.01f, 0.01f);
            if (ItemList.Count > 35)
                elements.Add(
                    new CuiButton
                    {
                        RectTransform = {AnchorMin = "1 0", AnchorMax = "1.1 1"},
                        Button =
                        {
                            Color = "1 1 1 0.3", Command = $"ClanSkins_Update {categiries} {page}",
                            Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat"
                        },
                        Text =
                        {
                            Text = "", Align = TextAnchor.MiddleCenter, Font = "robotocondensed-bold.ttf", FontSize = 24
                        },
                    }, "clanSkins_CategoriesItems");
            var i = 0;
            foreach (var item in ItemList.Take(35))
            {
                elements.Add(new CuiElement
                {
                    Name = "clanSkins_skinlist" + item.Key, Parent = "clanSkins_CategoriesItems",
                    Components =
                    {
                        new CuiRawImageComponent
                        {
                            Color = "0.3294118 0.3294118 0.3294118 0.5",
                            Sprite = "assets/content/ui/ui.background.tile.psd"
                        },
                        new CuiRectTransformComponent {AnchorMin = poses[i].AnchorMin, AnchorMax = poses[i].AnchorMax}
                    }
                });
                if (data[player.userID].ContainsKey(item.Key))
                    elements.Add(new CuiElement
                    {
                        Parent = "clanSkins_skinlist" + item.Key,
                        Components =
                        {
                            new CuiRawImageComponent {Png = GetImageSkin(item.Key, data[player.userID][item.Key])},
                            new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "1 1"}
                        }
                    });
                else
                    elements.Add(new CuiElement
                    {
                        Parent = "clanSkins_skinlist" + item.Key,
                        Components =
                        {
                            new CuiRawImageComponent {Png = GetImageSkin(item.Key, item.Value)},
                            new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "1 1"}
                        }
                    });
                
                elements.Add(
                    new CuiButton
                    {
                        Button =
                        {
                            Color = "0.13 0.44 0.48 0", Command = $"clanSkins_getskinIds {item.Key} 0 {categiries}"
                        },
                        Text =
                        {
                            Text = "", Color = "1 0.9294118 0.8666667 1", FontSize = 14, Align = TextAnchor.MiddleCenter
                        },
                        RectTransform = {AnchorMin = $"0 0", AnchorMax = $"1 1"},
                    }, "clanSkins_skinlist" + item.Key);
                i++;
            }

            elements.Add(
                new CuiButton
                {
                    RectTransform = {AnchorMin = "0 -0.1", AnchorMax = "0.5 -0.0025"},
                    Button =
                    {
                        Color = categiries == "Attire" ? "0.4 0.7 0.4 0.85" : "0 0 0 0.6", Command = $"ClanSkins_Update Attire 1",
                        Sprite = "assets/content/ui/ui.background.tile.psd"
                    },
                    Text =
                    {
                        Text = "CLOTHING", Align = TextAnchor.MiddleCenter,
                        Font = "robotocondensed-bold.ttf", FontSize = 30
                    },
                }, "clanSkins_CategoriesItems");
            elements.Add(
                new CuiButton
                {
                    RectTransform = {AnchorMin = "0.5015 -0.1", AnchorMax = "0.998 -0.0025"},
                    Button =
                    {
                        Color = categiries == "Weapon" ? "0.4 0.7 0.4 0.85" : "0 0 0 0.6", Command = $"ClanSkins_Update Weapon 1",
                        Sprite = "assets/content/ui/ui.background.tile.psd"
                    },
                    Text =
                    {
                        Text = "WEAPON", Align = TextAnchor.MiddleCenter,
                        Font = "robotocondensed-bold.ttf", FontSize = 30
                    },
                }, "clanSkins_CategoriesItems");
            CuiHelper.AddUi(player, elements);
        }
       [ConsoleCommand("clanSkins_getskinIds")]
        private void cmdClansGetSkinList(ConsoleSystem.Arg args)
        {
            var player = args.Player();
            var page = int.Parse(args.Args[1]);
            var SkinList = GetImageSkins(args.Args[0]);
            if (_config.DopItems.ContainsKey(args.Args[0]))
            foreach (var check in _config.DopItems[args.Args[0]])
                SkinList.Add(check);
            
            var categories = args.Args[2];
            if (_config.DefaultSkin.DefaultSkins.ContainsKey(categories) &&
                _config.DefaultSkin.DefaultSkins[categories].ContainsKey(args.Args[0]))
            {
                SkinList.AddRange(_config.DefaultSkin.DefaultSkins[categories][args.Args[0]]);
            }

            var elements = new CuiElementContainer();
            CuiHelper.DestroyUi(player, "clanSkins_main2");
            CuiHelper.DestroyUi(player, "clanSkins_main3");
            CuiHelper.DestroyUi(player, "clanSkins_main");
            CuiHelper.DestroyUi(player, "clanSkins_mainReturn");
            elements.Add(new CuiElement
            {
                Name = "clanSkins_main3", Parent = "clanSkins_skinlist",
                Components =
                {
                    new CuiImageComponent {Color = "0 0 0 0.3"},
                    new CuiRectTransformComponent {AnchorMin = "0.17 0.1546297", AnchorMax = "0.83 0.8935185"},
                }
            });
            elements.Add(new CuiElement
            {
                Name = "clanSkins_main200", Parent = "clanSkins_main3",
                Components =
                {
                    new CuiRawImageComponent
                        {Color = "0 0 0 0.85", Sprite = "assets/content/ui/ui.background.tile.psd"},
                    new CuiRectTransformComponent {AnchorMin = "0 1", AnchorMax = "1 1.1"}
                }
            });
            elements.Add(new CuiElement
            {
                Parent = "clanSkins_main200",
                Components =
                {
                    new CuiTextComponent
                    {
                        Text = "", Color = "1 0.9294118 0.8666667 1",
                        Font = "robotocondensed-bold.ttf", Align = TextAnchor.MiddleCenter
                    },
                    new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "1 1"},
                }
            });
            elements.Add(new CuiElement()
            {
                Name = "clanSkins_mainReturn", Parent = $"clanSkins_main200",
                Components =
                {
                    new CuiRawImageComponent {Png = GetImage("ClanSkins_Return"), Color = "1 1 1 0.3"},
                    new CuiRectTransformComponent {AnchorMin = $"0.01 0.1", AnchorMax = "0.064 0.9"},
                }
            });
            elements.Add(
                new CuiButton
                {
                    RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                    Button =
                    {
                        Color = "1 1 1 0", Command = $"ClanSkins_Update {categories} 1",
                        Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat"
                    },
                    Text =
                    {
                        Text = "", Align = TextAnchor.MiddleCenter, Font = "robotocondensed-bold.ttf", FontSize = 24
                    },
                }, "clanSkins_mainReturn");
            elements.Add(new CuiElement()
            {
                Name = "clanSkins_mainUpdate", Parent = $"clanSkins_main200",
                Components =
                {
                    new CuiRawImageComponent {Png = GetImage("ClanSkins_Update"), Color = "1 1 1 0.3"},
                    new CuiRectTransformComponent {AnchorMin = $"0.925 0", AnchorMax = "0.99 1"},
                }
            });
            elements.Add(
                new CuiButton
                {
                    RectTransform = {AnchorMin = "0 0", AnchorMax = "1 1"},
                    Button =
                    {
                        Color = "1 1 1 0", Command = $"ClanSkins_Update {args.Args[0]} {page} {categories}",
                        Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat"
                    },
                    Text =
                    {
                        Text = "", Align = TextAnchor.MiddleCenter, Font = "robotocondensed-bold.ttf", FontSize = 24
                    },
                }, "clanSkins_mainUpdate");
            var poses = GetPositions(7, 5, 0.01f, 0.01f);
            if (SkinList.Count > 35)
                elements.Add(
                    new CuiButton
                    {
                        RectTransform = {AnchorMin = "1.004 0.001", AnchorMax = "1.08 0.997"},
                        Button =
                        {
                            Color = "1 1 1 0.3",
                            Command = $"clanSkins_getskinIds {args.Args[0]} {page + 1} {categories}"
                        },
                        Text =
                        {
                            Text = ">", Color = "1 0.9294118 0.8666667 1", Align = TextAnchor.MiddleCenter,
                            Font = "robotocondensed-bold.ttf", FontSize = 40
                        },
                    }, "clanSkins_main3");
            if (page > 1)
                elements.Add(
                    new CuiButton
                    {
                        RectTransform = {AnchorMin = "-0.08 0.001", AnchorMax = "-0.004 0.997"},
                        Button =
                        {
                            Color = "1 1 1 0.3",
                            Command = $"clanSkins_getskinIds {args.Args[0]} {page - 1} {categories}"
                        },
                        Text =
                        {
                            Text = "<", Color = "1 0.9294118 0.8666667 1", Align = TextAnchor.MiddleCenter,
                            Font = "robotocondensed-bold.ttf", FontSize = 40
                        },
                    }, "clanSkins_main3");
            var i = 0;
            foreach (var skin in SkinList.Skip(35 * page).Take(35))
            {
                elements.Add(new CuiElement
                {
                    Name = "clanSkins_skinlist" + skin, Parent = "clanSkins_main3",
                    Components =
                    {
                        new CuiRawImageComponent
                        {
                            Color = "0.3294118 0.3294118 0.3294118 0.5",
                            Sprite = "assets/content/ui/ui.background.tile.psd"
                        },
                        new CuiRectTransformComponent {AnchorMin = poses[i].AnchorMin, AnchorMax = poses[i].AnchorMax}
                    }
                });
                elements.Add(new CuiElement
                {
                    Parent = "clanSkins_skinlist" + skin,
                    Components =
                    {
                        new CuiRawImageComponent {Png = GetImageSkin(args.Args[0], skin)},
                        new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "1 1"}
                    }
                });
                elements.Add(
                    new CuiButton
                    {
                        Button =
                        {
                            Color = "0.13 0.44 0.48 0",
                            Command = $"ClanSkins_addnewSkin {args.Args[0]} {skin} {categories}"
                        },
                        Text =
                        {
                            Text = "", Color = "1 0.9294118 0.8666667 1", FontSize = 14, Align = TextAnchor.MiddleCenter
                        },
                        RectTransform = {AnchorMin = $"0 0", AnchorMax = $"1 1"},
                    }, "clanSkins_skinlist" + skin);
                i++;
            }
            CuiHelper.AddUi(player, elements);
        }
        [ConsoleCommand("ClanSkins_Update")]
        private void cmdClanSkinsUpdate(ConsoleSystem.Arg args)
        {
            var player = args.Player();
            var page = int.Parse(args.Args[1]);
            if (args.Args[0] == "Attire" || args.Args[0] == "Weapon") CreateMainMenu(player, args.Args[0], page);
            else cmdClansGetSkinList(args);
        }
        [ConsoleCommand("ClanSkins_changeskin")]
        private void cmdNewSkinOfClan(ConsoleSystem.Arg args)
        {		
            if (args.GetString(1) == "") return;
			
            if (args.Args.Length != 3) return;
            var player = args.Player();
            ulong skin;			
            if (!ulong.TryParse(args.Args[2], out skin)) return;
            CreateMainMenu(player, args.Args[0], 0);
        }

        [ChatCommand("skins")]
        private void cmdChatskins(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, perm)) return;
            CreateMainMenu(player);
        }
        [ConsoleCommand("ClanSkins_addnewSkin")]
        private void cmdAddNewSkin(ConsoleSystem.Arg args)
        {
            var player = args.Player();
            var shortname = args.Args[0];
            var skinid = ulong.Parse(args.Args[1]);
            var type = args.Args[2];
            if (data[player.userID].ContainsKey(shortname))
            {
                data[player.userID][shortname] = skinid;
                CreateMainMenu(player, type, 0);
                return;
            }
            data[player.userID].Add(shortname, skinid);
           CreateMainMenu(player, type, 0);
                
        }


        private static List<Position> GetPositions(int colums, int rows, float colPadding = 0, float rowPadding = 0,
            bool columsFirst = false)
        {
            var reply = 3075;
            if (reply == 0)
            {
            }

            if (colums == 0) throw new ArgumentException("Can't create positions for gui!", nameof(colums));
            if (rows == 0) throw new ArgumentException("Can't create positions for gui!", nameof(rows));
            var result = new List<Position>();
            result.Clear();
            var colsDiv = 1f / colums;
            var rowsDiv = 1f / rows;
            if (colPadding == 0) colPadding = colsDiv / 2;
            if (rowPadding == 0) rowPadding = rowsDiv / 2;
            if (!columsFirst)
                for (var j = rows; j >= 1; j--)
                for (var i = 1; i <= colums; i++)
                {
                    var pos = new Position
                    {
                        Xmin = (i - 1) * colsDiv + colPadding / 2f, Xmax = i * colsDiv - colPadding / 2f,
                        Ymin = (j - 1) * rowsDiv + rowPadding / 2f, Ymax = j * rowsDiv - rowPadding / 2f
                    };
                    result.Add(pos);
                }
            else
                for (var i = 1; i <= colums; i++)
                for (var j = rows; j >= 1; j--)
                {
                    var pos = new Position
                    {
                        Xmin = (i - 1) * colsDiv + colPadding / 2f, Xmax = i * colsDiv - colPadding / 2f,
                        Ymin = (j - 1) * rowsDiv + rowPadding / 2f, Ymax = j * rowsDiv - rowPadding / 2f
                    };
                    result.Add(pos);
                }

            return result;
        }
        private class Position
        {
            public float Xmax;
            public float Xmin;
            public float Ymax;
            public float Ymin;

            public string AnchorMin =>
                $"{Math.Round(Xmin, 4).ToString(CultureInfo.InvariantCulture)} {Math.Round(Ymin, 4).ToString(CultureInfo.InvariantCulture)}";

            public string AnchorMax =>
                $"{Math.Round(Xmax, 4).ToString(CultureInfo.InvariantCulture)} {Math.Round(Ymax, 4).ToString(CultureInfo.InvariantCulture)}";

            public override string ToString()
            {
                return $"----------\nAmin:{AnchorMin}\nAmax:{AnchorMax}\n----------";
            }
        }
        #endregion
    }
}