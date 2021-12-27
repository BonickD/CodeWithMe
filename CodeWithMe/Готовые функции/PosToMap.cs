using UnityEngine;

namespace Oxide.Plugins
{
    [Info("PosToMap", "CASHR", "1.0.0")]
    internal class PosToMap : RustPlugin
    {
        private string PosToMapCoords(Vector3 pos)
        {
            var alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            var coords = "";

            pos.z = -pos.z;
            pos += new Vector3(TerrainMeta.Size.x, 0, TerrainMeta.Size.z) * .5f;

            var cubeSize = 146.14f;

            var xCube = (int) (pos.x / cubeSize);
            var zCube = (int) (pos.z / cubeSize);

            var firstLetterIndex = (int) (xCube / alpha.Length) - 1;
            var firstLetter = "";
            if (firstLetterIndex >= 0)
                firstLetter = $"{alpha[firstLetterIndex]}";

            var xStr = $"{firstLetter}{alpha[xCube % 26]}";
            var zStr = $"{zCube}";


            return $"{xStr}{zStr}";
        }
    }
}