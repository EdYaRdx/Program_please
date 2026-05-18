using System;
using System.Collections.Generic;
using UnityEngine;
using UnityFunctools;

namespace ElementaryCase
{
    public class Cursor : MonoBehaviour, ISerializationCallbackReceiver
    {
        public enum State
        {
            Default,
            Select
        }

        [Serializable]
        private class CursorStateInfo
        {
            public GameObject body;
            public State state;
        }

        [SerializeField] private CursorStateInfo[] _cursorStateInfos;
         
        [SerializeField] private SpriteRenderer _followObject;

        private Dictionary<State, GameObject> _stateBodyPairs;

        private InputSystem_Actions _inputSystem;

        public void SetState(State state)
        {
            _stateBodyPairs.ForEach(p => p.Value.SetActive(false));

            _stateBodyPairs[state].SetActive(true);
        }

        public void SetFollowObject(Sprite sprite, Color color)
        {
            _followObject.sprite = sprite;
            _followObject.color = color;
        }

        public void Initialize(InputSystem_Actions inputSystem)
        {
            SetState(State.Default);
            _inputSystem = inputSystem;
        }

        public void LateTick()
        {
            Vector2 mousePosition = _inputSystem.UI.MousePosition.ReadValue<Vector2>();

#if UNITY_EDITOR
            if (mousePosition.x < 0 || mousePosition.x > Screen.width ||
                mousePosition.y < 0 || mousePosition.y > Screen.height)
            {
                return;
            }
#endif
            if (Camera.main == null)
                return;

            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            _stateBodyPairs = new();

            foreach (CursorStateInfo info in _cursorStateInfos)
                _stateBodyPairs.Add(info.state, info.body);
        }
    }
}
