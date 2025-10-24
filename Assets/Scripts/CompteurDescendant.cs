using UnityEngine;
using TMPro;

public class CompteurDescendant : MonoBehaviour
{
    [Header("Références")]
    public TextMeshProUGUI zoneTexteTemps;   // Zone d’affichage du temps à l’écran
    public controlerHelico controleurHelicoptere; // Référence pour signaler la fin du temps

    [Header("Configuration du minuteur")]
    public int valeurInitialeMinuteur = 120; // Temps de départ en secondes

    private int tempsRestant;
    private bool compteurEnCours = false;

    void Awake()
    {
        // Prépare le compteur dès le chargement
        ReinitialiserCompteur();
    }

    // Lance le compte à rebours si pas déjà en cours
    public void Demarrer()
    {
        if (compteurEnCours) return;

        compteurEnCours = true;
        ReinitialiserCompteur();
        InvokeRepeating(nameof(DecrementerCompteur), 1f, 1f); // Appelle chaque seconde
    }

    // Diminue le temps restant et déclenche la panne quand il atteint 0
    private void DecrementerCompteur()
    {
        if (tempsRestant <= 0) return;

        tempsRestant--;
        AfficherTemps(tempsRestant);

        if (tempsRestant == 0)
        {
            CancelInvoke(nameof(DecrementerCompteur));
            compteurEnCours = false;

            if (controleurHelicoptere != null)
                controleurHelicoptere.DeclencherPanneEssence(); // Fin du temps → panne sèche
        }
    }

    // Remet le minuteur à sa valeur initiale
    private void ReinitialiserCompteur()
    {
        tempsRestant = Mathf.Max(0, valeurInitialeMinuteur);
        AfficherTemps(tempsRestant);
    }

    // Met à jour le texte à l’écran
    private void AfficherTemps(int valeur)
    {
        if (zoneTexteTemps != null)
            zoneTexteTemps.text = valeur.ToString();
    }
}