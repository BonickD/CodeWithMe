using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("PrivateHorse", "AhigaO#4485", "1.0.0")]
    internal class PrivateHorse : RustPlugin
    {
        #region Static

        private Configuration _config;
        private Dictionary<uint, ulong> data;

        #endregion

        #region Config

        private class Configuration
        {
            [JsonProperty(PropertyName = "Allow the owner's allies to mount his horse")] 
            public bool forTeam = false;

            [JsonProperty(PropertyName = "Horse limit command")]
            public string hLimitCommand = "hlimit";
            
            [JsonProperty(PropertyName = "Command for claim horse")]
            public string claimCommand = "hclaim";
            
            [JsonProperty(PropertyName = "Command for unclaim horse")]
            public string unClaimCommand = "hrelease";
            
            [JsonProperty(PropertyName = "SteamID for icon in chat messages")]
            public ulong iconID = 0;
            
            [JsonProperty(PropertyName = "Maximum number of horses that a player can brand(The player automatically gets the first permission)", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<string, int> maxHorses = new Dictionary<string, int>
            {
                ["privatehorse.default"] = 2,
                ["privatehorse.vip"] = 3,
                ["privatehorse.premium"] = 5
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
        
        private void LoadData()
        {
            if (Interface.Oxide.DataFileSystem.ExistsDatafile($"{Name}/data"))
                data = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<uint, ulong>>(
                    $"{Name}/data");
            else data = new Dictionary<uint, ulong>();
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
            cmd.AddChatCommand(_config.claimCommand,this,  nameof(cmdChathclaim));
            cmd.AddChatCommand(_config.unClaimCommand,this,  nameof(cmdChathrelease));
            cmd.AddChatCommand(_config.hLimitCommand, this, nameof(cmdChathlimit));
            foreach (var check in _config.maxHorses) if (!permission.PermissionExists(check.Key)) permission.RegisterPermission(check.Key, this);
            LoadData();
            
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null || permission.UserHasPermission(player.UserIDString, _config.maxHorses.First().Key)) return;
            permission.GrantUserPermission(player.UserIDString, _config.maxHorses.First().Key, this);
        }

        private void Unload()
        {
            SaveData();
        }
        
        private object CanMountEntity(BasePlayer player, BaseMountable entity)
        {
            if (player == null || entity == null || !entity.GetParentEntity().IsValid()) return null;
            ulong id;
            if (!data.TryGetValue(entity.net.ID, out id))
            {
                Player.Message(player, GetMsg(player.UserIDString, "CM_NOOWNER", _config.claimCommand), _config.iconID);
                return null;
            }
            if (player.userID == id) return null;
            if (_config.forTeam && player.Team != null && player.Team.members.Contains(id)) return null;
            Player.Message(player, GetMsg(player.UserIDString, "CM_ONLYOWNER"), _config.iconID);
            return false;
        }

        private void OnEntityDeath(RidableHorse horse, HitInfo info)
        {
            if (!horse.IsValid() || !data.ContainsKey(horse.net.ID)) return;
            data.Remove(horse.net.ID);
        }

        #endregion

        #region Commands

        private void cmdChathlimit(BasePlayer player, string command, string[] args)
        {
            Player.Message(player, GetMsg(player.UserIDString, "CM_LIMIT", GetHorsesCount(player.userID).ToString(), GetMaxHorseCount(player.UserIDString).ToString()), _config.iconID);
        }

        private void cmdChathclaim(BasePlayer player, string command, string[] args)
        {
            var horse = player.GetMounted().GetParentEntity() as RidableHorse;
            if (!horse.IsValid())
            {
                Player.Message(player, GetMsg(player.UserIDString, "CM_ONHORSE"), _config.iconID);
                return;
            }
            var id = horse.net.ID;
            if (data.ContainsKey(id))
            {
                Player.Message(player, GetMsg(player.UserIDString, "CM_ALREADYHAVEOWNER"), _config.iconID);
                return;
            }
            if (GetHorsesCount(player.userID) >= GetMaxHorseCount(player.UserIDString))
            {
                Player.Message(player, GetMsg(player.UserIDString, "CM_LIMITISFULL"), _config.iconID);
                return;
            }
            data.Add(id, player.userID);
            Player.Message(player, GetMsg(player.UserIDString, "CM_BECOMEAOWNER", _config.hLimitCommand), _config.iconID);
        }

        private void cmdChathrelease(BasePlayer player, string command, string[] args)
        {
            var entity = player.GetMounted().GetParentEntity() as RidableHorse;
            if (!entity.IsValid())
            {
                Player.Message(player, GetMsg(player.UserIDString, "CM_ONHORSE"), _config.iconID);
                return;
            }

            var id = entity.net.ID;
            if (!data.ContainsKey(id))
            {
                Player.Message(player, GetMsg(player.UserIDString, "CM_NOTANOWNER"), _config.iconID);
                return;
            }
            data.Remove(id);
            Player.Message(player, GetMsg(player.UserIDString, "CM_UNCLAIM"), _config.iconID);
        }

        #endregion

        #region Functions

        private int GetHorsesCount(ulong id)
        {
            var amount = 0;
            foreach (var check in data) if (check.Value == id) amount++;
            return amount;
        }

        private int GetMaxHorseCount(string id)
        {
            var max = 0;
            foreach (var check in _config.maxHorses) if (permission.UserHasPermission(id, check.Key)) max = check.Value;
            return max;
        }

        #endregion

        #region Language

        private string GetMsg(string player, string msg, params object[] args) =>
            string.Format(lang.GetMessage(msg, this, player), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["CM_NOOWNER"] = "This horse has no owner. You can become the owner of this horse (Type /{0}).",
                ["CM_ONLYOWNER"] = "Only the owner of the horse can saddle it.",
                ["CM_ONHORSE"] = "You must be on a horse to use this command.",
                ["CM_LIMITISFULL"] = "Your horse limit is full",
                ["CM_BECOMEAOWNER"] = "You have successfully become the owner of this horse.(Type /{0} for informaton about your horse limit)",
                ["CM_NOTANOWNER"] = "You do not own this horse.",
                ["CM_UNCLAIM"] = "You set the horse free. She can now be branded by other players",
                ["CM_LIMIT"] = "Your horse limit: <color=green>{0}/{1}</color>",
                ["CM_ALREADYHAVEOWNER"] = "This horse already has an owner"
            }, this);
        }
        #endregion
    }
}