using System;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("RaidEnhancer", "CASHR#6906", "1.0.0")]
    internal class RaidEnhancer : RustPlugin
    {
        #region Static

        [PluginReference] private Plugin NoEscape;
        private bool IsBlocked(string userID) => (bool) NoEscape?.Call<bool>("IsRaidBlocked", userID);
        private static RaidEnhancer _ins;
        public string cID = "";
        #endregion

        #region OxideHooks

        private void OnServerInitialized()
        {
            _ins = this;
            PrintWarning(
                $"|-----------------------------------|\n|          Author: CASHR     |\n|          VK: vk.com/cashr         |\n|          Discord: CASHR#6906      |\n|          Email: pipnik99@gmail.com      |\n|-----------------------------------|\nIf you want to order a plugin from me, I am waiting for you in discord.");
            LoadConfig();
        }

        private void Unload()
        {
            _ins = null;
            var Player = GameObject.FindObjectsOfType<BlockHealth>();
            foreach (var check in Player)
            {
                check?.Kill();
            }
        }

        void OnEntityBuilt(Planner plan, GameObject go)
        {
            var player = plan?.GetOwnerPlayer() ?? null;
            if (player == null) return;
            if (!IsBlocked(player.UserIDString)) return;
            var block = go?.ToBaseEntity() as BuildingBlock;
            if (block == null) return;
            block.health = 1f;
            block.gameObject.AddComponent<BlockHealth>();
            block.SendNetworkUpdate();

        }
        void OnStructureUpgrade(BuildingBlock block, BasePlayer player, BuildingGrade.Enum grade)
        {
            if (block == null) return;
            if (!IsBlocked(player.UserIDString)) return;
            var oldHeal = block.Health() + 75f;
            NextTick(() =>
            {
                if (block == null) return;
                
                block.health = oldHeal;
                block.SendNetworkUpdate();
                if (block.health != block.MaxHealth())
                {
                    if (block.GetComponent<BlockHealth>() == null)
                    {
                        block.gameObject.AddComponent<BlockHealth>();
                    }
                }
            });
        }

        void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || info == null) return;
            var block = entity.GetComponent<BlockHealth>();
            if (block == null) return;
            if (!IsBlocked(entity.OwnerID.ToString())) return;
            var wall = entity;
            if ((bool)info?.WeaponPrefab?.name?.Contains("ak47"))
            {
                NextTick(() =>
                {
                    if (wall == null) return;
                    wall.health -= 5f;
                    wall?.SendNetworkUpdate();
                });
                return;
            }
            if ((bool) info?.WeaponPrefab?.name?.Contains("smg"))
            {
                NextTick(() =>
                {
                    if (wall == null) return;
                    wall.health -= 4f;
                    wall.SendNetworkUpdate();
                });
            }
        }

        #endregion


        #region Function

        class BlockHealth : FacepunchBehaviour
        {
            private BaseCombatEntity block;

            private void Awake()
            {
                
                block = GetComponent<BaseCombatEntity>();
                InvokeRepeating(StartHealth,1f,1f);
            }

            private void StartHealth()
            {
                if (block == null)
                {
                    Kill();
                    return;
                }
                var heal = _ins.IsBlocked(block.OwnerID.ToString()) ? 0.8f * block.health / 100 : 3;
                block.health += heal;
                if (block.health >= block.MaxHealth())
                {
                    Kill();
                }
            }
            public void Kill()
            {
                CancelInvoke(StartHealth);
                Destroy(this);
            }
        }

        #endregion
    }
}