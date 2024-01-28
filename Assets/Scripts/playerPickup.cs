using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using System;

public class playerPickup : NetworkBehaviour
{
    [SerializeField]
    private KeyCode pickupButton = KeyCode.E;
    [SerializeField]
    private KeyCode dropButton = KeyCode.A;
    [SerializeField]
    private float raycastDistance;
    [SerializeField]
    private LayerMask pickupLayer;
    [SerializeField]
    private Transform pickupPosition;
    [SerializeField]
    private Camera cam;

    bool hasObjectInHand;
    GameObject objInHand;
    Transform worldObjectHolder;


    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            enabled = false;
        }

        worldObjectHolder = GameObject.FindGameObjectWithTag("WorldObjects").transform;

    }

    private void Update()
    {
        if (Input.GetKeyDown(pickupButton))
        {
            Pickup();
        }

        if (Input.GetKeyDown(dropButton))
        {
            Drop();
        }
    }

    void Pickup()
    {
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, raycastDistance, pickupLayer))
        {
            if (!hasObjectInHand)
            {
                SetObjectInHandServer(hit.transform.gameObject, pickupPosition.position, pickupPosition.rotation, gameObject);
                objInHand = hit.transform.gameObject;
                hasObjectInHand = true;
            }
            else if (hasObjectInHand)
            {
                Drop();

                SetObjectInHandServer(hit.transform.gameObject, pickupPosition.position, pickupPosition.rotation, gameObject);
                objInHand = hit.transform.gameObject;
                hasObjectInHand = true;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SetObjectInHandServer(GameObject obj, Vector3 position, Quaternion rotation, GameObject player)
    {
        SetObjectInHandObserver(obj, position, rotation, player);
    }

    [ObserversRpc]
    void SetObjectInHandObserver(GameObject obj, Vector3 position, Quaternion rotation, GameObject player)
    {
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.parent = player.transform;

        if(obj.GetComponent<Rigidbody>() != null)
        {
            obj.GetComponent<Rigidbody>().isKinematic = true;
        }
    }


    private void Drop()
    {
        if (!hasObjectInHand)
            return;

        DropObjectServer(objInHand, worldObjectHolder);
        hasObjectInHand = false;
        objInHand = null;
    }

    [ServerRpc(RequireOwnership = false)]
    void DropObjectServer(GameObject obj, Transform worldObjects)
    {
        DropObjectObserver(obj, worldObjects);
    }

    [ObserversRpc]
    void DropObjectObserver(GameObject obj, Transform worldObjects)
    {
        obj.transform.parent = worldObjects;

        if (obj.GetComponent<Rigidbody>() != null)
        {
            obj.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

}
