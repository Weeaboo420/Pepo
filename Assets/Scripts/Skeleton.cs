using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Skeleton : MonoBehaviour
{
    private int _maxHealth = 100;
    private int _currentHealth;

    public bool _isSuperSkeleton = false;    

    private Vector3 _newScale;
    private GameObject _healthBar, _background;

    private GameManager _gameManagerReference;

    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private Vector2 _nextNode;
    private float _speed;
    private const float _depthMultiplier = 0.075f;
    private Rigidbody2D _rigidbody2D;
    
    private int _nodeIndex = 0; //Used to determine which node on the path we are on

    private Path _path;
    private const float _nodeHeightOffset = 0.8f;

    private bool _canAttack = true;
    private bool _canMove = false;
    private bool _hasReachedPumpkin = false;

    private void Start()
    {
        _gameManagerReference = FindObjectOfType<GameManager>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        
        _path = _gameManagerReference.GetPath();

        //Set the position to be the first node in the path
        transform.position = new Vector3(_path.GetNodes()[_nodeIndex].x, _path.GetNodes()[_nodeIndex].y, transform.position.z);
        FindNextNode();

        //Health bar
        _healthBar = new GameObject("waterBar");
        _healthBar.transform.parent = this.transform;
        _healthBar.transform.position = new Vector3(transform.position.x - 0.8f, transform.position.y + 1.1f, transform.position.z - 5f);
        _healthBar.transform.localScale = new Vector3(1f, 0.08f, 1f);

        SpriteRenderer healthBarSpriteRenderer = _healthBar.AddComponent<SpriteRenderer>();
        healthBarSpriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Level/Box");
        healthBarSpriteRenderer.color = new Color32(255, 211, 32, 255);

        _background = new GameObject("background");
        _background.transform.parent = this.transform;
        _background.transform.position = new Vector3(transform.position.x - 0.8f, transform.position.y + 1.1f, transform.position.z - 4.9f);
        _background.transform.localScale = new Vector3(1f, 0.08f, 1f);

        SpriteRenderer backgroundSpriteRenderer = _background.AddComponent<SpriteRenderer>();
        backgroundSpriteRenderer.sprite = Resources.Load<Sprite>("Sprites/Level/Box");
        backgroundSpriteRenderer.color = new Color32(108, 109, 28, 255);

        if (!_isSuperSkeleton)
        {
            _currentHealth = Random.Range(55, _maxHealth);
            _maxHealth = _currentHealth;
        }
        else
        {
            _maxHealth = 240;
            _currentHealth = _maxHealth;
            transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        }

        UpdateHealthBar();
        StartCoroutine(StartDelay());
    }

    public void PlayFootstepSound()
    {
        _gameManagerReference.PlayFootstepSound(true);
    }

    private IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(Random.Range(0.9f, 3f));
        
        if (!_isSuperSkeleton)
        {
            _speed = Random.Range(1.8f, 2.35f);
        } else
        {
            _speed = 1.4f;
        }

        _canMove = true;
    }

    private void UpdateHealthBar()
    {
        if (_currentHealth == _maxHealth)
        {
            _newScale = new Vector3(1, 0.08f, 1f);
        }

        else
        {
            _newScale = new Vector3((float)_currentHealth / (float)_maxHealth, 0.08f, 1f);
        }
    }

    public int GetCurrentHealth()
    {
        return _currentHealth;
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;

        if(_currentHealth <= 0)
        {
            _gameManagerReference.CreateDustPrefab(transform.position);

            if (_path.GetPumpkin().GetBarVisibility())
            {
                _path.GetPumpkin().SetBarVisibility(false);
            }

            _gameManagerReference.UpdateSkeletonCount();
            Destroy(this.gameObject);
        }

        if (!_isSuperSkeleton)
        {
            _animator.SetTrigger("FlashTrigger");
        }

        UpdateHealthBar();        
    }    

    private IEnumerator AttackDelay()
    {
        _canAttack = false;

        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, Vector2.one, 0f, Vector2.zero);
        foreach(RaycastHit2D hit in hits) 
        { 
            if(hit.transform.CompareTag("Pumpkin") && hit.collider.isTrigger)
            {
                Pumpkin pumpkinScript = hit.transform.GetComponent<Pumpkin>();

                if (pumpkinScript.GetStage() > 0)
                {
                    pumpkinScript.SetBarVisibility(true);

                    if (!_isSuperSkeleton)
                    {
                        pumpkinScript.SetWaterValue(pumpkinScript.GetWaterValue() - 1);
                    } else
                    {
                        pumpkinScript.SetWaterValue(pumpkinScript.GetWaterValue() - 3);
                    }

                    _gameManagerReference.CreateHitPrefab(new Vector3(hit.transform.position.x, hit.transform.position.y, transform.position.z - 1f));

                    if (pumpkinScript.GetStage() == 0)
                    {
                        pumpkinScript.SetBarVisibility(false);

                        _gameManagerReference.DecreaseLives();
                        TakeDamage(_maxHealth);
                    }
                }

                break;
            }
        }

        if (!_isSuperSkeleton)
        {
            yield return new WaitForSeconds(1);
        } else
        {
            yield return new WaitForSeconds(2);
        }
        _canAttack = true;
    }

    private void FindNextNode()
    {
        if(_nodeIndex < _path.GetNodes().Length - 1)
        {
            _nodeIndex++;
            _nextNode = _path.GetNodes()[_nodeIndex];
            _nextNode.y += _nodeHeightOffset;            
        } else
        {
            _hasReachedPumpkin = true;
        }
    }

    private void Update()
    {
        if (_canMove)
        {
            if (_hasReachedPumpkin)
            {
                if (_canAttack)
                {
                    StartCoroutine(AttackDelay());
                }
            }

            //If the pumpkin at the end of the path is "dead" then the skeleton also dies.
            if (_path.GetPumpkin().GetStage() == 0)
            {
                TakeDamage(_maxHealth);

                if(_path.GetPumpkin().GetBarVisibility())
                {
                    _path.GetPumpkin().SetBarVisibility(false);
                }
            }

            if (_healthBar.transform.localScale != _newScale)
            {
                _healthBar.transform.localScale = Vector3.Lerp(_healthBar.transform.localScale, _newScale, Time.deltaTime * 12);
            }

            Vector2 newVelocity = _rigidbody2D.velocity;
            float margin = 0.1f;

            if (_nextNode.x - margin > transform.position.x)
            {
                newVelocity.x = _speed;
                _spriteRenderer.flipX = false;

            }
            else if (_nextNode.x + margin < transform.position.x)
            {
                newVelocity.x = -_speed;
                _spriteRenderer.flipX = true;

            }
            else
            {
                newVelocity.x = 0;
            }


            if (_nextNode.y - margin > transform.position.y)
            {
                newVelocity.y = _speed;
            }
            else if (_nextNode.y + margin < transform.position.y)
            {
                newVelocity.y = -_speed;
            }
            else
            {
                newVelocity.y = 0;
            }

            _rigidbody2D.velocity = Vector2.ClampMagnitude(newVelocity, _speed);

            //If the skeleton is standing still, set the animation to "idle"
            if (newVelocity.x == 0 && newVelocity.y == 0)
            {
                FindNextNode();

                //If the "idle" state is not playing
                if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    _animator.SetBool("Moving", false);
                }
            }

            //If the skeleton is moving, set the animation to "moving"
            else
            {
                //If the "moving" state is not playing
                if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Moving"))
                {
                    _animator.SetBool("Moving", true);
                }
            }

            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * _depthMultiplier);

        }
    }

}
