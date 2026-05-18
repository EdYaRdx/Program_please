using UnityEngine;

namespace UnityFunctools
{
    public static class AnimatorExtensions
    {
        public static bool HasParameter(this Animator animator, string paramName, AnimatorControllerParameterType? paramType = null)
        {
            foreach (var param in animator.parameters)
            {
                if (param.name == paramName)
                {
                    if (paramType == null || param.type == paramType)
                        return true;
                }
            }
            return false;
        }
    }
}