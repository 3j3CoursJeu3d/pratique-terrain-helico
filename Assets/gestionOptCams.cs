using UnityEngine;

public class gestionOptCams : MonoBehaviour
{
    public GameObject[] lesCams;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        activeCam(3);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            activeCam(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            activeCam(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            activeCam(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            activeCam(3);
        }
    }


    void activeCam(int inDexCam)
    {
        foreach(GameObject laCam in lesCams)
        {
            laCam.SetActive(false);
        }

        lesCams[inDexCam].SetActive(true);
    }
}
