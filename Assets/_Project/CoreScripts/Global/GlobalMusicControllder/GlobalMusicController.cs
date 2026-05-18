using UnityEngine;

namespace ElementaryCase
{
    public class GlobalMusicController : IGlobalMusicController
    {
        private readonly MusicController _musicController;

        public GlobalMusicController()
        {
            MusicController musicControllerPrefab = Resources.Load<MusicController>(nameof(MusicController));

            _musicController = Object.Instantiate(musicControllerPrefab);

            Object.DontDestroyOnLoad(_musicController);
        }

        public float CurrentVolume => _musicController.Volume;

        public void AddVolume(float volume)
        {
            _musicController.AddVolume(volume);
        }
    }
}
