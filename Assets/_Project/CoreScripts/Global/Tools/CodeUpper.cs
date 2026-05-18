using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ElementaryCase
{
    public class CodeUpper : MonoBehaviour
    {
        [TextArea]
        [SerializeField] private string _text;
        [SerializeField] private float _delay;
        [SerializeField] private TMP_Text _textObject;

        private Coroutine _coroutine;
        private Queue<string> textStrings;

        private void Awake()
        {
            textStrings = new Queue<string>(_text.Split("\n"));
        }

        private IEnumerator TextChangeRoutine()
        {
            WaitForSeconds wait = new(_delay);

            while (true)
            {
                string sw = textStrings.Dequeue();
                textStrings.Enqueue(sw);
                _textObject.text = string.Join("\n", textStrings);
                yield return wait;
            }
        }

        private void OnEnable()
        {
            _coroutine = StartCoroutine(TextChangeRoutine());
        }

        private void OnDisable()
        {
            StopCoroutine(_coroutine);
        }
    }
}
