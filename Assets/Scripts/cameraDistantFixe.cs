using UnityEngine;

public class cameraDistantFixe : MonoBehaviour
{
    public Transform cible;
    public Vector3 distance;

    // Update is called once per frame
    void Update()
    {
        transform.position = cible.position + distance;
        transform.LookAt(cible);
    }
}
