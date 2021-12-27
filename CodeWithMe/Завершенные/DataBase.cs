using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Plugins;
using System.Collections.Generic;
using ConVar;
using UnityEngine;
using Oxide.Plugins;
using System;

namespace Oxide.Plugins
{
    [Info("DataBase", "CASHR", "1.0.0")]
    public class DataBase : RustPlugin
    {
        #region Static

        private Configuration _config;
        public static StorageData Data { get; private set; }
        public Dictionary<ulong, PlayerComponents> PlayersComponents = new Dictionary<ulong, PlayerComponents>();


        public enum Fraction
        {
            Warden,
            Colonial,
            None
        }

        #region Classes

        public class PlayerData
        {
            public Fraction fraction = Fraction.None;
            public int playerTime;
        }
        
        public class PlayerComponents
        {
            public PlayerTimer timer;
        }

        public class FractionData
        {
            public List<ulong> members = new List<ulong>();
            
            public int GetFractionTimePlayed()
            {
                var time = 0;
                foreach (var check in members) time += Data.GetPlayerData(check).playerTime;
                return time;
            }
        }

        #endregion

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

        public class StorageData
        {
            public Dictionary<ulong, PlayerData> PlayersData = new Dictionary<ulong, PlayerData>();

            public Dictionary<Fraction, FractionData> FractionsData = new Dictionary<Fraction, FractionData>
            {
                [Fraction.Warden] = new FractionData(),
                [Fraction.Colonial] = new FractionData()
            };


            public void SetFraction(Fraction fraction, ulong id)
            {
                GetPlayerData(id).fraction = fraction;
                GetFractionData(fraction).members.Add(id);
            }
            public PlayerData GetPlayerData(ulong id) => PlayersData[id];
            public FractionData GetFractionData(Fraction fraction) => FractionsData[fraction];
        }

        private void LoadData()
        {
            Data = Interface.Oxide.DataFileSystem.ExistsDatafile($"{Name}/data") ? Interface.Oxide.DataFileSystem.ReadObject<StorageData>($"{Name}/data") : new StorageData();
            Interface.Oxide.DataFileSystem.WriteObject($"{Name}/data", Data);
        }

        private void OnServerSave()
        {
            SaveData();
        }

        private void SaveData()
        {
            if (Data != null) Interface.Oxide.DataFileSystem.WriteObject($"{Name}/data", Data);
        }

        #endregion

        #region OxideHooks

        private void OnServerInitialized()
        {
            LoadData();
            foreach (var check in BasePlayer.activePlayerList) OnPlayerConnected(check);
        }

        private void Unload()
        {
            foreach (var check in BasePlayer.activePlayerList) OnPlayerDisconnected(check);
                
            if (!ServerMgr.Instance.Restarting) SaveData();
            Data = null;
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;
            var playerID = player.userID;
            if (!Data.PlayersData.ContainsKey(playerID)) Data.PlayersData.Add(playerID, new PlayerData());
            PlayersComponents.Add(playerID, new PlayerComponents());
            PlayersComponents[playerID].timer = player.gameObject.AddComponent<PlayerTimer>();
        }

        private void OnPlayerDisconnected(BasePlayer player)
        {
            if (player == null) return;
            var components = PlayersComponents[player.userID];
            components.timer.Kill();
            PlayersComponents.Remove(player.userID);
        }

        #endregion

        #region Commands

        

        #endregion

        #region Functions

        public class PlayerTimer : FacepunchBehaviour
        {
            private ulong id;

            private void Awake()
            {
                id = GetComponent<BasePlayer>().userID;
                InvokeRepeating(Timer, 0, 1);
            }

            private void Timer() => Data.PlayersData[id].playerTime++;
            
            public void Kill()
            {
                CancelInvoke(Timer);
                Destroy(this);
            }
        }

        #endregion
    }
}