using System.Linq;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("HideMapMarker", "CASHR#6906", "1.0.0")]
    internal class HideMapMarker : RustPlugin
    {
        #region OxideHooks

        private void OnServerInitialized()
        {
            PrintWarning(
                $"|-----------------------------------|\n|          Author: CASHR     |\n|          VK: vk.com/cashr         |\n|          Discord: CASHR#6906      |\n|          Email: pipnik99@gmail.com      |\n|-----------------------------------|\nIf you want to order a plugin from me, I am waiting for you in discord.");

            timer.In(10, () =>
            {
                var markers = GameObject.FindObjectsOfType<VendingMachineMapMarker>();
                foreach (var check in markers)
                {
                    check?.Kill();
                }
            });
        }

        #endregion

        
    }
}