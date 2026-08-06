using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CharacterControls : MonoBehaviour
{
    private Inputs inputActions;
    public Animator animator;

    public AudioSource CharacterAudioSource;

    

    [System.Serializable]
    public class MovementClass
    {
        public float JumpForce;
        public float GravityFall;
        public float AirTime = 0.5f;
        public float GravityAcs;
        public bool IsGrounded;
        public bool JumpPressed;
        public float JumpSpeed = 0;
        public Transform GroundCheck;
        public ParticleSystem JumpCloud;
        public Vector2 MoveInput;
        public Vector3 LookDirection;
        public float RotationSpeed;
        public float Speed;
        public float Acceleration;
        public float MaxSpeed;
        public float WalkingAnimationSpeed;

        public AudioClip JumpSound;

    }
    public MovementClass Movement;

    [SerializeField] private LayerMask GroundLayer;
    void Awake()
    {
        inputActions = new Inputs();
        animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Jump.performed += ctx => Movement.JumpPressed = true;
        inputActions.Player.Move.performed += ctx => Movement.MoveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled+= ctx => Movement.MoveInput = Vector2.zero;

        Movement.IsGrounded = false;
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        DoGroundCheck();
        Gravity();
        Jump();
        Move();
    }

    void Jump()
    {
        if (Movement.IsGrounded && Movement.JumpPressed)
        {
            CharacterAudioSource.PlayOneShot(Movement.JumpSound);
            Movement.MaxSpeed = 10;
            Movement.JumpCloud.Play();
            animator.SetTrigger("JumpAnimation");
            Movement.JumpSpeed = Movement.JumpForce;
            Movement.JumpPressed = false;
        } else if (!Movement.IsGrounded && Movement.JumpSpeed > 0)
        {
            Movement.JumpPressed = false;
            Movement.JumpSpeed-=1f;
        }
        transform.position += new Vector3(0, Movement.JumpSpeed, 0)*Time.deltaTime;
    }

    void DoGroundCheck()
    {
        Movement.IsGrounded = Physics.CheckSphere(Movement.GroundCheck.position, 0.2f, GroundLayer);
        if (Movement.IsGrounded){
            Movement.MaxSpeed = 15;
            Movement.AirTime = 0.5f;
        } else
        {
            Movement.AirTime+=Movement.GravityAcs;
        }
    }
    void Gravity()
    {
        if (!Movement.IsGrounded)
        {
            transform.position += new Vector3(0, Movement.GravityFall*Movement.AirTime, 0)*Time.deltaTime;
        }
    }
    void Move()
    {
        if (Movement.MoveInput != new Vector2(0, 0))
        {   
            if (Movement.Speed < Movement.MaxSpeed)
            {
                Movement.Speed += Mathf.Pow(Movement.Acceleration, 3);
            }
            
            Movement.LookDirection = new Vector3(Movement.MoveInput.x, 0, Movement.MoveInput.y);
            animator.speed = Mathf.Abs(Mathf.Log10(Movement.Speed)*Movement.WalkingAnimationSpeed);
            animator.SetBool("WalkingAnimation", true);
            
        } else if (Movement.Speed > 0)
        {
            Movement.Speed -=  Mathf.Pow(Movement.Acceleration, 3)*1.7f;
        } else
        {
            Movement.Speed = 0;
            animator.speed = 1;
            animator.SetBool("WalkingAnimation", false);
        }

        if (Movement.LookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Movement.LookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,Movement.RotationSpeed*Time.deltaTime);
        }
        transform.position += Movement.LookDirection*Movement.Speed*Time.deltaTime;
    }
}
