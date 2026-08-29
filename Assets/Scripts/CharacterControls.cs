using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CharacterControls : MonoBehaviour
{

    private Inputs InputActions;
    public Animator animator;

    public AudioSource CharacterAudioSource;
    public enum PlayerState{Idle, Aim, Combat};
    public PlayerState currentState = PlayerState.Idle;
    private bool AimingBool = false;

    public Light RedLight;

    public Camera CameraView;

    public GameObject AimSpot;
    
    public UIScript UI;

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
        public float GroundCheckRadius;
        public ParticleSystem JumpCloud;
        public Vector2 MoveInput;
        public Vector3 LookDirection;
        public float RotationSpeed;
        public float AimingRotationSpeed;
        public float Speed;
        public float Acceleration;
        public float MaxSpeed;
        public float WalkingAnimationSpeed;

        public AudioClip JumpSound;

    }
    public MovementClass Movement;

    [System.Serializable]
    public class CollitionClass
    {
        public float radius = 0.4f;
        public float height = 1.8f;
        public LayerMask collisionMask;
        public float skinWidth = 0.02f;
        public int maxSlideIterations = 3;
        public float maxSlopeAngle = 45f;
    }
    public CollitionClass Collision;

    [SerializeField] private LayerMask GroundLayer;
    void Awake()
    {
        InputActions = new Inputs();
        animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        InputActions.Player.Enable();
        InputActions.Player.Jump.performed += ctx => Movement.JumpPressed = true;
        InputActions.Player.Move.performed += ctx => Movement.MoveInput = ctx.ReadValue<Vector2>();
        InputActions.Player.Move.canceled += ctx => Movement.MoveInput = Vector2.zero;
        InputActions.Player.Aim.performed += ctx => AimingBool = !AimingBool;
        InputActions.Player.Shoot.performed += ctx => Shoot();

        Movement.IsGrounded = false;
    }

    void OnDisable()
    {
        InputActions.Player.Disable();
    }
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if (AimingBool == true && Movement.IsGrounded)
        {
            currentState = PlayerState.Aim;
            Movement.JumpPressed = false;
            Movement.Speed = 0;

        } else
        {
            currentState = PlayerState.Idle;
        }
        switch (currentState)
        {
            case PlayerState.Idle:
                UpdateIdle();
                break;
            case PlayerState.Aim:
                UpdateAim();
                break;
        }
    }



    void DoGroundCheck()
    {
        Movement.IsGrounded = Physics.CheckSphere(Movement.GroundCheck.position, Movement.GroundCheckRadius, GroundLayer);
        if (Movement.IsGrounded){
            Movement.MaxSpeed = 15;
            Movement.AirTime = 0f;
        } else {
            Movement.AirTime+=Movement.GravityAcs;
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
        animator.speed = Mathf.Abs(Mathf.Log10(Movement.Speed) * Movement.WalkingAnimationSpeed);
        animator.SetBool("WalkingAnimation", true);
    }
    else if (Movement.Speed > 0)
    {
        Movement.Speed -= Mathf.Pow(Movement.Acceleration, 3) * 1.7f;
    }
    else
    {
        Movement.Speed = 0;
        animator.speed = 1;
        animator.SetBool("WalkingAnimation", false);
    }

    if (Movement.LookDirection != Vector3.zero)
    {
        Quaternion targetRotation = Quaternion.LookRotation(Movement.LookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Movement.RotationSpeed * Time.deltaTime);
    }

    Vector3 motion = Movement.LookDirection * Movement.Speed * Time.deltaTime;
    motion += CalculateVerticalMotion(); 
    MoveWithCollision(motion);
}

void MoveWithCollision(Vector3 motion)
{
    Vector3 position = transform.position;
    Vector3 remaining = motion;

    int maxBounces = 3;
    for (int i = 0; i < maxBounces; i++)
    {
        float distance = remaining.magnitude;
        if (distance <= Mathf.Epsilon) break;

        Vector3 direction = remaining.normalized;
        Vector3 bottom = position + Vector3.up * Collision.radius;
        Vector3 top = position + Vector3.up * (Collision.height - Collision.radius);

        if (Physics.CapsuleCast(bottom, top, Collision.radius, direction, out RaycastHit hit,
                distance + Collision.skinWidth, Collision.collisionMask))
        {
            float safeDistance = Mathf.Max(hit.distance - Collision.skinWidth, 0f);
            position += direction * safeDistance;

            Vector3 leftover = direction * (distance - safeDistance);

            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (slopeAngle <= Collision.maxSlopeAngle)
            {
                remaining = Vector3.ProjectOnPlane(leftover, hit.normal);
            }
            else
            {
                remaining = Vector3.ProjectOnPlane(leftover, hit.normal);
            }

            DrawDebugCapsule(bottom, top, Collision.radius, new Color(0f, 1f, 0f, 1f));
        }
        else
        {
            position += direction * distance;
            remaining = Vector3.zero;
        }
    }

    transform.position = position;
}

    void Shoot()
    {
        if(currentState == PlayerState.Aim)
        {
            StartCoroutine(LightTimer(0.1f));
            UI.TakeDamage(2);
        }
        
    }
    void UpdateIdle()
    {
        animator.SetBool("AimingAnimation", false);
        AimSpot.SetActive(false);
        DoGroundCheck();
        Move();
    }

    void UpdateAim()
    {
        
        AimSpot.SetActive(true);
        Ray ray = CameraView.ScreenPointToRay(Mouse.current.position.ReadValue());
        animator.SetBool("AimingAnimation", true);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, GroundLayer))
        {
            Vector3 worldPos = hit.point;
            AimSpot.transform.position = worldPos;

            Vector3 AimingDirection = AimSpot.transform.position-transform.position;
            AimingDirection.x *= 2f;
            AimingDirection.z *= 2f;
            
            Quaternion AimingRotation = Quaternion.LookRotation(AimingDirection);

            
            Vector3 euler = AimingRotation.eulerAngles;
            float x = NormalizeAngle(euler.x);
            float z = NormalizeAngle(euler.z);

            x = Mathf.Clamp(x, -20, 20);
            z = Mathf.Clamp(z, -20, 20);

            AimingRotation = Quaternion.Euler(x, euler.y, z);

            transform.rotation = Quaternion.Slerp(transform.rotation, AimingRotation, Movement.AimingRotationSpeed * Time.deltaTime);
        }
    }
    float NormalizeAngle(float angle)

    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    void DrawDebugCapsule(Vector3 bottom, Vector3 top, float capRadius, Color color)
        {
        int segments = 16;
        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angleA = i * angleStep * Mathf.Deg2Rad;
            float angleB = (i + 1) * angleStep * Mathf.Deg2Rad;

            Vector3 offsetA = new Vector3(Mathf.Cos(angleA), 0, Mathf.Sin(angleA)) * capRadius;
            Vector3 offsetB = new Vector3(Mathf.Cos(angleB), 0, Mathf.Sin(angleB)) * capRadius;

            Debug.DrawLine(bottom + offsetA, bottom + offsetB, color, 0, false);
            Debug.DrawLine(top + offsetA, top + offsetB, color, 0, false);

            if (i % 4 == 0)
                Debug.DrawLine(bottom + offsetA, top + offsetA, color, 0, false);
        }

        Debug.DrawLine(bottom, bottom - Vector3.up * capRadius, color, 0, false);
        Debug.DrawLine(top, top + Vector3.up * capRadius, color, 0, false);
    }

    IEnumerator LightTimer(float duration)
    {
        RedLight.enabled = true;
        yield return new WaitForSeconds(duration);
        RedLight.enabled = false;
    }

    Vector3 CalculateVerticalMotion()
    {
        float vertical = 0f;
        //Jump
        if (Movement.IsGrounded && Movement.JumpPressed)
        {
            CharacterAudioSource.PlayOneShot(Movement.JumpSound);
            Movement.MaxSpeed = 10;
            Movement.JumpCloud.Play();
            animator.SetTrigger("JumpAnimation");
            Movement.JumpSpeed = Movement.JumpForce;
            Movement.JumpPressed = false;
        }
        else if (!Movement.IsGrounded && Movement.JumpSpeed > 0)
        {
            Movement.JumpPressed = false;
            Movement.JumpSpeed -= 1f;
        }

        vertical += Movement.JumpSpeed * Time.deltaTime;
        //Gravitas
        if (!Movement.IsGrounded)
        {
            float fallAmount = Movement.GravityFall * Movement.AirTime * Time.deltaTime;
            vertical -= fallAmount;
        }

        return new Vector3(0, vertical, 0);
    }

}
