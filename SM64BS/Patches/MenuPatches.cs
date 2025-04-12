using HMUI;
using IPA.Utilities;
using SiraUtil.Affinity;
using SM64BS.UI;
using System.Linq;
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

            Plugin.Log.Info("Pushing SM64BS settings view...");
            typeof(MainFlowCoordinator)
                .GetMethod("PresentViewController", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(__instance, new object[] { _settingsViewController, null, false });
        }
    }
}
