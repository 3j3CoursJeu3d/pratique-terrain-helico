using UnityEngine;

public class controlerDome : MonoBehaviour
{
    [Header("Références")]
    public string paramOuverture = "ouvertureDome"; // Nom du paramètre booléen dans l'Animator

    private Animator anim;
    private AudioSource audioSrc;

    void Awake()
    {
        // Récupère les composants Animator et AudioSource du même objet
        anim = GetComponent<Animator>();
        audioSrc = GetComponent<AudioSource>();

        // Définit l’état initial du dôme : fermé
        anim.SetBool(paramOuverture, false);

        // Configure le son pour ne pas se lancer automatiquement
        if (audioSrc != null)
        {
            audioSrc.playOnAwake = false;
            audioSrc.loop = false;
        }
    }

    void Update()
    {
        // Ne fait rien tant que la partie n’a pas commencé
        if (!DemarrerJeu.Demarre) return;

        // Ouvre le dôme avec la touche O
        if (Input.GetKeyDown(KeyCode.O))
            anim.SetBool(paramOuverture, true);

        // Ferme le dôme avec la touche F
        if (Input.GetKeyDown(KeyCode.F))
            anim.SetBool(paramOuverture, false);
    }

    // Appelée par un "Animation Event" au moment de l'ouverture ou fermeture du dôme
    public void JoueSon()
    {
        if (audioSrc == null || audioSrc.clip == null) return;

        // Joue le son d'ouverture/fermeture
        audioSrc.Stop();
        audioSrc.Play();
    }
}