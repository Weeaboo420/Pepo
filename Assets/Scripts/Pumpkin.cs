using UnityEngine;

public class Pumpkin : MonoBehaviour
{
    private const int _maxWaterValue = 6;
    private int _currentWaterValue = 3;
        
    private int _stage = 1;
    private bool _hasBecomePumpkin = true;
    /*
     * 0 = growing seed
     * 1 = pumpkin
     * 2 = jack o'lantern     
     */

    private Vector3 _newScale;
    private GameObject _waterBar, _background;
    private Transform _shadowTransform;

    private Sprite[] _seedSprites;
    private SpriteRenderer _spriteRenderer;

    private GameManager _gameManagerReference;

    private void Start()
    {
        _gameManagerReference = FindObjectOfType<GameManager>();

        _waterBar = new GameObject("waterBar");
        _waterBar.transform.parent = this.transform;
        _waterBar.transform.position = new Vector3(transform.position.x - 0.8f, transform.position.y + 0.8f, transform.position.z - 0.2f);
        _waterBar.transform.localScale = new Vector3(1f, 0.08f, 1f);

        SpriteRenderer waterBarSpriteRenderer = _waterBar.AddComponent<SpriteRenderer>();
        waterBarSpriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Level/Box");
        waterBarSpriteRenderer.color = new Color32(32, 138, 255, 255);

        _shadowTransform = transform.Find("shadow");

        _seedSprites = new Sprite[5];
        _seedSprites = Resources.LoadAll<Sprite>("Sprites/Level/seed_sheet");
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _background = new GameObject("background");
        _background.transform.parent = this.transform;
        _background.transform.position = new Vector3(transform.position.x - 0.8f, transform.position.y + 0.8f, transform.position.z - 0.1f);
        _background.transform.localScale = new Vector3(1f, 0.08f, 1f);

        SpriteRenderer backgroundSpriteRenderer = _background.AddComponent<SpriteRenderer>();
        backgroundSpriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Level/Box");
        backgroundSpriteRenderer.color = new Color32(11, 54, 102, 255);

        UpdateWaterBar();
        SetBarVisibility(false);
        UpdateSprite();
    }

    private void UpdateWaterBar()
    {
        if (_currentWaterValue == _maxWaterValue)
        {           
            _newScale = new Vector3(1, 0.08f, 1f);
        }

        else
        {            
            _newScale = new Vector3((float)_currentWaterValue / (float)_maxWaterValue, 0.08f, 1f);
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public bool GetBarVisibility()
    {
        return _waterBar.activeSelf;
    }

    public void SetBarVisibility(bool visible)
    {
        _waterBar.SetActive(visible);
        _background.SetActive(visible);
    }

    private void UpdateSprite()
    {
        if(_currentWaterValue == _maxWaterValue + 1 && _stage < 2)
        {
            _stage++;
            _currentWaterValue = 1;
        }

        else if(_currentWaterValue == 0 && _stage > 0)
        {
            _stage--;
            if (_stage > 0)
            {
                _currentWaterValue = _maxWaterValue;
            } else
            {
                _currentWaterValue = 0;
            }
        }

        //Seed or stem
        if(_stage == 0)
        {
            _spriteRenderer.sprite = _seedSprites[_currentWaterValue];
            _spriteRenderer.color = new Color32(85, 181, 90, 255);
            _shadowTransform.position = new Vector3(transform.position.x, transform.position.y - 0.34f, 0.1f);
            _hasBecomePumpkin = false;
        }

        //Pumpkin
        else if(_stage == 1)
        {
            //Only load the pumpkin texture once
            if (_spriteRenderer.sprite.name != "pumpkin")
            {
                _spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Level/pumpkin");

                if (!_hasBecomePumpkin)
                {
                    _gameManagerReference.PlayThudSound();
                    _gameManagerReference.IncreaseLives();                    
                    _hasBecomePumpkin = true;
                }
            }
            
            _spriteRenderer.color = new Color32(255, 255, 255, 255);
            _shadowTransform.position = new Vector3(transform.position.x, transform.position.y - 0.52f, 0.1f);
        }

        //Jack O'Lantern
        else if(_stage == 2)
        {
            //Only load the jack o'lantern texture once
            if (_spriteRenderer.sprite.name != "jack")
            {
                _spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Level/jack");
                _gameManagerReference.PlayPumpkinSound();
            }            
        }

    }

    public void SetWaterValue(int newValue)
    {
        if (_stage <= 1)
        {
            newValue = Mathf.Clamp(newValue, 0, _maxWaterValue + 1);
        } else
        {
            newValue = Mathf.Clamp(newValue, 0, _maxWaterValue);
        }

        _currentWaterValue = newValue;
        UpdateSprite();
        UpdateWaterBar();        
    }

    public int GetWaterValue()
    {
        return _currentWaterValue;
    }

    public int GetMaxWaterValue()
    {
        return _maxWaterValue;
    }

    public int GetStage()
    {
        return _stage;
    }

    private void Update()
    {
        if (_waterBar.transform.localScale != _newScale)
        {            
            if (_currentWaterValue != _maxWaterValue && _currentWaterValue > 0)
            {
                //Smoothly scale the water bar instead of having it change instantly
                _waterBar.transform.localScale = Vector3.Lerp(_waterBar.transform.localScale, _newScale, Time.deltaTime * 10f);
            } else
            {
                if (_currentWaterValue == 0)
                {
                    //If the water value goes from 5 to 0 then we change the scale instantly.
                    _waterBar.transform.localScale = _newScale;
                } 
                
                else
                {
                    _waterBar.transform.localScale = Vector3.Lerp(_waterBar.transform.localScale, _newScale, Time.deltaTime * 10f);
                }
            }
        }
    }
}
