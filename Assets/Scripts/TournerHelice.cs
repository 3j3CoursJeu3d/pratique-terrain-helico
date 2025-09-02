using UnityEngine;

public class TournerHelice : MonoBehaviour
{
    public Vector3 vitesseRotation;
    public float vitesseMaxRotation;
    public float acceleration;
    public bool enMarche;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            enMarche = !enMarche;
        }

        if (enMarche)
        {
            if(vitesseRotation.y < vitesseMaxRotation)
            {
                vitesseRotation.y += acceleration;

            }
            transform.Rotate(vitesseRotation, Space.Self);
        }
    }
}
