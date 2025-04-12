using HMUI;
using IPA.Utilities;
using SiraUtil.Affinity;
using SM64BS.UI;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace SM64BS.Patches
{
    internal class MainMenuFlowPatch : IAffinity
    {
        [Inject] private readonly SettingsViewController _settingsViewController;

        [AffinityPostfix]
        [AffinityPatch(typeof(MainFlowCoordinator), "DidActivate")]
        internal void ShowSettingsView(
            MainFlowCoordinator __instance,
            bool firstActivation,
            bool addedToHierarchy,
            bool screenSystemEnabling)
        {
            if (!firstActivation) return;

            Plugin.Log.Info("Pushing SM64BS settings view to right screen...");

            // Safely push onto the right screen view controller stack
            var navController = __instance.rightScreenViewController;

            if (navController != null && _settingsViewController != null)
            {
                typeof(FlowCoordinator)
                    .GetMethod("PushViewControllerToNavigationController", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(__instance, new object[] { navController, _settingsViewController, null, false });
            }
            else
            {
                Plugin.Log.Error("Failed to get rightScreenViewController or SettingsViewController is null!");
            }
        }
    }
}
