﻿using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.zehsteam.Supercharger.MonoBehaviours;

public class SuperchargeStationBehaviour : MonoBehaviour
{
    [Header("SuperchargeStation")]
    [Space(5f)]
    public AudioClip ChargeItemSFX = null;
    public RuntimeAnimatorController ChargeStationAnimatorController = null;

    [Header("Player")]
    public AnimatorOverrideController PlayerAnimatorOverrideController = null;

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
        _interactTrigger = transform.parent.GetComponentInChildren<InteractTrigger>();
        _chargeStationAudio = transform.parent.GetComponentInChildren<AudioSource>();
        _chargeStationAnimator = transform.parent.GetComponent<Animator>();
        _chargeStationParticleSystem = transform.parent.GetComponentInChildren<ParticleSystem>(includeInactive: true);

        _originalChargeStationAnimatorController = _chargeStationAnimator.runtimeAnimatorController;
    }

    public bool TrySuperchargeNext()
    {
        SuperchargeNext = Utils.RandomPercent(Plugin.ConfigManager.SuperchargeChance);

        if (SuperchargeNext)
        {
            SetCustomChargeStationValues();
        }

        return SuperchargeNext;
    }

    public void SuperchargeItem(GrabbableObject grabbableObject, PlayerControllerB playerScript)
    {
        PluginNetworkBehaviour.Instance.SuperchargeItemServerRpc(PlayerUtils.GetLocalPlayerId());

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

        if (isLocalPlayer && grabbableObject != null)
        {
            grabbableObject.insertedBattery = new Battery(isEmpty: false, 2f);
            grabbableObject.SyncBatteryServerRpc(200);
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

        if (isLocalPlayer && Utils.RandomPercent(Plugin.ConfigManager.ExplodeChance))
        {
            PluginNetworkBehaviour.Instance.SpawnExplosionServerRpc(grabbableObject.transform.position, 50, 4.5f);
        }

        yield return new WaitForSeconds(0.85f);

        _chargeStationParticleSystem.gameObject.SetActive(false);
        SetPlayerAnimatorHoldAnimation(playerScript, grabbableObject);

        yield return new WaitForSeconds(1f);

        SetOriginalChargeStationValues();
        ResetPlayerAnimator(playerScript, grabbableObject);

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
        playerScript.playerBodyAnimator.SetBool("Grab", true);

        string grabAnimation = grabbableObject.itemProperties.grabAnim;

        if (!string.IsNullOrEmpty(grabAnimation))
        {
            playerScript.playerBodyAnimator.SetBool(grabAnimation, true);
        }

        playerScript.playerBodyAnimator.Play("chooseHoldAnimation");
    }
}
