using UnityEngine;

namespace ElementaryCase
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicController : MonoBehaviour
    {
        public float Volume => _musicSource.volume;

        private AudioSource _musicSource;

        private void Awake()
        {
            _musicSource = GetComponent<AudioSource>();
        }

        public void AddVolume(float volume)
        {
            _musicSource.volume += volume;
        }
    }
}
