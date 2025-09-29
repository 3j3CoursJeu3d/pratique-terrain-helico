using UnityEngine;

public class TournerHelice : MonoBehaviour
{
    [Header("Configuration rotation")]
    public Vector3 vitesseRotation;
    public float vitesseMaxRotation;
    public float acceleration;

    [Header("Contrôle")]
    public bool enMarche;

    [Header("Décélération")]
    public float deceleration = 50f;

    void Update()
    {
        if (enMarche)
        {
            if (vitesseRotation.y < vitesseMaxRotation)
            {
                vitesseRotation.y += acceleration * Time.deltaTime;
                vitesseRotation.y = Mathf.Min(vitesseRotation.y, vitesseMaxRotation);
            }
        }
        else
        {
            if (vitesseRotation.y > 0)
            {
                vitesseRotation.y -= deceleration * Time.deltaTime;
                vitesseRotation.y = Mathf.Max(vitesseRotation.y, 0f);
            }
        }

        if (vitesseRotation.y > 0)
        {
            transform.Rotate(vitesseRotation * Time.deltaTime, Space.Self);
        }
    }
}