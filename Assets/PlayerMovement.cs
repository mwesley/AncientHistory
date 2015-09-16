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
    private bool _isCrouching;
    private bool _isRunning;
    private bool _isRolling;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController2D>();
        _collider = GetComponent<BoxCollider2D>();

        // listen to some events for illustration purposes
        _controller.onControllerCollidedEvent += onControllerCollider;
        _controller.onTriggerEnterEvent += onTriggerEnterEvent;
        _controller.onTriggerExitEvent += onTriggerExitEvent;
    }


    #region Event Listeners

    void onControllerCollider(RaycastHit2D hit)
    {
        // bail out on plain old ground hits cause they arent very interesting
        if (hit.normal.y == 1f)
            return;

        // logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
        //   Debug.Log("flags: " + _controller.collisionState + ", hit.normal: " + hit.normal);
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

        #region Animators
        _animator.SetBool("Running", _isRunning);
        _animator.SetBool("Crouching", _isCrouching);
        _animator.SetBool("Rolling", _isRolling);
        #endregion

        _controller.move(_velocity * Time.deltaTime);

        // grab our current _velocity to use as a base for all calculations
        _velocity = _controller.velocity;
    }

    void BasicMovement()
    {
        if (_controller.isGrounded)
            _velocity.y = 0;

        normalizedHorizontalSpeed = Input.GetAxis("Horizontal");
        if (normalizedHorizontalSpeed > 0)
        {
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }
        else if (normalizedHorizontalSpeed < 0)
        {
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }

        if (Mathf.Abs(normalizedHorizontalSpeed) > 0)
        {
            _isRunning = true;
        }
        else
        {
            _isRunning = false;
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
    }
}
