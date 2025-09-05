using UnityEngine;

public class cameraDistant : MonoBehaviour
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
