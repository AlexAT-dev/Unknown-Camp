using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [SerializeField] private AudioSource audioSource;
    public void Play(AudioClip audio) => audioSource.PlayOneShot(audio);
    public void Stop() => audioSource.Stop();
}
