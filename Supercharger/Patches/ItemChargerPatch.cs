using com.github.zehsteam.Supercharger.Helpers;
using com.github.zehsteam.Supercharger.MonoBehaviours;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace com.github.zehsteam.Supercharger.Patches;

[HarmonyPatch(typeof(ItemCharger))]
internal static class ItemChargerPatch
{
    [HarmonyPatch(nameof(ItemCharger.__initializeVariables))]
    [HarmonyPostfix]
    private static void __initializeVariablesPatch(ItemCharger __instance)
    {
        Object.Instantiate(Content.SuperchargeStationPrefab, Vector3.zero, Quaternion.identity, __instance.chargeStationAnimator.transform);
    }

    [HarmonyPatch(nameof(ItemCharger.ChargeItem))]
    [HarmonyPrefix]
    private static bool ChargeItemPatch(ref ItemCharger __instance, ref Coroutine ___chargeItemCoroutine)
    {
        SuperchargeStationBehaviour superchargeStationBehaviour = null;

        try
        {
            superchargeStationBehaviour = __instance.transform.parent.parent.GetComponentInChildren<SuperchargeStationBehaviour>();
        }
        catch
        {

        }

        if (superchargeStationBehaviour == null)
        {
            return true;
        }

        if (!ShipHelper.IsShipSupercharger(superchargeStationBehaviour))
        {
            return true;
        }

        if (!superchargeStationBehaviour.SuperchargeNext)
        {
            superchargeStationBehaviour.SetOriginalChargeStationValues();
            return true;
        }

        PlayerControllerB playerScript = PlayerUtils.GetLocalPlayerScript();
        if (playerScript == null) return true;

        GrabbableObject currentlyHeldObjectServer = playerScript.currentlyHeldObjectServer;
        if (currentlyHeldObjectServer == null) return true;

        if (!currentlyHeldObjectServer.itemProperties.requiresBattery)
        {
            return true;
        }

        if (___chargeItemCoroutine != null)
        {
            __instance.StopCoroutine(___chargeItemCoroutine);
        }

        superchargeStationBehaviour.SuperchargeItem(currentlyHeldObjectServer, playerScript);

        return false;
    }

    [HarmonyPatch(nameof(ItemCharger.chargeItemDelayed))]
    [HarmonyPrefix]
    private static void chargeItemDelayedPatch(ref GrabbableObject itemToCharge)
    {
        if (itemToCharge == null) return;
        if (itemToCharge.insertedBattery == null) return;
        if (itemToCharge.insertedBattery.empty) return;

        if (itemToCharge.insertedBattery.charge > 1f)
        {
            itemToCharge = null;
        }
    }
}
