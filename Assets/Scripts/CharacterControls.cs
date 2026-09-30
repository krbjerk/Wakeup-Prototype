using System;
using System.Collections;
using System.Data;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CharacterControls : MonoBehaviour
{

    private Inputs InputActions;
    public Animator animator;

    public AudioSource CharacterAudioSource;
    public enum PlayerState{Idle, Aim, Combat, Crouch};
    public PlayerState currentState = PlayerState.Idle;
    private bool AimingBool = false;
    private bool CrouchBool = false;

    public Light RedLight;

    public Camera CameraView;

    public GameObject AimSpot;
    private GameObject AimedObject;
    public GameObject AimArmObject;
    
    public UIScript UI;
    public CharacterSheet PlayerSheet;

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
        public Vector3 StartMove;
        public GameObject MovementRing;
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

    [System.Serializable]
    public class Gun
    {
        public int Damage;
        public int AP;
        public ParticleSystem GunTrail;
        public AudioClip GunSound;
        public float GunVolume;
        
    }
    public Gun CurrentGun;
    
    private Quaternion GunRotation;
    

    [SerializeField] private LayerMask GroundLayer;
    void Awake()
    {
        InputActions = new Inputs();
        animator = GetComponentInChildren<Animator>();
        GunRotation = AimArmObject.transform.localRotation;
        Movement.StartMove = transform.position;
    }

    void OnEnable()
    {
        InputActions.Player.Enable();
        InputActions.Player.Jump.performed += ctx => Movement.JumpPressed = true;
        InputActions.Player.Move.performed += ctx => Movement.MoveInput = ctx.ReadValue<Vector2>();
        InputActions.Player.Move.canceled += ctx => Movement.MoveInput = Vector2.zero;
        InputActions.Player.Crouch.performed += ctx => CrouchBool = !CrouchBool;
        InputActions.Player.Crouch.performed += ctx => Crouch();
        InputActions.Player.Aim.performed += ctx => AimingBool = !AimingBool;
        InputActions.Player.Aim.performed += ctx => StateChanger();
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

        } else if(CrouchBool == true && Movement.IsGrounded) {
            currentState = PlayerState.Crouch;
        } else
        {
            animator.SetBool("CrouchAnimation", false);
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
            case PlayerState.Crouch:
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
    } else {
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
    Vector3 pos = transform.position;
    Vector3 remaining = motion;

    int maxBounces = 3;
    for (int i = 0; i < maxBounces; i++)
    {
        Vector3 clampedTarget = ClampMovement(pos + remaining);
        remaining = clampedTarget - pos;

        float distance = remaining.magnitude;
        if (distance <= Mathf.Epsilon) break;

        Vector3 direction = remaining.normalized;
        Vector3 bottom = pos + Vector3.up * Collision.radius;
        Vector3 top = pos + Vector3.up * (Collision.height - Collision.radius);

        if (Physics.CapsuleCast(bottom, top, Collision.radius, direction, out RaycastHit hit,
                distance + Collision.skinWidth, Collision.collisionMask))
        {
            float safeDistance = Mathf.Max(hit.distance - Collision.skinWidth, 0f);
            pos += direction * safeDistance;

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
        }
        else
        {
            pos += direction * distance;
            remaining = Vector3.zero;
        }
    }

    transform.position = pos;
    PlayerSheet.PlayerStats.currentMovement = Vector2.Distance(new Vector2(Movement.StartMove.x, Movement.StartMove.z), new Vector2(transform.position.x, transform.position.z));
}

    Vector3 ClampMovement(Vector3 pos)
    {
        Vector3 center = Movement.StartMove;

        Vector3 offset = pos - center;
        offset.y = 0f;                                   // horizontal (XZ) clamp only

        float maxDist = PlayerSheet.PlayerStats.MovementMax;  // keep the capsule's edge inside, not just its center

        if (offset.sqrMagnitude > maxDist * maxDist)
        {
            Vector3 clamped = center + offset.normalized * maxDist;
            pos.x = clamped.x;
            pos.z = clamped.z;                           // Y untouched, so gravity/jumping still work
        }
        return pos;
    }

    void Shoot()
    {
        if(currentState == PlayerState.Aim && PlayerSheet.PlayerStats.currentAP >= CurrentGun.AP)
        {
            
            if (AimedObject.TryGetComponent<Entities>(out Entities Entity))
            {
                CurrentGun.GunTrail.Play();
                CharacterAudioSource.PlayOneShot(CurrentGun.GunSound, CurrentGun.GunVolume);
                Vector3 Direction = transform.position-AimedObject.transform.position;
                Entity.TakeDamage(CurrentGun.Damage,Direction);
                UI.UseAP(CurrentGun.AP);

            }
        }
    }

    void UpdateIdle()
    {
        DoGroundCheck();
        Move();
        UpdateMovementAP();
    }

void UpdateAim()
{
    
    Ray ray = CameraView.ScreenPointToRay(Mouse.current.position.ReadValue());
    
    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, GroundLayer))
    {
        Vector3 worldPos = hit.point;
        AimSpot.transform.position = worldPos;

        Vector3 AimingDirection = AimSpot.transform.position - transform.position;
        

        Quaternion FullAimingRotation = Quaternion.LookRotation(AimingDirection);
        
        AimingDirection.x *= 2f;
        AimingDirection.z *= 2f;

        Vector3 euler = FullAimingRotation.eulerAngles;
        float x = NormalizeAngle(euler.x);
        float z = NormalizeAngle(euler.z);

        x = Mathf.Clamp(x, -20, 20);
        z = Mathf.Clamp(z, -20, 20);

        Quaternion ClampedBodyRotation = Quaternion.Euler(x, euler.y, z);

        transform.rotation = Quaternion.Slerp(transform.rotation, ClampedBodyRotation, Movement.AimingRotationSpeed * Time.deltaTime);

        AimArmObject.transform.rotation = Quaternion.Slerp(AimArmObject.transform.rotation, FullAimingRotation, Movement.AimingRotationSpeed * Time.deltaTime*0.8f);
        
        AimedObject = hit.collider.gameObject;
        UI.ShowObject(AimedObject);
        DoGroundCheck();
        CalculateVerticalMotion();
    }
}
    float NormalizeAngle(float angle)

    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }


    Vector3 CalculateVerticalMotion()
    {
        float vertical = 0f;
        if (Movement.AirTime > 20f)
        {
            Movement.GroundCheckRadius = 1;
        } else
        {
            Movement.GroundCheckRadius = 0.4f;
        }
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

    void StateChanger()
    {
        if (AimingBool)
        {
            AimSpot.SetActive(true);
            animator.SetBool("AimingAnimation", true);
            ParticleSystem Ring = Movement.MovementRing.GetComponent<ParticleSystem>();
            Ring.Stop();
        } else
        {
            AimArmObject.transform.localRotation = GunRotation;
            animator.SetBool("AimingAnimation", false);
            AimSpot.SetActive(false);
            ParticleSystem Ring = Movement.MovementRing.GetComponent<ParticleSystem>();
            Ring.Play();
        }
    }

    public void UpdateMovementRing()
    {
        PlayerSheet.PlayerStats.MovementMax = PlayerSheet.PlayerStats.currentAP * PlayerSheet.PlayerStats.MovementperAP + PlayerSheet.PlayerStats.MovementperAP;
        Movement.StartMove = transform.position;
        ParticleSystem Ring = Movement.MovementRing.GetComponent<ParticleSystem>();
        var shape = Ring.shape;
        shape.radius = PlayerSheet.PlayerStats.MovementMax*0.78f;
        var emission = Ring.emission;
        emission.rateOverTime = 300 * PlayerSheet.PlayerStats.MovementMax/8;
        Vector3 pos = Movement.MovementRing.transform.position;
        pos = transform.position;
        pos.y = -2;
        Movement.MovementRing.transform.position = pos;
    }

    private int lastDisplayedAP = -1;

    public void UpdateMovementAP()
    {
        int movementUsed = Mathf.Max(0, Mathf.CeilToInt(PlayerSheet.PlayerStats.currentMovement / PlayerSheet.PlayerStats.MovementperAP) - 1);
        int newAP = Mathf.Max(0, PlayerSheet.PlayerStats.IntermidiateAP - movementUsed);

        PlayerSheet.PlayerStats.currentAP = newAP;

        if (newAP != lastDisplayedAP)
        {
            lastDisplayedAP = newAP;
            UI.setAP(newAP);
        }
    }

    private Coroutine crouchCoroutine;

    void Crouch(){
        if (AimingBool == false && Movement.IsGrounded){
            {
                GameObject Model = animator.gameObject;
                Vector3 targetPos = Model.transform.position;
                targetPos.y += CrouchBool ? -0.5f : 0.5f;

                animator.SetBool("CrouchAnimation", CrouchBool);

                if (crouchCoroutine != null) StopCoroutine(crouchCoroutine);
                crouchCoroutine = StartCoroutine(MoveSmoothly(Model.transform, targetPos, 0.3f));
            }

            IEnumerator MoveSmoothly(Transform target, Vector3 destination, float duration)
            {
                Vector3 start = target.position;
                float t = 0f;
                while (t < duration)
                {
                    t += Time.deltaTime;
                    target.position = Vector3.Lerp(start, destination, t / duration);
                    yield return null;
                }
                target.position = destination;
            }
        }
    }
}
