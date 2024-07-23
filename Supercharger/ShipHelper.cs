using com.github.zehsteam.Supercharger.MonoBehaviours;
using System.Collections;
using UnityEngine;

namespace com.github.zehsteam.Supercharger;

internal class ShipHelper
{
    public static bool PowerSurgedShip = false;

    private static Coroutine _flickerLightsOnLocalClientAnimation;

    public static bool IsInOrbit()
    {
        if (StartOfRound.Instance.inShipPhase) return true;

        return false;
    }

    public static bool IsShipSupercharger(SuperchargeStationBehaviour superchargeStationBehaviour)
    {
        if (TryGetSuperchargeStationBehaviour(out SuperchargeStationBehaviour hangarShipSuperchargeStationBehaviour))
        {
            return superchargeStationBehaviour == hangarShipSuperchargeStationBehaviour;
        }

        return false;
    }

    public static bool TryGetSuperchargeStationBehaviour(out SuperchargeStationBehaviour superchargeStationBehaviour)
    {
        superchargeStationBehaviour = null;

        if (TryGetChargeStationGameObject(out GameObject chargeStationObject))
        {
            superchargeStationBehaviour = chargeStationObject.GetComponentInChildren<SuperchargeStationBehaviour>();
            return superchargeStationBehaviour != null;
        }

        return false;
    }

    private static bool TryGetChargeStationGameObject(out GameObject gameObject)
    {
        gameObject = null;

        try
        {
            gameObject = GameObject.Find("/Environment/HangarShip/ShipModels2b/ChargeStation");
        }
        catch
        {
            return false;
        }

        return gameObject != null;
    }

    public static void SetLightSwitchInteractable(bool value)
    {
        if (TryGetLightSwitchInteractTrigger(out InteractTrigger interactTrigger))
        {
            interactTrigger.interactable = value;
        }
    }

    private static bool TryGetLightSwitchInteractTrigger(out InteractTrigger interactTrigger)
    {
        interactTrigger = null;

        GameObject lightSwitchObject;

        try
        {
            lightSwitchObject = GameObject.Find("/Environment/HangarShip/LightSwitchContainer");
        }
        catch
        {
            return false;
        }

        interactTrigger = lightSwitchObject.GetComponentInChildren<InteractTrigger>();
        return interactTrigger != null;
    }

    public static bool FlickerLightsOnLocalClient(float duration, float minInterval, float maxInterval)
    {
        if (!StartOfRound.Instance.shipRoomLights.areLightsOn) return false;

        if (_flickerLightsOnLocalClientAnimation != null)
        {
            StartOfRound.Instance.StopCoroutine(_flickerLightsOnLocalClientAnimation);
        }

        _flickerLightsOnLocalClientAnimation = StartOfRound.Instance.StartCoroutine(FlickerLightsOnLocalClientAnimation(duration, minInterval, maxInterval));
        return true;
    }

    private static IEnumerator FlickerLightsOnLocalClientAnimation(float duration, float minInterval, float maxInterval)
    {
        bool previouslyPowerSurged = PowerSurgedShip;

        if (PowerSurgedShip)
        {
            PowerSurgedShip = false;
        }

        float timer = 0f;
        float intervalTimer = 0f;

        float interval = Random.Range(minInterval, maxInterval);

        while (timer < duration)
        {
            if (PowerSurgedShip && !previouslyPowerSurged)
            {
                PowerSurgedShip = false;
                StartOfRound.Instance.shipRoomLights.SetShipLightsOnLocalClientOnly(false);
                yield break;
            }

            if (intervalTimer >= interval)
            {
                StartOfRound.Instance.shipRoomLights.ToggleShipLightsOnLocalClientOnly();

                interval = Random.Range(minInterval, maxInterval);
                intervalTimer = 0;
            }

            yield return null;
            timer += Time.deltaTime;
            intervalTimer += Time.deltaTime;
        }

        if (PowerSurgedShip && !previouslyPowerSurged)
        {
            PowerSurgedShip = false;
            StartOfRound.Instance.shipRoomLights.SetShipLightsOnLocalClientOnly(false);
            yield break;
        }

        StartOfRound.Instance.shipRoomLights.SetShipLightsOnLocalClientOnly(true);
    }
}
