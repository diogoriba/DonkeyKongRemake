using UnityEngine;
using System.Collections;

public class VictoryManager : MonoBehaviour {
    public static VictoryManager instance;
    public GameObject victoryScreen;

    public void Win()
    {
        victoryScreen.SetActive(true);
        Invoke("Restart", 10f);
    }

	// Use this for initialization
	void Start () {
        victoryScreen.SetActive(false);
        instance = this;
	}

    public void Restart()
    {
        Application.LoadLevel("Connect");
    }
}
