﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.WebCam;

namespace Oxide.Plugins
{
    [Info("InventoryRestore", "CASHR", "1.0.0")]
    class InventoryRestore : RustPlugin
    {
        #region Static
        private Dictionary<ulong, List<KitItem>> InventoryList = new Dictionary<ulong, List<KitItem>>();
        private static string perm = "inventoryrestore.allow";

        public class KitItem
        {
            public string ShortName { get; set; }
            public int Amount { get; set; }
            public int Blueprint { get; set; }
            public ulong SkinID { get; set; }
            public string Container { get; set; }
            public float Condition { get; set; }
            public Weapon Weapon { get; set; }
            public List<ItemContent> Content { get; set; }
        }
        public class Weapon
        {
            public string ammoType { get; set; }
            public int ammoAmount { get; set; }
        }
        public class ItemContent
        {
            public string ShortName { get; set; }
            public float Condition { get; set; }
            public int Amount { get; set; }
        }
        private KitItem ItemToKit(Item item, string container)
        {
            KitItem kitem = new KitItem();
            kitem.Amount = item.amount;
            kitem.Container = container;
            kitem.SkinID = item.skin;
            kitem.Blueprint = item.blueprintTarget;
            kitem.ShortName = item.info.shortname;
            kitem.Condition = item.condition;
            kitem.Weapon = null;
            kitem.Content = null;
            if (item.info.category == ItemCategory.Weapon)
            {
                BaseProjectile weapon = item.GetHeldEntity() as BaseProjectile;
                if (weapon != null)
                {
                    kitem.Weapon = new Weapon();
                    kitem.Weapon.ammoType = weapon.primaryMagazine.ammoType.shortname;
                    kitem.Weapon.ammoAmount = weapon.primaryMagazine.contents;
                }
            }
            if (item.contents != null)
            {
                kitem.Content = new List<ItemContent>();
                foreach (var cont in item.contents.itemList)
                {
                    kitem.Content.Add(new ItemContent()
                    {
                        Amount = cont.amount,
                        Condition = cont.condition,
                        ShortName = cont.info.shortname
                    });
                }
            }
            return kitem;
        }
        #endregion

        #region OxideHooks

        private void OnServerInitialized()
        {
            PrintError($"|-------------------------------------------|");
            PrintWarning($"|          Author: CASHR                  |");
            PrintWarning($"|          VK: vk.com/cashr               |");
            PrintWarning($"|          Discord: CASHR#6906            |");
            PrintWarning($"|          Email: pipnik99@gmail.com      |");
            PrintError($"|-------------------------------------------|");
            permission.RegisterPermission(perm, this);
            foreach (var check in BasePlayer.activePlayerList)
            {
                check.EndSleeping();
            }
        }
        private void OnEntitySpawned(BaseNetworkable entity)
        {
            if (entity != null && entity.name.Contains("item_drop_backpack"))
            {
                entity?.Kill();
            }
        }
        private void OnPlayerRespawned(BasePlayer player)
        {
            if (!permission.UserHasPermission(player.UserIDString, perm)) return;
            if (!InventoryList.ContainsKey(player.userID)) return;
            var items = InventoryList[player.userID];
            ClearInventory(player);           
            foreach (var kitem in InventoryList[player.userID])
            {
                GiveItem(player,
                    BuildItem(kitem.ShortName, kitem.Amount, kitem.SkinID, kitem.Condition, kitem.Blueprint,
                        kitem.Weapon, kitem.Content),
                    kitem.Container == "belt" ? player.inventory.containerBelt :
                    kitem.Container == "wear" ? player.inventory.containerWear : player.inventory.containerMain);
            }
            InventoryList.Remove(player.userID);
        }
        private Item BuildItem(string ShortName, int Amount, ulong SkinID, float Condition, int blueprintTarget, Weapon weapon, List<ItemContent> Content)
        {
            Item item = ItemManager.CreateByName(ShortName, Amount > 1 ? Amount : 1, SkinID);
            item.condition = Condition;

            if (blueprintTarget != 0)
                item.blueprintTarget = blueprintTarget;

            if (weapon != null)
            {
                (item.GetHeldEntity() as BaseProjectile).primaryMagazine.contents = weapon.ammoAmount;
                (item.GetHeldEntity() as BaseProjectile).primaryMagazine.ammoType = ItemManager.FindItemDefinition(weapon.ammoType);
            }
            if (Content != null)
            {
                foreach (var cont in Content)
                {
                    Item new_cont = ItemManager.CreateByName(cont.ShortName, cont.Amount);
                    new_cont.condition = cont.Condition;
                    new_cont.MoveToContainer(item.contents);
                }
            }
            return item;
        }
		
        void OnPlayerDropActiveItem(BasePlayer player, Item item)
        {
            if (player == null || item == null) return;

            var restore = ItemManager.CreateByName(item.info.shortname, item.amount, item.skin);
            restore.name = item.name;
            item.UseItem();
            player.GiveItem(restore);
        }
        private void GiveItem(BasePlayer player, Item item, ItemContainer cont = null)
        {
            if (item == null) return;
            var inv = player.inventory;
            if (cont == inv.containerBelt)
            {
                player.GiveItem(item);
                return;
            }
            var moved =  item.MoveToContainer(cont);
            if (!moved)
            {
                if (cont == inv.containerBelt)
                    moved = item.MoveToContainer(inv.containerWear);
                if (cont == inv.containerWear)
                    moved = item.MoveToContainer(inv.containerBelt);
            }

            if (!moved)
                item.Drop(player.GetCenter(),player.GetDropVelocity());
        }
        private List<KitItem> GetPlayerItems(BasePlayer player)
        {
            List<KitItem> kititems = new List<KitItem>();
            foreach (Item item in player.inventory.containerWear.itemList)
            {
                if (item != null)
                {
                    var iteminfo = ItemToKit(item, "wear");
                    kititems.Add(iteminfo);
                }
            }
            foreach (Item item in player.inventory.containerMain.itemList)
            {
                if (item != null)
                {
                    var iteminfo = ItemToKit(item, "main");
                    kititems.Add(iteminfo);
                }
            }
            foreach (Item item in player.inventory.containerBelt.itemList)
            {
                if (item != null)
                {
                    var iteminfo = ItemToKit(item, "belt");
                    kititems.Add(iteminfo);
                }
            }
            return kititems;
        }
      object OnPlayerDeath(BasePlayer player, HitInfo info)
{
            if (player == null) return null;
            if (!permission.UserHasPermission(player.UserIDString, perm)) return null;
            var Items = GetPlayerItems(player);
            
            InventoryList.Add(player.userID, Items);		
            ClearInventory(player);
    return null;
}
        private void ClearInventory(BasePlayer player)
        {
            player.inventory.containerBelt.Clear();
            player.inventory.containerMain.Clear();
            player.inventory.containerWear.Clear();
        }
        #endregion
    }
}