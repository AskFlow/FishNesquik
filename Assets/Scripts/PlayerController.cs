using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

//This is made by Bobsi Unity - Youtube
public class PlayerController : NetworkBehaviour
{
    [Header("Base setup")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    [SerializeField]
    private float cameraYOffset = 0.4f;
    private Camera playerCamera;

    [Header ("Grenades")]
    public FragGrenade grenadePrefab;
    public int maxGrenadeCount = 3;
    private int currentGrenadeCount;



    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            playerCamera = Camera.main;
            playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z);
            playerCamera.transform.SetParent(transform);
        }
        else
        {
            gameObject.GetComponent<PlayerController>().enabled = false;
        }
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Initialise nb of grenades
        currentGrenadeCount = maxGrenadeCount;
    }

    void Update()
    {
        bool isRunning = false;

        // Press Left Shift to run
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // We are grounded, so recalculate move direction based on axis
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
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
        if (Input.GetKeyDown(KeyCode.G) && currentGrenadeCount > 0)
        {
            ThrowGrenadeServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ThrowGrenadeServerRpc()
    {
        // Server side
        if (currentGrenadeCount <= 0)
            return;

        if (grenadePrefab != null)
        {
            Vector3 spawnPosition = transform.position + transform.forward * 6.0f;

            // Créer la grenade et diminuer son nombre
            FragGrenade grenadeInstance = Instantiate(grenadePrefab, spawnPosition, Quaternion.identity);
            currentGrenadeCount--;

            // Répliquer la grenade
            ServerManager.Spawn(grenadeInstance.gameObject);
        }
    }

    [Client]
    public void RechargeGrenades(int amount)
    {
        currentGrenadeCount = Mathf.Min(currentGrenadeCount + amount, maxGrenadeCount);
        // Debug.Log("Grenades rechargées. Nombre actuel de grenades : " + currentGrenadeCount);
    }
}