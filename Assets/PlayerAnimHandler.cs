using UnityEngine;
using System.Collections;
using Prime31;

public class PlayerAnimHandler : MonoBehaviour
{

    private Animator _animator;
    private PlayerMovement _movementScript;
    private CharacterController2D _controller;

    //flags to check animations
    bool _isPlayer_moving;
    bool _isPlayer_idle;

    //animation states
    const int STATE_IDLE = 0;
    const int STATE_RUNNING = 1;
    const int STATE_JUMPING = 2;
    const int STATE_CROUCHING = 3;
    const int STATE_CRAWLING = 4;
    const int STATE_ROLLING = 5;

    int _currentAnimationState = STATE_IDLE;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponentInParent<CharacterController2D>();
        _movementScript = GetComponentInParent<PlayerMovement>();
    }


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("anim" + _controller.isGrounded + " " + Input.GetButtonDown("Jump"));
        if (_controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            ChangeState(STATE_JUMPING);
        }
        else if (_movementScript.IsRolling)
        {
            ChangeState(STATE_ROLLING);
        }
        else if (_controller.isGrounded && _movementScript.IsCrouching && Mathf.Abs(Input.GetAxis("Horizontal")) > 0)
        {
            ChangeState(STATE_CRAWLING);
        }
        else if (_controller.isGrounded && _movementScript.IsCrouching)
        {
            ChangeState(STATE_CROUCHING);
        }
        else if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0 && _controller.isGrounded)
        {
            ChangeState(STATE_RUNNING);
        }
        else if (_controller.isGrounded)
        {
            ChangeState(STATE_IDLE);
        }
        Debug.Log(_currentAnimationState);
    }

    void ChangeState(int state)
    {
        if (_currentAnimationState == state)
            return;

        switch (state)
        {
            case STATE_IDLE:
                _animator.SetInteger("state", STATE_IDLE);
                break;

            case STATE_RUNNING:
                _animator.SetInteger("state", STATE_RUNNING);
                break;

            case STATE_JUMPING:
                _animator.SetInteger("state", STATE_JUMPING);
                break;

            case STATE_CROUCHING:
                _animator.SetInteger("state", STATE_CROUCHING);
                break;

            case STATE_CRAWLING:
                _animator.SetInteger("state", STATE_CRAWLING);
                break;

            case STATE_ROLLING:
                _animator.SetInteger("state", STATE_ROLLING);
                break;
        }

        _currentAnimationState = state;
    }
}
