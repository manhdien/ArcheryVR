using UnityEngine;

/// <summary>
/// Quản lý âm thanh tập trung cho toàn bộ game: BGM và SFX (Bow, Arrow, Target, NPC, UI).
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<AudioManager>();
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips - Bow")]
    public AudioClip bowDrawClip;
    public AudioClip bowReleaseClip;

    [Header("Audio Clips - Arrow Hits")]
    public AudioClip hitTargetClip;
    public AudioClip hitNpcClip;
    public AudioClip hitGroundClip;

    [Header("Audio Clips - UI")]
    public AudioClip buttonClickClip;

    [Header("Audio Clips - Environment")]
    public AudioClip bgmClip;

    [Header("Volume Settings")]
    [Range(0f, 1f)] [SerializeField] private float bgmVolume = 0.35f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.85f;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        SetupAudioSources();
    }

    private void Start()
    {
        PlayBGM();
    }

    private void SetupAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }

    public void PlayBGM()
    {
        if (bgmSource != null && bgmClip != null)
        {
            if (!bgmSource.isPlaying)
            {
                bgmSource.clip = bgmClip;
                bgmSource.volume = bgmVolume;
                bgmSource.Play();
            }
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void PlayBowDraw()
    {
        if (sfxSource != null && bowDrawClip != null)
        {
            sfxSource.PlayOneShot(bowDrawClip, sfxVolume * 0.7f);
        }
    }

    public void PlayBowRelease()
    {
        if (sfxSource != null && bowReleaseClip != null)
        {
            sfxSource.PlayOneShot(bowReleaseClip, sfxVolume);
        }
    }

    public void PlayHitTarget(Vector3 position)
    {
        if (hitTargetClip != null)
        {
            AudioSource.PlayClipAtPoint(hitTargetClip, position, sfxVolume);
        }
    }

    public void PlayHitNPC(Vector3 position)
    {
        if (hitNpcClip != null)
        {
            AudioSource.PlayClipAtPoint(hitNpcClip, position, sfxVolume);
        }
    }

    public void PlayHitGround(Vector3 position)
    {
        if (hitGroundClip != null)
        {
            AudioSource.PlayClipAtPoint(hitGroundClip, position, sfxVolume * 0.6f);
        }
    }

    public void PlayButtonClick()
    {
        if (sfxSource != null && buttonClickClip != null)
        {
            sfxSource.PlayOneShot(buttonClickClip, sfxVolume * 0.9f);
        }
    }
}
