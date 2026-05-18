namespace ElementaryCase
{
    public interface IGlobalMusicController
    {
        float CurrentVolume { get; }
        void AddVolume(float volume);
    }
}
