using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;

public class CameraPlayer : NetworkBehaviour
{
    [Header("Base setup")]
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;


    float rotationX = 0;
    [SerializeField]
    private float cameraYOffset = 0.4f;
    public Camera playerCamera;

    Vector3 moveDirection = Vector3.zero;



    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z);
            playerCamera.transform.SetParent(transform);
        }
        else
        {
            gameObject.GetComponent<CameraPlayer>().enabled = false;
        }
    }

    void Start()
    {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {

        // We are grounded, so recalculate move direction based on axis
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);


        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * Input.GetAxis("Vertical")) + (right * Input.GetAxis("Horizontal"));


        
        if (playerCamera != null)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }
}
