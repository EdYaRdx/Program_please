using UnityEngine;
using Zenject;

namespace ElementaryCase
{
    public class DefaultCursorController : ICursorController, ILateTickable
    {
        private Cursor _cursor;

        public DefaultCursorController(InputSystem_Actions inputSystem)
        {
            Cursor cursorPrefab = Resources.Load<Cursor>(nameof(Cursor));

            _cursor = Object.Instantiate(cursorPrefab);

            _cursor.Initialize(inputSystem);

            Object.DontDestroyOnLoad(_cursor);
#if !UNITY_EDITOR
            UnityEngine.Cursor.visible = false;
#endif
        }

        public void SetState(Cursor.State state)
        {
            _cursor.SetState(state);
        }

        public void Hide()
        {
            _cursor.gameObject.SetActive(false);
        }

        public void Show()
        {
            _cursor.gameObject.SetActive(true);
        }

        public void SetFollowObject(Sprite sprite, Color color)
        {
            _cursor.SetFollowObject(sprite, color);
        }

        public void LateTick()
        {
            _cursor.LateTick();
        }
    }
}