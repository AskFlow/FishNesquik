using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class PlayerInteract : NetworkBehaviour
{
    [Header("Interact variable")]
    [SerializeField]
    private KeyCode interactButton = KeyCode.E;
    [SerializeField]
    private List<LayerMask> interactLayerList;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private float raycastDistance;

    [Header("Pickup variable")]
    [SerializeField]
    private KeyCode dropButton = KeyCode.A;
    [SerializeField]
    private Transform pickupPosition;


    PlayerController playerController;
    PlayerWeapon weapon;
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
        if(worldObjectHolder == null)
        {
            worldObjectHolder = transform;
        }
    }

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }


    private void Update()
    {
        if(objInHand != null)
        {
            playerController.canShoot = true;
        }
        else
        {
            playerController.canShoot = false;
        }
        if (Input.GetKeyDown(interactButton))
        {
            Interact();
        }
        if (Input.GetKeyDown(dropButton))
        {
            Drop();
        }
    }

    void Interact()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, raycastDistance))
        {
            string layerName = LayerMask.LayerToName(hit.transform.gameObject.layer);

            if(layerName == "Chest")
            {
                InteractWithChestServer(hit.transform.gameObject);
            }
            else if (layerName == "Pickup")
            {
                Pickup(hit);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void InteractWithChestServer(GameObject obj)
    {
        obj.GetComponent<Chest>().LootObject();
    }


    void Pickup(RaycastHit hit)
    {
        //l'arme que tu viens d'essayer de prendre
        if (hit.transform.gameObject.CompareTag("Weapon"))
        {
            weapon = hit.transform.gameObject.GetComponent<PlayerWeapon>();
            weapon.playerShoot = GetComponent<PlayerShoot>();
        }
        if (!hasObjectInHand)
        {
            weapon.SwitchWeapon();
            SetObjectInHandServer(hit.transform.gameObject, pickupPosition.position, pickupPosition.rotation, gameObject);
            objInHand = hit.transform.gameObject;
            hasObjectInHand = true;
        }
        else if (hasObjectInHand)
        {
            Drop();
            weapon.SwitchWeapon();
            SetObjectInHandServer(hit.transform.gameObject, pickupPosition.position, pickupPosition.rotation, gameObject);
            objInHand = hit.transform.gameObject;
            hasObjectInHand = true;

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

        if (obj.GetComponent<Rigidbody>() != null)
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

    [ServerRpc(RequireOwnership = false)]    void DropObjectServer(GameObject obj, Transform worldObjects)
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
