using UnityEngine;

namespace com.github.zehsteam.Supercharger.Helpers;

public class PlayerAnimatorStateHelper
{
    private Animator _animator;
    private bool _isCrouching;
    private bool _isJumping;
    private bool _isWalking;
    private bool _isSprinting;
    private float _currentAnimationTime;
    private AnimatorStateInfo _currentStateInfo;
    private RuntimeAnimatorController _originalAnimatorController;

    public PlayerAnimatorStateHelper(Animator animator)
    {
        if (animator == null)
        {
            return;
        }

        _animator = animator;
        _originalAnimatorController = animator.runtimeAnimatorController;
    }

    //We need to Save the important states due to how unity handles switching animator overrides (So stupid)
    public void SaveAnimatorStates()
    {
        if (_animator == null)
        {
            return;
        }

        _isCrouching = _animator.GetBool("crouching");
        _isJumping = _animator.GetBool("Jumping");
        _isWalking = _animator.GetBool("Walking");
        _isSprinting = _animator.GetBool("Sprinting");
        _currentStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        _currentAnimationTime = _currentStateInfo.normalizedTime;
    }

    //We need to Restore the important states due to how unity handles switching animator overrides
    public void RestoreAnimatorStates()
    {
        if (_animator == null)
        {
            return;
        }

        _animator.Play(_currentStateInfo.fullPathHash, 0, _currentAnimationTime);
        _animator.SetBool("crouching", _isCrouching);
        _animator.SetBool("Jumping", _isJumping);
        _animator.SetBool("Walking", _isWalking);
        _animator.SetBool("Sprinting", _isSprinting);
    }

    public void RestoreOriginalAnimatorController()
    {
        if (_animator == null)
        {
            return;
        }

        _animator.runtimeAnimatorController = _originalAnimatorController;

        RestoreAnimatorStates();
    }

    public void SetAnimatorOverrideController(AnimatorOverrideController overrideController)
    {
        if (_animator == null)
        {
            return;
        }

        _animator.runtimeAnimatorController = overrideController;

        RestoreAnimatorStates();
    }

    public RuntimeAnimatorController GetOriginalAnimatorController()
    {
        return _originalAnimatorController;
    }
}
