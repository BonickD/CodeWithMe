using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("DiamondDrop", "AhigaO#4485", "1.0.0")]
    internal class DiamondDrop : RustPlugin
    {
        #region Static
        
        private Configuration _config;

        private class DropChange
        {
            [JsonProperty(PropertyName = "Шанс выпадения в %")]
            public int change;
            [JsonProperty(PropertyName = "Минимальеное количество")]
            public int minAmount;
            [JsonProperty(PropertyName = "Максимальное количество")]
            public int maxAmount;
        }
        
        
        #endregion

        #region Config

        private class Configuration
        {
            [JsonProperty(PropertyName = "ShortName Алмаза")]
            public string shortName; 
            [JsonProperty(PropertyName = "SkinID Алмаза")]
            public ulong skinID;

            [JsonProperty(PropertyName = "LootCrate - шанс выпадения", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<string, DropChange> DropCHanges = new Dictionary<string, DropChange>
            {
                ["crate_elite"] = new DropChange
                {
                    change = 50,
                    minAmount = 1,
                    maxAmount = 5
                }
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
        
        #region OxideHooks
        private void OnLootSpawn(LootContainer container)
        {
            if (container == null) return;
            var entity = container.GetEntity();
            if (entity == null) return;
            foreach (var check in _config.DropCHanges)
            {
                if (!check.Key.Contains(entity.ShortPrefabName) ||  UnityEngine.Random.Range(0, 101) > check.Value.change) continue;
                ItemManager.CreateByName(_config.shortName, UnityEngine.Random.Range(check.Value.minAmount, check.Value.maxAmount + 1), _config.skinID).MoveToContainer(container.inventory);
            }
        }
       
        #endregion
    }
}