using System;
using System.Collections.Generic;
using ConVar;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using Physics = UnityEngine.Physics;

namespace Oxide.Plugins
{
    [Info("RandomSpawnRadius", "CASHR#6906", "1.0.0")]
    internal class RandomSpawnRadius : RustPlugin
    {
        #region Static
        private Configuration _config;


        #endregion

        #region Config

        private class Configuration
        {
            [JsonProperty("Radius")]
            public int ItemSide = 100;
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
            LoadConfig();

        }


        #endregion
        
        private BasePlayer.SpawnPoint OnFindSpawnPoint()
        {
            Vector3 vect = new Vector3(0, 0, 0);
            Vector3 center = new Vector3(0, 0, 0);
            float pos = 0;
            do 
            {
                center = RandomCircle(new Vector3(0, 0, 0), _config.ItemSide);
                pos = GetGroundPosition(center);
                vect = new Vector3(center.x, pos, center.z);
            } while (TestPosAgain(vect));
            return new BasePlayer.SpawnPoint
            {
                pos = vect, rot = new Quaternion()
            };
        }

        private bool TestPosAgain(Vector3 spawnPos)
        {
            if (WaterLevel.Test(spawnPos))
                return false;
            

            if (!ValidBounds.Test(spawnPos))
                return false;
            

            var colliders = Facepunch.Pool.GetList<Collider>();
            Vis.Colliders(spawnPos, 3f, colliders);
            foreach (var collider in colliders)
                switch (collider.gameObject.layer)
                {
                    case (int)Rust.Layer.Prevent_Building:
                        Facepunch.Pool.FreeList(ref colliders);
                        return false;
                    case (int)Rust.Layer.Vehicle_Large: 
                    case (int)Rust.Layer.Vehicle_World:
                    case (int)Rust.Layer.Vehicle_Detailed:
                        Facepunch.Pool.FreeList(ref colliders);
                        return false;
                }
            Facepunch.Pool.FreeList(ref colliders);

            return true;
        }

        private Vector3 RandomCircle(Vector3 center, float radius = 5)
        {
            var ang = UnityEngine.Random.value * 360;
            Vector3 pos;
            pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
            pos.z = center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
            pos.y = center.y;
            pos.y = GetGroundPosition(pos);
            return pos;
        }
        private static float GetGroundPosition(Vector3 pos)
        {
            var y = TerrainMeta.HeightMap.GetHeight(pos);
            RaycastHit hitInfo;

            if (!Physics.Raycast(
                new Vector3(pos.x, pos.y + 200f, pos.z),
                Vector3.down,
                out hitInfo,
                float.MaxValue,
                Rust.Layers.Mask.Vehicle_Large))
                return Physics.Raycast(
                    new Vector3(pos.x, pos.y + 200f, pos.z),
                    Vector3.down,
                    out hitInfo,
                    float.MaxValue,
                    Rust.Layers.Solid | Rust.Layers.Mask.Water
                )
                    ? Mathf.Max(hitInfo.point.y, y)
                    : y;
            var cargoShip = hitInfo.GetEntity() as CargoShip;
            if (cargoShip == null)
                return Physics.Raycast(
                    new Vector3(pos.x, pos.y + 200f, pos.z),
                    Vector3.down,
                    out hitInfo,
                    float.MaxValue,
                    Rust.Layers.Solid | Rust.Layers.Mask.Water
                )
                    ? Mathf.Max(hitInfo.point.y, y)
                    : y;
            y = hitInfo.point.y;
            return y;
        }
    }
}