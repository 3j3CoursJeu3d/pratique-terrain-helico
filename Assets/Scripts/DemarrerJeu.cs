using UnityEngine;

public class DemarrerJeu : MonoBehaviour
{
    [Header("Objets à gérer au démarrage")]
    public GameObject[] objetAActiver;
    public GameObject[] objetADesactiver;

    [Header("Références des systèmes")]
    public controlerHelico controleurHelicoptere;
    public CompteurDescendant compteurTemps;

    // Variable statique qui persiste entre les rechargements
    private static bool partieDejaCommencee = false;

#if UNITY_EDITOR
    // CHANGEMENT CRUCIAL : On utilise playModeStateChanged au lieu de RuntimeInitializeOnLoadMethod
    static DemarrerJeu()
    {
        UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
    {
        // Reset UNIQUEMENT quand on sort du mode Play
        if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
        {
            partieDejaCommencee = false;
            Debug.Log("Arrêt Unity détecté - Menu réapparaîtra au prochain lancement");
        }
    }
#endif

    void Start()
    {
        Debug.Log($"DemarrerJeu Start - partieDejaCommencee = {partieDejaCommencee}");

        if (partieDejaCommencee)
        {
            // Cache le menu après relance
            Debug.Log("Partie déjà commencée - Menu caché");
            CacherMenuDirectement();
        }
        else
        {
            // Premier lancement - affiche le menu
            Debug.Log("Nouvelle partie - Menu affiché");
            AfficherMenu();
        }
    }

    public void PartirJeu()
    {
        Debug.Log("Bouton Démarrer cliqué!");
        partieDejaCommencee = true;
        CacherMenuEtDemarrer();
    }

    private void CacherMenuDirectement()
    {
        // Cache le menu sans redémarrer les systèmes (ils le sont déjà)
        foreach (var objet in objetAActiver)
            if (objet) objet.SetActive(true);

        foreach (var objet in objetADesactiver)
            if (objet) objet.SetActive(false);
    }

    private void CacherMenuEtDemarrer()
    {
        foreach (var objet in objetAActiver)
            if (objet) objet.SetActive(true);

        foreach (var objet in objetADesactiver)
            if (objet) objet.SetActive(false);

        if (controleurHelicoptere != null)
            controleurHelicoptere.DemarrerPartie();

        if (compteurTemps != null)
            compteurTemps.Demarrer();
    }

    private void AfficherMenu()
    {
        foreach (var objet in objetADesactiver)
            if (objet) objet.SetActive(true);

        foreach (var objet in objetAActiver)
            if (objet) objet.SetActive(false);
    }
}