using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using static PlayerWeapon;

//This is made by Bobsi Unity - Youtube
public class PlayerController : NetworkBehaviour
{
    [Header("Base setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    private PlayerGrenade playerGrenade;
    private CharacterController characterController;
    private PlayerShoot playerShoot;
    private PlayerWeapon playerWeapon;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    [SerializeField]
    public bool canShoot = false;

    [SerializeField]
    private float cameraYOffset = 0.4f;
    public Camera playerCamera;


    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            playerCamera.gameObject.SetActive(false);
        }

        if (base.IsOwner)
        {
            if(playerCamera == null)
                playerCamera = gameObject.GetComponent<Camera>();
            playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z);
            playerCamera.transform.SetParent(transform);
        }

    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerGrenade = GetComponent<PlayerGrenade>();
        playerShoot = GetComponent<PlayerShoot>();
        playerWeapon = GetComponent<PlayerWeapon>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!IsOwner)
            return;

        bool isRunning = false;
        moveDirection = Vector3.zero;
        // Press Left Shift to run
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // We are grounded, so recalculate move direction based on axis
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove && playerCamera != null)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        // throw grenade
        if (Input.GetKeyDown(KeyCode.G) && playerGrenade.getCurrentGrenadeCount() > 0)
        {
            playerGrenade.ThrowGrenadeServerRpc();
        }

        // shoot
        if (Input.GetKeyDown(KeyCode.Mouse0) && canShoot)
        {
            playerShoot.TryShoot();
        }


    }
}