//Requires: DataBase

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Fraction", "CASHR", "1.0.0")]
    public class Fraction : RustPlugin
    {
        #region Static

        private Configuration _config;
        public static int BalanceTime = 0;

        #endregion

        #region Config

        private class Configuration
        {
            [JsonProperty(PropertyName = "Разница в игроом времени фракций, при котором нельзя вступить во фракцию, в которой больше онлайн по времени(В секундах)")]
            public int BalanceTime = 86400;
            
            [JsonProperty(PropertyName = "Настройка фракций", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<DataBase.Fraction, FractionSettings> frationSettings = new Dictionary<DataBase.Fraction, FractionSettings>
            {
                [DataBase.Fraction.Warden] = new FractionSettings
                {
                    spawnPosition = new List<Vector3>(),
                    skins = new Dictionary<string, ulong>
                    {
                        
                    },
                    replacementItems = new Dictionary<string, string>
                    {

                    }
                },
                [DataBase.Fraction.Colonial] = new FractionSettings
                {
                    spawnPosition = new List<Vector3>(),
                    skins = new Dictionary<string, ulong>
                    {

                    },
                    replacementItems = new Dictionary<string, string>
                    {

                    }
                }
            };
        }

        public class FractionSettings
        {
            [JsonProperty(PropertyName = "Позиции спавна игрока на базе фракции(Желательно добавить хотя бы 5 штук)", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<Vector3> spawnPosition;

            [JsonProperty(PropertyName = "Скины фракции", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<string, ulong> skins;

            [JsonProperty(PropertyName = "Заменяемые предметы у фракции(К примеру Хазмат)", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<string, string> replacementItems;
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
        
        private void OnServerInitialized()
        {
            BalanceTime = _config.BalanceTime;
        }

        private void Unload()
        {
            
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            
        }

        #endregion

        #region Commands

        [ChatCommand("setfractionspawn")]
        private void cmdChatsetfractionspawn(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin || args.Length != 1) return;
            switch (args[0])
            {
                case "warden":
                    _config.frationSettings[DataBase.Fraction.Warden].spawnPosition.Add(player.transform.position);
                    break;
                case "colonial":
                    _config.frationSettings[DataBase.Fraction.Colonial].spawnPosition.Add(player.transform.position);
                    break;
                default:
                    player.ChatMessage("Фракция не найдена");
                    return;
            }
            player.ChatMessage("Позиция спавна добавлена");
            SaveConfig();
        }

        [ConsoleCommand("UI_FRACTION")]
        private void cmdConsoleUI_FRACTION(ConsoleSystem.Arg arg)
        {
            if (arg?.Args == null || arg.Args.Length < 1) return;
            var player = arg.Player();
            switch (arg.Args[0])
            {
                case "setfration":
                    if (arg.Args[1] == "w")
                    {
                        DataBase.Data.SetFraction(DataBase.Fraction.Warden, player.userID);
                        player.Teleport(_config.frationSettings[DataBase.Fraction.Warden].spawnPosition.GetRandom());
                    }
                    else
                    {
                        DataBase.Data.SetFraction(DataBase.Fraction.Colonial, player.userID);
                        player.Teleport(_config.frationSettings[DataBase.Fraction.Colonial].spawnPosition.GetRandom());
                    }
                    break;
            }
        }

        #endregion

        #region Functions

        public static bool CanJoinToWarden()
        {
            var timeForWarden = DataBase.Data.FractionsData[DataBase.Fraction.Warden].GetFractionTimePlayed();
            var timeForColonial = DataBase.Data.FractionsData[DataBase.Fraction.Colonial].GetFractionTimePlayed();
            return timeForWarden - timeForColonial < BalanceTime;
        }
            
        public static bool CanJoinToColonials()
        {
            var timeForWarden = DataBase.Data.FractionsData[DataBase.Fraction.Warden].GetFractionTimePlayed();
            var timeForColonial = DataBase.Data.FractionsData[DataBase.Fraction.Colonial].GetFractionTimePlayed();
            return timeForColonial - timeForWarden < BalanceTime;
        }

        #endregion
    }
}