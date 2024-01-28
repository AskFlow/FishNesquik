using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;
    public static GameManager Instance => instance;

    public string sceneToLoad;

    public NetworkObject playerManager;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this.gameObject);


    }

    public void StartGame()
    {
        SceneLoadData sceneLoad = new SceneLoadData(sceneToLoad);
        sceneLoad.MovedNetworkObjects = new NetworkObject[] { playerManager };
        sceneLoad.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.SceneManager.LoadGlobalScenes(sceneLoad);
        //playerManager.GetComponent<PlayerManager>().SpawnPlayer();


    }
}
