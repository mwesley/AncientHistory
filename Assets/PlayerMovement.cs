using UnityEngine;
using System.Collections;
using Prime31;


public class PlayerMovement : MonoBehaviour
{
    // movement config
    public float gravity = -25f;
    public float runSpeed = 8f;
    public float groundDamping = 20f; // how fast do we change direction? higher means faster
    public float inAirDamping = 5f;
    public float jumpHeight = 3f;

    private BoxCollider2D _collider;

    [HideInInspector]
    private float normalizedHorizontalSpeed = 0;

    private CharacterController2D _controller;
    private Animator _animator;
    private RaycastHit2D _lastControllerColliderHit;
    private Vector3 _velocity;

    //animator bools/states
    public bool IsCrouching;
    private bool _isRunning;
    public bool IsRolling;

    private float _rollTimer;
    private float rollVel;

    void Awake()
    {
       // _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController2D>();
        _collider = GetComponent<BoxCollider2D>();

        // listen to some events for illustration purposes
        _controller.onControllerCollidedEvent += onControllerCollider;
        _controller.onTriggerEnterEvent += onTriggerEnterEvent;
        _controller.onTriggerExitEvent += onTriggerExitEvent;
    }

    void Start()
    {
    }


    #region Event Listeners

    void onControllerCollider(RaycastHit2D hit)
    {
        // bail out on plain old ground hits cause they arent very interesting
        if (hit.normal.y == 1f)
            return;

        // logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
         //  Debug.Log("flags: " + _controller.collisionState + ", hit.normal: " + hit.normal);
    }


    void onTriggerEnterEvent(Collider2D col)
    {
        Debug.Log("onTriggerEnterEvent: " + col.gameObject.name);
    }


    void onTriggerExitEvent(Collider2D col)
    {
        Debug.Log("onTriggerExitEvent: " + col.gameObject.name);
    }

    #endregion


    // the Update loop contains a very simple example of moving the character around and controlling the animation
    void Update()
    {
        
        BasicMovement();
        Roll();
        Crouch();

      /*  #region Animators
        _animator.SetBool("Running", _isRunning);
        _animator.SetBool("Crouching", _isCrouching);
        _animator.SetBool("Rolling", _isRolling);
        #endregion*/

        _controller.move(_velocity * Time.deltaTime);

        // grab our current _velocity to use as a base for all calculations
        _velocity = _controller.velocity;

    }

    void BasicMovement()
    {
        if (_controller.isGrounded)
            _velocity.y = 0;

        normalizedHorizontalSpeed = Input.GetAxis("Horizontal");
        if (normalizedHorizontalSpeed > 0 && !IsRolling)
        {
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }
        else if (normalizedHorizontalSpeed < 0 && !IsRolling)
        {
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }

        // we can only jump whilst grounded
        if (_controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            _velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
        }

        var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
        _velocity.x = Mathf.SmoothDamp(_velocity.x, normalizedHorizontalSpeed * runSpeed, ref _controller.velocity.x, Time.deltaTime * smoothedMovementFactor);

        // apply gravity before moving
        _velocity.y += gravity * Time.deltaTime;

        // if holding down bump up our movement amount and turn off one way platform detection for a frame.
        // this lets uf jump down through one way platforms
        if (_controller.isGrounded && Input.GetKey(KeyCode.DownArrow))
        {
            _velocity.y *= 3f;
            _controller.ignoreOneWayPlatformsThisFrame = true;
        }
    }

    void Roll()
    {
        RaycastHit2D upRay = Physics2D.Raycast(transform.position, new Vector2(0, 1), 1f, LayerMask.GetMask("Ground"));

        if (Input.GetButtonDown("Crouch") && Mathf.Abs(normalizedHorizontalSpeed) > 0)
        {
            _collider.size = new Vector3(1, 1);
            _controller.recalculateDistanceBetweenRays();
            rollVel = _velocity.x;
            IsRolling = true;
        }

        if (IsRolling)
        {
            _velocity.x = rollVel;
            _rollTimer += Time.deltaTime;
            if(_rollTimer >= 0.7f)
            {
                if(Input.GetButton("Crouch") || upRay.collider != null)
                {
                    runSpeed *= 0.5f;
                    IsCrouching = true;
                }
                else if (!Input.GetButton("Crouch"))
                {
                    _collider.size = new Vector3(1, 2);
                    transform.Translate(new Vector3(0, 0.5f));
                    _controller.recalculateDistanceBetweenRays();
                }
                IsRolling = false;
                _rollTimer = 0f;
            }
        }
    }

    void Crouch()
    {
        RaycastHit2D upRay = Physics2D.Raycast(transform.position, new Vector2(0, 1), 1f, LayerMask.GetMask("Ground"));

        if (Input.GetButtonDown("Crouch") && Mathf.Abs(normalizedHorizontalSpeed) < 0.1 && !IsRolling)
        {
            _collider.size = new Vector3(1, 1);
            _controller.recalculateDistanceBetweenRays();
            runSpeed *= 0.5f;
            IsCrouching = true;
        }
        else if (!Input.GetButton("Crouch") && !IsRolling && upRay.collider == null && IsCrouching)
        {
            _collider.size = new Vector3(1, 2);
            transform.Translate(new Vector3(0, 0.5f));
            _controller.recalculateDistanceBetweenRays();
            runSpeed *= 2f;
            IsCrouching = false;
        }

    }

}
