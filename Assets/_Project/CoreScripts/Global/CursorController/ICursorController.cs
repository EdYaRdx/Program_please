using UnityEngine;

namespace ElementaryCase
{
    public interface ICursorController
    {
        void Show();
        void Hide();

        void SetState(Cursor.State state);
        void SetFollowObject(Sprite sprite, Color color);
    }
}