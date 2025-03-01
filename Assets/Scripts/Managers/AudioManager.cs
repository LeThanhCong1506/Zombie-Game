using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("---------- Audio Source ----------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;

    [Header("---------- Audio Clip ----------")]
    public AudioClip Background;
    public AudioClip Chop;
    public AudioClip Down;
    public AudioClip EnemyAttack;
    public AudioClip EnemyDeath;
    public AudioClip Footstep;
    public AudioClip Fruit;
    public AudioClip Soda;

    private void Start()
    {
        musicSource.clip = Background;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}
