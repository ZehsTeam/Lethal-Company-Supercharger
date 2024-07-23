using System.Linq;
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
        string message = "";

        int nameMaxLength = animator.parameters.Select(_ => _.name).OrderByDescending(_ => _.Length).First().Length;

        foreach (var parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger) continue;

            message += $"\"{parameter.name}\", ".PadRight(nameMaxLength + 3);

            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    message += $"Float: {animator.GetFloat(parameter.name)}";
                    break;
                case AnimatorControllerParameterType.Int:
                    message += $"Int:   {animator.GetInteger(parameter.name)}";
                    break;
                case AnimatorControllerParameterType.Bool:
                    message += $"Bool:  {animator.GetBool(parameter.name)}";
                    break;
            }

            message += "\n";
        }

        Plugin.Instance.LogInfoExtended($"\n\n{message.Trim()}\n");
    }
}
