using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Configuration;
using System.Globalization;
using UnityEngine;
using Oxide.Game.Rust.Cui;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("Top", "TopPlugin.ru", "1.3.5")]
    [Description("Топ игроков для сервера")]

    class Top : RustPlugin
    {
        private string Layer = "UI_TOP_LAYER";
        //ID Сообщения в чат
        private int MessageNum = 0;
        //Создаем конфиг 
        protected override void LoadDefaultConfig()
        {
            PrintWarning("Создание конфига");
            Config.Clear();
            Config["ЦветПанели"] = "0.0 0.0 0.0 0.8";
            //Config["КлавишаДляБинда"] = "P";
            Config["ВремяМеждуСообщениями"] = 300f;
            Config["ЦветОповещаний"] = "#ffa500";
            SaveConfig();
        }

        //Создаем чат команды
        [ChatCommand("rank")]
        void TurboRankCommand(BasePlayer player, string command)
        {
            var TopPlayer = (from x in Tops where x.UID == player.UserIDString select x).OrderByDescending(x => x.РакетВыпущено + x.ВзрывчатокИспользовано);
			string message="";
            foreach (var top in TopPlayer)
            {
				message=$"<size=14><color=#ffffff>Убийств игроков: <color=#ffa500>{top.УбийствPVP}</color> | Смертей: <color=#ffa500>{top.Смертей}</color></color></size>";
				message+="\n\n"+$"<size=14><color=#ffffff>Ракет выпущено: <color=#ffa500>{top.РакетВыпущено}</color> | Взрывчаток использовано: <color=#ffa500>{top.ВзрывчатокИспользовано}</color></color></size>";
				message+="\n\n"+$"<size=14><color=#ffffff>Ресурсов собрано: <color=#ffa500>{top.РесурсовСобрано}</color> | Животных убито: <color=#ffa500>{top.УбийствЖивотных}</color></color></size>";
				message+="\n\n"+$"<size=14><color=#ffffff>Пуль выпущено: <color=#ffa500>{top.ПульВыпущено}</color> | Стрел выпущено: <color=#ffa500>{top.СтрелВыпущено}</color></color></size>";
				message+="\n\n"+$"<size=14><color=#ffffff>Предметов скрафчено: <color=#ffa500>{top.ПредметовСкрафчено}</color> | Вертолетов сбито: <color=#ffa500>{top.ВертолётовУничтожено}</color></color></size>";
				message+="\n\n"+$"<size=14><color=#ffffff>NPC убито: <color=#ffa500>{top.NPCУбито}</color> | Танков уничтожено: <color=#ffa500>{top.ТанковУничтожено}</color></color></size>";
                //SendReply(player, message);
            }
            player.ChatMessage($"<size=15>Статистика игрока <color=#ffa500>{player.displayName}</color></size>\n\n"+message);
            return;
        }
        [ChatCommand("top")]
        void TurboTopCommand(BasePlayer player, string command, string[] args)
        {
            if (args.Length == 1)
            {
                int n = 0;
                // Очистка статистики игроков
                if (args[0] == "reset")
                {
                    if (!player.IsAdmin)
                    {
                        SendReply(player, $"<size=14><color=#FFA500>Ты кто такой? Давай досвиданье!</color></size>");
                        return;
                    }
                    var TopPlayer = (from x in Tops select x);
                    foreach (var top in TopPlayer)
                    {
                        top.Reset();
                        Saved();
                    }
                    SendReply(player, $"<size=14><color=#FFA500>Статистика игроков обнулена!</color></size>");
                    return;
                }
                if (args[0] == "farm")
                {
                    bool prov = false;
                    player.ChatMessage("<size=14><color=#FF6347>[СТАТИСТИКА]</color> ТОП Фармеров</size>");
                    var TopPlayer = (from x in Tops select x).OrderByDescending(x => x.РесурсовСобрано);
                    foreach (var top in TopPlayer)
                    {
                        n++;
						Puts($"{n}");
                        if (n <= 5)
                        {
							player.SendConsoleCommand("chat.add", new object[] { 0, top.UID, $"<size=14><color=#FFA500>{n}.</color> <color=#FF8C00>{top.Ник}</color> ({top.РесурсовСобрано})</size>" });
                            if (top.UID == player.UserIDString)
                            {
                                prov = true;
                            }
                        }
                    }
                    if (!prov)
                    {
                        player.ChatMessage("...");
                        int i = 0;
                        foreach (var top in TopPlayer)
                        {
                            i++;
                            if (top.UID == player.UserIDString)
                            {
								player.SendConsoleCommand("chat.add", new object[] { 0, player.UserIDString, $"<size=14><color=#FFA500>{i}.</color> <color=#FF8C00>{top.Ник}</color> ({top.РесурсовСобрано})</size>"});
                                //player.SendConsoleCommand("chat.add", player.UserIDString, $"<size=14><color=#FFA500>{i}.</color> <color=#FF8C00>{top.Ник}</color> ({top.РесурсовСобрано})</size>");
                            }
                        }
                    }
                    return;
                }
                if (args[0] == "pvp")
                {
                    bool prov = false;
                    player.ChatMessage("<size=14><color=#FF6347>[СТАТИСТИКА]</color> ТОП Убийств PVP</size>");
                    var TopPlayer = (from x in Tops select x).OrderByDescending(x => x.УбийствPVP);
                    foreach (var top in TopPlayer)
                    {
                        n++;
                        if (n <= 5)
                        {
							player.SendConsoleCommand("chat.add", new object[] { 0, top.UID, $"<size=14><color=#FFA500>{n}.</color> <color=#FF8C00>{top.Ник}</color> ({top.УбийствPVP})</size>"});
                            if (top.UID == player.UserIDString)
                            {
                                prov = true;
                            }
                        }
                    }
                    if (!prov)
                    {
                        player.ChatMessage("<size=14>...</size>");
                        int i = 0;
                        foreach (var top in TopPlayer)
                        {
                            i++;
                            if (top.UID == player.UserIDString)
                            {
								player.SendConsoleCommand("chat.add", new object[] { 0, player.UserIDString, $"<size=14><color=#FFA500>{i}.</color> <color=#FF8C00>{top.Ник}</color> ({top.УбийствPVP})</size>"});                                
                            }
                        }
                    }
                    return;
                }
                if (args[0] == "raid")
                {
                    bool prov = false;
                    player.ChatMessage("<size=14><color=#FF6347>[СТАТИСТИКА]</color> ТОП Рейдеров</size>");
                    var TopPlayer = (from x in Tops select x).OrderByDescending(x => x.РакетВыпущено + x.ВзрывчатокИспользовано);
                    foreach (var top in TopPlayer)
                    {
                        n++;
                        if (n <= 5)
                        {
							player.SendConsoleCommand("chat.add", new object[] { 0, top.UID, $"<size=14><color=#FFA500>{n}.</color> <color=#FF8C00>{top.Ник}</color> ({top.РакетВыпущено + top.ВзрывчатокИспользовано})</size>"});                                
                            //player.SendConsoleCommand("chat.add", top.UID, $"<size=14><color=#FFA500>{n}.</color> <color=#FF8C00>{top.Ник}</color> ({top.РакетВыпущено + top.ВзрывчатокИспользовано})</size>");
                            if (top.UID == player.UserIDString)
                            {
                                prov = true;
                            }
                        }
                    }
                    if (!prov)
                    {
                        player.ChatMessage("<size=14>...</size>");
                        int i = 0;
                        foreach (var top in TopPlayer)
                        {
                            i++;
                            if (top.UID == player.UserIDString)
                            {
								player.SendConsoleCommand("chat.add", new object[] { 0, player.UserIDString, $"<size=14><color=#FFA500>{i}.</color> <color=#FF8C00>{top.Ник}</color> ({top.РакетВыпущено + top.ВзрывчатокИспользовано})</size>"});
                            }
                        }
                    }
                    return;
                }
            }
            else
            {
                CommunityEntity.ServerInstance.ClientRPCEx(new Network.SendInfo() { connection = player.net.connection }, null, "DestroyUI", "Панелька");
                CuiElementContainer elements = CreatePanel("0", false);
                CuiHelper.AddUi(player, elements);
            }
        }

        private void ShowTopUI(BasePlayer player, bool ishook)
        {
            CommunityEntity.ServerInstance.ClientRPCEx(new Network.SendInfo() { connection = player.net.connection }, null, "DestroyUI", "Панелька");
            CuiElementContainer elements = CreatePanel("0", ishook);
            CuiHelper.AddUi(player, elements);
        }
        // GUI панелька
        [ConsoleCommand("top.show")]
        private void TopShowOpenCmd2(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            if (player == null)
                return;
            CommunityEntity.ServerInstance.ClientRPCEx(new Network.SendInfo() { connection = player.net.connection }, null, "DestroyUI", "Панелька");
            string number = arg.Args[0];
            CuiElementContainer elements = CreatePanel(number, false);
            CuiHelper.AddUi(player, elements);
            return;
        }
        CuiElementContainer CreatePanel(string number, bool menu)
        {
            string cvet = Convert.ToString(Config["ЦветПанели"]);
            var elements = new CuiElementContainer();
            if (menu)
            { 
                elements.Add(new CuiPanel
                {
                    Image = {Color = "0 0 0 0"},
                    RectTransform = {AnchorMin = "0.2 0", AnchorMax = "0.8 1"},
                    CursorEnabled = true
                }, "UI_SERVERMENU_WORKINGPANEL", Layer);
            }
            else
            {
               elements.Add(new CuiPanel
                {
                    Image = {Color = "0.2 0.5 0.9 0.5"},
                    RectTransform = {AnchorMin = "0.2 0.13", AnchorMax = "0.8 0.95"},
                    CursorEnabled = true
                }, "Hud", Layer);
            }
            elements.Add(new CuiPanel
            {
                Image = { Color = $"{cvet}" },
                RectTransform = { AnchorMin = "0 0.81", AnchorMax = "1 1" },
            }, Layer);

            elements.Add(new CuiLabel
            {
                Text = { Text = "<color=#00ffff>TOP 10 ИГРОКОВ</color>", FontSize = 30, Align = TextAnchor.MiddleCenter },
                RectTransform = { AnchorMin = "0 0.89", AnchorMax = "1 1" },
            }, Layer);
            if (!menu)
            {
                elements.Add(new CuiButton
                {
                    Button = {Command = "top.exit", Color = $"{cvet}"},
                    RectTransform = {AnchorMin = "0.9 0.90", AnchorMax = "1 1"},
                    Text = {Text = "<color=#FF0000>☒</color>", FontSize = 18, Align = TextAnchor.MiddleCenter}
                }, Layer);
            }
            
            elements.Add(new CuiPanel
            {
                Image = { Color = $"{cvet}" },
                RectTransform = { AnchorMin = "0 0.81", AnchorMax = "0.298 0.8899999" },
            }, Layer);
            elements.Add(new CuiLabel
            {
                Text = { Text = "<color=#ffa500>ИГРОК</color>", FontSize = 20, Align = TextAnchor.MiddleCenter },
                RectTransform = { AnchorMin = "0 0.81", AnchorMax = "0.29 0.8899999" },
            }, Layer);
            elements.Add(new CuiButton
            {
                Text = { Text = "<color=#FF0000>УБИЙСТВА</color>", FontSize = 15, Align = TextAnchor.MiddleCenter },
                Button = { Command = "top.show 1", Color = $"{cvet}" },
                RectTransform = { AnchorMin = "0.30 0.81", AnchorMax = "0.454 0.8899999" }
            }, Layer);
            elements.Add(new CuiButton
            {
                Text = { Text = "<color=#CFDEDD>СМЕРТИ</color>", FontSize = 15, Align = TextAnchor.MiddleCenter },
                Button = { Command = "top.show 2", Color = $"{cvet}" },
                RectTransform = { AnchorMin = "0.455 0.81", AnchorMax = "0.578 0.8899999" }
            }, Layer);
            elements.Add(new CuiButton
            {
                Text = { Text = "<color=#00ff00>ЖИВОТНЫЕ</color>", FontSize = 15, Align = TextAnchor.MiddleCenter },
                Button = { Command = "top.show 3", Color = $"{cvet}" },
                RectTransform = { AnchorMin = "0.58 0.81", AnchorMax = "0.678 0.8899999" }
            }, Layer);
            elements.Add(new CuiButton
            {
                Text = { Text = "<color=#FF0000>ВЗРЫВОВ</color>", FontSize = 15, Align = TextAnchor.MiddleCenter },
                Button = { Command = "top.show 4", Color = $"{cvet}" },
                RectTransform = { AnchorMin = "0.68 0.81", AnchorMax = "0.859 0.8899999" }
            }, Layer);
            elements.Add(new CuiButton
            {
                Text = { Text = "<color=#ffff00>ФЕРМЕР</color>", FontSize = 15, Align = TextAnchor.MiddleCenter },
                Button = { Command = "top.show 5", Color = $"{cvet}" },
                RectTransform = { AnchorMin = "0.86 0.81", AnchorMax = "1 0.8899999" }
            }, Layer);

            string polosa = "0 0 0 0.9";
            int n = 0;
            var TopPlayer = (from x in Tops select x).OrderByDescending(x => x.УбийствPVP).Take(10);
            if (number == "2")
            {
                TopPlayer = (from x in Tops select x).OrderByDescending(x => x.Смертей).Take(10);
            }
            else if (number == "3")
            {
                TopPlayer = (from x in Tops select x).OrderByDescending(x => x.УбийствЖивотных).Take(10);
            }
            else if (number == "4")
            {
                TopPlayer = (from x in Tops select x).OrderByDescending(x => x.РакетВыпущено + x.ВзрывчатокИспользовано).Take(10);
            }
            else if (number == "5")
            {
                TopPlayer = (from x in Tops select x).OrderByDescending(x => x.РесурсовСобрано).Take(10);
            }
            else
            {
                TopPlayer = (from x in Tops select x).OrderByDescending(x => x.УбийствPVP).Take(10);
            }
            foreach (var top in TopPlayer)
            {
                if (n % 2 == 0)
                {
                    polosa = "1 0 0 0.7";
                }
                else
                {
                    polosa = "0.5 0 0.5 0.05";
                }
                elements.Add(new CuiPanel
                {
                    Image = { Color = polosa },
                    RectTransform = { AnchorMin = $"0 {0.72 - (n * 0.08)}", AnchorMax = $"1 {0.8 - (n * 0.08)}" },
                }, Layer);
                elements.Add(new CuiLabel
                {
                    Text = { Text = Convert.ToString(top.Ник), FontSize = 15, Align = TextAnchor.MiddleCenter },
                    RectTransform = { AnchorMin = $"0 {0.72 - (n * 0.08)}", AnchorMax = $"0.29 {0.8 - (n * 0.08)}" },
                }, Layer);
                elements.Add(new CuiLabel
                {
                    Text = { Text = Convert.ToString(top.УбийствPVP), FontSize = 15, Align = TextAnchor.MiddleCenter },
                    RectTransform = { AnchorMin = $"0.3 {0.72 - (n * 0.08)}", AnchorMax = $"0.454 {0.8 - (n * 0.08)}" },
                }, Layer);
                elements.Add(new CuiLabel
                {
                    Text = { Text = Convert.ToString(top.Смертей), FontSize = 15, Align = TextAnchor.MiddleCenter },
                    RectTransform = { AnchorMin = $"0.455 {0.72 - (n * 0.08)}", AnchorMax = $"0.578 {0.8 - (n * 0.08)}" },
                }, Layer);
                elements.Add(new CuiLabel
                {
                    Text = { Text = Convert.ToString(top.УбийствЖивотных), FontSize = 15, Align = TextAnchor.MiddleCenter },
                    RectTransform = { AnchorMin = $"0.58 {0.72 - (n * 0.08)}", AnchorMax = $"0.678 {0.8 - (n * 0.08)}" },
                }, Layer);
                elements.Add(new CuiLabel
                {
                    Text = { Text = $"{ Convert.ToString(top.ВзрывчатокИспользовано + top.РакетВыпущено)}", FontSize = 15, Align = TextAnchor.MiddleCenter },
                    RectTransform = { AnchorMin = $"0.68 {0.72 - (n * 0.08)}", AnchorMax = $"0.85 {0.8 - (n * 0.08)}" },
                }, Layer);
                elements.Add(new CuiLabel
                {
                    Text = { Text = Convert.ToString(top.РесурсовСобрано), FontSize = 15, Align = TextAnchor.MiddleCenter },
                    RectTransform = { AnchorMin = $"0.86 {0.72 - (n * 0.08)}", AnchorMax = $"0.99 {0.8 - (n * 0.08)}" },
                }, Layer);
                n++;
            }
            return elements;
        }

        // Выход с панельки
        [ConsoleCommand("top.exit")]
        private void MagazineOpenCmd2(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null)
                return;
            BasePlayer player = arg.Player();
            CommunityEntity.ServerInstance.ClientRPCEx(new Network.SendInfo() { connection = player.net.connection }, null, "DestroyUI", "Панелька");
        }
        
        private Dictionary<uint, string> LastHeliHit = new Dictionary<uint, string>();
        
        void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (entity is BaseHelicopter && info.Initiator is BasePlayer)
                LastHeliHit[entity.net.ID] = info.InitiatorPlayer.UserIDString;
        }
        
        void OnEntityDeath(BaseCombatEntity victim, HitInfo info)
        {
            if (victim == null || info == null) return;
            BasePlayer victimBP = victim.ToPlayer();
            BasePlayer initiator = info.InitiatorPlayer;
            if (victimBP != null && !IsNPC(victimBP))
            {
                string death = victimBP.UserIDString;
                TopData con = (from x in Tops where x.UID == death select x).FirstOrDefault();
                con.Смертей += 1;
                Saved();
            }
            if (initiator == null)
            {
                if (victim is BaseHelicopter)
                {
                    if (LastHeliHit.ContainsKey(victim.net.ID))
                    {
                        TopData data = Tops.Where(p => p.UID == LastHeliHit[victim.net.ID]).FirstOrDefault();
                        data.ВертолётовУничтожено += 1;
                        LastHeliHit.Remove(victim.net.ID);
                    }
                }
                return;
            }
            if (initiator != null && !IsNPC(initiator))
            {
                string killer = initiator.UserIDString;
                TopData con2 = (from x in Tops where x.UID == killer select x).FirstOrDefault();
                if (IsNPC(victimBP))
                {
                    con2.NPCУбито++;
                    Saved();
                    return;
                }
                if (victim is BaseAnimalNPC)
                {
                    con2.УбийствЖивотных += 1;
                    Saved();
                    return;
                }
                if (victim is BradleyAPC)
                {
                    con2.ТанковУничтожено++;
                    Saved();
                    return;
                }
                if (victimBP != null && victimBP != initiator)
                {
                    con2.УбийствPVP += 1;
                    Saved();
                    return;
                }
            }
            return;
        }
        void OnExplosiveThrown(BasePlayer player, BaseEntity entity)
        {
            TopData con = (from x in Tops where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();
            con.ВзрывчатокИспользовано += 1;
            Saved();
        }
        void OnRocketLaunched(BasePlayer player, BaseEntity entity)
        {
            TopData con = (from x in Tops where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();
            con.РакетВыпущено += 1;
            Saved();
        }
        void OnWeaponFired(BaseProjectile projectile, BasePlayer player, ItemModProjectile mod, ProtoBuf.ProjectileShoot projectiles)
        {
            TopData con = (from x in Tops where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();
            //if (projectile.primaryMagazine.ammoType.itemid == -420273765 || projectile.primaryMagazine.ammoType.itemid == -1280058093)
            if (projectile.primaryMagazine.definition.ammoTypes == Rust.AmmoTypes.BOW_ARROW)
            {
                con.СтрелВыпущено += 1;
            }
            else
            {
                con.ПульВыпущено += 1;
            }
            Saved();
        }
        void OnItemCraftFinished(ItemCraftTask task, Item item)
        {
            if (task.owner is BasePlayer)
            {
                TopData con = (from x in Tops where x.UID == Convert.ToString(task.owner.userID) select x).FirstOrDefault();
                con.ПредметовСкрафчено += 1;
                Saved();
            }
        }

        void OnCollectiblePickup(Item item, BasePlayer player)
        {
            DoGather(player, item);
        }

        void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            if (entity == null || !(entity is BasePlayer) || item == null || dispenser == null) return;
            if (entity.ToPlayer() is BasePlayer)
                DoGather(entity.ToPlayer(), item);
        }

        //Подсчитываем количество собраных ресурсов
        void DoGather(BasePlayer player, Item item)
        {
            if (player == null) return;
            //item.amount = (int)(item.amount);
            TopData con = (from x in Tops where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();
            con.РесурсовСобрано += item.amount;
            Saved();
            return;
        }

        void CreateInfo(BasePlayer player)
        {
            if (player == null) return;
            Tops.Add(new TopData(player.displayName, player.UserIDString));
            Saved();
        }

        //Добавляем игрока в Базу Данных и Биндим клавишу
        void OnPlayerConnected(BasePlayer player)
        {
            //timer.Once(2f, () =>
            //{
            //    player.SendConsoleCommand($"bind {Convert.ToString(Config["КлавишаДляБинда"])} top.show");
            //});
            var check = (from x in Tops where x.UID == player.UserIDString select x).Count();
            if (check == 0) CreateInfo(player);
            //Обновляем игровой ник
            TopData con = (from x in Tops where x.UID == Convert.ToString(player.userID) select x).FirstOrDefault();
            con.Ник = (string)player.displayName;
            Saved();
        }

        //Загружаем TopData.json и проверяем есть ли все игроки в Базе Данных
        void Loaded()
        {
            Tops = Interface.Oxide.DataFileSystem.ReadObject<List<TopData>>("TopData");
            foreach (var player in BasePlayer.activePlayerList)
            {
                var check = (from x in Tops where x.UID == player.UserIDString select x).Count();
                if (check == 0) CreateInfo(player);
            }
            //что то делаем
            timer.Repeat(Convert.ToInt32(Config["ВремяМеждуСообщениями"]), 0, () =>
            {
                MessageNum++;
                TopData data = null;
                switch (MessageNum)
                {
                    case 1:
                        data = Tops.OrderByDescending(p => p.УбийствPVP).FirstOrDefault();
                        if (data != null) if (data.УбийствPVP>0){
                            Server.Broadcast($"<size=16><color={Convert.ToString(Config["ЦветОповещаний"])}>ТОП</color>-<color=red>Киллер</color> <color=#00fff3>{data.Ник}</color> <color=yellow>|</color> <color=red>{data.УбийствPVP}</color> <color=yellow>|</color></size>");
						}else MessageNum++;
                        break;
                    case 2:
                        data = Tops.OrderByDescending(p => p.ВзрывчатокИспользовано + p.РакетВыпущено).FirstOrDefault();
                        if (data != null) if ((data.ВзрывчатокИспользовано + data.РакетВыпущено)>0){
                            Server.Broadcast($"<size=16><color={Convert.ToString(Config["ЦветОповещаний"])}>ТОП</color>-<color=#F64A46>Рейдер</color> <color=#00fff3>{data.Ник}</color> <color=yellow>|</color> <color=red>{data.ВзрывчатокИспользовано + data.РакетВыпущено}</color> <color=yellow>|</color></size>");
						}else MessageNum++;
                        break;
                    case 3:
                        data = Tops.OrderByDescending(p => p.УбийствЖивотных).FirstOrDefault();
                        if (data != null) if (data.УбийствЖивотных>0){
                            Server.Broadcast($"<size=16><color={Convert.ToString(Config["ЦветОповещаний"])}>ТОП</color>-<color=#9ACEEB>Живодер</color> <color=#00fff3>{data.Ник}</color> <color=yellow>|</color> <color=red>{data.УбийствЖивотных}</color> <color=yellow>|</color></size>");
						}else MessageNum++;
                        break;
                    case 4:
                        data = Tops.OrderByDescending(p => p.ПульВыпущено).FirstOrDefault();
                        if (data != null) if (data.ПульВыпущено>0){
                            Server.Broadcast($"<size=16><color={Convert.ToString(Config["ЦветОповещаний"])}>ТОП</color>-<color=#FFB02E>Пулемет</color> <color=#00fff3>{data.Ник}</color> <color=yellow>|</color> <color=red>{data.ПульВыпущено}</color> <color=yellow>|</color></size>");
						}else MessageNum++;
                        break;
                    case 5:
                        data = Tops.OrderByDescending(p => p.СтрелВыпущено).FirstOrDefault();
                        if (data != null) if (data.СтрелВыпущено>0){
                            Server.Broadcast($"<size=16><color={Convert.ToString(Config["ЦветОповещаний"])}>ТОП</color>-<color=#FAE7B5>Лучник</color> <color=#00fff3>{data.Ник}</color> <color=yellow>|</color> <color=red>{data.СтрелВыпущено}</color> <color=yellow>|</color></size>");
						}else MessageNum++;
                        break;
                    case 6:
                        data = Tops.OrderByDescending(p => p.Смертей).FirstOrDefault();
                        if (data != null) if (data.Смертей>0){
                            Server.Broadcast($"<size=16><color={Convert.ToString(Config["ЦветОповещаний"])}>ТОП</color>-<color=#DBD7D2>Терпила</color> <color=#00fff3>{data.Ник}</color> <color=yellow>|</color> <color=red>{data.Смертей}</color> <color=yellow>|</color></size>");
						}else MessageNum++;
                        break;
                    case 7:
                        data = Tops.OrderByDescending(p => p.ПредметовСкрафчено).FirstOrDefault();
                        if (data != null) if (data.ПредметовСкрафчено>0){
                            Server.Broadcast($"<size=16><color={Convert.ToString(Config["ЦветОповещаний"])}>ТОП</color>-<color=#DB39B2>Крафтер</color> <color=#00fff3>{data.Ник}</color> <color=yellow>|</color> <color=red>{data.ПредметовСкрафчено}</color> <color=yellow>|</color></size>");
						}else MessageNum++;
                        break;
                    case 8:
                        data = Tops.OrderByDescending(p => p.РесурсовСобрано).FirstOrDefault();
                        if (data != null) if (data.РесурсовСобрано>0){
                            Server.Broadcast($"<size=16><color={Convert.ToString(Config["ЦветОповещаний"])}>ТОП</color>-<color=yellow>Фермер</color> <color=#00fff3>{data.Ник}</color> <color=yellow>|</color> <color=red>{data.РесурсовСобрано}</color> <color=yellow>|</color></size>");
						}else MessageNum++;
                        break;
                    case 9:
                        data = Tops.OrderByDescending(p => p.ВертолётовУничтожено).FirstOrDefault();
                        if (data != null) if (data.ВертолётовУничтожено>0){
                            Server.Broadcast($"<size=16><color={Convert.ToString(Config["ЦветОповещаний"])}>ТОП</color>-<color=#00ff95>Вертолётчик</color> <color=#00fff3>{data.Ник}</color> <color=yellow>|</color> <color=red>{data.ВертолётовУничтожено}</color> <color=yellow>|</color></size>");
						}else MessageNum++;
                        break;
                    case 10:
                        data = Tops.OrderByDescending(p => p.NPCУбито).FirstOrDefault();
                        if (data != null) if (data.NPCУбито>0){
                            Server.Broadcast($"<size=16><color={Convert.ToString(Config["ЦветОповещаний"])}>ТОП</color>-<color=#00dcff>NPC</color> <color=#00fff3>{data.Ник}</color> <color=yellow>|</color> <color=red>{data.NPCУбито}</color> <color=yellow>|</color></size>");
						}else MessageNum++;
                        break;
                    case 11:
                        data = Tops.OrderByDescending(p => p.ТанковУничтожено).FirstOrDefault();
                        if (data != null) if (data.ТанковУничтожено>0){
                            Server.Broadcast($"<size=15><color={Convert.ToString(Config["ЦветОповещаний"])}>ТОП</color>-<color=#ff5200>Танкист</color> <color=#00fff3>{data.Ник}</color> <color=yellow>|</color> <color=red>{data.ТанковУничтожено}</color> <color=yellow>|</color></size>");
						}else MessageNum++;
                        MessageNum = 0;
                        break;
                }
            });
        }

        //Сохраняем инфу в TopData
        void Saved()
        {
            Interface.Oxide.DataFileSystem.WriteObject("TopData", Tops);
        }
        private bool IsNPC(BasePlayer player)
        {
            if (player == null) return false;
            //BotSpawn
            if (player is NPCPlayer)
                return true;
            //HumanNPC
            if (!(player.userID >= 76560000000000000L || player.userID <= 0L))
                return true;
            return false;
        }
        public List<TopData> Tops = new List<TopData>();
        public class TopData
        {
            public TopData(string Ник, string UID)
            {
                this.Ник = Ник;
                this.UID = UID;
                this.РакетВыпущено = 0;
                this.УбийствPVP = 0;
                this.ВзрывчатокИспользовано = 0;
                this.УбийствЖивотных = 0;
                this.ПульВыпущено = 0;
                this.СтрелВыпущено = 0;
                this.Смертей = 0;
                this.ПредметовСкрафчено = 0;
                this.РесурсовСобрано = 0;
                this.ВертолётовУничтожено = 0;
                this.NPCУбито = 0;
                this.ТанковУничтожено = 0;
            }
            public void Reset()
            {
                this.РакетВыпущено = 0;
                this.УбийствPVP = 0;
                this.ВзрывчатокИспользовано = 0;
                this.УбийствЖивотных = 0;
                this.ПульВыпущено = 0;
                this.СтрелВыпущено = 0;
                this.Смертей = 0;
                this.ПредметовСкрафчено = 0;
                this.РесурсовСобрано = 0;
                this.ВертолётовУничтожено = 0;
                this.NPCУбито = 0;
                this.ТанковУничтожено = 0;
            }

            public string Ник { get; set; }
            public string UID { get; set; }
            public int РакетВыпущено { get; set; }
            public int УбийствPVP { get; set; }
            public int ВзрывчатокИспользовано { get; set; }
            public int УбийствЖивотных { get; set; }
            public int ПульВыпущено { get; set; }
            public int СтрелВыпущено { get; set; }
            public int Смертей { get; set; }
            public int ПредметовСкрафчено { get; set; }
            public int РесурсовСобрано { get; set; }
            public int ВертолётовУничтожено { get; set; }
            public int ТанковУничтожено { get; set; }
            public int NPCУбито { get; set; }
        }
        
        private int GetKills(string playerid)
        {
            for (int i = 0; i < Tops.Count; i++)
            {
                if (Tops[i].UID != playerid) continue;
                return Tops[i].УбийствPVP;
            }
            return 0;
        }
        private int GetKillsNPC(string playerid)
        {
            for (int i = 0; i < Tops.Count; i++)
            {
                if (Tops[i].UID != playerid) continue;
                return Tops[i].NPCУбито;
            }
            return 0;
        }
        private int GetKillsAnimals(string playerid)
        {
            for (int i = 0; i < Tops.Count; i++)
            {
                if (Tops[i].UID != playerid) continue;
                return Tops[i].УбийствЖивотных;
            }
            return 0;
        }
        private int GetKillsTanks(string playerid)
        {
            for (int i = 0; i < Tops.Count; i++)
            {
                if (Tops[i].UID != playerid) continue;
                return Tops[i].ТанковУничтожено;
            }
            return 0;
        }
        private int GetKillsHeli(string playerid)
        {
            for (int i = 0; i < Tops.Count; i++)
            {
                if (Tops[i].UID != playerid) continue;
                return Tops[i].ВертолётовУничтожено;
            }
            return 0;
        }
        private int GetDeaths(string playerid)
        {
            for (int i = 0; i < Tops.Count; i++)
            {
                if (Tops[i].UID != playerid) continue;
                return Tops[i].Смертей;
            }
            return 0;
        }
        private int GetExpUse(string playerid)
        {
            for (int i = 0; i < Tops.Count; i++)
            {
                if (Tops[i].UID != playerid) continue;
                return Tops[i].ВзрывчатокИспользовано;
            }
            return 0;
        }
        private int GetBulletsUse(string playerid)
        {
            for (int i = 0; i < Tops.Count; i++)
            {
                if (Tops[i].UID != playerid) continue;
                return Tops[i].ПульВыпущено;
            }
            return 0;
        }
        private int GetRocketUse(string playerid)
        {
            for (int i = 0; i < Tops.Count; i++)
            {
                if (Tops[i].UID != playerid) continue;
                return Tops[i].РакетВыпущено;
            }
            return 0;
        }
        private int GetArrowUse(string playerid)
        {
            for (int i = 0; i < Tops.Count; i++)
            {
                if (Tops[i].UID != playerid) continue;
                return Tops[i].СтрелВыпущено;
            }
            return 0;
        }
        private int GetResourses(string playerid)
        {
            for (int i = 0; i < Tops.Count; i++)
            {
                if (Tops[i].UID != playerid) continue;
                return Tops[i].РесурсовСобрано;
            }
            return 0;
        }
        private int GetCrafted(string playerid)
        {
            for (int i = 0; i < Tops.Count; i++)
            {
                if (Tops[i].UID != playerid) continue;
                return Tops[i].ПредметовСкрафчено;
            }
            return 0;
        }
    }
}
