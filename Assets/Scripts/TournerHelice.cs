using UnityEngine;

public class TournerHelice : MonoBehaviour
{
    [Header("Configuration rotation")]
    public Vector3 vitesseRotation;       // Vitesse actuelle de rotation (par axe)
    public float vitesseMaxRotation;      // Limite de vitesse de rotation
    public float acceleration;            // Accélération vers la vitesse max

    [Header("Contrôle")]
    public bool enMarche;                 // Indique si l’hélice doit tourner

    [Header("Décélération")]
    public float deceleration = 50f;      // Vitesse de ralentissement quand on arrête l’hélice

    void Update()
    {
        // Si le moteur est actif → accélère la rotation
        if (enMarche)
        {
            if (vitesseRotation.y < vitesseMaxRotation)
            {
                vitesseRotation.y += acceleration * Time.deltaTime;
                vitesseRotation.y = Mathf.Min(vitesseRotation.y, vitesseMaxRotation);
            }
        }
        // Sinon → ralentit progressivement jusqu’à l’arrêt
        else
        {
            if (vitesseRotation.y > 0)
            {
                vitesseRotation.y -= deceleration * Time.deltaTime;
                vitesseRotation.y = Mathf.Max(vitesseRotation.y, 0f);
            }
        }

        // Fait tourner l’objet uniquement si la vitesse est positive
        if (vitesseRotation.y > 0)
        {
            transform.Rotate(vitesseRotation * Time.deltaTime, Space.Self);
        }
    }
}