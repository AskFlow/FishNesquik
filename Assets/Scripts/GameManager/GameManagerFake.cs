using UnityEngine;
using System.Collections.Generic;

public class GameManagerFake : MonoBehaviour
{
    public static GameManagerFake Instance;
    private List<PlayerController> players = new List<PlayerController>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void GetAllPlayers()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerObject in playerObjects)
        {
            PlayerController playerController = playerObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                players.Add(playerController);
            }
        }
    }

    private bool AreAllPlayersDead()
    {
        foreach (PlayerController player in players)
        {
            if (!player.isDead)
            {
                return false;
            }
        }
        return true;
    }

    private void Update()
    {
        if (AreAllPlayersDead())
        {
            Defeat();
        }
    }

    public void Victory()
    {
        Debug.Log("Victory!");
    }


    public void Defeat()
    {
        Debug.Log("Defeat!");
    }
}
