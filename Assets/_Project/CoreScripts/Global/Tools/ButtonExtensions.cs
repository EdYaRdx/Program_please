using System.Threading.Tasks;
using UnityEngine.UI;

namespace ElementaryCase
{
    public static class ButtonExtensions
    {
        public static Task WaitClickAsync(this Button button)
        {
            var tcs = new TaskCompletionSource<bool>();

            void Handler()
            {
                button.onClick.RemoveListener(Handler);
                tcs.TrySetResult(true);
            }

            button.onClick.AddListener(Handler);
            return tcs.Task;
        }
    }
}