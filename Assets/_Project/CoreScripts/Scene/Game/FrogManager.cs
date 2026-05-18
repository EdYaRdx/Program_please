using PrimeTween;   
using System;
using System.Linq;
using UnityEngine;

namespace ElementaryCase
{
    public class FrogManager : MonoBehaviour
    {
        [SerializeField] private LongString[] _strings;

        [Header("Frog body")]
        [SerializeField] private Ease _ease;
        [SerializeField] private RectTransform _frogBody;
        [SerializeField] private float _frogMiddlePosition;
        [SerializeField] private float _frogEndPosition;
        [SerializeField] private float _appearDelay;

        [SerializeField] private Dialogue _dial;

        private async void Awake()
        {
            await Cysharp.Threading.Tasks.UniTask.WaitForSeconds(_appearDelay);
            await Tween.UIAnchoredPositionX(_frogBody, _frogMiddlePosition, 3f, _ease);
            _dial.Run(_strings.Select(st => st.Val).ToArray(), () =>
            {

            });
        }

        [Serializable]
        private class LongString
        {
            [TextArea(3, 8)] public string Val;
        }
    }
}
