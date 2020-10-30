using System.Collections.Generic;
using UnityEngine;

public class Scythe : MonoBehaviour
{
    private int _damage;
    private GameObject _hitPrefab;

    private List<AudioSource> _audioSources;
    private AudioClip _hitSound;    

    private PlayerController _playerController;

    private void Start()
    {
        _hitPrefab = Resources.Load<GameObject>("Prefabs/HitPrefab");
        _hitSound = Resources.Load<AudioClip>("SFX/hit");
        _audioSources = new List<AudioSource>();

        _playerController = FindObjectOfType<PlayerController>();

        for(int i = 0; i < 5; i++)
        {
            GameObject audioSourceGameObject = new GameObject("AudioSource_Scythe");
            AudioSource newAudioSource = audioSourceGameObject.AddComponent<AudioSource>();
            newAudioSource.loop = false;
            newAudioSource.playOnAwake = false;
            newAudioSource.volume = 0.8f;
            _audioSources.Add(newAudioSource);
        }
    }

    public void SetDamage(int newDamage)
    {
        _damage = newDamage;
    }

    private void PlayHitSound()
    {
        foreach(AudioSource audioSource in _audioSources)
        {
            if(!audioSource.isPlaying)
            {
                if(audioSource.clip == null || audioSource.clip != _hitSound)
                {
                    audioSource.clip = _hitSound;
                }

                audioSource.Play();

                break;
            }
        }
    }

    //Check a rectangular area around the scythe for any object with the tag "destructible"
    public void Check()
    {
        bool playHitSound = false;

        float hitboxOffset = 0.2f;

        if(_playerController.GetScytheDirection() == Direction.Left)
        {
            hitboxOffset = -hitboxOffset;
        }

        //Check for multiple hits. With this the player can hit multiple targets in one swing.
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position + new Vector3(hitboxOffset, 0, 0), new Vector2(0.75f, 1.2f), 0f, Vector2.zero);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform.CompareTag("Skeleton") && hit.collider.isTrigger)
            {
                Skeleton skeletonScript = hit.transform.GetComponent<Skeleton>();
                skeletonScript.TakeDamage(_damage);

                Instantiate(_hitPrefab, new Vector3(hit.transform.position.x, hit.transform.position.y, transform.position.z - 1), Quaternion.identity);
                playHitSound = true;
            }
        }

        if(playHitSound)
        {
            PlayHitSound();
        }
    }

}
