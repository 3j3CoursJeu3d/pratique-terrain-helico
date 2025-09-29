using UnityEngine;
using TMPro;

public class CompteurDescendant : MonoBehaviour
{
    [Header("Références")]
    public TextMeshProUGUI zoneTexteTemps;
    public controlerHelico controleurHelicoptere;

    [Header("Configuration minuterie")]
    public int valeurInitialeMinuteur = 120;

    private int tempsRestant;
    private bool compteurEnCours = false;

    void Awake()
    {
        ReinitialiserCompteur();
    }

    public void Demarrer()
    {
        if (compteurEnCours) return;

        compteurEnCours = true;
        ReinitialiserCompteur();
        InvokeRepeating(nameof(DecrémenterCompteur), 1f, 1f);
    }

    private void DecrémenterCompteur()
    {
        if (tempsRestant > 0)
        {
            tempsRestant--;
            AfficherTemps(tempsRestant);

            if (tempsRestant == 0)
            {
                CancelInvoke(nameof(DecrémenterCompteur));
                compteurEnCours = false;

                if (controleurHelicoptere != null)
                {
                    // ⇩⇩⇩ CHANGEMENT : panne sèche, pas explosion
                    controleurHelicoptere.DeclencherPanneEssence();
                }
            }
        }
    }

    private void ReinitialiserCompteur()
    {
        tempsRestant = Mathf.Max(0, valeurInitialeMinuteur);
        AfficherTemps(tempsRestant);
    }

    private void AfficherTemps(int valeur)
    {
        if (zoneTexteTemps != null)
            zoneTexteTemps.text = valeur.ToString();
    }
}