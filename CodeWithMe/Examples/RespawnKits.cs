using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("Respawn Kits", "Orange", "1.0.0")]
    [Description("https://rustworkshop.space/")]
    public class RespawnKits : RustPlugin
    {
        #region Vars

        [PluginReference] private Plugin Kits, Loadout;
        
        private Dictionary<string, RespawnKitEntry> cache = new Dictionary<string, RespawnKitEntry>();

        #endregion
        
        #region Oxide Hooks

        private void Init()
        {
            foreach (var entry in config.list)
            {
                permission.RegisterPermission(entry.perm, this);
            }
        }

        private void OnServerInitialized()
        {
            if (config.cacheTime > 0)
            {
                timer.Every(config.cacheTime, () =>
                {
                    cache.Clear();
                });
            }
        }

        private void OnPlayerRespawned(BasePlayer player)
        {
            if (Loadout == null)
            {
                NextTick(() =>
                {
                    CheckRespawn(player);
                });
            }
            else
            {
                CheckRespawn(player);
            }
        }

        #endregion

        #region Core

        private void CheckRespawn(BasePlayer player)
        {
            var entry = GetEntry(player.UserIDString);
            if (entry == null) {return;}
            ClearInventory(player);
            Kits?.Call("GiveKit", player, entry.kitName);
        }

        private RespawnKitEntry GetEntry(string playerID)
        {
            var value = (RespawnKitEntry) null;
            
            if (config.cacheTime > 0)
            {
                if (cache.TryGetValue(playerID, out value))
                {
                    return value;
                }
                else
                {
                    value = GetEntryFromList(playerID);
                    cache.Add(playerID, value);
                    return value;
                }
            }

            value = GetEntryFromList(playerID);
            return value;
        }

        private RespawnKitEntry GetEntryFromList(string playerID)
        {
            var value = (RespawnKitEntry) null;
            var num = -1;
            
            foreach (var entry in config.list)
            {
                if (permission.UserHasPermission(playerID, entry.perm) == true && entry.priority > num)
                {
                    num = entry.priority;
                    value = entry;
                }
            }

            return value;
        }

        private void ClearInventory(BasePlayer player)
        {
            var items = player.inventory.AllItems().ToList();
            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }
                
                if (item.position < 24)
                {
                    item.GetHeldEntity()?.Kill();
                    item.DoRemove();
                }
            }
        }

        #endregion
        
        #region Configuration 1.1.0

        private static ConfigData config;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Cache clear time (0 to disable)")]
            public int cacheTime;
            
            [JsonProperty(PropertyName = "List")]
            public List<RespawnKitEntry> list = new List<RespawnKitEntry>();
        }

        private class RespawnKitEntry
        {
            [JsonProperty(PropertyName = "Permission")]
            public string perm;

            [JsonProperty(PropertyName = "Priority")]
            public int priority;

            [JsonProperty(PropertyName = "Kit name")]
            public string kitName;
        }

        private ConfigData GetDefaultConfig()
        {
            return new ConfigData
            {
                cacheTime = 600,
                list = new List<RespawnKitEntry>
                {
                    new RespawnKitEntry
                    {
                        perm = "respawnkits.default",
                        priority = 0,
                        kitName = "respawn_default"
                    },
                    new RespawnKitEntry
                    {
                        perm = "respawnkits.vip1",
                        priority = 1,
                        kitName = "respawn_vip1"
                    },
                    new RespawnKitEntry
                    {
                        perm = "respawnkits.vip2",
                        priority = 2,
                        kitName = "respawn_vip2"
                    }
                }
            };
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();

            try
            {
                config = Config.ReadObject<ConfigData>();

                if (config == null)
                {
                    LoadDefaultConfig();
                }
            }
            catch
            {
                PrintError("Configuration file is corrupt! Unloading plugin...");
                Interface.Oxide.RootPluginManager.RemovePlugin(this);
                return;
            }

            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(config);
        }

        #endregion
    }
}