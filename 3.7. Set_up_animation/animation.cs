using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerControls controls;
    private CharacterController characterController;
    private Animator animator;

    [Header("Move info")]
    [SerializeField] private float walkSpeed;

    [Header("Aim info")]
    [SerializeField] private LayerMask aimLayerMask;
    [SerializeField] private Transform aim;
    private Vector3 lookingDirection;

    public Vector3 movementDirection;
    private float verticalVelocity;

    private Vector2 moveInput;
    private Vector2 aimInput;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Character.Movement.performed += context => moveInput = context.ReadValue<Vector2>();
        controls.Character.Movement.canceled += context => moveInput = Vector2.zero;

        controls.Character.Aim.performed += context => aimInput = context.ReadValue<Vector2>();
        controls.Character.Aim.canceled += context => aimInput = Vector2.zero;

    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        ApplyMovement();
        AimTowardMouse();
        AnimatorControllers();
    }

    private void AnimatorControllers()
    {
        Vector3 normalizedMovement = movementDirection.normalized;

        float xVelocity = Vector3.Dot(normalizedMovement, transform.right);
        float zVelocity = Vector3.Dot(normalizedMovement, transform.forward);

        animator.SetFloat("xVelocity", xVelocity, .1f, Time.deltaTime);
        animator.SetFloat("zVelocity", zVelocity, .1f, Time.deltaTime);
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
            characterController.Move(movementDirection * Time.deltaTime * walkSpeed);
        }
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded == false)
        {
            //verticalVelocity = verticalVelocity - 9.8f * Time.deltaTime;
            verticalVelocity -= 9.8f * Time.deltaTime;
            movementDirection.y = verticalVelocity;
        }
        else
            verticalVelocity = -.5f;
    }


    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}
