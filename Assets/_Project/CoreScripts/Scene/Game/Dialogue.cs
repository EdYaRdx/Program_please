using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElementaryCase
{
    public class Dialogue : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _dialogue;
        [SerializeField] private Button _nextButton;
        [SerializeField] private float _delay;

        private string[] _strings;
        private int _currentIndex;

        private Coroutine _routine;

        private Action _callback;

        public void Run(string[] all, Action callback)
        {
            _panel.SetActive(true);
            _callback = callback;
            _currentIndex = 0;
            _strings = all;
            Next();
        }

        public void Next()
        {
            if (_routine != null)
                StopCoroutine(_routine);

            if (_currentIndex >= _strings.Length)
            {
                _panel.SetActive(false);
                _callback.Invoke();
                return;
            }

            string current = _strings[_currentIndex++];

            _routine = StartCoroutine(ShowNext(current));
        }

        private IEnumerator ShowNext(string line)
        {
            WaitForSeconds wait = new(_delay);
            _dialogue.text = "";

            for (int i = 0; i < line.Length; i++)
            {
                _dialogue.text += line[i];
                yield return wait;
            }

            _routine = null;
        }
    }
}
