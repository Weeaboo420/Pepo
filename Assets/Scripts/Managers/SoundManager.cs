using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private Settings _settingsReference;

    private AudioSource _bucketSoundSource;
    private AudioClip[] _bucketRefillSounds;

    private AudioSource[] _stepSoundSources;
    private AudioClip[] _stepSounds;

    private AudioSource _hitAudioSource;

    private AudioSource[] _pumpkinSoundSources;
    private AudioClip[] _jackLanternSpawnSounds;

    private AudioSource _explosionAudioSource;

    private void Awake()
    {
        _settingsReference = FindObjectOfType<Settings>();

        GameObject explosionSoundGameObject = new GameObject("audio_explosion");
        _explosionAudioSource = explosionSoundGameObject.AddComponent<AudioSource>();
        _explosionAudioSource.volume = 0.5f;
        _explosionAudioSource.loop = false;
        _explosionAudioSource.playOnAwake = false;
        _explosionAudioSource.clip = Resources.Load<AudioClip>("SFX/bomb");

        //Sounds for hits on skeletons
        GameObject hitSoundGameObject = new GameObject("audio_hit_skeleton");
        _hitAudioSource = hitSoundGameObject.AddComponent<AudioSource>();
        _hitAudioSource.clip = Resources.Load<AudioClip>("SFX/hit");
        _hitAudioSource.volume = 0.75f;
        _hitAudioSource.playOnAwake = false;
        _hitAudioSource.loop = false;

        //Footstep sounds
        _stepSoundSources = new AudioSource[4];
        for (int i = 0; i < 4; i++)
        {
            GameObject stepSoundSourceGameObject = new GameObject("audio_step");
            _stepSoundSources[i] = stepSoundSourceGameObject.AddComponent<AudioSource>();
            _stepSoundSources[i].volume = 0.8f;
            _stepSoundSources[i].playOnAwake = false;
            _stepSoundSources[i].loop = false;
        }

        _stepSounds = new AudioClip[4]
        {
            Resources.Load<AudioClip>("SFX/Steps/step1"),
            Resources.Load<AudioClip>("SFX/Steps/step2"),
            Resources.Load<AudioClip>("SFX/Steps/step3"),
            Resources.Load<AudioClip>("SFX/Steps/step4"),
        };

        //Sounds for the bucket
        GameObject bucketSoundGameObject = new GameObject("audio_bucketRefill");
        _bucketSoundSource = bucketSoundGameObject.AddComponent<AudioSource>();
        _bucketSoundSource.loop = false;
        _bucketSoundSource.playOnAwake = false;

        _bucketRefillSounds = new AudioClip[3]
        {
            Resources.Load<AudioClip>("SFX/Splashes/splash01"),
            Resources.Load<AudioClip>("SFX/Splashes/splash02"),
            Resources.Load<AudioClip>("SFX/Splashes/splash03")
        };

        //Sounds for the pumpkins
        _pumpkinSoundSources = new AudioSource[4];

        for (int i = 0; i < 4; i++)
        {
            GameObject audioPumpkin = new GameObject("audio_pumpkin");
            AudioSource pumpkinSoundSource = audioPumpkin.AddComponent<AudioSource>();
            pumpkinSoundSource.playOnAwake = false;
            pumpkinSoundSource.loop = false;


            _jackLanternSpawnSounds = new AudioClip[4]
            {
            Resources.Load<AudioClip>("SFX/Spawn/spawn1"),
            Resources.Load<AudioClip>("SFX/Spawn/spawn2"),
            Resources.Load<AudioClip>("SFX/Spawn/spawn3"),
            Resources.Load<AudioClip>("SFX/thud")
            };

            _pumpkinSoundSources[i] = pumpkinSoundSource;

        }
    }

    public void PlayBucketSound()
    {
        _bucketSoundSource.clip = _bucketRefillSounds[Random.Range(0, _bucketRefillSounds.Length)];
        _bucketSoundSource.volume = _settingsReference.GetSfxVolume("Splash");
        _bucketSoundSource.Play();
    }

    public void PlayExplosionSound()
    {
        _explosionAudioSource.volume = _settingsReference.GetSfxVolume("Explosion");
        _explosionAudioSource.Play();
    }

    public void PlayHitSound()
    {
        _hitAudioSource.volume = _settingsReference.GetSfxVolume("Hit");
        _hitAudioSource.Play();
    }

    public void PlayFootstepSound(bool forSkeleton = false)
    {
        foreach (AudioSource stepSoundSource in _stepSoundSources)
        {
            if (!stepSoundSource.isPlaying)
            {
                stepSoundSource.clip = _stepSounds[Random.Range(0, _stepSounds.Length)];

                if (forSkeleton)
                {
                    stepSoundSource.volume = _settingsReference.GetSfxVolume("Footstep_Skeleton");
                }
                else
                {
                    stepSoundSource.volume = _settingsReference.GetSfxVolume("Footstep_Player");
                }

                stepSoundSource.Play();
                break;
            }
        }
    }

    public void PlayPumpkinSound()
    {
        foreach (AudioSource audioSource in _pumpkinSoundSources)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = _jackLanternSpawnSounds[Random.Range(0, _jackLanternSpawnSounds.Length - 1)];
                audioSource.volume = _settingsReference.GetSfxVolume("Pumpkin");
                audioSource.Play();
                break;
            }
        }
    }

    public void PlayThudSound()
    {
        foreach (AudioSource audioSource in _pumpkinSoundSources)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = _jackLanternSpawnSounds[_jackLanternSpawnSounds.Length - 1];
                audioSource.volume = _settingsReference.GetSfxVolume("Pumpkin");
                audioSource.Play();
                break;
            }
        }
    }
}
