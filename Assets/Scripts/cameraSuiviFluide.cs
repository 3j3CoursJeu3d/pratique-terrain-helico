using UnityEngine;

public class cameraSuiviFluide : MonoBehaviour
{
    public GameObject cible;
    public Vector3 distance;

    public float amortissement;

    // Update is called once per frame
    void Update()
    {
        Vector3 positionFin = cible.transform.TransformPoint(distance);
        transform.position = Vector3.Lerp(transform.position, positionFin, amortissement);
        transform.LookAt(cible.transform);

    }
}
