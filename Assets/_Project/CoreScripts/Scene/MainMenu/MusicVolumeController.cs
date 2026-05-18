using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ElementaryCase
{
    public class MusicVolumeController : MonoBehaviour
    {
        [SerializeField] private Image _fill;

        private IGlobalMusicController _musicController;

        [Inject]
        public void Construct(IGlobalMusicController musicController)
        {
            _musicController = musicController;

            _fill.fillAmount = _musicController.CurrentVolume;
        }

        public void Increase()
        {
            _musicController.AddVolume(0.2f);
            _fill.fillAmount = _musicController.CurrentVolume;
        }

        public void Decrease()
        {
            _musicController.AddVolume(-0.2f);
            _fill.fillAmount = _musicController.CurrentVolume;
        }
    }
}
