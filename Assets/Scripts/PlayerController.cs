using System.Collections;
using UnityEngine;

public enum Direction
{
    Left,
    Right
}

public class PlayerController : MonoBehaviour
{
    private float _speed = 4.2f;
    private const float _depthMultiplier = 0.075f;
    private Vector2 _scytheOffset = new Vector2(1.3f, 0.15f);

    private Vector2 _minBounds, _maxBounds;

    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _scytheSpriteRenderer;
    private Animator _scytheAnimator;
    private Animator _playerAnimator;
    private Transform _scytheTransform;

    private Vector2 _currentVelocity;
    private Direction _currentDirection = Direction.Right;
    private Direction _currentScytheDirection = Direction.Right;
    private Rigidbody2D _rb;

    private bool _canAttack = true;
    private const float _attackCooldown = 0.32f;

    private GameManager _gameManagerReference;

    private void Start() 
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        GameObject scythe = GameObject.Find("Scythe");
        _scytheAnimator = scythe.GetComponent<Animator>();
        _playerAnimator = GetComponent<Animator>();
        _scytheTransform = scythe.transform;
        _scytheSpriteRenderer = scythe.GetComponent<SpriteRenderer>();

        _gameManagerReference = FindObjectOfType<GameManager>();

        _minBounds = new Vector2(-9f, -15f);
        _maxBounds = new Vector2(13f, 5f);
    }

    public void SetSpeed(float newSpeed)
    {
        _speed = newSpeed;

        if(_playerAnimator == null)
        {
            _playerAnimator = GetComponent<Animator>();
        }

        _playerAnimator.speed = (float)System.Math.Round(_speed / 4.2, 2);
    }

    private void SetDirection(Direction newDirection)
    {        
        _currentDirection = newDirection;
        if(newDirection == Direction.Right)
        {
            _spriteRenderer.flipX = false;
        } else
        {
            _spriteRenderer.flipX = true;
        }             
    }

    private void UpdateScytheDirection()
    {
        if (_scytheAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            if(_currentScytheDirection == Direction.Left)
            {
                _scytheSpriteRenderer.flipX = true;
            } else
            {
                _scytheSpriteRenderer.flipX = false;
            }
        }
    }

    IEnumerator AttackCooldown()
    {
        _canAttack = false;
        yield return new WaitForSeconds(_attackCooldown);
        _canAttack = true;
    }

    public Direction GetScytheDirection()
    {
        return _currentScytheDirection;
    }

    private void Update()
    {
        if (!_gameManagerReference.GetPaused())
        {

            //Check if the player is in range of the well.
            RaycastHit2D[] hits = Physics2D.BoxCastAll((Vector2)transform.position, new Vector2(1f, 1f), 0f, Vector2.zero);
            bool inRangeOfWell = false;

            bool inRangeOfPumpkin = false;
            Pumpkin selectedPumpkin = null;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.transform.CompareTag("Well"))
                {
                    inRangeOfWell = true;
                }

                else if (hit.transform.CompareTag("Pumpkin") && hit.collider.isTrigger)
                {
                    inRangeOfPumpkin = true;
                    selectedPumpkin = hit.transform.GetComponent<Pumpkin>();
                }
            }

            _gameManagerReference.UpdateInRangeOfWell(inRangeOfWell);
            _gameManagerReference.UpdateInRangeOfPumpkin(inRangeOfPumpkin, selectedPumpkin);

            Vector3 clampedPosition = transform.position;
            _currentVelocity = _rb.velocity;

            //Scythe

            //Left swing
            if (Input.GetKeyDown(KeyCode.J) || Input.GetKey(KeyCode.J) || Input.GetMouseButton(0) || Input.GetMouseButtonDown(0))
            {
                if (_canAttack)
                {
                    _currentScytheDirection = Direction.Left;
                    UpdateScytheDirection();
                    _scytheAnimator.SetTrigger("Swing");
                    StartCoroutine(AttackCooldown());
                }
            }

            //Right swing
            else if (Input.GetKeyDown(KeyCode.K) || Input.GetKey(KeyCode.K) || Input.GetMouseButton(1) || Input.GetMouseButtonDown(1))
            {
                if (_canAttack)
                {
                    _currentScytheDirection = Direction.Right;
                    UpdateScytheDirection();
                    _scytheAnimator.SetTrigger("Swing");
                    StartCoroutine(AttackCooldown());
                }
            }

            //Horizontal movement
            if (Input.GetKey(KeyCode.D))
            {
                _currentVelocity.x = _speed;
                SetDirection(Direction.Right);
            }

            else if (Input.GetKey(KeyCode.A))
            {
                _currentVelocity.x = -_speed;
                SetDirection(Direction.Left);
            }

            else
            {
                _currentVelocity.x = 0;
            }

            //Vertical movement
            if (Input.GetKey(KeyCode.W))
            {
                _currentVelocity.y = _speed;
            }

            else if (Input.GetKey(KeyCode.S))
            {
                _currentVelocity.y = -_speed;
            }

            else
            {
                _currentVelocity.y = 0;
            }

            _rb.velocity = Vector2.ClampMagnitude(_currentVelocity, _speed); //Clamp the movement to prevent faster diagonal movement

            if (_rb.velocity.x != 0 || _rb.velocity.y != 0)
            {
                _playerAnimator.SetBool("Moving", true);
            }
            else if (_rb.velocity.x == 0 && _rb.velocity.y == 0)
            {
                _playerAnimator.SetBool("Moving", false);
            }

            //Scythe placement
            Vector2 newScythePosition = Vector2.zero;

            if (_currentScytheDirection == Direction.Right)
            {
                newScythePosition = (Vector2)transform.position + new Vector2(_scytheOffset.x, _scytheOffset.y);
            }
            else
            {
                newScythePosition = (Vector2)transform.position + new Vector2(-_scytheOffset.x, _scytheOffset.y);
            }

            _scytheTransform.position = new Vector3(newScythePosition.x, newScythePosition.y, _scytheTransform.position.z);


            //Confine the player's position to a rectangular area
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, _minBounds.x, _maxBounds.x);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, _minBounds.y, _maxBounds.y);

            clampedPosition.z = clampedPosition.y * _depthMultiplier;

            transform.position = clampedPosition;
        }
    }
}
