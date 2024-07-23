using com.github.zehsteam.Supercharger.MonoBehaviours;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace com.github.zehsteam.Supercharger.Patches;

[HarmonyPatch(typeof(ItemCharger))]
internal class ItemChargerPatch
{
    [HarmonyPatch("__initializeVariables")]
    [HarmonyPostfix]
    static void __initializeVariablesPatch(ItemCharger __instance)
    {
        Object.Instantiate(Content.SuperchargeStationPrefab, Vector3.zero, Quaternion.identity, __instance.chargeStationAnimator.transform);
    }

    [HarmonyPatch(nameof(ItemCharger.ChargeItem))]
    [HarmonyPrefix]
    static bool ChargeItemPatch(ref ItemCharger __instance, ref Coroutine ___chargeItemCoroutine)
    {
        SuperchargeStationBehaviour superchargeStationBehaviour = __instance.transform.parent.parent.GetComponentInChildren<SuperchargeStationBehaviour>();
        if (superchargeStationBehaviour == null) return true;

        if (!ShipHelper.IsShipSupercharger(superchargeStationBehaviour)) return true;

        if (!superchargeStationBehaviour.SuperchargeNext)
        {
            superchargeStationBehaviour.SetOriginalChargeStationValues();
            return true;
        }

        PlayerControllerB playerScript = PlayerUtils.GetLocalPlayerScript();

        GrabbableObject currentlyHeldObjectServer = playerScript.currentlyHeldObjectServer;
        if (currentlyHeldObjectServer == null) return true;
        if (!currentlyHeldObjectServer.itemProperties.requiresBattery) return true;

        if (___chargeItemCoroutine != null)
        {
            __instance.StopCoroutine(___chargeItemCoroutine);
        }

        superchargeStationBehaviour.SuperchargeItem(currentlyHeldObjectServer, playerScript);

        return false;
    }

    [HarmonyPatch("chargeItemDelayed")]
    [HarmonyPrefix]
    static void chargeItemDelayedPatch(ref GrabbableObject itemToCharge)
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
