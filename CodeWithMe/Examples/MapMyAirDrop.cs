using System;
using System.Collections.Generic;
using System.Linq;
using Oxide.Game.Rust.Cui;
using UnityEngine; //dict
//Vector3
//List

//String.

namespace Oxide.Plugins
{
    [Info("Map My AirDrop", "BuzZ[PHOQUE]", "0.0.7")]
    [Description("Display a popup on Cargo Plane spawn, and a marker on ingame map at drop position.")]
    public class MapMyAirDrop : RustPlugin
    {
        private const string MapMyAirdropHUD = "mapmyairdrop.hud";

        private const string MapMyAirdropBanner = "mapmyairdrop.banner";
        public bool bannerdrop = true;
        public Dictionary<BasePlayer, List<string>> bannerlist = new Dictionary<BasePlayer, List<string>>();
        public bool bannerspawn = true;

        //List<string> HUDlist = new List<string>();
        //List<string> bannerlist = new List<string>();
        private Timer cargorefresh;

        //private string CargoHUDBanner;
        //private string CargoBanner;
        private bool ConfigChanged;
        private bool debug = false;

        public Dictionary<BaseEntity, Vector3> dropposition = new Dictionary<BaseEntity, Vector3>();

        public Dictionary<BaseEntity, MapMarkerGenericRadius> dropradius =
            new Dictionary<BaseEntity, MapMarkerGenericRadius>();

        public Dictionary<BaseEntity, SupplyDrop> entsupply = new Dictionary<BaseEntity, SupplyDrop>();

        //public Dictionary<string, BasePlayer> HUDlist = new Dictionary<string,BasePlayer>();
        //public Dictionary<string, BasePlayer> bannerlist = new Dictionary<string,BasePlayer>();
        public Dictionary<BasePlayer, List<string>> HUDlist = new Dictionary<BasePlayer, List<string>>();

        public Dictionary<BaseEntity, bool> lootedornot = new Dictionary<BaseEntity, bool>();

        private float mapmarkerradius = 10;

        private void Init()
        {
            LoadVariables();
            permission.RegisterPermission(MapMyAirdropHUD, this);
            permission.RegisterPermission(MapMyAirdropBanner, this);
        }

        private void Unload()
        {
            foreach (var paire in dropradius)
                if (paire.Value != null)
                {
                    paire.Value.Kill();
                    paire.Value.SendUpdate();
                    if (debug) Puts("AIRDROP MAPMARKER KILLED");
                }

            DestoyAllUi();
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            DestroyOneBanner(player);
            DestroyOneHUD(player);
        }

        private void DestoyAllUi()
        {
            DestroyAllHUD();
            DestroyAllBanner();
        }

        #region MESSAGES

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                {"DroppedMsg", "Aidrop dropped ! Check its position on your MAP (G) - magenta marker."},
                {"SpawnMsg", "A Cargo Plane has spawn. Airdrop will be notified on time."},
                {"LootedMsg", "Someone is looting a SupplyDrop. Marker changed to cyan color."},
                {"KilledMsg", "A Supplydrop has despawn."},
                {"HUDDistanceMsg", "<size=12><color=orange>{0}m.</color></size> away"},
                {"HUDAirdropMsg", "<color=white>AIRDROP</color><color=black>#</color>{0}\n{1}"}
            }, this);

            lang.RegisterMessages(new Dictionary<string, string>
            {
                {"DroppedMsg", "Le Cargo a laché sa cargaison ! Retrouvez la sur la carte (G) - marqueur magenta."},
                {"SpawnMsg", "Un avion cargo vient d'apparaître. Vous serez informé au moment du largage."},
                {"LootedMsg", "Quelqu'un a ouvert une cargaison. Le marqueur est dorénavant bleu ciel."},
                {"KilledMsg", "Une cargaison a été supprimée."},
                {"HUDDistanceMsg", "à <size=12><color=orange>{0}m.</color></size>"},
                {"HUDAirdropMsg", "<color=white>AIRDROP</color><color=black>#</color>{0}\n{1}"}
            }, this, "fr");
        }

        #endregion

////////////////////////////////////////////////////////////////////////////////

        private void Destroytimer()
        {
            if (cargorefresh != null) cargorefresh.Destroy();
        }

        private void DestroyAllHUD()
        {
            var todel = new List<string>();
            if (HUDlist != null)
                foreach (var player in BasePlayer.activePlayerList.ToList())
                {
                    todel = new List<string>();
                    foreach (var playerhud in HUDlist)
                        if (playerhud.Key == player)
                            todel = playerhud.Value;

                    foreach (var item in todel) CuiHelper.DestroyUi(player, item);
                }
        }

        private void DestroyOneHUD(BasePlayer player)
        {
            var todel = new List<string>();
            if (HUDlist != null)
            {
                foreach (var playerhud in HUDlist)
                    if (playerhud.Key == player)
                        todel = playerhud.Value;

                foreach (var item in todel) CuiHelper.DestroyUi(player, item);
            }

            HUDlist.Remove(player);
        }

        private void DestroyAllBanner()
        {
            var todel = new List<string>();
            if (HUDlist != null)
                foreach (var player in BasePlayer.activePlayerList.ToList())
                {
                    todel = new List<string>();
                    foreach (var playerbanner in bannerlist)
                        if (playerbanner.Key == player)
                            todel = playerbanner.Value;

                    foreach (var item in todel) CuiHelper.DestroyUi(player, item);
                }
        }

        private void DestroyOneBanner(BasePlayer player)
        {
            var todel = new List<string>();
            if (bannerlist != null)
            {
                foreach (var playerbanner in bannerlist)
                    if (playerbanner.Key == player)
                        todel = playerbanner.Value;

                foreach (var item in todel) CuiHelper.DestroyUi(player, item);
            }

            bannerlist.Remove(player);
        }

        private void MarkerDisplayingDelete(BaseEntity Entity)
        {
            MapMarkerGenericRadius delmarker;
            dropradius.TryGetValue(Entity, out delmarker);
            foreach (var paire in dropradius)
                if (paire.Value == delmarker)
                {
                    delmarker.Kill();
                    delmarker.SendUpdate();
                }

            if (debug) Puts("AIRDROP MAPMARKER KILLED");
        }

        private void GenerateMarkers()
        {
            if (dropradius != null)
                foreach (var paire in dropradius)
                {
                    var MapMarkerDel = paire.Value;
                    if (MapMarkerDel != null)
                    {
                        MapMarkerDel.Kill();
                        MapMarkerDel.SendUpdate();
                    }
                }

            foreach (var paire in dropposition)
            {
                Vector3 position;
                position = paire.Value;
                bool looted;
                lootedornot.TryGetValue(paire.Key, out looted);
                var MapMarker =
                    GameManager.server.CreateEntity("assets/prefabs/tools/map/genericradiusmarker.prefab", position) as
                        MapMarkerGenericRadius;
                if (MapMarker == null) return;
                MapMarker.alpha = 0.4f;
                MapMarker.color1 = Color.magenta;
                if (looted) MapMarker.color1 = Color.cyan;

                MapMarker.color2 = Color.black;
                MapMarker.radius = mapmarkerradius;
                dropradius.Remove(paire.Key);
                dropradius.Add(paire.Key, MapMarker);
                if (debug) Puts("CARGO MARKER ADDED IN DICO");
            }

            foreach (var markers in dropradius)
            {
                markers.Value.Spawn();
                markers.Value.SendUpdate();
            }
        }

        #region HUD

        private void CargoHUD(string reason)
        {
            if (debug) Puts("HUD STARTS");

            DestroyAllHUD();
            HUDlist.Clear();
            var colonnegauche = 0.10;
            var colonnedroite = colonnegauche + 0.07;
            var lignehaut = 1.00;
            var lignebas = lignehaut - 0.05;
            var Round = 1;
            var round = -1;
            var positionlist = new List<Vector3>();
            var droplist = new List<BaseEntity>();
            Vector3[] positionarray;
            BaseEntity[] droparray;
            if (reason == "dropped")
            {
                if (debug) Puts("HUD FOR DROP");

                foreach (var Suppliez in entsupply)
                {
                    var supplyupdated = Suppliez.Key.transform.position;
                    if (debug) Puts($"REFRESHED SUPPLY POSITION {supplyupdated}");

                    dropposition.Remove(Suppliez.Key);
                    dropposition.Add(Suppliez.Key, supplyupdated);
                }

                foreach (var pair in dropposition)
                {
                    //droplist.Remove(pair.Key);					
                    droplist.Add(pair.Key);
                    positionlist.Add(pair.Value);
                }

                droparray = droplist.ToArray();
                positionarray = positionlist.ToArray();
                var dropnum = droplist.Count;
                var message = "";
                var HUDforplayers = new List<string>();
                foreach (var player in BasePlayer.activePlayerList.ToList())
                {
                    HUDforplayers = new List<string>();
                    for (Round = 1; Round <= dropnum; Round++)
                    {
                        if (debug) Puts($"round {round} on {dropnum}");

                        round = round + 1;
                        var colonnedecalage = 0.08 * round;
                        var HUDview = permission.UserHasPermission(player.UserIDString, MapMyAirdropHUD);
                        var CuiElement = new CuiElementContainer();
                        var CargoHUDBanner = CuiElement.Add(new CuiPanel
                        {
                            Image = {Color = "0.5 0.5 0.5 0.2"},
                            RectTransform =
                            {
                                AnchorMin = $"{colonnegauche + colonnedecalage} {lignebas}",
                                AnchorMax = $"{colonnedroite + colonnedecalage} {lignehaut}"
                            },
                            CursorEnabled = false
                        });
                        //}, new CuiElement().Parent = "Overlay", CargoHUDBanner);
                        var closeButton = new CuiButton
                        {
                            Button = {Close = CargoHUDBanner, Color = "0.0 0.0 0.0 0.6"},
                            RectTransform = {AnchorMin = "0.90 0.00", AnchorMax = "1.00 1.00"},
                            Text = {Text = "X", FontSize = 8, Align = TextAnchor.MiddleCenter}
                        };
                        CuiElement.Add(closeButton,
                            CargoHUDBanner); // close button in case plugin reload while HUD are on.	
                        if (debug) Puts("PLAYER BEFORE DISTANCE");

                        var dropis = positionarray[round];
                        var dist = (int) Vector3.Distance(dropis, player.transform.position);
                        message = string.Format(lang.GetMessage("HUDDistanceMsg", this, player.UserIDString),
                            dist.ToString());
                        if (debug) Puts($"PLAYER DISTANCE MESSAGE DONE : {message}");

                        var playerdistance = CuiElement.Add(new CuiLabel
                        {
                            Text =
                            {
                                Text = string.Format(lang.GetMessage("HUDAirdropMsg", this, player.UserIDString),
                                    round + 1, message),
                                Color = "1.0 1.0 1.0 1.0", FontSize = 10, Align = TextAnchor.MiddleCenter
                            },
                            RectTransform = {AnchorMin = "0.0 0.0", AnchorMax = "0.85 1.0"}
                        }, CargoHUDBanner);
                        if (HUDview) CuiHelper.AddUi(player, CuiElement);

                        HUDforplayers.Add(CargoHUDBanner);
                    }

                    HUDlist.Remove(player);
                    HUDlist.Add(player, HUDforplayers);
                }
            }
        }

        #endregion

        #region SPAWN DETECTION

        private void OnEntitySpawned(BaseEntity Entity)
        {
            if (Entity == null) return;
            if (Entity is CargoPlane)
            {
                if (bannerspawn) DisplayBannerToAll("spawn");
                if (debug) Puts("SPAWN - CARGO");

                //CargoHUD("spawn", null);
            }

            if (Entity is SupplyDrop)
            {
                if (bannerdrop) DisplayBannerToAll("dropped");
                var dropped = Entity as SupplyDrop;
                entsupply.Add(Entity, dropped);
                var supplyposition = Entity.transform.position;
                var supplyx = Entity.transform.position.x;
                var supplyz = Entity.transform.position.z;
                if (debug) Puts($"SUPPLY SPAWNED x={supplyx} z={supplyz}");

                dropposition.Add(Entity, supplyposition);
                GenerateMarkers();
                Destroytimer();
                cargorefresh = timer.Repeat(5, 0, () =>
                {
                    if (Entity != null) CargoHUD("dropped");
                });
                //dropped.RemoveParachute();
            }
        }

        #endregion

        private void OnEntityKill(BaseNetworkable entity)
        {
            var killed = entity as BaseEntity;
            if (entsupply.ContainsKey(killed))
            {
                if (debug) Puts("KILL OF A SUPPLYDROP");

                MarkerDisplayingDelete(killed);
                entsupply.Remove(killed);
                dropposition.Remove(killed);
                dropradius.Remove(killed);
                lootedornot.Remove(killed);
                DisplayBannerToAll("killed");
                IfNoMore();
            }
        }

        private void IfNoMore()
        {
            if (dropposition.Count == 0)
            {
                Destroytimer();
                GenerateMarkers();
                dropradius.Clear();
                dropposition.Clear();
                lootedornot.Clear();
                entsupply.Clear();
                DestoyAllUi();
            }
        }

        private void OnLootEntity(BasePlayer player, BaseEntity entity)
        {
            if (entity is SupplyDrop)
            {
                // Vector3 playerpos = player.transform.position;
                if (lootedornot.ContainsKey(entity))
                {
                    bool looted;
                    lootedornot.TryGetValue(entity, out looted);
                    if (looted) return;
                }

                foreach (var paire in entsupply)
                    if (paire.Key == entity)
                    {
                        if (debug) Puts("PLAYER LOOTING A MARKED SUPPLYDROP");

                        lootedornot.Remove(entity);
                        lootedornot.Add(entity, true);
                        DisplayBannerToAll("looted");
                    }

                GenerateMarkers();
            }
        }

        #region BANNER

        private void DisplayBannerToAll(string reason)
        {
            DestroyAllBanner();
            bannerlist.Clear();
            foreach (var player in BasePlayer.activePlayerList.ToList())
            {
                var bannerforplayers = new List<string>();

                var lignehaut = 0.88;
                var lignebas = 0.85;
                var message = string.Empty;
                switch (reason)
                {
                    case "spawn":
                    {
                        message = lang.GetMessage("SpawnMsg", this, player.UserIDString);
                        break;
                    }
                    case "dropped":
                    {
                        message = lang.GetMessage("DroppedMsg", this, player.UserIDString);
                        break;
                    }
                    case "looted":
                    {
                        message = lang.GetMessage("LootedMsg", this, player.UserIDString);
                        break;
                    }
                    case "killed":
                    {
                        message = lang.GetMessage("KilledMsg", this, player.UserIDString);
                        lignehaut = lignehaut - 0.06;
                        lignebas = lignebas - 0.06;
                        break;
                    }
                }

                var CuiElement = new CuiElementContainer();
                var CargoBanner = CuiElement.Add(new CuiPanel
                {
                    Image = {Color = "0.5 0.5 0.5 0.30"},
                    RectTransform = {AnchorMin = $"0.0 {lignebas}", AnchorMax = $"1.0 {lignehaut}"},
                    CursorEnabled = false
                });
                var closeButton = new CuiButton
                {
                    Button = {Close = CargoBanner, Color = "0.0 0.0 0.0 0.6"},
                    RectTransform = {AnchorMin = "0.90 0.01", AnchorMax = "0.99 0.99"},
                    Text = {Text = "X", FontSize = 12, Align = TextAnchor.MiddleCenter}
                };
                CuiElement.Add(closeButton, CargoBanner);
                CuiElement.Add(new CuiLabel
                {
                    Text =
                    {
                        Text = $"{message}", FontSize = 14, FadeIn = 1.0f, Align = TextAnchor.MiddleCenter,
                        Color = "1.0 1.0 1.0 1"
                    },
                    RectTransform = {AnchorMin = "0.10 0.10", AnchorMax = "0.90 0.90"}
                }, CargoBanner);

                var bannerview = permission.UserHasPermission(player.UserIDString, MapMyAirdropBanner);

                if (bannerview)
                {
                    CuiHelper.AddUi(player, CuiElement);
                    timer.Once(6, () =>
                    {
                        //DestroyAllBanner();
                        CuiHelper.DestroyUi(player, CargoBanner);
                    });
                }

                bannerforplayers.Add(CargoBanner);
                bannerlist.Remove(player);
                bannerlist.Add(player, bannerforplayers);


                //var sound = new Effect("assets/bundled/prefabs/fx/player/howl.prefab", player, 0, Vector3.zero, Vector3.forward);
                //EffectNetwork.Send(sound, player.net.connection);
                //if (debug == true) {Puts($"BANNER AND SOUNDFX PLAYING FOR PLAYER {player.displayName}");}
            }
        }

        #endregion

        #region CONFIG

        protected override void LoadDefaultConfig()
        {
            LoadVariables();
        }

        private void LoadVariables()
        {
            //SteamIDIcon = Convert.ToUInt64(GetConfig("Chat Settings", "SteamIDIcon", "76561197987461623"));        // SteamID FOR PLUGIN ICON - STEAM PROFILE CREATED FOR THIS PLUGIN / NONE YET /
            mapmarkerradius = Convert.ToSingle(GetConfig("Map Marker settings", "Radius (10 by default)", "10"));
            bannerspawn = Convert.ToBoolean(GetConfig("Display banner (for players with permission .banner)",
                "On Cargo spawn", "true"));
            bannerdrop = Convert.ToBoolean(GetConfig("Display banner (for players with permission .banner)",
                "On Air Drop", "true"));

            if (!ConfigChanged) return;
            SaveConfig();
            ConfigChanged = false;
        }

        private object GetConfig(string menu, string datavalue, object defaultValue)
        {
            var data = Config[menu] as Dictionary<string, object>;
            if (data == null)
            {
                data = new Dictionary<string, object>();
                Config[menu] = data;
                ConfigChanged = true;
            }

            object value;
            if (!data.TryGetValue(datavalue, out value))
            {
                value = defaultValue;
                data[datavalue] = value;
                ConfigChanged = true;
            }

            return value;
        }

        #endregion
    }
}