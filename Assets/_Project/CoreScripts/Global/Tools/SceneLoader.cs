using UnityEngine;
using UnityEngine.SceneManagement;

namespace ElementaryCase
{
    public class SceneLoader : MonoBehaviour
    {
        public void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
