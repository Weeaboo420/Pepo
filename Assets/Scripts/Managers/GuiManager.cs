using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

public class GuiManager : MonoBehaviour
{
    private EventSystem _eventSystem;
    private GameObject _defaultSelection;

    private GameManager _gameManagerReference;

    private bool _usingController = false;
    private const float _analogDeadzone = 0.5f;
    private const float _triggerDeadzone = 0.5f;
    private const float _mouseDeadzone = 0.05f;

    private GameObject[] _controllerButtons;
    private GameObject[] _pcButtons;

    public void SetControllerButtons(GameObject[] buttons)
    {
        _controllerButtons = buttons;
    }

    public void SetPcButtons(GameObject[] buttons)
    {
        _pcButtons = buttons;
    }

    private void Start()
    {
        _eventSystem = FindObjectOfType<EventSystem>();
        _defaultSelection = _eventSystem.firstSelectedGameObject;
        
        if(_usingController)
        {
            _eventSystem.SetSelectedGameObject(_defaultSelection);
        } else
        {
            _eventSystem.firstSelectedGameObject = null;
            _eventSystem.SetSelectedGameObject(null);
        }

        _gameManagerReference = FindObjectOfType<GameManager>();
        UpdateControllerButtons(_usingController);
    }

    //Used when changing menus. This gives us a default element to refocus on when we go from using keyboard & mouse to using a controller.
    public void SetDefaultSelection(GameObject newDefaultSelection)
    {
        _defaultSelection = newDefaultSelection;

        if (_usingController)
        {
            _eventSystem.SetSelectedGameObject(_defaultSelection);
        } else
        {
            _eventSystem.SetSelectedGameObject(null);            
        }
    }

    private void UpdateControllerButtons(bool visible)
    {
        foreach(GameObject button in _controllerButtons)
        {
            button.SetActive(visible);
        }
        
        foreach(GameObject button in _pcButtons)
        {
            Image buttonImage;
            if (button.TryGetComponent<Image>(out buttonImage))
            {
                buttonImage.enabled = !visible;
            } else
            {
                button.SetActive(!visible);
            }
        }

        if (_gameManagerReference.GetPaused())
        {
            Cursor.visible = !visible;
        } else
        {
            Cursor.visible = false;
        }
    }

    public void OnPauseChanged()
    {
        UpdateControllerButtons(_usingController);
    }

    private void Update()
    {

        //Detect the current input method. It's either a controller or mouse and keyboard.
        if (Input.GetButtonDown("A") || Input.GetButtonDown("B") || Input.GetButtonDown("X") || Input.GetButtonDown("Y") ||
            Input.GetAxis("LS_horizontal") > _analogDeadzone || Input.GetAxis("LS_horizontal") < -_analogDeadzone ||
            Input.GetAxis("LS_vertical") < -_analogDeadzone || Input.GetAxis("LS_vertical") > _analogDeadzone ||
            Input.GetAxis("RT") > _triggerDeadzone || Input.GetAxis("LT") > _triggerDeadzone ||
            Input.GetAxis("Horizontal") > _analogDeadzone || Input.GetAxis("Horizontal") < -_analogDeadzone ||
            Input.GetAxis("Vertical") < -_analogDeadzone || Input.GetAxis("Vertical") > _analogDeadzone ||
            Input.GetButtonDown("RB") || Input.GetButtonDown("LB") || Input.GetButtonDown("Start"))
        {
            if (!_usingController)
            {
                _usingController = true;
                UpdateControllerButtons(true);
                _eventSystem.SetSelectedGameObject(_defaultSelection);                
            }
        }

        //If any key is pressed or the mouse is moved then apply the appropriate changes
        else if(Input.anyKeyDown || Mathf.Abs(Input.GetAxis("Mouse X")) > _mouseDeadzone || Mathf.Abs(Input.GetAxis("Mouse Y")) > _mouseDeadzone)
        {
            if (_usingController)
            {
                _usingController = false;
                UpdateControllerButtons(false);
                _eventSystem.SetSelectedGameObject(null);
            }
        }               
    }
}
