using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioClip bounceSFX;//legacy sfx
    //[SerializeField] AudioClip[] collectSFX;
    [Header("Collect‑item clips in ascending pitch (1‑16)")]
    [SerializeField] private AudioClip[] collectClips = new AudioClip[16];
    // ------------------------------------------------------------------
    //  Internal state for the 5‑note phrase
    // ------------------------------------------------------------------
    private int lastClipIndex = -1; // what we played last
    private int phraseStep = 0; // 0…4 within each 5‑note phrase

    [SerializeField] AudioClip hamsterHitSFX;
    [SerializeField] AudioClip rewardSFX;
    [SerializeField] AudioClip accelerateSFX;

    [Header("Hit Sounds")]
    [SerializeField] AudioClip ballImpactSFX;
    [SerializeField] AudioClip hamsterFallSFX;
    [SerializeField] AudioClip splashingSFX;

    [Header("Bonus Stage Sounds")]
    [SerializeField] AudioClip puzzleSolvedSFX;

    [Range(0.0f, 1.0f)]
    [SerializeField] float minCollectVolume = 0.5f;

    [Range(0.0f, 1.0f)]
    [SerializeField] float maxCollectVolume = 0.6f;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource fullVolumeAS;

    [Header("Jump clips")]
    [SerializeField] private AudioClip singleJumpClip;
    [SerializeField] private AudioClip doubleJumpClip;
    [SerializeField] private AudioClip tripleJumpClip;

    public void PlaySingleJump() => PlaySFX(singleJumpClip);
    public void PlayDoubleJump() => PlaySFX(doubleJumpClip);
    public void PlayTripleJump() => PlaySFX(tripleJumpClip);
    public void PlayBounceSFX()
    {
        //PlaySFX(bounceSFX);
    }

    public void PlayCollectSFX()
    {
        if (collectClips == null || collectClips.Length != 16) return;

        int nextIndex;

        if (phraseStep == 0)
        {
            // 1st of 5: random from [0..11], but not the same as lastClipIndex
            do
            {
                nextIndex = Random.Range(0, 12);
            }
            while (nextIndex == lastClipIndex);
            phraseStep = 1;
        }
        else
        {
            // 2nd–5th: strictly greater than lastClipIndex, up to 15
            int min = lastClipIndex + 1;
            if (min > 15) min = 15;  // clamp if we were at the very top

            nextIndex = Random.Range(min, 16);
            phraseStep = (phraseStep + 1) % 5;
        }

        // play and record
        lastClipIndex = nextIndex;
        PlaySFX(collectClips[nextIndex], 0.6f);
    }

    bool playFallNext = true;

    public void PlayDeathSFX()
    {
        if (playFallNext)
        {
            fullVolumeAS.PlayOneShot(hamsterFallSFX, 1.0f);
        }
        else
        {
            PlaySFX(hamsterHitSFX, 0.3f);
        }

        // Toggle for other sound
        playFallNext = !playFallNext;
    }

    public void PlayRewardSFX()
    {
        PlaySFX(rewardSFX, 0.8f);
    }

    public void PlayAccelerateSFX()
    {
        PlaySFX(accelerateSFX, 0.45f);
    }

    public void PlayBallImpactSFX()
    {
        PlaySFX(ballImpactSFX, 1f);
    }

    public void PlayHamsterSplashSFX()
    {
        PlaySFX(splashingSFX, 0.4f);
    }

    public void PlayPuzzleSoundSFX()
    {
        PlaySFX(puzzleSolvedSFX, 0.7f);
    }

    void PlaySFX(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    void PlaySFX(AudioClip clip, float volume)
    {
        audioSource.PlayOneShot(clip, volume);
    }

    void PlaySFX(AudioClip clip, float volume, float pitch)
    {
        GameObject tempAudio = new GameObject("TempAudio");
        AudioSource tempSource = tempAudio.AddComponent<AudioSource>();

        tempSource.clip = clip;
        tempSource.volume = volume;
        tempSource.pitch = pitch;
        tempSource.PlayOneShot(clip);

        Destroy(tempAudio, clip.length);
    }
}
