using com.github.zehsteam.Supercharger.Helpers;
using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.Supercharger.MonoBehaviours;

public class SuperchargeStationBehaviour : MonoBehaviour
{
    [Header("SuperchargeStation")]
    [Space(5f)]
    public AudioClip ChargeItemSFX;
    public RuntimeAnimatorController ChargeStationAnimatorController;

    [Header("Player")]
    public AnimatorOverrideController PlayerAnimatorOverrideController;

    public bool SuperchargeNext { get; private set; }

    private InteractTrigger _interactTrigger;
    private AudioSource _chargeStationAudio;
    private Animator _chargeStationAnimator;
    private ParticleSystem _chargeStationParticleSystem;

    private RuntimeAnimatorController _originalChargeStationAnimatorController;
    private Dictionary<ulong, PlayerAnimatorStateHelper> _playerAnimatorStateHelpers = [];

    private Coroutine _superchargeItemAnimation;

    private void Start()
    {
        if (transform.parent == null)
        {
            Destroy(gameObject);
            return;
        }

        _interactTrigger = transform.parent.GetComponentInChildren<InteractTrigger>();
        _chargeStationAudio = transform.parent.GetComponentInChildren<AudioSource>();
        _chargeStationAnimator = transform.parent.GetComponent<Animator>();
        _chargeStationParticleSystem = transform.parent.GetComponentInChildren<ParticleSystem>(includeInactive: true);

        _originalChargeStationAnimatorController = _chargeStationAnimator.runtimeAnimatorController;
    }

    public bool TrySuperchargeNext()
    {
        if (!AllowedToSupercharge()) return false;

        SuperchargeNext = Utils.RandomPercent(Plugin.ConfigManager.Supercharger_Chance.Value);

        if (SuperchargeNext)
        {
            SetCustomChargeStationValues();
        }

        return SuperchargeNext;
    }

    public bool AllowedToSupercharge()
    {
        PlayerControllerB playerScript = PlayerUtils.GetLocalPlayerScript();
        if (playerScript == null) return false;

        if (playerScript.currentlyHeldObjectServer == null)
        {
            return false;
        }

        if (playerScript.currentlyHeldObjectServer.insertedBattery == null)
        {
            return false;
        }

        float charge = playerScript.currentlyHeldObjectServer.insertedBattery.charge;

        if (Plugin.ConfigManager.Supercharger_OnlyWhenFullyCharged.Value && charge < 1f)
        {
            return false;
        }

        if (!Plugin.ConfigManager.Supercharger_EnableInOrbit.Value && ShipHelper.IsInOrbit())
        {
            return false;
        }

        return true;
    }

    public void SuperchargeItem(GrabbableObject grabbableObject, PlayerControllerB playerScript)
    {
        PluginNetworkBehaviour.Instance.SuperchargeItemServerRpc();

        if (_superchargeItemAnimation != null)
        {
            StopCoroutine(_superchargeItemAnimation);
        }

        SuperchargeNext = false;

        _superchargeItemAnimation = StartCoroutine(SuperchargeItemAnimation(grabbableObject, playerScript));
    }

    public void SuperchargeItemForOtherClient(PlayerControllerB playerScript)
    {
        GrabbableObject grabbableObject = playerScript.currentlyHeldObjectServer;

        if (_superchargeItemAnimation != null)
        {
            StopCoroutine(_superchargeItemAnimation);
        }

        SuperchargeNext = false;

        _superchargeItemAnimation = StartCoroutine(SuperchargeItemAnimation(grabbableObject, playerScript));
    }

    private IEnumerator SuperchargeItemAnimation(GrabbableObject grabbableObject, PlayerControllerB playerScript)
    {
        Plugin.Instance.LogInfoExtended($"Player \"{playerScript.playerUsername}\" is supercharging \"{grabbableObject.itemProperties.itemName}\".");

        bool isLocalPlayer = PlayerUtils.IsLocalPlayer(playerScript);

        SetCustomChargeStationValues();
        OverridePlayerAnimator(playerScript);

        _chargeStationAudio.PlayOneShot(ChargeItemSFX);
        yield return new WaitForSeconds(0.65f);

        _chargeStationAnimator.SetTrigger("zap");

        if (Plugin.ConfigManager.Supercharger_FlickerShipLights.Value)
        {
            if (ShipHelper.FlickerLightsOnLocalClient(duration: 3f, minInterval: 0.15f, maxInterval: 0.4f))
            {
                ShipHelper.SetLightSwitchInteractable(false);
            }
        }

        if (isLocalPlayer && grabbableObject != null)
        {
            grabbableObject.insertedBattery = new Battery(isEmpty: false, Plugin.ConfigManager.Supercharger_ItemChargeAmount.Value / 100f);
            grabbableObject.SyncBatteryServerRpc(Plugin.ConfigManager.Supercharger_ItemChargeAmount.Value);
        }

        _chargeStationParticleSystem.gameObject.SetActive(true);

        float duration = 2f;
        float timePerIteration = 0.15f;
        int iterations = Mathf.RoundToInt(duration / timePerIteration);

        for (int i = 0; i < iterations; i++)
        {
            yield return new WaitForSeconds(timePerIteration);

            _chargeStationParticleSystem.Stop();
            _chargeStationParticleSystem.Play();
        }

        if (isLocalPlayer && Utils.RandomPercent(Plugin.ConfigManager.Explosion_Chance.Value))
        {
            Vector3 position = grabbableObject.transform.position;
            int damage = Plugin.ConfigManager.Explosion_Damage.Value;
            int enemyDamage = Plugin.ConfigManager.Explosion_EnemyDamage.Value;
            float range = Plugin.ConfigManager.Explosion_Range.Value;

            PluginNetworkBehaviour.Instance.SpawnExplosionServerRpc(position, damage, enemyDamage, range, senderClientId: NetworkUtils.GetLocalClientId());

            if (Plugin.ConfigManager.Explosion_TurnsOffShipLights.Value)
            {
                PluginNetworkBehaviour.Instance.PowerSurgeShipServerRpc();
            }
        }

        yield return new WaitForSeconds(0.85f);

        _chargeStationParticleSystem.gameObject.SetActive(false);
        SetPlayerAnimatorHoldAnimation(playerScript, grabbableObject);

        yield return new WaitForSeconds(0.35f);
        
        SetOriginalChargeStationValues();
        ResetPlayerAnimator(playerScript, grabbableObject);

        ShipHelper.SetLightSwitchInteractable(true);

        Plugin.Instance.LogInfoExtended("Supercharge animation finished.");
    }
    
    public void SetCustomChargeStationValues()
    {
        _interactTrigger.animationWaitTime = 3.75f;
        _chargeStationAnimator.runtimeAnimatorController = ChargeStationAnimatorController;
    }

    public void SetOriginalChargeStationValues()
    {
        _interactTrigger.animationWaitTime = 2f;
        _chargeStationAnimator.runtimeAnimatorController = _originalChargeStationAnimatorController;
    }

    private void OverridePlayerAnimator(PlayerControllerB playerScript)
    {
        ulong playerId = playerScript.actualClientId;

        if (!_playerAnimatorStateHelpers.ContainsKey(playerId))
        {
            _playerAnimatorStateHelpers[playerId] = new PlayerAnimatorStateHelper(playerScript.playerBodyAnimator);
        }

        var playerAnimatorStateHelper = _playerAnimatorStateHelpers[playerId];
        playerAnimatorStateHelper.SaveAnimatorStates();
        playerAnimatorStateHelper.SetAnimatorOverrideController(PlayerAnimatorOverrideController);

        Plugin.Instance.LogInfoExtended($"Animator override set for player \"{playerScript.playerUsername}\".");
    }

    private void ResetPlayerAnimator(PlayerControllerB playerScript, GrabbableObject grabbableObject)
    {
        ulong playerId = playerScript.actualClientId;

        if (!_playerAnimatorStateHelpers.ContainsKey(playerId))
        {
            return;
        }

        var playerAnimatorStateHelper = _playerAnimatorStateHelpers[playerId];
        playerAnimatorStateHelper.SaveAnimatorStates();
        playerAnimatorStateHelper.RestoreOriginalAnimatorController();

        SetPlayerAnimatorHoldAnimation(playerScript, grabbableObject);

        Plugin.Instance.LogInfoExtended($"Animator restored for player \"{playerScript.playerUsername}\".");
    }

    private void SetPlayerAnimatorHoldAnimation(PlayerControllerB playerScript, GrabbableObject grabbableObject)
    {
        Animator animator = playerScript.playerBodyAnimator;

        animator.SetBool("Grab", true);
        animator.SetBool("GrabValidated", true);

        string grabAnimation = grabbableObject.itemProperties.grabAnim;

        if (!string.IsNullOrEmpty(grabAnimation))
        {
            animator.SetBool(grabAnimation, true);
        }
        
        if (grabbableObject.itemProperties.twoHandedAnimation)
        {
            animator.ResetTrigger("SwitchHoldAnimationTwoHanded");
            animator.SetTrigger("SwitchHoldAnimationTwoHanded");
        }

        animator.ResetTrigger("SwitchHoldAnimation");
        animator.SetTrigger("SwitchHoldAnimation");

        animator.Play("chooseHoldAnimation");
    }
}
