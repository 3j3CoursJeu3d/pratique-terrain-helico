using UnityEngine;

public class cameraSuiveillance : MonoBehaviour
{
    public Transform cible;

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(cible);
    }
}
