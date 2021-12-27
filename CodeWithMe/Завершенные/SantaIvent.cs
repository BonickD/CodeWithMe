using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("SantaIvent", "AhigaO#4485", "1.0.0")]
    internal class SantaIvent : RustPlugin
    {
        #region Static

        private const string Layer = "UI_SantaIvent";
        private int ImageLibraryCheck = 0;
        private Configuration _config;
        private Data data;
        [PluginReference] private Plugin ImageLibrary;


        #endregion

        #region Config

        private class Configuration
        {
            [JsonProperty(PropertyName = "MinAmount of gifts")]
            public int minAmountGifts = 4;
            
            [JsonProperty(PropertyName = "MaxAmount of gifts")]
            public int maxAmountGifts = 8;
            
            [JsonProperty(PropertyName = "Radius of gifts on the map")] // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            public int radSpawn = 800;
            
            [JsonProperty(PropertyName = "Stages of event")]
            public List<Stage> stages = new List<Stage>
            {
                new Stage
                {
                    nameStage = "Santa's helper little",
                    items = new List<CItem>
                    {
                        new CItem
                        {
                            shortname = "coal",
                            amount = 10000,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "Iron",
                            amount = 1500,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "Salat",
                            amount = 1300,
                            skinID = 0,
                        },
                    }
                },
                new Stage
                {
                    nameStage = "Elf on a side job",
                    items = new List<CItem>
                    {
                        new CItem
                        {
                            shortname = "coal",
                            amount = 10000,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "Iron",
                            amount = 1500,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "Salat",
                            amount = 1300,
                            skinID = 0,
                        },
                    }
                },
                new Stage
                {
                    nameStage = "Sucking Santa",
                    items = new List<CItem>
                    {
                        new CItem
                        {
                            shortname = "coal",
                            amount = 10000,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "Iron",
                            amount = 1500,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "Salat",
                            amount = 1300,
                            skinID = 0,
                        },
                    }
                },
                new Stage
                {
                    nameStage = "Almost Santa but not yet",
                    items = new List<CItem>
                    {
                        new CItem
                        {
                            shortname = "coal",
                            amount = 10000,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "Iron",
                            amount = 1500,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "Salat",
                            amount = 1300,
                            skinID = 0,
                        },
                    }
                },
            };

            [JsonProperty(PropertyName = "List of gifts")]
            public List<Gift> gifts = new List<Gift>
            {
                new Gift
                {
                    minAmountOfItems = 4,
                    maxAmountOfItems = 12,
                    items = new List<CItem>
                    {
                        new CItem
                        {
                            shortname = "rifle.ak",
                            amount = 1,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "coal",
                            amount = 1000,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "jackhammer",
                            amount = 1,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "grenade.f1",
                            amount = 2,
                            skinID = 0,
                        },

                    }
                },
                new Gift
                {
                    minAmountOfItems = 6,
                    maxAmountOfItems = 22,
                    items = new List<CItem>
                    {
                        new CItem
                        {
                            shortname = "pistol.m92",
                            amount = 1,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "ammo.pistol",
                            amount = 100,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "metalpipe",
                            amount = 5,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "scrap",
                            amount = 800,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "metal.fragments",
                            amount = 1000,
                            skinID = 0,
                        },

                    }
                },
                new Gift
                {
                    minAmountOfItems = 4,
                    maxAmountOfItems = 12,
                    items = new List<CItem>
                    {
                        new CItem
                        {
                            shortname = "lowgradefuel",
                            amount = 400,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "metal.refined",
                            amount = 200,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "workbench2",
                            amount = 1,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "grenade.f1",
                            amount = 2,
                            skinID = 0,
                        },
                        new CItem
                        {
                            shortname = "ammo.pistol",
                            amount = 2,
                            skinID = 0,
                        },

                    }
                },

            };
        }

        private class Gift
        {
            [JsonProperty(PropertyName = "Min amount of items")]
            public int minAmountOfItems;

            [JsonProperty(PropertyName = "Max amount of items")]
            public int maxAmountOfItems;

            [JsonProperty(PropertyName = "Item list")]
            public List<CItem> items = new List<CItem>();
        }

        private class Stage
        {
            [JsonProperty(PropertyName = "Stage name")]
            public string nameStage;

            [JsonProperty(PropertyName = "List of items request")]
            public List<CItem> items = new List<CItem>();
        }

        private class CItem
        {
            [JsonProperty(PropertyName = "shortname")]
            public string shortname;

            [JsonProperty(PropertyName = "amount")]
            public int amount;

            [JsonProperty(PropertyName = "skinid")]
            public ulong skinID;
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
            public Dictionary<string, int> requirmentItems = new Dictionary<string, int>();
            public int stage = 0;
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
            if (data.requirmentItems.Count == 0) GenerateDataStage(0);
        }

        private void Unload()
        {
            SaveData();
        }

        private void OnPlayerConnected(BasePlayer player)
        {

        }

        #endregion

        #region Commands

        [ChatCommand("test")]
        private void cmdChattest(BasePlayer player, string command, string[] args)
        {
            EventSpawn();
        }

        #endregion

        #region Functions

        private void GenerateDataStage(int stage)
        {
            var _data = data.requirmentItems;
            data.stage = stage;
            _data.Clear();
            foreach (var check in _config.stages[stage].items) _data.Add(check.shortname, 0);

        }

        private void CheckStage()
        {
            var _data = data.requirmentItems;
            foreach (var item in _config.stages[data.stage].items)
            {
                if (item.amount <= _data[item.shortname]) continue;
                return;
            }

            if (_config.stages.Count > data.stage + 1)
            {
                GenerateDataStage(data.stage + 1);
                return;
            }

            EventSpawn();
            GenerateDataStage(0);
        }

        private Vector3 GetRandomCirclePosition(Vector3 center, int radius)
        {
            var angle = UnityEngine.Random.value * 360 * Mathf.Deg2Rad;
            var x = center.x + radius * Mathf.Sin(angle);
            var z = center.z + radius * Mathf.Cos(angle);
            return new Vector3(x,300,z);
        }
        
        private void EventSpawn()
        {
            var centerSpawn = GetRandomCirclePosition(Vector3.zero,  UnityEngine.Random.Range(0, _config.radSpawn));
            BasePlayer.activePlayerList.FirstOrDefault().Teleport(centerSpawn);
            var giftAmount = UnityEngine.Random.Range(_config.minAmountGifts, _config.maxAmountGifts + 1);
            var angle = 360 / giftAmount;
            for (int i = 0; i < giftAmount; i++)
            {
                var airDrop = GameManager.server.CreateEntity("assets/prefabs/misc/xmas/sleigh/presentdrop.prefab", GetCirclePosition(centerSpawn, 9, angle * i)) as SupplyDrop;
                airDrop.Spawn();
                var rnGift = _config.gifts.GetRandom();
                foreach (var check in airDrop.inventory.itemList.ToArray()) check.DoRemove();
                for (int j = 0; j < UnityEngine.Random.Range(rnGift.minAmountOfItems, rnGift.maxAmountOfItems + 1); j++)
                {
                    var currentItem = rnGift.items.GetRandom();
                    ItemManager.CreateByName(currentItem.shortname, currentItem.amount, currentItem.skinID).MoveToContainer(airDrop.inventory);
                }

                airDrop.transform.position -= new Vector3(0,270,0);
                airDrop.SendNetworkUpdateImmediate();
            }
            
        }
        
        private Vector3 GetCirclePosition(Vector3 center, int radius, float angle)
        {
            var x = center.x + radius * Mathf.Sin(angle);
            var z = center.z + radius * Mathf.Cos(angle);
            return new Vector3(x,300,z);
        }
        #endregion

        #region UI


        #endregion
    }
}