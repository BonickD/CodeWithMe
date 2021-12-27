/*
 
 private void OnPlayerInput(BasePlayer player, InputState input)
        {
            if (!input.WasJustPressed(BUTTON.USE)) return;

            Quaternion currentRot;
            TryGetPlayerView(player, out currentRot);
            var hitpoints = Physics.RaycastAll(new Ray(player.transform.position + eyesPosition, currentRot * Vector3.forward), 5f, LayerMask.GetMask("Player (Server)"));
            Array.Sort(hitpoints, (a, b) => a.distance == b.distance ? 0 : a.distance > b.distance ? 1 : -1);
            for (var i = 0; i < hitpoints.Length; i++)
            {
                var humanPlayer = hitpoints[i].collider.GetComponentInParent<DecoderPlayer>();
                if (humanPlayer != null)
                {
                    humanPlayer.LookTowards(player.transform.position);
                    humanPlayer.SignalBroadcast(BaseEntity.Signal.Gesture, "wave", (Connection)null);
                    Interface.Oxide.CallHook("UseMapDecoder", humanPlayer.player, player);
                    DrawDecodUI(player);
                    break;
                }
            }
        }
		public class DecoderPlayer : MonoBehaviour
        {
            public BasePlayer player;
            MapMarkerGenericRadius mapmarker;
            VendingMachineMapMarker MarkerName;
            private void Awake()
            {
                player = GetComponent<BasePlayer>();
                SpawnMapMarkers();
            }
            public void SpawnMapMarkers()
            {
                MarkerName = GameManager.server.CreateEntity("assets/prefabs/deployable/vendingmachine/vending_mapmarker.prefab", player.transform.position, Quaternion.identity, true) as VendingMachineMapMarker;
                MarkerName.markerShopName = "Картограф";
                MarkerName.Spawn();
                mapmarker = GameManager.server.CreateEntity("assets/prefabs/tools/map/genericradiusmarker.prefab", player.transform.position, Quaternion.identity, true) as MapMarkerGenericRadius;
                mapmarker.enableSaving = true;
                mapmarker.Spawn();
                mapmarker.radius = 0.3f;
                mapmarker.alpha = 1f;

                UnityEngine.Color color = new UnityEngine.Color(0.23f, 0.48f, 0.18f, 1.00f);
                UnityEngine.Color color2 = new UnityEngine.Color(0, 0, 0, 0);
                mapmarker.color1 = color;
                mapmarker.color2 = color2;

                mapmarker.SendUpdate();
                //InvokeRepeating("MarkeUpdate", 1f, 1f);
            }
            public void MarkeUpdate()
            {
                mapmarker.SendUpdate();
            }
            public void LookTowards(Vector3 pos)
            {
                if (pos != player.transform.position)
                    SetViewAngle(Quaternion.LookRotation(pos - player.transform.position));
            }
            public void SetViewAngle(Quaternion viewAngles)
            {
                if (viewAngles.eulerAngles == default(Vector3)) return;
                player.viewAngles = viewAngles.eulerAngles;
                player.SendNetworkUpdate();
            }
            public HeldEntity GetFirstWeapon()
            {
                foreach (Item item in player.inventory.containerBelt.itemList)
                {
                    if (item.CanBeHeld() && (item.info.category == ItemCategory.Weapon))
                        return item.GetHeldEntity() as HeldEntity;
                }
                return null;
            } 

            public void Equip()
            {
                HeldEntity weapon1 = GetFirstWeapon();
                if (weapon1 != null)
                {
                    weapon1.SetHeld(true);
                }
            }
            public void SignalBroadcast(BaseEntity.Signal signal, string arg, Network.Connection sourceConnection = null)
            {
                if (player.net == null || player.net.group == null)
                    return;
                player.ClientRPCEx<int, string>(new SendInfo((List<Network.Connection>)player.net.group.subscribers)
                {
                    method = SendMethod.Unreliable,
                    priority = Priority.Immediate
                }, sourceConnection, "SignalFromServerEx", (int)signal, arg);
            }

            void OnDestroy()
            {
                Destroy(this);
                if (mapmarker != null) mapmarker.Invoke("KillMessage", 0.1f);
                if (MarkerName != null) MarkerName.Invoke("KillMessage", 0.1f);
            }
        }
    
*/