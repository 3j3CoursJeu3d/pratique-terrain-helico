using UnityEngine;

public class DemarrerJeu : MonoBehaviour
{
    // Indique si la partie est en cours (accessible depuis d'autres scripts)
    public static bool Demarre { get; private set; } = false;

    [Header("Objets à gérer au démarrage")]
    public GameObject[] objetAActiver;     // Objets visibles après le lancement
    public GameObject[] objetADesactiver;  // Objets masqués après le lancement

    [Header("Scripts de contrôle du joueur")]
    public MonoBehaviour[] scriptsDeControle; // Scripts à activer une fois le jeu lancé

    [Header("Références de gameplay")]
    public controlerHelico controleurHelicoptere;
    public CompteurDescendant compteurTemps;

    // Garde en mémoire si une partie a déjà été commencée
    private static bool partieDejaCommencee = false;

#if UNITY_EDITOR
    // Réinitialise les états quand on quitte le mode Play dans l’éditeur
    static DemarrerJeu()
    {
        UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
    {
        if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
        {
            partieDejaCommencee = false;
            Demarre = false;
        }
    }
#endif

    void Start()
    {
        // Si une partie est déjà en cours, on relance directement le jeu
        if (partieDejaCommencee)
        {
            CacherMenuDirectement();
            ActiverScriptsDeControle(true);
            Demarre = true;
        }
        else
        {
            // Sinon, on affiche le menu principal
            AfficherMenu();
            ActiverScriptsDeControle(false);
            Demarre = false;
        }
    }

    // Appelé par le bouton "Démarrer" dans le menu
    public void PartirJeu()
    {
        partieDejaCommencee = true;
        CacherMenuEtDemarrer();
        ActiverScriptsDeControle(true);
        Demarre = true;
    }

    // Active ou désactive tous les scripts de contrôle
    private void ActiverScriptsDeControle(bool actif)
    {
        if (scriptsDeControle == null) return;

        foreach (var s in scriptsDeControle)
            if (s) s.enabled = actif;
    }

    // Applique directement l’état "jeu en cours" (sans animation/menu)
    private void CacherMenuDirectement()
    {
        foreach (var objet in objetAActiver)
            if (objet) objet.SetActive(true);

        foreach (var objet in objetADesactiver)
            if (objet) objet.SetActive(false);
    }

    // Cache le menu et démarre le jeu complet
    private void CacherMenuEtDemarrer()
    {
        foreach (var objet in objetAActiver)
            if (objet) objet.SetActive(true);

        foreach (var objet in objetADesactiver)
            if (objet) objet.SetActive(false);

        // Lance les systèmes principaux
        if (controleurHelicoptere != null)
            controleurHelicoptere.DemarrerPartie();

        if (compteurTemps != null)
            compteurTemps.Demarrer();
    }

    // Affiche le menu de départ (désactive le jeu)
    private void AfficherMenu()
    {
        foreach (var objet in objetADesactiver)
            if (objet) objet.SetActive(true);

        foreach (var objet in objetAActiver)
            if (objet) objet.SetActive(false);
    }
}