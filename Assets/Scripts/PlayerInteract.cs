using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class PlayerInteract : NetworkBehaviour
{
    [SerializeField]
    private KeyCode interactButton = KeyCode.E;
    [SerializeField]
    private LayerMask interactLayer;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private float raycastDistance;




    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(interactButton))
        {
            Interact();
        }

    }

    void Interact()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, raycastDistance, interactLayer))
        {
            InteractWithObjServer(hit.transform.gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void InteractWithObjServer(GameObject obj)
    {
        obj.GetComponent<Chest>().LootObject();
    }


}
