using com.github.zehsteam.Supercharger.MonoBehaviours;
using HarmonyLib;

namespace com.github.zehsteam.Supercharger.Patches;

[HarmonyPatch(typeof(InteractTrigger))]
internal class InteractTriggerPatch
{
    [HarmonyPatch("specialInteractAnimation")]
    [HarmonyPostfix]
    static void specialInteractAnimationPatch(InteractTrigger __instance)
    {
        if (TryGetSuperchargeStationBehaviour(__instance, out SuperchargeStationBehaviour superchargeStationBehaviour))
        {
            if (!Utils.IsHangarShipSuperchargeStationBehaviour(superchargeStationBehaviour)) return;

            superchargeStationBehaviour.TrySuperchargeNext();
        }
    }

    private static bool TryGetSuperchargeStationBehaviour(InteractTrigger interactTrigger, out SuperchargeStationBehaviour superchargeStationBehaviour)
    {
        superchargeStationBehaviour = null;

        if (!interactTrigger.TryGetComponent(out ItemCharger itemCharger))
        {
            return false;
        }

        superchargeStationBehaviour = itemCharger.chargeStationAnimator.GetComponentInChildren<SuperchargeStationBehaviour>();
        return superchargeStationBehaviour != null;
    }
}
