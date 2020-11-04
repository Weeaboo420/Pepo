using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const bool _devMode = false;

    private bool _paused = true;
    private bool _acceptsInput = false;
    private bool _hasStarted = false;
    private GameObject _gameOverScreen;
    private GameObject _titleScreen;
    private GameObject _pauseScreen;
    private GameObject _settingsScreen;

    private PlayerController _playerControllerReference;

    private Settings _settingsReference;

    private int _lives;
    private Text _livesText;
    private List<Pumpkin> _pumpkins;

    private int _bombPoints = 0; //Can be used to create a bomb that will instantly kill a skeleton that touches it
    private const int _pointsPerBomb = 7; //How many points are needed to create one bomb
    private const int _maxBombs = 3; //How many bombs that can be carried at the same time
    private Text _bombText;
    private Image _bombControls;
    private Color32 _defaultControlColor;
    private Color32 _disabledControlColor = new Color32(19, 53, 92, 255);
    private GameObject _bombPrefab;

    private bool _waterBucketFilled = false;
    private bool _canFillBucket = false;
    private bool _canWater = false;

    private GameObject _messagePrefab;
    private GameObject _guiRoot;
    private GuiManager _guiManagerReference;

    //The default buttons to refocus on when swapping to using a controller
    private GameObject _titleScreenDefault;
    private GameObject _settingsScreenDefault;
    private GameObject _pauseScreenDefault;
    private GameObject _gameOverScreenDefault;

    private Image _dashSlot;

    private Sprite _filledBucketSprite, _emptyBucketSprite;
    private Color32 _emptyBucketSlotColor, _filledBucketSlotColor;    
    private Image _bucketSlotImage, _bucketImage;
    private GameObject _fillBucketPrompt;
    private Image _bucketControls;

    private GameObject _waterPumpkinPrompt;
    private GameObject _selection;
    private Pumpkin _selectedPumpkin, _pumpkinCandidate;

    private GameObject _splashPrefab;
    private Transform _wellTransform;

    private AudioSource _bucketSoundSource;
    private AudioClip[] _bucketRefillSounds;

    private AudioSource[] _stepSoundSources;
    private AudioClip[] _stepSounds;

    private AudioSource _hitAudioSource;

    private AudioSource[] _pumpkinSoundSources;
    private AudioClip[] _jackLanternSpawnSounds;

    private AudioSource _explosionAudioSource;

    private List<Path> _paths = new List<Path>();    

    private GameObject _dustPrefab;
    private GameObject _hitPrefab;

    private int _wave = 0;
    private bool _waveActive = false;
    private Text _waveText;
    private Text _skeletonText;
    private WaveParams _currentDifficulty;
    private List<Skeleton> _skeletonsInWave = new List<Skeleton>();
    private GameObject _skeletonPrefab;
    private GameObject _superSkeletonPrefab;

    private GameObject _puddlePrefab;         

    private Scythe _scythe;

    private const int _countdownLength = 15;
    private float _nextWaveCountdown = _countdownLength;
    private GameObject _countdownGameObject;
    private Text _countdownText;
    private bool _countdownActive = false;

    private Dictionary<int, WaveParams> _waveDifficulties = new Dictionary<int, WaveParams>() 
    {
        {1, new WaveParams(4, 5, 20, 4.2f, "Defend the pumpkins") },
        {3, new WaveParams(5, 6, 20, 4.2f) },
        {5, new WaveParams(5, 7, 20, 4.4f, "Speed upgrade", 1) },
        {6, new WaveParams(6, 7, 30, 4.4f, "Damage upgrade") },
        {10, new WaveParams(7, 9, 30, 4.6f, "Speed upgrade", 2) },
        {11, new WaveParams(7, 9, 30, 4.6f) },
        {15, new WaveParams(7, 9, 35, 4.6f, "Damage upgrade", 2) },
        {16, new WaveParams(8, 10, 35, 4.8f, "Speed upgrade") },
        {20, new WaveParams(8, 10, 40, 4.8f, "Damage upgrade", 1) },
        {21, new WaveParams(8, 11, 40, 4.8f) },
        {25, new WaveParams(7, 9, 45, 4.9f, "Speed & damage upgrade", 3) },
        {30, new WaveParams(6, 8, 50, 5.0f, "Speed & damage upgrade", 4) }
    };
    
    private void Awake()
    {
        //Add paths
        foreach(Transform child in transform)
        {
            _paths.Add(child.GetComponent<Path>());
        }

        List<GameObject> controllerButtons = new List<GameObject>();
        foreach(GameObject button in GameObject.FindGameObjectsWithTag("Button"))
        {
            controllerButtons.Add(button);
        }

        List<GameObject> pcButtons = new List<GameObject>();
        foreach(GameObject button in GameObject.FindGameObjectsWithTag("Key"))
        {
            pcButtons.Add(button);
        }

        _settingsReference = FindObjectOfType<Settings>();
        _guiManagerReference = FindObjectOfType<GuiManager>();

        _guiManagerReference.SetControllerButtons(controllerButtons.ToArray());
        _guiManagerReference.SetPcButtons(pcButtons.ToArray());

        _gameOverScreen = GameObject.Find("GameOverScreen");
        _gameOverScreen.SetActive(false);

        _settingsScreen = GameObject.Find("SettingsScreen");
        _settingsScreen.SetActive(false);

        _pauseScreen = GameObject.Find("PauseScreen");
        _pauseScreen.SetActive(false);

        _titleScreen = GameObject.Find("TitleScreen");
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.Confined;

        _bombText = GameObject.Find("Slot_Bomb").transform.Find("Text").GetComponent<Text>();
        _bombText.text = _bombPoints.ToString() + "/" + _pointsPerBomb.ToString();
        _bombPrefab = Resources.Load<GameObject>("Prefabs/BombPrefab");
        _bombControls = GameObject.Find("Controls_Bomb").GetComponent<Image>();
        _defaultControlColor = _bombControls.color;
        _bombControls.color = _disabledControlColor;

        _messagePrefab = Resources.Load<GameObject>("Prefabs/MessagePrefab");
        _guiRoot = GameObject.Find("UI");

        _dashSlot = GameObject.Find("Slot_Dash").GetComponent<Image>();

        _scythe = FindObjectOfType<Scythe>();
        _playerControllerReference = FindObjectOfType<PlayerController>();

        _waveText = GameObject.Find("Slot_Wave").transform.Find("Text").GetComponent<Text>();
        _skeletonText = GameObject.Find("Slot_Wave").transform.Find("Count").GetComponent<Text>();

        _countdownGameObject = GameObject.Find("Slot_Countdown");
        _countdownText = _countdownGameObject.transform.Find("Text").GetComponent<Text>();

        _livesText = GameObject.Find("Slot_Hearts").transform.Find("Text").GetComponent<Text>();
        _pumpkins = new List<Pumpkin>();

        _skeletonPrefab = Resources.Load<GameObject>("Prefabs/SkeletonPrefab");
        _superSkeletonPrefab = Resources.Load<GameObject>("Prefabs/SuperSkeletonPrefab");

        _puddlePrefab = Resources.Load<GameObject>("Prefabs/PuddlePrefab");

        _filledBucketSprite = Resources.Load<Sprite>("Sprites/Misc/bucket_filled");
        _emptyBucketSprite = Resources.Load<Sprite>("Sprites/Misc/bucket_empty");
        _bucketSlotImage = GameObject.Find("Slot_Bucket").GetComponent<Image>();
        _bucketImage = GameObject.Find("Slot_Bucket").transform.Find("Bucket").GetComponent<Image>();
        _bucketControls = GameObject.Find("Controls_Bucket").GetComponent<Image>();
        _bucketControls.color = _disabledControlColor;

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
        for(int i = 0; i < 4; i++)
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
        foreach(Pumpkin pumpkin in FindObjectsOfType<Pumpkin>())
        {
            _lives++;
        }

        //Gui defaults
        _titleScreenDefault = _titleScreen.transform.Find("PlayBtn").gameObject;
        _settingsScreenDefault = _settingsScreen.transform.Find("BackBtn").gameObject;
        _pauseScreenDefault = _pauseScreen.transform.Find("ResumeBtn").gameObject;
        _gameOverScreenDefault = _gameOverScreen.transform.Find("RetryBtn").gameObject;

        _countdownGameObject.SetActive(false);
        UpdateLives();
        StartCoroutine(NewWave());
    }

    public void UpdateDash(bool canDash)
    { 
        if(canDash)
        {
            _dashSlot.color = _defaultControlColor;
        } else
        {
            _dashSlot.color = _emptyBucketSlotColor;
        }
    }

    private void ShowMesssage(string message)
    {
        GameObject newMessage = (GameObject)Instantiate(_messagePrefab, _guiRoot.transform);
        newMessage.transform.SetAsFirstSibling();
        newMessage.transform.Find("Text").GetComponent<Text>().text = message;
    }

    private void Start()
    {
        //Set the correct z position of all the trees
        foreach(GameObject tree in GameObject.FindGameObjectsWithTag("Tree"))
        {
            tree.transform.position = new Vector3(tree.transform.position.x, tree.transform.position.y, (tree.transform.position.y * 0.075f) + 11);
        }

        foreach (GameObject tallGrass in GameObject.FindGameObjectsWithTag("Tallgrass"))
        {
            tallGrass.transform.position = new Vector3(tallGrass.transform.position.x, tallGrass.transform.position.y, (tallGrass.transform.position.y * 0.075f) + 11);
        }        
    }

    public bool GetPaused()
    {
        return _paused;
    }

    public void TogglePause()
    {
        StartCoroutine(TogglePauseDelay());
    }

    private IEnumerator TogglePauseDelay()
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
            _guiManagerReference.OnPauseChanged();

            if (_pauseScreen.activeSelf)
            {
                _guiManagerReference.SetDefaultSelection(_pauseScreenDefault);
            }

            if (_paused)
            {
                _acceptsInput = false;
                Time.timeScale = 0f;                
            }
            else
            {
                Time.timeScale = 1f;
                yield return new WaitForSeconds(0.1f);
                _acceptsInput = true;
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

    public void ShowSettings(GameObject newDefault)
    {
        _settingsScreen.SetActive(true);
        _guiManagerReference.SetDefaultSelection(newDefault);
    }

    public void HideSettings()
    {
        _settingsScreen.SetActive(false);
        
        if(_titleScreen.activeSelf)
        {
            _guiManagerReference.SetDefaultSelection(_titleScreenDefault);
        } else
        {
            _guiManagerReference.SetDefaultSelection(_pauseScreenDefault);
        }
    }

    public IEnumerator NewWave()
    {
        _wave++;
        _countdownGameObject.SetActive(false);

        //Set the appropriate difficulty depending on the wave;
        if (_waveDifficulties.ContainsKey(_wave))
        {
            _currentDifficulty = _waveDifficulties[_wave];

            //Show a message for a particular wave
            if(_currentDifficulty.GetMessage().Length > 0)
            {
                ShowMesssage(_currentDifficulty.GetMessage());
            }
        }

        //Wave modifiers
        _scythe.SetDamage(_currentDifficulty.GetScytheDamage());
        _playerControllerReference.SetSpeed(_currentDifficulty.GetPlayerSpeed());

        //Regular skeletons
        int skeletonsToSpawn = Random.Range(_currentDifficulty.GetMin(), _currentDifficulty.GetMax() + 1);
        for(int i = 0; i < skeletonsToSpawn; i++)
        {
            Instantiate(_skeletonPrefab, new Vector3(-100, -100, 0), Quaternion.identity);
        }

        //Super skeletons
        if (_currentDifficulty.GetSuperSkeletonCount() > 0)
        {
            for (int supers = 0; supers < _currentDifficulty.GetSuperSkeletonCount(); supers++)
            {
                Instantiate(_superSkeletonPrefab, new Vector3(-100, -100, 0), Quaternion.identity);
            }
        }

        yield return new WaitForSeconds(1f);
        foreach (Skeleton skeletonScript in FindObjectsOfType<Skeleton>())
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
            _guiManagerReference.SetDefaultSelection(_gameOverScreenDefault);
            Cursor.visible = true;
        }
    }

    public void IncreaseLives()
    {
        _lives++;
        UpdateLives();
    }    

    public void IncreaseBombPoints()
    {
        _bombPoints++;

        _bombPoints = Mathf.Clamp(_bombPoints, 0, _pointsPerBomb * _maxBombs);

        _bombText.text = _bombPoints.ToString() + "/" + _pointsPerBomb.ToString();

        if(_bombPoints >= _pointsPerBomb)
        {
            _bombControls.color = _defaultControlColor;
        }
    }

    public Path GetPath()
    {
        List<Path> validPaths = new List<Path>();

        foreach (Path path in _paths)
        {            
            if (path.GetPumpkin().GetStage() != 0)
            {
                validPaths.Add(path);
            }
        }

        return validPaths[Random.Range(0, validPaths.Count)];
    }

    public void CreateDustPrefab(Vector3 position)
    {
        Instantiate(_dustPrefab, position, Quaternion.identity);
    }

    public void CreateHitPrefab(Vector3 position)
    {
        Instantiate(_hitPrefab, position, Quaternion.identity);

        _hitAudioSource.volume = _settingsReference.GetSfxVolume("Hit");
        _hitAudioSource.Play();
    }

    public void CreateExplosionPrefab(Vector3 position)
    {
        Instantiate(_hitPrefab, position, Quaternion.identity);
        Instantiate(_hitPrefab, position + new Vector3(0.2f, 0.05f, 0f), Quaternion.identity);
        Instantiate(_hitPrefab, position + new Vector3(-0.1f, 0f, 0f), Quaternion.identity);
        Instantiate(_hitPrefab, position + new Vector3(-0.15f, -0.2f, 0f), Quaternion.identity);

        _explosionAudioSource.volume = _settingsReference.GetSfxVolume("Explosion");
        _explosionAudioSource.Play();
    }

    public void PlayFootstepSound(bool forSkeleton = false)
    {
        foreach (AudioSource stepSoundSource in _stepSoundSources)
        {
            if (!stepSoundSource.isPlaying)
            {
                stepSoundSource.clip = _stepSounds[Random.Range(0, _stepSounds.Length)];

                if(forSkeleton)
                {
                    stepSoundSource.volume = _settingsReference.GetSfxVolume("Footstep_Skeleton");
                } else
                {
                    stepSoundSource.volume = _settingsReference.GetSfxVolume("Footstep_Player");
                }

                stepSoundSource.Play();
                break;
            }
        }

    }

    public void PlayLanternSound()
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
            _bucketControls.color = _defaultControlColor;
            _fillBucketPrompt.SetActive(false);
        }
        else
        {
            _bucketControls.color = _disabledControlColor;
            _bucketImage.sprite = _emptyBucketSprite;
            _bucketSlotImage.color = _emptyBucketSlotColor;
        }

        _bucketSoundSource.clip = _bucketRefillSounds[Random.Range(0, _bucketRefillSounds.Length)];
        _bucketSoundSource.volume = _settingsReference.GetSfxVolume("Splash");
        _bucketSoundSource.Play();

        _waterBucketFilled = filled;        
    }    

    private IEnumerator StartWaveDelay()
    {
        _waveActive = false;

        _nextWaveCountdown = _countdownLength;
        _countdownGameObject.SetActive(true);
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

    public bool GetAcceptsInput()
    {
        return _acceptsInput;
    }

    private void Update()
    {        
        //Next wave countdown
        if(_countdownActive)
        {
            _nextWaveCountdown -= Time.deltaTime;

            int timeLeft = Mathf.RoundToInt(_nextWaveCountdown + 1);
            timeLeft = (int)Mathf.Clamp(timeLeft, 1, _nextWaveCountdown+1);

            _countdownText.text = timeLeft.ToString();
        }

        //Pausing
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            //If the settings screen is not displayed then toggle pause like normal
            if (!_settingsScreen.activeSelf && _hasStarted)
            {
                TogglePause();                
            }

            //If the settings screen is displayed then close it
            else
            {
                HideSettings();
            }
        }

        else if(Input.GetButtonDown("Start"))
        {
            //If the settings screen is not displayed then toggle pause like normal
            if (!_settingsScreen.activeSelf && _hasStarted)
            {
                TogglePause();
            }
        }

        //If "B" is pressed while the settings menu is up
        if(Input.GetButtonDown("B") && _settingsScreen.activeSelf)
        {
            HideSettings();
        }

        //If "B" is pressed while the pause menu is up
        else if(Input.GetButtonDown("B") && _pauseScreen.activeSelf && _paused)
        {
            TogglePause();
        }

        //Bomb placement
        if(Input.GetKeyDown(KeyCode.R) && _bombPoints >= _pointsPerBomb || Input.GetButtonDown("X") && _bombPoints >= _pointsPerBomb)
        {
            if (_acceptsInput)
            {
                _bombPoints -= _pointsPerBomb;
                _bombText.text = _bombPoints.ToString() + "/" + _pointsPerBomb.ToString();
                Instantiate(_bombPrefab, _playerControllerReference.transform.position, Quaternion.identity);
                PlayThudSound();

                if (_bombPoints < _pointsPerBomb)
                {
                    _bombControls.color = _disabledControlColor;
                }
            }
        }

        //Dev feature. Add bombs to inventory at will.
        if(Input.GetKeyDown(KeyCode.F) && _devMode)
        {
            for (int i = 0; i < 7; i++)
            {
                IncreaseBombPoints();
            }
        }

        //Puddles
        if(Input.GetKeyDown(KeyCode.E) && _waterBucketFilled && _selectedPumpkin == null && !_canFillBucket ||
            Input.GetButtonDown("A") && _waterBucketFilled && _selectedPumpkin == null && !_canFillBucket)
        {
            if (_acceptsInput)
            {
                Instantiate(_puddlePrefab, _playerControllerReference.transform.position - new Vector3(0, 0.6f, 0), Quaternion.identity);
                Instantiate(_splashPrefab, _playerControllerReference.transform.position - new Vector3(0, 0.6f, 0), Quaternion.identity);
                _waterBucketFilled = false;
                UpdateWaterBucket(false);
            }
        }

        //Pumpkin watering
        else if(Input.GetKeyDown(KeyCode.E) && _canWater && _selectedPumpkin != null && _waterBucketFilled && !_canFillBucket ||
            Input.GetButtonDown("A") && _canWater && _selectedPumpkin != null && _waterBucketFilled && !_canFillBucket)
        {
            if (_acceptsInput)
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
        }

        //Water bucket refill
        else if (Input.GetKeyDown(KeyCode.E) && _canFillBucket && !_waterBucketFilled ||
            Input.GetButtonDown("A") && _canFillBucket && !_waterBucketFilled)
        {
            if (_acceptsInput)
            {
                UpdateWaterBucket(true);
                _canFillBucket = false;
                Instantiate(_splashPrefab, new Vector3(_wellTransform.position.x, _wellTransform.position.y - 1f, -2f), Quaternion.identity);
            }
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
