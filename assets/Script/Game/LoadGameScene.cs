using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadGameScene : MonoBehaviour
{
    void Start()
    {
        NetworkManager.Singleton.Shutdown();
        List<GameObject> netObjects = 
            FindObjectsOfType<NetworkObject>().Select(obj => obj.transform.gameObject).ToList();

        foreach (var obj in netObjects)
        {
            Destroy(obj);
        }

        GameObject startGameARObject = FindObjectOfType<StartGameAR>().gameObject;
        Destroy(startGameARObject);

        Destroy(FindObjectOfType<NetworkManager>().transform.gameObject);
        SceneManager.LoadScene("miniGame", LoadSceneMode.Single);
    }
}
