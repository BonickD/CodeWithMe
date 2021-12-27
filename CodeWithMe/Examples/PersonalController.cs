using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("PersonalController", "CASHR#6906", "1.0.0")]
    internal class PersonalController : RustPlugin
    {
        #region Static
		[PluginReference] private Plugin DiscordMessages, ImageLibrary;
		 private StoredData storedData;
		  private static PersonalController ins;
        private Dictionary<ulong, GroupData> groupCreator = new Dictionary<ulong, GroupData>();        
        private enum MenuType { Permissions, Groups }
        private string[] charFilter = new string[] { "~", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        private enum SelectType { Player, String }
        private enum PermSub { View, Player, Group }
        private enum CommSub { Chat, Console, Give }
        private Hash<ulong, Timer> popupTimers = new Hash<ulong, Timer>();
        private Dictionary<string, string> uiColors = new Dictionary<string, string>();
        private List<KeyValuePair<string, bool>> permissionList = new List<KeyValuePair<string, bool>>();
        private enum GroupSub { View, UserGroups, AddGroup, RemoveGroup }
        private Dictionary<ulong, SelectionData> selectData = new Dictionary<ulong, SelectionData>();       
        private PluginConfig _config;
        private static string LayerActive = $"UI_PersonalController_ACTIVEPLAYER";
        private List<ulong> ActivePlayer = new List<ulong>();
        private void SendMsg(string msg) =>  DiscordMessages?.Call("API_SendTextMessage", _config.WebHook, msg);
        private class SelectionData
        {
            public MenuType menuType;
            public string subType, selectDesc = string.Empty, returnCommand = string.Empty, target1_Name = string.Empty, target1_Id = string.Empty, target2_Name = string.Empty, target2_Id = string.Empty, character = string.Empty;
            public bool requireTarget1, requireTarget2, isOnline, isGroup, forceOnline;
            public int pageNum, listNum;
        }
		 private class StoredData
        {
            public Hash<string, double> offlinePlayers = new Hash<string, double>();

            public void AddOfflinePlayer(string userId) => offlinePlayers[userId] = CurrentTime();

            public void OnPlayerInit(string userId)
            {
                if (offlinePlayers.ContainsKey(userId))
                    offlinePlayers.Remove(userId);                
            }

            public void RemoveOldPlayers()
            {
                double currentTime = CurrentTime();

                for (int i = offlinePlayers.Count - 1; i >= 0; i--)
                {
                    var user = offlinePlayers.ElementAt(i);
                    if (currentTime - user.Value > 604800)
                        offlinePlayers.Remove(user);
                }
            }

            public List<IPlayer> GetOfflineList() => ins.covalence.Players.All.Where(x => offlinePlayers.ContainsKey(x.Id)).ToList();

            public double CurrentTime() => DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;            
        }
        #endregion

        #region Config

        public class PluginConfig
        {
            [JsonProperty("WebHook Discord")] public string WebHook;
            [JsonProperty("OFFSET MIN")] public string OffsetMin;
            [JsonProperty("OFFSET MAX")] public string OffsetMax;
            [JsonProperty("Anchor MIN")] public string AnchorMin;
            [JsonProperty("Anchor MAX")] public string AnchorMax;
            [JsonProperty("Image")] public string Image;
        }

        protected override void LoadDefaultConfig()
        {
            _config = new PluginConfig
            {
                WebHook = "",
                OffsetMin = "-260 20",
                OffsetMax = "-210 70",
                AnchorMin = "1 0",
                AnchorMax = "1 0",
                Image = "https://i.imgur.com/Ub6Ecf4.png"
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

         private void OnServerInitialized()
        {
            ins = this;
            if (storedData == null || storedData.offlinePlayers == null)
                storedData = new StoredData();
            else storedData.RemoveOldPlayers();
            SetUIColors();
			 permission.RegisterPermission("personalcontroller.use", this);
            permission.RegisterPermission("personalcontroller.permissions", this);
            permission.RegisterPermission("personalcontroller.groups", this);
             lang.RegisterMessages(Messages, this);
			 UpdatePermissionList();
             ImageLibrary?.Call("AddImage", _config.Image, _config.Image);
            foreach (BasePlayer player in BasePlayer.activePlayerList)
			{				 
            CuiHelper.DestroyUi(player, UIElement);
            CuiHelper.DestroyUi(player, UIMain);
            CuiHelper.DestroyUi(player, UIPopup);        
				OnPlayerConnected(player);
			}
        }
		
		private void OnPlayerConnected(BasePlayer player)
		{
		     storedData.OnPlayerInit(player.UserIDString);		
		}
		
        void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (!ActivePlayer.Contains(player.userID)) return;
            var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month,DateTime.Now.Day, DateTime.Now.Hour,DateTime.Now.Minute, DateTime.Now.Second);
            SendMsg($"The {player.displayName}/{player.userID} attendant left the server at {date}");
        }
        void OnLootEntity(BasePlayer player, BaseEntity entity)
        {
            if (player == null) return;
            if (!ActivePlayer.Contains(player.userID)) return;
            NextTick(() =>
            {
                player.EndLooting();
            });
        }
        object OnEntityTakeDamage(BasePlayer player, HitInfo info)
        {
            if (player == null || info == null) return null;
            var attacker = info?.InitiatorPlayer;
            if (attacker == null) return null;
            if (!ActivePlayer.Contains(attacker.userID)) return null;
            return false; 
        }

        [ChatCommand("staff")]
        private void cmdChatstaff(BasePlayer player, string command, string[] args)
        {
            if (args == null || args?.Count() == 0)
            {
                player.ChatMessage("Use command /staff on/off");
                return;
            }
            switch (args[0])
            {
                case "on":
                    if (ActivePlayer.Contains(player.userID)) return;
                    ActivePlayer.Add(player.userID);
                    player.ChatMessage("You enable StaffMode");
                    ShowActivePlayer(player);
                    var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month,DateTime.Now.Day, DateTime.Now.Hour,DateTime.Now.Minute, DateTime.Now.Second);
                    SendMsg($"[{date}] Player {player.displayName} / {player.userID} enable staff mode");
                    break;
                case "off":
                    ActivePlayer.Remove(player.userID);
                    player.ChatMessage("You disabled StaffMode");
                    CuiHelper.DestroyUi(player, LayerActive);
                    var dat = new DateTime(DateTime.Now.Year, DateTime.Now.Month,DateTime.Now.Day, DateTime.Now.Hour,DateTime.Now.Minute, DateTime.Now.Second);
                    SendMsg($"[{dat}] Player {player.displayName} / {player.userID} disabled staff mode");
                    break;
                default:
                    player.ChatMessage("Use command /staff on/off");
                    break;
            }
        }
        #endregion


        #region UI
        const string UIMain = "AMUI_MenuMain";        
        const string UIElement = "AMUI_MenuElement";
        const string UIPopup = "AMUI_PopupMessage";
              
        private void Openpersonalcontroller(BasePlayer player)
        {
            var container = UI.Container(UIMain, uiColors["bg1"], "0.05 0.08", "0.95 0.92", true);
            CuiHelper.AddUi(player, container);
CreateMenuPermissions(player);
        }

        private void CreateMenuButtons(ref CuiElementContainer container, MenuType menuType, string playerId)
        {
            UI.Panel(ref container, UIElement, uiColors["bg3"], "0.005 0.925", "0.995 0.99");
            UI.Label(ref container, UIElement, "PERSONAL CONTROLLER", 24, "0.02 0.93", "0.25 0.98", TextAnchor.UpperLeft);
            
            if (HasPermission(playerId, "personalcontroller.permissions"))
                UI.Button(ref container, UIElement, menuType == MenuType.Permissions ? uiColors["button3"] : uiColors["button1"], msg(MenuType.Permissions.ToString(), playerId), 16, "0.425 0.93", "0.575 0.985", menuType == MenuType.Permissions ? "" : "amui.switchelement permissions");
            if (HasPermission(playerId, "personalcontroller.groups"))
                UI.Button(ref container, UIElement, menuType == MenuType.Groups ? uiColors["button3"] : uiColors["button1"], msg(MenuType.Groups.ToString(), playerId), 16, "0.58 0.93", "0.73 0.985", menuType == MenuType.Groups ? "" : "amui.switchelement groups");
            UI.Button(ref container, UIElement, uiColors["button1"], msg("exit", playerId), 16, "0.855 0.93", "0.985 0.985", "amui.switchelement exit");
        }

        private void CreateSubMenu(ref CuiElementContainer container, MenuType menuType, string playerId, string subType)
        {
            UI.Panel(ref container, UIElement, uiColors["bg3"], "0.005 0.87", "0.995 0.92");
            switch (menuType)
            {
                case MenuType.Permissions:
                    PermSub permSub = ParseType<PermSub>(subType);
                    UI.Button(ref container, UIElement, permSub == PermSub.View ? uiColors["button3"] : uiColors["button1"], msg("view", playerId), 16, "0.27 0.875", "0.42 0.915", permSub == PermSub.View ? "" : "amui.switchelement permissions view");
                    UI.Button(ref container, UIElement, permSub == PermSub.Player ? uiColors["button3"] : uiColors["button1"], msg("player", playerId), 16, "0.425 0.875", "0.575 0.915", permSub == PermSub.Player ? "" : "amui.switchelement permissions player");
                    UI.Button(ref container, UIElement, permSub == PermSub.Group ? uiColors["button3"] : uiColors["button1"], msg("group", playerId), 16, "0.58 0.875", "0.73 0.915", permSub == PermSub.Group ? "" : "amui.switchelement permissions group");
                    return;
                case MenuType.Groups:
                    GroupSub groupSub = ParseType<GroupSub>(subType);
                    UI.Button(ref container, UIElement, groupSub == GroupSub.View ? uiColors["button3"] : uiColors["button1"], msg("view", playerId), 16, "0.2025 0.875", "0.3525 0.915", groupSub == GroupSub.View ? "" : "amui.switchelement groups view");
                    UI.Button(ref container, UIElement, groupSub == GroupSub.AddGroup ? uiColors["button3"] : uiColors["button1"], msg("addgroup", playerId), 16, "0.3575 0.875", "0.4975 0.915", groupSub == GroupSub.AddGroup ? "" : "amui.switchelement groups addgroup");
                    UI.Button(ref container, UIElement, groupSub == GroupSub.RemoveGroup ? uiColors["button3"] : uiColors["button1"], msg("removegroup", playerId), 16, "0.5025 0.875", "0.6525 0.915", groupSub == GroupSub.RemoveGroup ? "" : "amui.switchelement groups removegroup");
                    UI.Button(ref container, UIElement, groupSub == GroupSub.UserGroups ? uiColors["button3"] : uiColors["button1"], msg("usergroups", playerId), 16, "0.6575 0.875", "0.8075 0.915", groupSub == GroupSub.UserGroups ? "" : "amui.switchelement groups usergroups");
                    return;
            }
        }

        private void CreateMenuPermissions(BasePlayer player, int page = 0, string filter = "")
        {
            var container = UI.Container(UIElement, "0 0 0 0", "0.05 0.08", "0.95 0.92");
            CreateMenuButtons(ref container, MenuType.Permissions, player.UserIDString);
            CreateSubMenu(ref container, MenuType.Permissions, player.UserIDString, "view");
            CreateCharacterFilter(ref container, player.userID, filter, $"amui.switchelement permissions view 0");

            List<KeyValuePair<string, bool>> permList = new List<KeyValuePair<string, bool>>(permissionList);
            if (!string.IsNullOrEmpty(filter) && filter != "~")
                permList = permList.Where(x => x.Key.StartsWith(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            permList.OrderBy(x => x.Key);

            if (page > 0)
                UI.Button(ref container, UIElement, uiColors["button1"], msg("back", player.UserIDString), 16, "0.015 0.875", "0.145 0.915", $"amui.switchelement permissions view {page - 1}");
            if (permList.Count > 72 && permList.Count > (72 * page + 72))
                UI.Button(ref container, UIElement, uiColors["button1"], msg("next", player.UserIDString), 16, "0.855 0.875", "0.985 0.915", $"amui.switchelement permissions view {page + 1}");

            int count = 0;
            for (int i = page * 72; i < permList.Count; i++)
            {
                KeyValuePair<string, bool> perm = permList[i];
                float[] position = CalculateButtonPosVert(count);
                
                if (!perm.Value)
                {
                    UI.Panel(ref container, UIElement, uiColors["button2"], $"{position[0]} {position[1]}", $"{position[2]} {position[3]}");
                    UI.Label(ref container, UIElement, $"{perm.Key}", 12, $"{position[0]} {position[1]}", $"{position[2]} {position[3]}");
                }
                else
                {    
                    UI.Panel(ref container, UIElement, uiColors["button1"], $"{position[0]} {position[1]}", $"{position[2]} {position[3]}");
                    UI.Label(ref container, UIElement, $"{perm.Key}", 10, $"{position[0]} {position[1]}", $"{position[2]} {position[3]}");
                }
                ++count;

                if (count >= 72)
                    break;
            }

            CuiHelper.DestroyUi(player, UIElement);
            CuiHelper.AddUi(player, container);
        }

        private void CreateMenuGroups(BasePlayer player, GroupSub subType, int page = 0, string filter = "")
        {
            var container = UI.Container(UIElement, "0 0 0 0", "0.05 0.08", "0.95 0.92");
            CreateMenuButtons(ref container, MenuType.Groups, player.UserIDString);
            CreateSubMenu(ref container, MenuType.Groups, player.UserIDString, subType.ToString());

            switch (subType)
            {
                case GroupSub.View:
                    List<string> groupList = GetGroups();
                    groupList.Sort();

                    if (page > 0)
                        UI.Button(ref container, UIElement, uiColors["button1"], msg("back", player.UserIDString), 16, "0.015 0.875", "0.145 0.915", $"amui.switchelement groups view {page - 1}");
                    if (groupList.Count > 72 && groupList.Count > (72 * page + 72))
                        UI.Button(ref container, UIElement, uiColors["button1"], msg("next", player.UserIDString), 16, "0.855 0.875", "0.985 0.915", $"amui.switchelement groups view {page + 1}");

                    int count = 0;
                    for (int i = page * 72; i < groupList.Count; i++)
                    {
                        string groupId = groupList[i];
                        float[] position = CalculateButtonPos(count);

                        UI.Panel(ref container, UIElement, uiColors["button1"], $"{position[0]} {position[1]}", $"{position[2]} {position[3]}");
                        UI.Label(ref container, UIElement, $"{groupId}", 10, $"{position[0]} {position[1]}", $"{position[2]} {position[3]}");
                        ++count;

                        if (count >= 72)
                            break;
                    }
                    break;
                case GroupSub.UserGroups:
                    break;                
                case GroupSub.AddGroup:
                    GroupData groupData;
                    if (!groupCreator.TryGetValue(player.userID, out groupData))
                    {
                        groupCreator.Add(player.userID, new GroupData());
                        groupData = groupCreator[player.userID];
                    }

                    UI.Label(ref container, UIElement, msg("inputhelper", player.UserIDString), 18, "0.1 0.75", "0.9 0.85");
                                       
                    UI.Label(ref container, UIElement, msg("groupname", player.UserIDString), 16, "0.1 0.62", "0.3 0.7", TextAnchor.MiddleLeft);
                    UI.Label(ref container, UIElement, msg("uiwarning", player.UserIDString), 8, "0.1 0.15", "0.9 0.2", TextAnchor.MiddleLeft);
                    UI.Panel(ref container, UIElement, uiColors["bg3"], "0.3 0.63", "0.9 0.69");
                    if (string.IsNullOrEmpty(groupData.Title))
                        UI.Input(ref container, UIElement, "", groupData.Title, 16, "amui.registergroup input name", "0.32 0.63", "0.9 0.69");
                    else UI.Label(ref container, UIElement, groupData.Title, 16, "0.32 0.63", "0.9 0.69", TextAnchor.MiddleLeft);

                    UI.Label(ref container, UIElement, msg("grouptitle", player.UserIDString), 16, "0.1 0.54", "0.3 0.62", TextAnchor.MiddleLeft);
                    UI.Panel(ref container, UIElement, uiColors["bg3"], "0.3 0.55", "0.9 0.61");
                    if (string.IsNullOrEmpty(groupData.Title))
                        UI.Input(ref container, UIElement, "", groupData.Title, 16, "amui.registergroup input title", "0.32 0.55", "0.9 0.61");
                    else UI.Label(ref container, UIElement, groupData.Title, 16, "0.32 0.55", "0.9 0.61", TextAnchor.MiddleLeft);

                    UI.Label(ref container, UIElement, msg("grouprank", player.UserIDString), 16, "0.1 0.46", "0.3 0.54", TextAnchor.MiddleLeft);
                    UI.Panel(ref container, UIElement, uiColors["bg3"], "0.3 0.47", "0.9 0.53");
                    if (string.IsNullOrEmpty(groupData.Rank.ToString()))
                        UI.Input(ref container, UIElement, "", groupData.Rank.ToString(), 16, "amui.registergroup input rank", "0.32 0.47", "0.9 0.53");
                    else UI.Label(ref container, UIElement, groupData.Rank.ToString(), 16, "0.32 0.47", "0.9 0.53", TextAnchor.MiddleLeft);

                    UI.Button(ref container, UIElement, uiColors["button2"], msg("reset", player.UserIDString), 16, "0.345 0.38", "0.495 0.44", "amui.registergroup reset");
                    UI.Button(ref container, UIElement, uiColors["button3"], msg("create", player.UserIDString), 16, "0.505 0.38", "0.655 0.44", "amui.registergroup create");
                    break;                             
            }

            CuiHelper.DestroyUi(player, UIElement);
            CuiHelper.AddUi(player, container);
        }

        private void CreateMenuCommands(BasePlayer player, CommSub subType, int page = 0)
        {             
            var container = UI.Container(UIElement, "0 0 0 0", "0.05 0.08", "0.95 0.92");
            CuiHelper.DestroyUi(player, UIElement);
            CuiHelper.AddUi(player, container);
        }
         

        private void OpenSelectionMenu(BasePlayer player, SelectType selectType, object objList, bool sortList = false)
        {
            SelectionData data = selectData[player.userID];            

            var container = UI.Container(UIElement, "0 0 0 0", "0.05 0.08", "0.95 0.92");            
            UI.Panel(ref container, UIElement, uiColors["bg3"], "0.005 0.925", "0.995 0.99");
            UI.Panel(ref container, UIElement, uiColors["bg3"], "0.005 0.87", "0.995 0.92");
            CreateCharacterFilter(ref container, player.userID, data.character, string.Empty);
            UI.Label(ref container, UIElement, data.selectDesc, 24, "0.02 0.93", "0.8 0.985", TextAnchor.MiddleLeft);
            UI.Button(ref container, UIElement, uiColors["button1"], msg("return", player.UserIDString), 16, "0.855 0.93", "0.985 0.985", $"amui.switchelement {( data.menuType == MenuType.Groups ? "groups" : "permissions")} {data.subType}");

            List<IPlayer> playerList = null;
            List<string> stringList = null;

            switch (selectType)
            {
                case SelectType.Player:

                    playerList = (List<IPlayer>)objList;                    

                    if (!string.IsNullOrEmpty(data.character))
                        playerList = playerList.Where(x => x.Name.ToLower().StartsWith(data.character.ToLower())).ToList();

                    if (sortList)
                        playerList = playerList.OrderBy(x => x.Name).ToList();

                    if (!data.forceOnline)
                    {
                        UI.Button(ref container, UIElement, data.isOnline ? uiColors["button3"] : uiColors["button1"], msg("onlineplayers", player.UserIDString), 16, "0.3475 0.875", "0.4975 0.915", data.isOnline ? "" : $"amui.makeselection online");
                        UI.Button(ref container, UIElement, !data.isOnline ? uiColors["button3"] : uiColors["button1"], msg("offlineplayers", player.UserIDString), 16, "0.5025 0.875", "0.6525 0.915", !data.isOnline ? "" : $"amui.makeselection offline");
                    }
                    break;
                
                case SelectType.String:
                    stringList = (List<string>)objList;                   

                    if (!string.IsNullOrEmpty(data.character))
                        stringList = stringList.Where(x => x.StartsWith(data.character)).ToList();

                    if (sortList)
                        stringList.Sort();
                    break;                
            }  
           
            if (data.pageNum > 0)
                UI.Button(ref container, UIElement, uiColors["button1"], msg("back", player.UserIDString), 16, "0.015 0.875", "0.145 0.915", "amui.makeselection pageDown");
            if (selectType == SelectType.Player ? (playerList.Count > 72 && playerList.Count > (72 * data.pageNum + 72)) : stringList.Count > 72 && stringList.Count > (72 * data.pageNum + 72))
                UI.Button(ref container, UIElement, uiColors["button1"], msg("next", player.UserIDString), 16, "0.855 0.875", "0.985 0.915", "amui.makeselection pageUp");

            int count = 0;
            for (int i = data.pageNum * 72; i < (selectType == SelectType.Player ? playerList.Count : stringList.Count); i++)
            {
                float[] position = CalculateButtonPos(count);

                if (selectType == SelectType.Player)
                {
                    IPlayer target = playerList[i];
                    string userName = StripTags(target.Name);
                    UI.Button(ref container, UIElement, uiColors["button1"], $"{userName} <size=8>({target.Id})</size>", 10, $"{position[0]} {position[1]}", $"{position[2]} {position[3]}", $"amui.makeselection target {target.Id} {userName.Replace(" ", "_-!!-_")}");
                }
                else
                {
                    string button = stringList[i];
                    UI.Button(ref container, UIElement, uiColors["button1"], button, 10, $"{position[0]} {position[1]}", $"{position[2]} {position[3]}", $"amui.makeselection target {button.Replace(" ", "_-!!-_")}");
                }
                ++count;
                if (count >= 72)
                    break;
            }

            CuiHelper.DestroyUi(player, UIElement);
            CuiHelper.AddUi(player, container);
        }

        private void OpenPermissionMenu(BasePlayer player, string groupOrUserId, string playerName, string description, int page, string filter)
        {
            var container = UI.Container(UIElement, "0 0 0 0", "0.05 0.08", "0.95 0.92");

            UI.Panel(ref container, UIElement, uiColors["bg3"], "0.005 0.925", "0.995 0.99");
            UI.Panel(ref container, UIElement, uiColors["bg3"], "0.005 0.87", "0.995 0.92");

            UI.Label(ref container, UIElement, description, 24, "0.02 0.93", "0.8 0.985", TextAnchor.MiddleLeft);

            UI.Button(ref container, UIElement, uiColors["button1"], msg("return", player.UserIDString), 16, "0.855 0.93", "0.985 0.985", $"amui.switchelement permissions view");

            CreateCharacterFilter(ref container, player.userID, filter, string.IsNullOrEmpty(playerName) ? $"amui.switchelement permissions group 0 {groupOrUserId.Replace(" ", "_-!!-_")}" : $"amui.switchelement permissions player 0 {groupOrUserId} {playerName.Replace(" ", "_-!!-_")}");

            List<KeyValuePair<string, bool>> permList = new List<KeyValuePair<string, bool>>(permissionList);
            if (!string.IsNullOrEmpty(filter) && filter != "~")
                permList = permList.Where(x => x.Key.StartsWith(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            permList.OrderBy(x => x.Key);

            if (page > 0)
                UI.Button(ref container, UIElement, uiColors["button1"], msg("back", player.UserIDString), 16, "0.015 0.875", "0.145 0.915", string.IsNullOrEmpty(playerName) ? $"amui.switchelement permissions group {page - 1} {groupOrUserId.Replace(" ", "_-!!-_")} {filter}" : $"amui.switchelement permissions player {page - 1} {groupOrUserId} {playerName.Replace(" ", "_-!!-_")} {filter}");
            if (permList.Count > 72 && permList.Count > (72 * page + 72))
                UI.Button(ref container, UIElement, uiColors["button1"], msg("next", player.UserIDString), 16, "0.855 0.875", "0.985 0.915", string.IsNullOrEmpty(playerName) ? $"amui.switchelement permissions group {page + 1} {groupOrUserId.Replace(" ", "_-!!-_")} {filter}" : $"amui.switchelement permissions player {page + 1} {groupOrUserId} {playerName.Replace(" ", "_-!!-_")} {filter}");            

            int count = 0;
            for (int i = page * 72; i < permList.Count; i++)
            {
                KeyValuePair<string, bool> perm = permList[i];
                float[] position = CalculateButtonPosVert(count);
              
                if (!perm.Value)
                {
                    UI.Panel(ref container, UIElement, uiColors["button2"], $"{position[0]} {position[1]}", $"{position[2]} {position[3]}");
                    UI.Label(ref container, UIElement, $"{perm.Key}", 12, $"{position[0]} {position[1]}", $"{position[2]} {position[3]}");                    
                }
                else
                {
                    bool hasPermission = HasPermission(groupOrUserId, perm.Key, string.IsNullOrEmpty(playerName) ? true : false);

                    UI.Button(ref container, UIElement, hasPermission ? uiColors["button3"] : uiColors["button1"], perm.Key, 10, $"{position[0]} {position[1]}", $"{position[2]} {position[3]}", string.IsNullOrEmpty(playerName) ? $"amui.togglepermission group {groupOrUserId.Replace(" ", "_-!!-_")} {page} {perm.Key} {!hasPermission} {filter}" : $"amui.togglepermission player {groupOrUserId} {playerName.Replace(" ", "_-!!-_")} {page} {perm.Key} {!hasPermission} {filter}");
                }               
                ++count;

                if (count >= 72)
                    break;
            }

            CuiHelper.DestroyUi(player, UIElement);
            CuiHelper.AddUi(player, container);
        }

        private void OpenGroupMenu(BasePlayer player, string userId, string userName, string description, int page)
        {
            var container = UI.Container(UIElement, "0 0 0 0", "0.05 0.08", "0.95 0.92");

            UI.Panel(ref container, UIElement, uiColors["bg3"], "0.005 0.925", "0.995 0.99");
            UI.Panel(ref container, UIElement, uiColors["bg3"], "0.005 0.87", "0.995 0.92");

            UI.Label(ref container, UIElement, description, 24, "0.02 0.93", "0.8 0.985", TextAnchor.MiddleLeft);

            UI.Button(ref container, UIElement, uiColors["button1"], msg("return", player.UserIDString), 16, "0.855 0.93", "0.985 0.985", $"amui.switchelement groups view");
            List<string> groupList = GetGroups();
            groupList.Sort();

            if (page > 0)
                UI.Button(ref container, UIElement, uiColors["button1"], msg("back", player.UserIDString), 16, "0.015 0.875", "0.145 0.915", $"amui.switchelement groups usergroups {page - 1} {userId} {userName.Replace(" ", "_-!!-_")}");
            if (groupList.Count > 72 && groupList.Count > (72 * page + 72))
                UI.Button(ref container, UIElement, uiColors["button1"], msg("next", player.UserIDString), 16, "0.855 0.875", "0.985 0.915", $"amui.switchelement groups usergroups {page + 1} {userId} {userName.Replace(" ", "_-!!-_")}");

            int count = 0;
            for (int i = page * 72; i < groupList.Count; i++)
            {
                string groupId = groupList[i];
                float[] position = CalculateButtonPos(count);

                bool hasPermission = HasGroup(userId, groupId);

                UI.Button(ref container, UIElement, hasPermission ? uiColors["button3"] : uiColors["button1"], groupId, 10, $"{position[0]} {position[1]}", $"{position[2]} {position[3]}", $"amui.togglegroup {userId} {userName.Replace(" ", "_-!!-_")} {page} {groupId.Replace(" ", "_-!!-_")} {!hasPermission}");
                ++count;

                if (count >= 72)
                    break;
            }

            CuiHelper.DestroyUi(player, UIElement);
            CuiHelper.AddUi(player, container);
        }
        #endregion

        #region UI Functions
        private void CreateCharacterFilter(ref CuiElementContainer container, ulong playerId, string currentCharacter, string returnCommand)
        {
            float buttonHeight = 1f / 27f;
            int i = 0;
            foreach(var character in charFilter)
            {
                UI.Button(ref container, UIElement, currentCharacter == character ? uiColors["button3"] : uiColors["button1"], character, 10, $"-0.02 {1 - (buttonHeight * i) - buttonHeight + 0.002f}", $"-0.001 {1 - (buttonHeight * i) - 0.002f}", currentCharacter == character ? "" : $"{(string.IsNullOrEmpty(returnCommand) ? "amui.filterchar" : returnCommand)} {character}");
                i++;
            }
        }

        private float[] CalculateButtonPos(int number)
        {
            Vector2 position = new Vector2(0.014f, 0.78f);
            Vector2 dimensions = new Vector2(0.158f, 0.06f);
            float offsetY = 0;
            float offsetX = 0;
            if (number >= 0 && number < 6)
            {
                offsetX = (0.005f + dimensions.x) * number;
            }
            if (number > 5 && number < 12)
            {
                offsetX = (0.005f + dimensions.x) * (number - 6);
                offsetY = (-0.007f - dimensions.y) * 1;
            }
            if (number > 11 && number < 18)
            {
                offsetX = (0.005f + dimensions.x) * (number - 12);
                offsetY = (-0.007f - dimensions.y) * 2;
            }
            if (number > 17 && number < 24)
            {
                offsetX = (0.005f + dimensions.x) * (number - 18);
                offsetY = (-0.007f - dimensions.y) * 3;
            }
            if (number > 23 && number < 30)
            {
                offsetX = (0.005f + dimensions.x) * (number - 24);
                offsetY = (-0.007f - dimensions.y) * 4;
            }
            if (number > 29 && number < 36)
            {
                offsetX = (0.005f + dimensions.x) * (number - 30);
                offsetY = (-0.007f - dimensions.y) * 5;
            }
            if (number > 35 && number < 42)
            {
                offsetX = (0.005f + dimensions.x) * (number - 36);
                offsetY = (-0.007f - dimensions.y) * 6;
            }
            if (number > 41 && number < 48)
            {
                offsetX = (0.005f + dimensions.x) * (number - 42);
                offsetY = (-0.007f - dimensions.y) * 7;
            }
            if (number > 47 && number < 54)
            {
                offsetX = (0.005f + dimensions.x) * (number - 48);
                offsetY = (-0.007f - dimensions.y) * 8;
            }
            if (number > 53 && number < 60)
            {
                offsetX = (0.005f + dimensions.x) * (number - 54);
                offsetY = (-0.007f - dimensions.y) * 9;
            }
            if (number > 59 && number < 66)
            {
                offsetX = (0.005f + dimensions.x) * (number - 60);
                offsetY = (-0.007f - dimensions.y) * 10;
            }
            if (number > 65 && number < 72)
            {
                offsetX = (0.005f + dimensions.x) * (number - 66);
                offsetY = (-0.007f - dimensions.y) * 11;
            }
            if (number > 71 && number < 78)
            {
                offsetX = (0.005f + dimensions.x) * (number - 72);
                offsetY = (-0.007f - dimensions.y) * 12;
            }

            Vector2 offset = new Vector2(offsetX, offsetY);
            Vector2 posMin = position + offset;
            Vector2 posMax = posMin + dimensions;
            return new float[] { posMin.x, posMin.y, posMax.x, posMax.y };
        }

        private float[] CalculateButtonPosVert(int number)
        {
            Vector2 position = new Vector2(0.014f, 0.78f);
            Vector2 dimensions = new Vector2(0.158f, 0.06f);
            float offsetY = 0;
            float offsetX = 0;
            if (number >= 0 && number < 12)
            {
                offsetY = (-0.007f - dimensions.y) * number;
            }
            if (number > 11 && number < 24)
            {
                offsetX = (0.005f + dimensions.x) * 1;
                offsetY = (-0.007f - dimensions.y) * (number - 12);
            }
            if (number > 23 && number < 36)
            {
                offsetX = (0.005f + dimensions.x) * 2;
                offsetY = (-0.007f - dimensions.y) * (number - 24);
            }
            if (number > 35 && number < 48)
            {
                offsetX = (0.005f + dimensions.x) * 3;
                offsetY = (-0.007f - dimensions.y) * (number - 36);
            }
            if (number > 47 && number < 60)
            {
                offsetX = (0.005f + dimensions.x) * 4;
                offsetY = (-0.007f - dimensions.y) * (number - 48);
            }
            if (number > 59 && number < 72)
            {
                offsetX = (0.005f + dimensions.x) * 5;
                offsetY = (-0.007f - dimensions.y) * (number - 60);
            }
            
            Vector2 offset = new Vector2(offsetX, offsetY);
            Vector2 posMin = position + offset;
            Vector2 posMax = posMin + dimensions;
            return new float[] { posMin.x, posMin.y, posMax.x, posMax.y };
        }

        private float[] CalculateItemPos(int number)
        {
            Vector2 position = new Vector2(0.014f, 0.81f);
            Vector2 dimensions = new Vector2(0.158f, 0.03f);
            float offsetY = 0;
            float offsetX = 0;
            if (number >= 0 && number < 6)
            {
                offsetX = (0.005f + dimensions.x) * number;
            }
            if (number > 5 && number < 12)
            {
                offsetX = (0.005f + dimensions.x) * (number - 6);
                offsetY = (-0.007f - dimensions.y) * 1;
            }
            if (number > 11 && number < 18)
            {
                offsetX = (0.005f + dimensions.x) * (number - 12);
                offsetY = (-0.007f - dimensions.y) * 2;
            }
            if (number > 17 && number < 24)
            {
                offsetX = (0.005f + dimensions.x) * (number - 18);
                offsetY = (-0.007f - dimensions.y) * 3;
            }
            if (number > 23 && number < 30)
            {
                offsetX = (0.005f + dimensions.x) * (number - 24);
                offsetY = (-0.007f - dimensions.y) * 4;
            }
            if (number > 29 && number < 36)
            {
                offsetX = (0.005f + dimensions.x) * (number - 30);
                offsetY = (-0.007f - dimensions.y) * 5;
            }
            if (number > 35 && number < 42)
            {
                offsetX = (0.005f + dimensions.x) * (number - 36);
                offsetY = (-0.007f - dimensions.y) * 6;
            }
            if (number > 41 && number < 48)
            {
                offsetX = (0.005f + dimensions.x) * (number - 42);
                offsetY = (-0.007f - dimensions.y) * 7;
            }
            if (number > 47 && number < 54)
            {
                offsetX = (0.005f + dimensions.x) * (number - 48);
                offsetY = (-0.007f - dimensions.y) * 8;
            }
            if (number > 53 && number < 60)
            {
                offsetX = (0.005f + dimensions.x) * (number - 54);
                offsetY = (-0.007f - dimensions.y) * 9;
            }
            if (number > 59 && number < 66)
            {
                offsetX = (0.005f + dimensions.x) * (number - 60);
                offsetY = (-0.007f - dimensions.y) * 10;
            }
            if (number > 65 && number < 72)
            {
                offsetX = (0.005f + dimensions.x) * (number - 66);
                offsetY = (-0.007f - dimensions.y) * 11;
            }
            if (number > 71 && number < 78)
            {
                offsetX = (0.005f + dimensions.x) * (number - 72);
                offsetY = (-0.007f - dimensions.y) * 12;
            }
            if (number > 77 && number < 84)
            {
                offsetX = (0.005f + dimensions.x) * (number - 78);
                offsetY = (-0.007f - dimensions.y) * 13;
            }
            if (number > 83 && number < 90)
            {
                offsetX = (0.005f + dimensions.x) * (number - 84);
                offsetY = (-0.007f - dimensions.y) * 14;
            }
            if (number > 89 && number < 96)
            {
                offsetX = (0.005f + dimensions.x) * (number - 90);
                offsetY = (-0.007f - dimensions.y) * 15;
            }
            if (number > 95 && number < 102)
            {
                offsetX = (0.005f + dimensions.x) * (number - 96);
                offsetY = (-0.007f - dimensions.y) * 16;
            }
            if (number > 101 && number < 108)
            {
                offsetX = (0.005f + dimensions.x) * (number - 102);
                offsetY = (-0.007f - dimensions.y) * 17;
            }
            if (number > 107 && number < 114)
            {
                offsetX = (0.005f + dimensions.x) * (number - 108);
                offsetY = (-0.007f - dimensions.y) * 18;
            }
            if (number > 113 && number < 120)
            {
                offsetX = (0.005f + dimensions.x) * (number - 114);
                offsetY = (-0.007f - dimensions.y) * 19;
            }
            if (number > 119 && number < 126)
            {
                offsetX = (0.005f + dimensions.x) * (number - 120);
                offsetY = (-0.007f - dimensions.y) * 20;
            }
            if (number > 125 && number < 132)
            {
                offsetX = (0.005f + dimensions.x) * (number - 126);
                offsetY = (-0.007f - dimensions.y) * 21;
            }
            if (number > 131 && number < 138)
            {
                offsetX = (0.005f + dimensions.x) * (number - 132);
                offsetY = (-0.007f - dimensions.y) * 22;
            }
            if (number > 137 && number < 144)
            {
                offsetX = (0.005f + dimensions.x) * (number - 138);
                offsetY = (-0.007f - dimensions.y) * 23;
            }
            if (number > 143 && number < 150)
            {
                offsetX = (0.005f + dimensions.x) * (number - 144);
                offsetY = (-0.007f - dimensions.y) * 24;
            }
            if (number > 149 && number < 156)
            {
                offsetX = (0.005f + dimensions.x) * (number - 150);
                offsetY = (-0.007f - dimensions.y) * 25;
            }
            if (number > 155 && number < 162)
            {
                offsetX = (0.005f + dimensions.x) * (number - 156);
                offsetY = (-0.007f - dimensions.y) * 26;
            }

            Vector2 offset = new Vector2(offsetX, offsetY);
            Vector2 posMin = position + offset;
            Vector2 posMax = posMin + dimensions;
            return new float[] { posMin.x, posMin.y, posMax.x, posMax.y };
        }

        private string StripTags(string str)
        {
            if (str.StartsWith("[") && str.Contains("]") && str.Length > str.IndexOf("]"))            
                str = str.Substring(str.IndexOf("]") + 1).Trim();
            
            if (str.StartsWith("[") && str.Contains("]") && str.Length > str.IndexOf("]"))
                StripTags(str);

            return str;
        }

        private void PopupMessage(BasePlayer player, string message)
        {
            var container = UI.Container(UIPopup, uiColors["bg2"], "0.05 0.92", "0.95 0.98");
            UI.Label(ref container, UIPopup, message, 17, "0 0", "1 1");

            Timer destroyIn;
            if (popupTimers.TryGetValue(player.userID, out destroyIn))
                destroyIn.Destroy();
            popupTimers[player.userID] = timer.Once(5, () =>
            {
                CuiHelper.DestroyUi(player, UIPopup);
                popupTimers.Remove(player.userID);
            });

            CuiHelper.DestroyUi(player, UIPopup);
            CuiHelper.AddUi(player, container);
        }
        #endregion

        #region UI Commands
        private void ShowActivePlayer(BasePlayer player)
        {
            var container = new CuiElementContainer();
            container.Add(new CuiPanel
            {
                RectTransform =
                    {AnchorMin = "1 0", AnchorMax = "1 0", OffsetMin = $"-260 20", OffsetMax = $"-210 70"},
                Image = {Color = "1 1 1 0"}
            }, "Overlay", LayerActive);

            container.Add(new CuiElement
            {
                Parent = LayerActive,
                Components =
                {
                    new CuiRawImageComponent {Png = (string) ImageLibrary?.Call("GetImage", _config.Image)},
                    new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "1 1"}
                }
            });
            CuiHelper.DestroyUi(player, LayerActive);
            CuiHelper.AddUi(player, container);
        }
        
  
        #region CUI Helper
        public class UI
        {
            static public CuiElementContainer Container(string panelName, string color, string aMin, string aMax, bool useCursor = false, string parent = "Overlay")
            {
                var NewElement = new CuiElementContainer()
                {
                    {
                        new CuiPanel
                        {
                            Image = {Color = color},
                            RectTransform = {AnchorMin = aMin, AnchorMax = aMax},
                            CursorEnabled = useCursor
                        },
                        new CuiElement().Parent = parent,
                        panelName
                    }
                };
                return NewElement;
            }

            static public void Panel(ref CuiElementContainer container, string panel, string color, string aMin, string aMax, bool cursor = false)
            {
                container.Add(new CuiPanel
                {
                    Image = { Color = color },
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                    CursorEnabled = cursor
                },
                panel);
            }

            static public void Label(ref CuiElementContainer container, string panel, string text, int size, string aMin, string aMax, TextAnchor align = TextAnchor.MiddleCenter)
            {
                container.Add(new CuiLabel
                {
                    Text = { FontSize = size, Align = align, Text = text },
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax }
                },
                panel);

            }   
            
            static public void Button(ref CuiElementContainer container, string panel, string color, string text, int size, string aMin, string aMax, string command, TextAnchor align = TextAnchor.MiddleCenter)
            {
                container.Add(new CuiButton
                {
                    Button = { Color = color, Command = command, FadeIn = 0f },
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                    Text = { Text = text, FontSize = size, Align = align }
                },
                panel);
            }           
           
            static public void Input(ref CuiElementContainer container, string panel, string color, string text, int size, string command, string aMin, string aMax)
            {                
                container.Add(new CuiElement
                {
                    Name = CuiHelper.GetGuid(),
                    Parent = panel,
                    Components =
                    {
                        new CuiInputFieldComponent
                        {
                            Align = TextAnchor.MiddleLeft,
                            CharsLimit = 50,
                            Color = color,
                            Command = command + text,
                            FontSize = size,
                            IsPassword = false,
                            Text = text                           
                        },
                        new CuiRectTransformComponent {AnchorMin = aMin, AnchorMax = aMax }
                    }                
                });
            }

            static public string Color(string hexColor, float alpha)
            {
                if (hexColor.StartsWith("#"))
                    hexColor = hexColor.TrimStart('#');
                int red = int.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                int green = int.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                int blue = int.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);
                return $"{(double)red / 255} {(double)green / 255} {(double)blue / 255} {alpha}";
            }
        }
        #endregion
        #endregion

        #region Function
		 #region UI Commands
      

        [ConsoleCommand("amui.filterchar")]
        private void ccmdFilterChar(ConsoleSystem.Arg arg)
        {
            var player = arg.Connection.player as BasePlayer;
            if (player == null)
                return;

            if (!HasPermission(player.UserIDString, "personalcontroller.use")) return;

            SelectionData data = selectData[player.userID];

            data.character = arg.GetString(0) == "~" ? string.Empty : arg.GetString(0);

            switch (data.returnCommand)
            {
                case "amui.runcommand":
                    rust.RunClientCommand(player, data.returnCommand, data.subType, data.listNum);
                    break;
                case "amui.selectforpermission":
                    rust.RunClientCommand(player, data.returnCommand, data.isGroup);
                    break;
                case "amui.selectremovegroup":
                    rust.RunClientCommand(player, data.returnCommand);
                    break;
                case "amui.selectforgroup":
                    rust.RunClientCommand(player, data.returnCommand);
                    break;
                default:
                    rust.RunClientCommand(player, data.returnCommand);
                    break;
            }
        }

        [ConsoleCommand("amui.registergroup")]
        private void ccmdRegisterGroup(ConsoleSystem.Arg arg)
        {
            var player = arg.Connection.player as BasePlayer;
            if (player == null)
                return;

            if (!HasPermission(player.UserIDString, "personalcontroller.use")) return;

            GroupData groupData = groupCreator[player.userID];
            if (!groupCreator.TryGetValue(player.userID, out groupData))
            {
                groupCreator.Add(player.userID, new GroupData());
                groupData = groupCreator[player.userID];
            }

            switch (arg.Args[0])
            {
                case "input":
                    switch (arg.GetString(1))
                    {
                        case "name":
                            groupData.ParentGroup = string.Join(" ", arg.Args.Skip(2).ToArray());
                            break;
                        case "title":
                            groupData.Title = string.Join(" ", arg.Args.Skip(2).ToArray());
                            break;
                        case "rank":
                            groupData.Rank = int.Parse(string.Join(" ", arg.Args.Skip(2).ToArray()));
                            break;
                    }
                    CreateMenuGroups(player, GroupSub.AddGroup);
                    return;
                case "create":
                    if (string.IsNullOrEmpty(groupData.ParentGroup))
                    {
                        PopupMessage(player, msg("nogroupname", player.UserIDString));
                        return;
                    }
                    int rank = 0;
                    int.TryParse(groupData.Rank.ToString(), out rank);

                    if (CreateGroup(groupData.ParentGroup, groupData.Title, rank))
                        PopupMessage(player, string.Format(msg("groupcreated", player.UserIDString), groupData.ParentGroup));

                    CreateMenuGroups(player, GroupSub.View);
                    groupCreator.Remove(player.userID);
                    return;
                case "reset":
                    groupCreator[player.userID] = new GroupData();
                    CreateMenuGroups(player, GroupSub.AddGroup);
                    return;
            }
        }

        [ConsoleCommand("amui.selectforpermission")]
        private void ccmdSelectPermission(ConsoleSystem.Arg arg)
        {
            var player = arg.Connection.player as BasePlayer;
            if (player == null)
                return;

            if (!HasPermission(player.UserIDString, "personalcontroller.use")) return;

            bool isGroup = arg.GetBool(0);

            SelectionData data;
            if (!selectData.TryGetValue(player.userID, out data))
            {
                selectData.Add(player.userID, new SelectionData
                {
                    listNum = arg.GetInt(1),
                    menuType = MenuType.Permissions,
                    pageNum = 0,
                    requireTarget1 = true,
                    returnCommand = "amui.selectforpermission",                    
                    isGroup = isGroup,
                    selectDesc = isGroup ? msg("selectgroup", player.UserIDString) : msg("selectplayer", player.UserIDString),
                    subType = "view",
                    isOnline = true,                    
                });
                data = selectData[player.userID];
            }
            if (data.isGroup)
            {
                if (!string.IsNullOrEmpty(data.target1_Id))
                {
                    OpenPermissionMenu(player, data.target1_Id, string.Empty, string.Format(msg("togglepermgroup", player.UserIDString), data.target1_Id), 0, "");
                    selectData.Remove(player.userID);
                    return;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(data.target1_Id) && !string.IsNullOrEmpty(data.target1_Name))
                {
                    OpenPermissionMenu(player, data.target1_Id, data.target1_Name, string.Format(msg("togglepermplayer", player.UserIDString), data.target1_Name), 0, "");
                    selectData.Remove(player.userID);
                    return;
                }
            }

            object obj;
            if (isGroup)
                obj = GetGroups();
            else obj = data.isOnline ? covalence.Players.Connected.ToList() : storedData.GetOfflineList();

            OpenSelectionMenu(player, isGroup ? SelectType.String : SelectType.Player, obj, true);
        }

        [ConsoleCommand("amui.selectforgroup")]
        private void ccmdSelectGroup(ConsoleSystem.Arg arg)
        {
            var player = arg.Connection.player as BasePlayer;
            if (player == null)
                return;

            if (!HasPermission(player.UserIDString, "personalcontroller.use")) return;
                   
            SelectionData data;
            if (!selectData.TryGetValue(player.userID, out data))
            {
                selectData.Add(player.userID, new SelectionData
                {
                    listNum = arg.GetInt(1),
                    menuType = MenuType.Permissions,
                    pageNum = 0,
                    requireTarget1 = true,
                    returnCommand = "amui.selectforgroup",
                    selectDesc = msg("selectplayer", player.UserIDString),
                    subType = "view",
                    isOnline = true,
                });
                data = selectData[player.userID];
            }
            if (!string.IsNullOrEmpty(data.target1_Id) && !string.IsNullOrEmpty(data.target1_Name))
            {
                OpenGroupMenu(player, data.target1_Id, data.target1_Name, string.Format(msg("togglegroupplayer", player.UserIDString), data.target1_Name), 0);
                selectData.Remove(player.userID);
                return;
            }
            
            OpenSelectionMenu(player, SelectType.Player, data.isOnline ? covalence.Players.Connected.ToList() : storedData.GetOfflineList(), true);
        }

        [ConsoleCommand("amui.selectremovegroup")]
        private void ccmdSelectRemoveGroup(ConsoleSystem.Arg arg)
        {
            var player = arg.Connection.player as BasePlayer;
            if (player == null)
                return;

            if (!HasPermission(player.UserIDString, "personalcontroller.use")) return;
                 
            SelectionData data;
            if (!selectData.TryGetValue(player.userID, out data))
            {
                selectData.Add(player.userID, new SelectionData
                {
                    listNum = arg.GetInt(1),
                    menuType = MenuType.Groups,
                    pageNum = 0,
                    requireTarget1 = true,
                    returnCommand = "amui.selectremovegroup",
                    selectDesc = msg("selectremovegroup", player.UserIDString),
                    subType = "view",
                    isOnline = true,
                });
                data = selectData[player.userID];
            }
            if (!string.IsNullOrEmpty(data.target1_Id))
            {
                RemoveGroup(data.target1_Id);
                PopupMessage(player, string.Format(msg("groupremoved", player.UserIDString), data.target1_Id));
                selectData.Remove(player.userID);
                CreateMenuGroups(player, GroupSub.View);
                return;
            }
           
            OpenSelectionMenu(player, SelectType.String, GetGroups(), true);
        }

        [ConsoleCommand("amui.togglepermission")]
        private void ccmdTogglePermission(ConsoleSystem.Arg arg)
        {
            var player = arg.Connection.player as BasePlayer;
            if (player == null)
                return;

            if (!HasPermission(player.UserIDString, "personalcontroller.permissions")) return;

            switch (arg.Args[0])
            {
                case "player":
                    {
                        string userId = arg.GetString(1);
                        string userName = arg.GetString(2).Replace("_-!!-_", " ");
                        if (arg.GetBool(5))
                            GrantPermission(userId, arg.GetString(4));
                        else RevokePermission(userId, arg.GetString(4));
                        OpenPermissionMenu(player, userId, userName, string.Format(msg("togglepermplayer", player.UserIDString), userName), arg.GetInt(3), arg.Args.Length > 6 ? arg.GetString(6) : "");
                    }
                    break;
                case "group":
                    string groupId = arg.GetString(1).Replace("_-!!-_", " ");
                    if (arg.GetBool(4))
                        GrantPermission(groupId, arg.GetString(3), true);
                    else RevokePermission(groupId, arg.GetString(3), true);
                    OpenPermissionMenu(player, groupId, string.Empty, string.Format(msg("togglepermgroup", player.UserIDString), groupId), arg.GetInt(2), arg.Args.Length > 5 ? arg.GetString(5) : "");
                    break;
                default:
                    break;
            }

        }

        [ConsoleCommand("amui.togglegroup")]
        private void ccmdToggleGroup(ConsoleSystem.Arg arg)
        {
            var player = arg.Connection.player as BasePlayer;
            if (player == null)
                return;

            if (!HasPermission(player.UserIDString, "personalcontroller.groups")) return;

            string userId = arg.GetString(0);
            string userName = arg.GetString(1).Replace("_-!!-_", " ");
            if (arg.GetBool(4))
                AddToGroup(userId, arg.GetString(3).Replace("_-!!-_", " "));
            else RemoveFromGroup(userId, arg.GetString(3).Replace("_-!!-_", " "));
            OpenGroupMenu(player, userId, userName, string.Format(msg("togglegroupplayer", player.UserIDString), userName), arg.GetInt(2));               
        }

        [ConsoleCommand("amui.makeselection")]
        private void ccmdMakeSelection(ConsoleSystem.Arg arg)
        {
            var player = arg.Connection.player as BasePlayer;
            if (player == null)
                return;

            if (!HasPermission(player.UserIDString, "personalcontroller.use")) return;

            SelectionData data = selectData[player.userID];

            switch (arg.Args[0])
            {
                case "target":
                    if (string.IsNullOrEmpty(data.target1_Id))
                    {
                        data.target1_Id = arg.Args[1].Replace("_-!!-_", " ");                        
                        data.target1_Name = arg.Args.Length == 3 ? arg.Args[2].Replace("_-!!-_", " ") : string.Empty;
                    }
                    else
                    {
                        data.target2_Id = arg.Args[1].Replace("_-!!-_", " ");                        
                        data.target2_Name = arg.Args.Length == 3 ? arg.Args[2].Replace("_-!!-_", " ") : string.Empty;
                    }
                    break;
                case "pageUp":
                    ++data.pageNum;
                    break;
                case "pageDown":
                    --data.pageNum;
                    break;
                case "online":
                    data.isOnline = true;
                    break;
                case "offline":
                    data.isOnline = false;
                    break;                
            }

            if (data.returnCommand.StartsWith("amui.giveitem"))
                rust.RunClientCommand(player, $"{data.returnCommand} {data.target1_Id}");            
            else
            {
                switch (data.returnCommand)
                {
                    case "amui.runcommand":
                        rust.RunClientCommand(player, "amui.runcommand", data.subType, data.listNum);
                        break;
                    case "amui.selectforpermission":
                        rust.RunClientCommand(player, "amui.selectforpermission", data.isGroup);
                        break;
                    case "amui.selectremovegroup":
                        rust.RunClientCommand(player, "amui.selectremovegroup");
                        break;
                    case "amui.selectforgroup":
                        rust.RunClientCommand(player, "amui.selectforgroup");
                        break;
                    default:
                        break;
                }
            }
        }

        [ConsoleCommand("amui.switchelement")]
        private void ccmdUISwitch(ConsoleSystem.Arg arg)
        {
            var player = arg.Connection.player as BasePlayer;
            if (player == null)
                return;

            if (!HasPermission(player.UserIDString, "personalcontroller.use")) return;

            if (selectData.ContainsKey(player.userID))
                selectData.Remove(player.userID);

            int page = 0;
            if (arg.Args.Length > 2)
                page = arg.GetInt(2);

            switch (arg.Args[0])
            {
                case "permissions":
                    PermSub permSub = PermSub.View;                    
                    if (arg.Args.Length > 1)
                        permSub = ParseType<PermSub>(arg.Args[1]);

                    switch (permSub)
                    {
                        case PermSub.View:
                            CreateMenuPermissions(player, page, arg.Args.Length > 3 ? arg.GetString(3) : "");
                            return;
                        case PermSub.Player:
                            if (arg.Args.Length >= 5)
                                OpenPermissionMenu(player, arg.GetString(3), arg.GetString(4).Replace("_-!!-_", " "), string.Format(msg("togglepermplayer", player.UserIDString), arg.GetString(4).Replace("_-!!-_", " ")), arg.GetInt(2), arg.Args.Length > 5 ? arg.GetString(5) : "");
                            else rust.RunClientCommand(player, "amui.selectforpermission", false);
                            return;
                        case PermSub.Group:
                            if (arg.Args.Length >= 4)
                                OpenPermissionMenu(player, arg.GetString(3).Replace("_-!!-_", " "), string.Empty, string.Format(msg("togglepermgroup", player.UserIDString), arg.GetString(3).Replace("_-!!-_", " ")), arg.GetInt(2), arg.Args.Length > 4 ? arg.GetString(4) : "");
                            else rust.RunClientCommand(player, "amui.selectforpermission", true);
                            return;                       
                    }
                    return;
                case "groups":
                    GroupSub groupSub = GroupSub.View;
                    if (arg.Args.Length > 1)
                        groupSub = ParseType<GroupSub>(arg.Args[1]);

                    switch (groupSub)
                    {
                        case GroupSub.View:
                            break;
                        case GroupSub.UserGroups:
                            if (arg.Args.Length == 5)
                                OpenGroupMenu(player, arg.GetString(3), arg.GetString(4).Replace("_-!!-_", " "), string.Format(msg("togglegroupplayer", player.UserIDString), arg.GetString(4).Replace("_-!!-_", " ")), arg.GetInt(2));
                            else rust.RunClientCommand(player, "amui.selectforgroup");
                            return;
                        case GroupSub.AddGroup:
                            break;
                        case GroupSub.RemoveGroup:
                            rust.RunClientCommand(player, "amui.selectremovegroup");
                            return;                        
                    }
                    CreateMenuGroups(player, groupSub, page);
                    return;
                case "commands":
                    CommSub commSub = CommSub.Chat;
                    if (arg.Args.Length > 1)
                        commSub = ParseType<CommSub>(arg.Args[1]);
                    CreateMenuCommands(player, commSub, page);
                    return;
                case "exit":
                    DestroyUI(player);
                    return;              
            }
        }
#endregion
        private void SetUIColors()
        {            
            uiColors.Add("bg1", "0 0 0 0.8");
            uiColors.Add("bg2", "0 0 0 0.8");
            uiColors.Add("bg3", "0 0 0 0.8");
            uiColors.Add("button1", "1 1 1 0.5");
            uiColors.Add("button2", "0.5 0.2 0.2 0.8");
            uiColors.Add("button3", "0.67 1.00 0.10 0.8");
        }        
        private List<string> GetGroups() => permission.GetGroups().ToList();

        private bool CreateGroup(string name, string title, int rank) => permission.CreateGroup(name, title, rank);  
        
        private void RemoveGroup(string name) => permission.RemoveGroup(name);

        private void AddToGroup(string userId, string groupId) => permission.AddUserGroup(userId, groupId);

        private void RemoveFromGroup(string userId, string groupId) => permission.RemoveUserGroup(userId, groupId);

        private bool HasGroup(string userId, string groupId) => permission.UserHasGroup(userId, groupId);

        private List<string> GetPermissions()
        {
            List<string> permissions = permission.GetPermissions().ToList();
            permissions.RemoveAll(x => x.ToLower().StartsWith("oxide."));
            return permissions;
        }

        private void GrantPermission(string groupOrID, string perm, bool isGroup = false)
        {
            if (isGroup)
                permission.GrantGroupPermission(groupOrID, perm, null);
            else permission.GrantUserPermission(groupOrID, perm, null);
        }

        private void RevokePermission(string groupOrID, string perm, bool isGroup = false)
        {
            if (isGroup)
                permission.RevokeGroupPermission(groupOrID, perm);
            else permission.RevokeUserPermission(groupOrID, perm);
        }

        private bool HasPermission(string groupOrID, string perm, bool isGroup = false)
        {
            if (isGroup)
                return permission.GroupHasPermission(groupOrID, perm);
            return permission.UserHasPermission(groupOrID, perm);
        }        

        private void DestroyUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, UIElement);
            CuiHelper.DestroyUi(player, UIMain);
            CuiHelper.DestroyUi(player, UIPopup);
        }
        private T ParseType<T>(string type) => (T)Enum.Parse(typeof(T), type, true);

        private bool IsDivisable(int number) => number % 2 == 0;

        private void UpdatePermissionList()
        {
            permissionList.Clear();
            List<string> permissions = GetPermissions();
            permissions.Sort();

            string lastName = string.Empty;
            foreach(string perm in permissions)
            {
                string name = string.Empty;
                if (perm.Contains("."))
                {
                    string permStart = perm.Substring(0, perm.IndexOf("."));
                    name = plugins.PluginManager.GetPlugins().ToList().Find(x => x?.Name?.ToLower() == permStart)?.Title ?? permStart;
                }
                else name = perm;
                if (lastName != name)
                {
                    permissionList.Add(new KeyValuePair<string, bool>(name, false));
                    lastName = name;
                }

                permissionList.Add(new KeyValuePair<string, bool>(perm, true));
            }

        }
		[ChatCommand("admin")]
        private void cmdAdmin(BasePlayer player, string command, string[] args)
        {
            if (!HasPermission(player.UserIDString, "personalcontroller.use")) return;
            Openpersonalcontroller(player); 
        }
 #region Localization
        private string msg(string key, string playerId = null) => lang.GetMessage(key, this, playerId).ToUpper();

        Dictionary<string, string> Messages = new Dictionary<string, string>
        {
            ["title"] = "<color=#ce422b>PERSONAL CONTROLLER</color>",
            ["exit"] = "Exit",
            ["view"] = "View",
            ["player"] = "Player Permissions",
            ["group"] = "Group Permissions",
            ["usergroups"] = "User Groups",
            ["addgroup"] = "Create Group",
            ["removegroup"] = "Remove Group",
            ["chat"] = "Chat Commands",
            ["console"] = "Console Commands",
            ["command"] = "Command",
            ["description"] = "Description",
            ["use"] = "Use",
            ["back"] = "Back",
            ["next"] = "Next",
            ["return"] = "Return",
            ["selectplayer"] = "Select a player",
            ["togglepermplayer"] = "Toggle permissions for player : {0}",
            ["togglepermgroup"] = "Toggle permissions for group : {0}",
            ["togglegroupplayer"] = "Toggle groups for player : {0}",
            ["giveitem"] = "Select a player to give : {0} x {1}",
            ["selectgroup"] = "Select a group",
            ["selectremovegroup"] = "Select a group to remove. <color=#ce422b>WARNING! This can not be undone</color>",
            ["selecttarget"] = "Select a target",
            ["onlineplayers"] = "Online Players",
            ["offlineplayers"] = "Offline Players",
            ["inputhelper"] = "To create a new group type a group name, title, and rank. Press Enter after completing each field. Once you are ready hit the 'Create' button",
            ["create"] = "Create",
            ["groupname"] = "Name:",
            ["grouptitle"] = "Title (optional):",
            ["grouprank"] = "Rank (optional):",
            ["reset"] = "Reset",
            ["nogroupname"] = "You must set a group name",
            ["groupcreated"] = "You have successfully created the group: {0}",
            ["commandrun"] = "You have run the command : {0}",
            ["groupremoved"] = "You have removed the group : {0}",
            ["uiwarning"] = "** Note ** Close any other UI plugins you have running that automatically refresh (LustyMap or InfoPanel for example). Having these open will cause your input boxes to continually refresh!"
        };
        #endregion
        #endregion
    }
}