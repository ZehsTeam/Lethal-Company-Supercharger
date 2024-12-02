using GameNetcodeStuff;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.github.zehsteam.Supercharger;

internal class Utils
{
    public static bool RandomPercent(float percent)
    {
        if (percent <= 0f) return false;
        if (percent >= 100f) return true;
        return Random.value * 100 <= percent;
    }

    public static void LogAnimatorParameters(Animator animator)
    {
        StringBuilder builder = new StringBuilder();

        int nameMaxLength = animator.parameters.Select(_ => _.name).OrderByDescending(_ => _.Length).First().Length;

        foreach (var parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger) continue;

            builder.AppendLine($"\"{parameter.name}\", ".PadRight(nameMaxLength + 3));

            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    builder.AppendLine($"Float: {animator.GetFloat(parameter.name)}");
                    break;
                case AnimatorControllerParameterType.Int:
                    builder.AppendLine($"Int:   {animator.GetInteger(parameter.name)}");
                    break;
                case AnimatorControllerParameterType.Bool:
                    builder.AppendLine($"Bool:  {animator.GetBool(parameter.name)}");
                    break;
            }
        }

        Plugin.Instance.LogInfoExtended($"\n\n{builder.ToString()}\n");
    }

    public static bool TryParseValue<T>(string value, out T result)
    {
        try
        {
            if (typeof(T) == typeof(int) && int.TryParse(value, out var intValue))
            {
                result = (T)(object)intValue;
                return true;
            }

            if (typeof(T) == typeof(float) && float.TryParse(value, out var floatValue))
            {
                result = (T)(object)floatValue;
                return true;
            }

            if (typeof(T) == typeof(double) && double.TryParse(value, out var doubleValue))
            {
                result = (T)(object)doubleValue;
                return true;
            }

            if (typeof(T) == typeof(bool) && bool.TryParse(value, out var boolValue))
            {
                result = (T)(object)boolValue;
                return true;
            }

            if (typeof(T) == typeof(string))
            {
                result = (T)(object)value;
                return true;
            }
        }
        catch
        {
            // Ignored
        }

        result = default;
        return false;
    }

    public static void CreateExplosion(Vector3 explosionPosition, bool spawnExplosionEffect = false, int damage = 20, float minDamageRange = 0f, float maxDamageRange = 1f, int enemyHitForce = 6, CauseOfDeath causeOfDeath = CauseOfDeath.Blast, PlayerControllerB attacker = null)
    {
        Transform holder = null;

        if (RoundManager.Instance != null && RoundManager.Instance.mapPropsContainer != null && RoundManager.Instance.mapPropsContainer.transform != null)
        {
            holder = RoundManager.Instance.mapPropsContainer.transform;
        }

        if (spawnExplosionEffect)
        {
            Object.Instantiate(StartOfRound.Instance.explosionPrefab, explosionPosition, Quaternion.Euler(-90f, 0f, 0f), holder).SetActive(true);
        }

        float distanceFromExplosion = Vector3.Distance(PlayerUtils.GetLocalPlayerScript().transform.position, explosionPosition);

        if (distanceFromExplosion < 14f)
        {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
        }
        else if (distanceFromExplosion < 25f)
        {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
        }

        Collider[] colliders = Physics.OverlapSphere(explosionPosition, maxDamageRange, 2621448, QueryTriggerInteraction.Collide);
        
        PlayerControllerB playerScript = null;

        for (int i = 0; i < colliders.Length; i++)
        {
            float distanceFromExplosion2 = Vector3.Distance(explosionPosition, colliders[i].transform.position);

            if (distanceFromExplosion2 > 4f && Physics.Linecast(explosionPosition, colliders[i].transform.position + Vector3.up * 0.3f, 256, QueryTriggerInteraction.Ignore))
            {
                continue;
            }

            if (colliders[i].gameObject.layer == 3)
            {
                playerScript = colliders[i].gameObject.GetComponent<PlayerControllerB>();

                if (playerScript != null && playerScript.IsOwner)
                {
                    float damageMultiplier = 1f - Mathf.Clamp01((distanceFromExplosion2 - minDamageRange) / (maxDamageRange - minDamageRange));
                    Vector3 kickDirection = (playerScript.transform.position - explosionPosition).normalized;

                    if (playerScript.TryGetComponent(out Rigidbody rigidbody))
                    {
                        rigidbody.AddForce(kickDirection * 500f);
                    }

                    Vector3 bodyVelocity = Vector3.Normalize((playerScript.transform.position + Vector3.up * 0.75f - explosionPosition) * 100f) * 30f;

                    playerScript.DamagePlayer((int)(damage * damageMultiplier), hasDamageSFX: true, callRPC: true, causeOfDeath, 0, fallDamage: false, bodyVelocity);
                }
            }
            else if (colliders[i].gameObject.layer == 21)
            {
                Landmine landmine = colliders[i].gameObject.GetComponentInChildren<Landmine>();

                if (landmine != null && !landmine.hasExploded && distanceFromExplosion2 < 6f)
                {
                    Plugin.Instance.LogInfoExtended("Setting off other mine");

                    landmine.StartCoroutine(landmine.TriggerOtherMineDelayed(landmine));
                }
            }
            else if (colliders[i].gameObject.layer == 19)
            {
                EnemyAICollisionDetect enemyAICollisionDetect = colliders[i].gameObject.GetComponentInChildren<EnemyAICollisionDetect>();

                if (enemyAICollisionDetect != null && enemyAICollisionDetect.mainScript.IsOwner && distanceFromExplosion2 < 4.5f)
                {
                    enemyAICollisionDetect.mainScript.HitEnemyOnLocalClient(force: enemyHitForce, playerWhoHit: attacker);
                    enemyAICollisionDetect.mainScript.HitFromExplosion(distanceFromExplosion2);
                }
            }
        }

        int layerMask = ~LayerMask.GetMask("Room");
        layerMask = ~LayerMask.GetMask("Colliders");

        colliders = Physics.OverlapSphere(explosionPosition, 10f, layerMask);

        for (int j = 0; j < colliders.Length; j++)
        {
            if (colliders[j].TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.AddExplosionForce(70f, explosionPosition, 10f);
            }
        }
    }
}
