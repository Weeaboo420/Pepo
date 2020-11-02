using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    private float _masterVolume;
    private float _musicVolume;
    private float _sfxVolume;
    private bool _useVsync;

    private Text _masterVolumeText;
    private Text _musicValueText;
    private Text _sfxValueText;
    private Toggle _vsyncToggle;

    private AudioSource _musicAudioSource;

    private Dictionary<string, float> _soundsDict;

    private void Awake()
    {        
        Application.targetFrameRate = 1337;

        _soundsDict = new Dictionary<string, float>()
        {
            {"Splash", 1.0f },
            {"Pumpkin", 1.0f },
            {"Footstep_Player", 0.7f },
            {"Footstep_Skeleton", 0.4f },
            {"Dash", 0.5f },
            {"Hit", 0.75f },
            {"Explosion", 0.45f }
        };

        Dictionary<string, float> clampedSoundsDict = new Dictionary<string, float>();

        //Clamp the volume of the sounds between 0 and 1
        foreach(string key in _soundsDict.Keys)
        {
            clampedSoundsDict[key] = Mathf.Clamp(_soundsDict[key], 0f, 1f);
        }

        _soundsDict = clampedSoundsDict;
        _musicAudioSource = GameObject.FindGameObjectWithTag("Music").GetComponent<AudioSource>();

        GameObject settingsScreen = GameObject.Find("SettingsScreen");

        _masterVolumeText = settingsScreen.transform.Find("MasterVolume").transform.Find("Value").GetComponent<Text>();
        _musicValueText = settingsScreen.transform.Find("MusicVolume").transform.Find("Value").GetComponent<Text>();
        _sfxValueText = settingsScreen.transform.Find("SfxVolume").transform.Find("Value").GetComponent<Text>();
        _vsyncToggle = settingsScreen.transform.Find("VsyncToggle").GetComponent<Toggle>();

        //Look for setting stored inside PlayerPrefs. If there are none, then grab defaults
        if(!PlayerPrefs.HasKey("UseVsync"))
        {
            _useVsync = DefaultSettings.UseVsync;
        } else
        {            
            //Since PlayerPrefs doesn't store booleans we use ints instead. 1 = true, 0 = false
            if(PlayerPrefs.GetInt("UseVsync") == 1)
            {
                _useVsync = true;
            } else
            {
                _useVsync = false;
            }
        }

        //Master volume
        if (!PlayerPrefs.HasKey("MasterVolume"))
        {
            _masterVolume = DefaultSettings.MasterVolume;
        }
        else
        {
            _masterVolume = PlayerPrefs.GetFloat("MasterVolume");
        }


        //Music volume
        if (!PlayerPrefs.HasKey("MusicVolume"))
        {
            _musicVolume = DefaultSettings.MusicVolume;
        } else
        {
            _musicVolume = PlayerPrefs.GetFloat("MusicVolume");            
        }


        //SFX volume
        if(!PlayerPrefs.HasKey("SfxVolume"))
        {
            _sfxVolume = DefaultSettings.MusicVolume;
        } else
        {
            _sfxVolume = PlayerPrefs.GetFloat("SfxVolume");
        }

        settingsScreen.transform.Find("MasterVolume").transform.Find("Slider").GetComponent<Slider>().value = _masterVolume * 10;
        settingsScreen.transform.Find("MusicVolume").transform.Find("Slider").GetComponent<Slider>().value = _musicVolume * 10;
        settingsScreen.transform.Find("SfxVolume").transform.Find("Slider").GetComponent<Slider>().value = _sfxVolume * 10;
        _vsyncToggle.isOn = _useVsync;

        SetMasterVolume(_masterVolume * 10);
        SetMusicVolume(_musicVolume * 10);
        SetSfxVolume(_sfxVolume * 10);
        SetUseVsync(_useVsync);
    }


    //Only use this when exporting a new build of the game
    [ContextMenu("Clear PlayerPrefs")]
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs cleared");
    }

    public void SetUseVsync(bool useVsync)
    {
        _useVsync = useVsync;

        if(_useVsync)
        {
            QualitySettings.vSyncCount = 1;
            PlayerPrefs.SetInt("UseVsync", 1);
        } else
        {
            QualitySettings.vSyncCount = 0;
            PlayerPrefs.SetInt("UseVsync", 0);
        }     
    }

    public void SetMasterVolume(System.Single newVolume)
    {
        newVolume = newVolume / 10;
        newVolume = Mathf.Clamp(newVolume, 0f, 1f);

        AudioListener.volume = newVolume;
        _masterVolume = AudioListener.volume;

        _masterVolumeText.text = (newVolume * 10).ToString() + "/10";        

        PlayerPrefs.SetFloat("MasterVolume", _masterVolume);
    }

    public void SetMusicVolume(System.Single newVolume)
    {
        newVolume = newVolume / 10;
        newVolume = Mathf.Clamp(newVolume, 0f, 1f);

        _musicAudioSource.volume = newVolume;
        _musicVolume = _musicAudioSource.volume;

        _musicValueText.text = (newVolume * 10).ToString() + "/10";

        PlayerPrefs.SetFloat("MusicVolume", _musicVolume);        
    }

    public void SetSfxVolume(System.Single newVolume)
    {
        newVolume = newVolume / 10;
        newVolume = Mathf.Clamp(newVolume, 0f, 1f);

        _sfxVolume = newVolume;
        _sfxValueText.text = (newVolume * 10).ToString() + "/10";

        PlayerPrefs.SetFloat("SfxVolume", _sfxVolume);
    } 

    public float GetSfxVolume(string soundName)
    {
        if(_soundsDict.ContainsKey(soundName))
        {
            return _soundsDict[soundName] * _sfxVolume;
        }

        return 1.0f;
    }

}
