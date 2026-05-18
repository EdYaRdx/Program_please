using System.Collections.Generic;
using UnityEngine;

namespace UnityFunctools
{
    public static class Collider2DOverlaper
    {
        public static List<T> GetAllComponentsInCollider<T>(Collider2D collider, LayerMask layerMask, bool useTriggers = false) where T : MonoBehaviour
        {
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(layerMask);
            contactFilter.useTriggers = useTriggers;

            List<Collider2D> results = new();

            int colliderCount = collider.Overlap(contactFilter, results);

            List<T> allNeedComponents = new();

            for (int i = 0; i < colliderCount; i++)
                if (results[i].TryGetComponent(out T characterBody))
                    allNeedComponents.Add(characterBody);

            return allNeedComponents;
        }
    }
}