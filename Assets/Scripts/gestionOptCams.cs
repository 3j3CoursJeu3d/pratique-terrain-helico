using UnityEngine;

public class gestionOptCams : MonoBehaviour
{
    public GameObject[] lesCams; // Liste des caméras disponibles dans la scène

    void Start()
    {
        // Active la caméra numéro 3 au lancement du jeu
        activeCam(3);
    }

    void Update()
    {
        // Ne fait rien tant que la partie n’a pas démarré
        if (!DemarrerJeu.Demarre) return;

        // Permet de changer de caméra avec les touches 1 à 4
        if (Input.GetKeyDown(KeyCode.Alpha1))
            activeCam(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            activeCam(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            activeCam(2);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            activeCam(3);
    }

    // Active la caméra demandée et désactive toutes les autres
    void activeCam(int indexCam)
    {
        foreach (GameObject laCam in lesCams)
            laCam.SetActive(false);

        lesCams[indexCam].SetActive(true);
    }
}