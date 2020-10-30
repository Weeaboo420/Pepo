using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    private bool _paused = true;
    private bool _hasStarted = false;
    private GameObject _gameOverScreen;
    private GameObject _titleScreen;
    private GameObject _pauseScreen;

    private int _lives;
    private Text _livesText;
    private List<Pumpkin> _pumpkins;

    private bool _waterBucketFilled = false;
    private bool _canFillBucket = false;
    private bool _canWater = false;

    private Sprite _filledBucketSprite, _emptyBucketSprite;
    private Color32 _emptyBucketSlotColor, _filledBucketSlotColor;    
    private Image _bucketSlotImage, _bucketImage;
    public GameObject _fillBucketPrompt;

    private GameObject _waterPumpkinPrompt;
    private GameObject _selection;
    private Pumpkin _selectedPumpkin, _pumpkinCandidate;

    private GameObject _splashPrefab;
    private Transform _wellTransform;

    private AudioSource _bucketSoundSource;
    private AudioClip[] _bucketRefillSounds;

    private AudioSource _hitAudioSource;

    private AudioSource _pumpkinSoundSource;
    private AudioClip[] _jackLanternSpawnSounds;

    private List<Transform> _paths = new List<Transform>();

    private GameObject _dustPrefab;
    private GameObject _hitPrefab;

    private int _wave = 0;
    private bool _waveActive = false;
    private Text _waveText;
    private Text _skeletonText;
    private WaveParams _currentDifficulty;
    private List<Skeleton> _skeletonsInWave = new List<Skeleton>();
    private GameObject _skeletonPrefab;

    private Scythe _scythe;

    private const int _countdownLength = 10;
    private float _nextWaveCountdown = _countdownLength;
    private GameObject _countdownGameobject;
    private Text _countdownText;
    private bool _countdownActive = false;

    private Dictionary<int, WaveParams> _waveDifficulties = new Dictionary<int, WaveParams>() 
    {
        {1, new WaveParams(1, 2, 25) },
        {3, new WaveParams(2, 3, 25) },
        {5, new WaveParams(2, 4, 25) },
        {10, new WaveParams(3, 5, 34) },
        {20, new WaveParams(4, 6, 40) },
        {25, new WaveParams(5, 7, 45) }
    };
    
    private void Awake()
    {
        foreach(Transform child in transform)
        {
            _paths.Add(child);
        }

        _gameOverScreen = GameObject.Find("GameOverScreen");
        _gameOverScreen.SetActive(false);

        _pauseScreen = GameObject.Find("PauseScreen");
        _pauseScreen.SetActive(false);

        _titleScreen = GameObject.Find("TitleScreen");
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.Confined;

        _scythe = FindObjectOfType<Scythe>();

        _waveText = GameObject.Find("Slot_Wave").transform.Find("Text").GetComponent<Text>();
        _skeletonText = GameObject.Find("Slot_Wave").transform.Find("Count").GetComponent<Text>();

        _countdownGameobject = GameObject.Find("Slot_Countdown");
        _countdownText = _countdownGameobject.transform.Find("Text").GetComponent<Text>();

        _livesText = GameObject.Find("Slot_Hearts").transform.Find("Text").GetComponent<Text>();
        _pumpkins = new List<Pumpkin>();

        _skeletonPrefab = Resources.Load<GameObject>("Prefabs/SkeletonPrefab");

        _filledBucketSprite = Resources.Load<Sprite>("Sprites/Misc/bucket_filled");
        _emptyBucketSprite = Resources.Load<Sprite>("Sprites/Misc/bucket_empty");
        _bucketSlotImage = GameObject.Find("Slot_Bucket").GetComponent<Image>();
        _bucketImage = GameObject.Find("Slot_Bucket").transform.Find("Bucket").GetComponent<Image>();

        _splashPrefab = Resources.Load<GameObject>("Prefabs/SplashPrefab");
        _wellTransform = GameObject.Find("WELL").transform;

        _dustPrefab = Resources.Load<GameObject>("Prefabs/DustPrefab");
        _hitPrefab = Resources.Load<GameObject>("Prefabs/HitPrefab");

        _emptyBucketSlotColor = new Color32(123, 34, 31, 255);
        _filledBucketSlotColor = new Color32(31, 74, 123, 255);
        _bucketSlotImage.color = _emptyBucketSlotColor;

        _fillBucketPrompt = GameObject.Find("Prompt_Fill");
        _fillBucketPrompt.SetActive(false);

        _waterPumpkinPrompt = GameObject.Find("Prompt_Water");
        _waterPumpkinPrompt.SetActive(false);

        _selection = GameObject.Find("selection");
        SpriteRenderer selectionSpriteRenderer = _selection.GetComponent<SpriteRenderer>();
        selectionSpriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Misc/dot");

        //Disable the red selection circle used for showing which pumpkin is selected for watering
        _selection.SetActive(false);

        //Sounds for hits on skeletons
        GameObject hitSoundGameObject = new GameObject("audio_hit_skeleton");
        _hitAudioSource = hitSoundGameObject.AddComponent<AudioSource>();
        _hitAudioSource.clip = Resources.Load<AudioClip>("SFX/hit");
        _hitAudioSource.volume = 0.8f;
        _hitAudioSource.playOnAwake = false;
        _hitAudioSource.loop = false;

        //Sounds for the bucket
        GameObject bucketSoundGameObject = new GameObject("audio_bucketRefill");
        _bucketSoundSource = bucketSoundGameObject.AddComponent<AudioSource>();
        _bucketSoundSource.loop = false;
        _bucketSoundSource.playOnAwake = false;

        _bucketRefillSounds = new AudioClip[3] 
        { 
            Resources.Load<AudioClip>("SFX/splash01"),
            Resources.Load<AudioClip>("SFX/splash02"),
            Resources.Load<AudioClip>("SFX/splash03")
        };


        //Sounds for the pumpkins
        GameObject audioPumpkin = new GameObject("audio_pumpkin");
        _pumpkinSoundSource = audioPumpkin.AddComponent<AudioSource>();
        _pumpkinSoundSource.playOnAwake = false;
        _pumpkinSoundSource.loop = false;

        _jackLanternSpawnSounds = new AudioClip[4]
        {
            Resources.Load<AudioClip>("SFX/spawn1"),
            Resources.Load<AudioClip>("SFX/spawn2"),
            Resources.Load<AudioClip>("SFX/spawn3"),
            Resources.Load<AudioClip>("SFX/thud")
        };

        foreach(Pumpkin pumpkin in FindObjectsOfType<Pumpkin>())
        {
            _lives++;
        }

        _countdownGameobject.SetActive(false);
        UpdateLives();
        StartCoroutine(NewWave());
    }

    public bool GetPaused()
    {
        return _paused;
    }

    public void TogglePause()
    {
        if (_lives > 0)
        {
            if (_titleScreen.activeSelf)
            {
                _titleScreen.SetActive(false);
                _hasStarted = true;
            }

            _paused = !_paused;
            _pauseScreen.SetActive(_paused);

            if (_paused)
            {
                Time.timeScale = 0f;
                Cursor.visible = true;
            }
            else
            {
                Time.timeScale = 1f;
                Cursor.visible = false;
            }
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public IEnumerator NewWave()
    {
        _wave++;
        _countdownGameobject.SetActive(false);

        //Set the appropriate difficulty depending on the wave;
        if (_waveDifficulties.ContainsKey(_wave))
        {
            _currentDifficulty = _waveDifficulties[_wave];
        }

        _scythe.SetDamage(_currentDifficulty.GetScytheDamage());

        int skeletonsToSpawn = Random.Range(_currentDifficulty.GetMin(), _currentDifficulty.GetMax() + 1);
        for(int i = 0; i < skeletonsToSpawn; i++)
        {
            Instantiate(_skeletonPrefab, new Vector3(-100, -100, 0), Quaternion.identity);
        }

        yield return new WaitForSeconds(1f);
        foreach(Skeleton skeletonScript in FindObjectsOfType<Skeleton>())
        {
            _skeletonsInWave.Add(skeletonScript);
        }

        _waveActive = true;
        _waveText.text = "Wave: " + _wave.ToString();
        _skeletonText.text = "x" + _skeletonsInWave.Count.ToString();

        
        StartCoroutine(CheckWave());
    }

    public void UpdateLives()
    {
        _livesText.text = "x" + _lives.ToString();
    }

    public void DecreaseLives()
    {
        _lives--;
        UpdateLives();

        if(_lives == 0)
        {
            Time.timeScale = 0f;
            _gameOverScreen.SetActive(true);
            Cursor.visible = true;
        }
    }

    public void IncreaseLives()
    {
        _lives++;
        UpdateLives();
    }

    public int GetRandomPathIndex()
    {
        return Random.Range(0, _paths.Count);
    }

    public Transform GetPath(int index)
    {        
        return _paths[index];
    }

    public void CreateDustPrefab(Vector3 position)
    {
        Instantiate(_dustPrefab, position, Quaternion.identity);
    }

    public void CreateHitPrefab(Vector3 position)
    {
        Instantiate(_hitPrefab, position, Quaternion.identity);
        _hitAudioSource.Play();
    }

    public void PlayLanternSound()
    {
        _pumpkinSoundSource.clip = _jackLanternSpawnSounds[Random.Range(0, _jackLanternSpawnSounds.Length-1)];
        _pumpkinSoundSource.Play();
    }

    public void PlayThudSound()
    {
        _pumpkinSoundSource.clip = _jackLanternSpawnSounds[_jackLanternSpawnSounds.Length - 1];
        _pumpkinSoundSource.Play();
    }

    public bool GetCanFillBucket()
    {
        return _canFillBucket;
    }    

    public void UpdateInRangeOfPumpkin(bool inRange, Pumpkin pumpkin)
    {
        if(pumpkin != null)
        {
            _pumpkinCandidate = pumpkin;
        }

        if (_waterBucketFilled)
        {
            _waterPumpkinPrompt.SetActive(inRange);
            _canWater = inRange;

            if (_canWater)
            {
                if (_selectedPumpkin != null && _selectedPumpkin != pumpkin) //If there is a new selected pumpkin, disable the water bar for the old one.
                {
                    _selectedPumpkin.SetBarVisibility(false);
                }

                _selectedPumpkin = pumpkin;
                _selectedPumpkin.SetBarVisibility(true);

            }

            else
            {
                if (_selectedPumpkin != null && _selectedPumpkin != pumpkin) //If there is a new selected pumpkin, disable the water bar for the old one.
                {
                    _selectedPumpkin.SetBarVisibility(false);
                }

                _selectedPumpkin = null;
            }
        }
        else
        {
            if (!inRange)
            {
                if (_selectedPumpkin != pumpkin)
                {
                    _selectedPumpkin.SetBarVisibility(false);
                }                
            }
        }
    }

    public void UpdateInRangeOfWell(bool inRange)
    {
        if(inRange && !_fillBucketPrompt.activeSelf && !_waterBucketFilled)
        {
            _fillBucketPrompt.SetActive(true);
            _canFillBucket = true;
        }

        else if(!inRange && _fillBucketPrompt.activeSelf)
        {
            _fillBucketPrompt.SetActive(false);
            _canFillBucket = false;
        }
    }    

    public void UpdateWaterBucket(bool filled)
    {
        if (filled)
        {
            _bucketImage.sprite = _filledBucketSprite;
            _bucketSlotImage.color = _filledBucketSlotColor;
            _fillBucketPrompt.SetActive(false);            
        }
        else
        {
            _bucketImage.sprite = _emptyBucketSprite;
            _bucketSlotImage.color = _emptyBucketSlotColor;
        }

        _bucketSoundSource.clip = _bucketRefillSounds[Random.Range(0, _bucketRefillSounds.Length)];
        _bucketSoundSource.Play();

        _waterBucketFilled = filled;        
    }    

    private IEnumerator StartWaveDelay()
    {
        _waveActive = false;

        _nextWaveCountdown = _countdownLength;
        _countdownGameobject.SetActive(true);
        _countdownActive = true;

        yield return new WaitForSeconds(_countdownLength);

        _countdownActive = false;
        StartCoroutine(NewWave());
    }

    public void UpdateSkeletonCount()
    {
        int activeSkeletons = 0;

        foreach (Skeleton skeletonScript in _skeletonsInWave)
        {
            if (skeletonScript != null && skeletonScript.GetCurrentHealth() > 0)
            {
                activeSkeletons++;
            }
        }

        _skeletonText.text = "x" + activeSkeletons.ToString();
    }

    private IEnumerator CheckWave()
    {
        yield return new WaitForSeconds(3);

        if (_waveActive)
        {
            //Wave management        
            int activeSkeletons = 0;

            foreach (Skeleton skeletonScript in _skeletonsInWave)
            {
                if (skeletonScript != null && skeletonScript.GetCurrentHealth() > 0)
                {
                    activeSkeletons++;
                }
            }

            if (activeSkeletons == 0)
            {
                _skeletonsInWave.Clear();
                StartCoroutine(StartWaveDelay());
            }

            StartCoroutine(CheckWave());
        }
        
    }

    private void Update()
    {        
        if(_countdownActive)
        {
            _nextWaveCountdown -= Time.deltaTime;
            _countdownText.text = Mathf.RoundToInt(_nextWaveCountdown).ToString();
        }

        if(Input.GetKeyDown(KeyCode.Escape) && _hasStarted)
        {
            TogglePause();
        }

        //Pumpkin watering
        if(Input.GetKeyDown(KeyCode.E) && _canWater && _selectedPumpkin != null && _waterBucketFilled && !_canFillBucket)
        {

            //Prevent the player from wasting a bucket on a pumpkin that is a jack o'lantern and that has a full water bar
            if (_selectedPumpkin.GetStage() <= 1 || _selectedPumpkin.GetStage() == 2 && _selectedPumpkin.GetWaterValue() < _selectedPumpkin.GetMaxWaterValue())
            {
                _selectedPumpkin.SetWaterValue(_selectedPumpkin.GetWaterValue() + 1);
                _canWater = false;
                _waterPumpkinPrompt.SetActive(false);
                _canFillBucket = false;

                UpdateWaterBucket(false);
                Instantiate(_splashPrefab, new Vector3(_selectedPumpkin.transform.position.x, _selectedPumpkin.transform.position.y - 0.1f, -2f), Quaternion.identity);
            }
        }

        //Water bucket refill
        else if (Input.GetKeyDown(KeyCode.E) && _canFillBucket && !_waterBucketFilled)
        {
            UpdateWaterBucket(true);
            _canFillBucket = false;
            Instantiate(_splashPrefab, new Vector3(_wellTransform.position.x, _wellTransform.position.y - 1f, -2f), Quaternion.identity);
        }

        if(_selectedPumpkin != null && _canWater && _waterBucketFilled)
        {
            if(!_selection.activeSelf)
            {
                _selection.SetActive(true);
                _selectedPumpkin.SetBarVisibility(true);
            }

            _selection.transform.position = new Vector3(_selectedPumpkin.GetPosition().x, _selectedPumpkin.GetPosition().y, 0.3f);

        } else
        {
            if(_selection.activeSelf)
            {
                _selection.SetActive(false);
            }

            if(_selectedPumpkin != null && _selectedPumpkin != _pumpkinCandidate && !_waterBucketFilled && !_canWater)
            {
                _selectedPumpkin.SetBarVisibility(false);
            }
        }
    }
}
