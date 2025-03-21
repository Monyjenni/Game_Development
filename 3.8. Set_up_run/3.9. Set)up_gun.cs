using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerControls controls;
    private CharacterController characterController;
    private Animator animator;

    [Header("Movement info")]
    private float speed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    public Vector3 movementDirection;
    private float verticalVelocity;
    private bool isRunning;

    [Header("Aim info")]
    [SerializeField] private LayerMask aimLayerMask;
    [SerializeField] private Transform aim;
    private Vector3 lookingDirection;

    private Vector2 moveInput;
    private Vector2 aimInput;

    private void Awake()
    {
        AssignInputEvents();

    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        speed = walkSpeed;
    }

    private void Update()
    {
        ApplyMovement();
        AimTowardMouse();
        AnimatorControllers();
    }

    private void Shoot()
    {
        animator.SetTrigger("Fire");
    }

    private void AnimatorControllers()
    {
        Vector3 normalizedMovement = movementDirection.normalized;

        float xVelocity = Vector3.Dot(normalizedMovement, transform.right);
        float zVelocity = Vector3.Dot(normalizedMovement, transform.forward);

        animator.SetFloat("xVelocity", xVelocity, .1f, Time.deltaTime);
        animator.SetFloat("zVelocity", zVelocity, .1f, Time.deltaTime);
        bool playRunAnimation = isRunning && movementDirection.magnitude > 0;
        animator.SetBool("isRunning", playRunAnimation);
    }


    private void AimTowardMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(aimInput);

        if(Physics.Raycast(ray, out var hitInfo,Mathf.Infinity,aimLayerMask)) 
        {
            lookingDirection = hitInfo.point - transform.position;
            lookingDirection.y = 0f;
            lookingDirection.Normalize();

            transform.forward = lookingDirection;

            aim.position = new Vector3(hitInfo.point.x, transform.position.y, hitInfo.point.z);
        }
    }

    private void ApplyMovement()
    {
        movementDirection = new Vector3(moveInput.x, 0, moveInput.y);
        ApplyGravity();

        if (movementDirection.magnitude > 0)
        {
            characterController.Move(movementDirection * Time.deltaTime * speed);
        }
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded == false)
        {
            verticalVelocity -= 9.8f * Time.deltaTime;
            movementDirection.y = verticalVelocity;
        }
        else
            verticalVelocity = -.5f;
    }

#region New Input System
    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
    private void AssignInputEvents()
    {
        controls = new PlayerControls();

        controls.Character.Fire.performed += context => Shoot();

        controls.Character.Movement.performed += context => moveInput = context.ReadValue<Vector2>();
        controls.Character.Movement.canceled += context => moveInput = Vector2.zero;

        controls.Character.Aim.performed += context => aimInput = context.ReadValue<Vector2>();
        controls.Character.Aim.canceled += context => aimInput = Vector2.zero;

        controls.Character.Run.performed += context =>
        {
            speed = runSpeed;
            isRunning = true;
        };
        controls.Character.Run.canceled += context =>
        {
            speed = walkSpeed;
            isRunning = false;
        };
    }
#endregion 
}
