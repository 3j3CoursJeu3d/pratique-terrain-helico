using UnityEngine;

public class gererCamera : MonoBehaviour
{
    public GameObject Cam1;
    public GameObject Cam2;
    public GameObject Cam3;
    public GameObject Cam4;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Cam1.SetActive(true);
            Cam1.SetActive(false);
            Cam1.SetActive(false);
            Cam1.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Cam2.SetActive(false);
            Cam2.SetActive(false);
            Cam2.SetActive(false);
            Cam2.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Cam3.SetActive(false);
            Cam3.SetActive(false);
            Cam3.SetActive(false);
            Cam3.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Cam4.SetActive(false);
            Cam4.SetActive(false);
            Cam4.SetActive(false);
            Cam4.SetActive(false);
        }
    }
}
