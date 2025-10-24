using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationCamera : MonoBehaviour
{
    public float vitesseRotationSouris = 2.0f;    // Sensibilité de la rotation à la souris
    public float vitesseDeDeplacement = 0.5f;     // Vitesse de déplacement de la caméra

    private float rotationX = 0.0f;               // Rotation horizontale accumulée (axe Y)
    private float rotationY = 0.0f;               // Rotation verticale accumulée (axe X)

    void Update()
    {
        // Ne permet pas le mouvement avant le début du jeu
        if (!DemarrerJeu.Demarre) return;

        // === Rotation de la caméra avec la souris ===
        rotationX += Input.GetAxis("Mouse X") * vitesseRotationSouris; // mouvement gauche/droite
        rotationY += Input.GetAxis("Mouse Y") * vitesseRotationSouris; // mouvement haut/bas

        // Limite la rotation verticale pour éviter un retournement complet
        rotationY = Mathf.Clamp(rotationY, -90, 90);

        // Applique la rotation selon les axes de la souris
        transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

        // === Déplacement de la caméra avec les touche ou flèches ===
        transform.position += transform.forward * vitesseDeDeplacement * Input.GetAxis("Vertical");
        transform.position += transform.right * vitesseDeDeplacement * Input.GetAxis("Horizontal");
    }
}