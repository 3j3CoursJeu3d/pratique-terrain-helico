using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class controlerHelico : MonoBehaviour
{
    [Header("Mouvements")]
    public float vitesseTourne;             // vitesse de rotation gauche/droite
    public float vitesseMonte;              // vitesse de montée/descente
    private float vitesseAvance;            // vitesse vers l'avant
    public float vitesseAvanceMax;          // limite de vitesse avant
    public float forceAcceleration;         // force d'accélération

    public GameObject uneHelice;            // référence visuelle pour la rotation

    [Header("Références objets")]
    public GameObject refHeliceAvant;       // lien vers hélice avant
    public GameObject refHeliceArriere;     // lien vers hélice arrière
    public GameObject gestionCameras;       // gestion post-explosion
    public GameObject objetMusiqueAmbiance; // musique du jeu
    public AudioClip sonBidon;              // son lors du ravitaillement
    public GameObject explosion;            // effet d'explosion

    [Header("UI / Audio")]
    public TextMeshProUGUI affichageInfo;   // texte d'information
    public Image barreEssence;              // jauge d'essence

    [Header("Essence")]
    public float essenceActuelle;           // quantité d'essence actuelle
    public float essenceMax;                // capacité maximale
    public float consoParSeconde = 0.2f;    // consommation d’essence

    [Header("Relance après panne")]
    public float delaiRelanceApresPanneEssence = 2f; // délai avant relance automatique

    [Header("Paramètres de chute")]
    public float dragChute = 0.0f;          // résistance linéaire
    public float angularDragChute = 0.05f;  // résistance angulaire
    public float masseLourde = 1000f;       // masse lors de la panne

    [Header("Inclinaison visuelle")]
    public float angleInclinaisonMax = 15f; // angle visuel max
    public float vitesseLerpInclinaison = 6f;

    [Header("Caméra de suivi fluide")]
    public Transform cameraSuivi;           // référence caméra
    public float facteurInclinaisonCam = 0.5f;

    [Header("Audio Hélice")]
    public float vitesseRotationMax = 1000f; // vitesse max simulée des pales
    public float fadeVitesse = 2.5f;         // vitesse de transition audio
    public float pitchMin = 0.5f;
    public float pitchMax = 1.0f;

    // États du jeu
    public bool finJeu;
    private bool partieCommencee = false;
    private bool helicoptereActif = false;
    private bool auSol = false;
    private bool panneEssence = false;
    private bool relancePartiePlanifiee = false;

    // Composants internes
    private Rigidbody rigidHelico;
    private AudioSource sonHelico;

    // Variables internes
    private float essencePrecedente;
    private float vitesseZ = 0f;
    private bool palesEnMarchePrev = false;
    private float rotorFactor = 0f;

    // Sauvegardes pour restaurer après une panne
    private float masseInitiale;
    private float linearDampingInitial;
    private float angularDampingInitial;
    private RigidbodyConstraints contraintesInitiales;
    private bool masseBoostee = false;

    void Start()
    {
        // Récupère les composants du Rigidbody et AudioSource
        rigidHelico = GetComponent<Rigidbody>();
        sonHelico = GetComponent<AudioSource>();

        // Sauvegarde les réglages d'origine pour réinitialiser après une panne
        masseInitiale = rigidHelico.mass;
        linearDampingInitial = rigidHelico.linearDamping;
        angularDampingInitial = rigidHelico.angularDamping;
        contraintesInitiales = rigidHelico.constraints;

        // Initialise l'essence
        essenceActuelle = essenceMax;
        essencePrecedente = essenceActuelle;

        // Désactive la musique tant que le jeu n’a pas commencé
        if (objetMusiqueAmbiance != null)
            objetMusiqueAmbiance.SetActive(false);

        // Prépare le son de l’hélico
        if (sonHelico)
        {
            sonHelico.volume = 0f;
            sonHelico.pitch = pitchMin;
            if (sonHelico.isPlaying) sonHelico.Stop();
        }
    }

    void Update()
    {
        // Ignore les entrées si la partie n’est pas démarrée
        if (!DemarrerJeu.Demarre) return;

        // Permet de couper ou réactiver le son global
        if (Input.GetKeyDown(KeyCode.M))
            AudioListener.pause = !AudioListener.pause;

        // Démarrage de l'hélicoptère
        if (partieCommencee && !helicoptereActif && !finJeu)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
                ActiverHelicoptere();
        }
    }

    void FixedUpdate()
    {
        // Vérifie la panne d’essence
        if (essenceActuelle <= 0f)
            panneEssence = true;

        // Récupère les hélices
        var heliceAvant = refHeliceAvant ? refHeliceAvant.GetComponent<TournerHelice>() : null;
        var heliceArriere = refHeliceArriere ? refHeliceArriere.GetComponent<TournerHelice>() : null;
        bool palesEnMarche = (heliceAvant && heliceAvant.enMarche) || (heliceArriere && heliceArriere.enMarche);

        // Gestion de la panne d’essence
        if (panneEssence)
        {
            if (heliceAvant) heliceAvant.enMarche = false;
            if (heliceArriere) heliceArriere.enMarche = false;

            rigidHelico.useGravity = true;
            vitesseAvance = 0f;

            // Rend l’hélico plus lourd et instable
            if (!masseBoostee)
            {
                rigidHelico.mass = masseLourde;
                rigidHelico.linearDamping = dragChute;
                rigidHelico.angularDamping = angularDragChute;
                rigidHelico.constraints = RigidbodyConstraints.None;
                masseBoostee = true;
            }

            // Couper progressivement le son
            if (sonHelico)
            {
                sonHelico.volume = Mathf.MoveTowards(sonHelico.volume, 0f, fadeVitesse * Time.fixedDeltaTime);
                sonHelico.pitch = Mathf.MoveTowards(sonHelico.pitch, pitchMin, fadeVitesse * Time.fixedDeltaTime);
                if (sonHelico.isPlaying && sonHelico.volume <= 0.001f)
                    sonHelico.Stop();
            }

            // Planifie une relance automatique après la chute
            if (auSol && !relancePartiePlanifiee)
            {
                relancePartiePlanifiee = true;
                Invoke(nameof(RelancerPartie), delaiRelanceApresPanneEssence);
            }

            if (affichageInfo != null)
                affichageInfo.text = "Panne sèche !";

            essencePrecedente = essenceActuelle;
            palesEnMarchePrev = false;
            return;
        }

        // Si le jeu est en pause ou non démarré
        if (!partieCommencee || !helicoptereActif || finJeu)
        {
            if (affichageInfo != null && partieCommencee)
                affichageInfo.text = helicoptereActif ? "Hélico actif" : "Appuyez sur Entrée";

            // Fade out du son
            if (sonHelico && sonHelico.isPlaying && !palesEnMarche)
            {
                sonHelico.volume = Mathf.MoveTowards(sonHelico.volume, 0f, fadeVitesse * Time.fixedDeltaTime);
                sonHelico.pitch = Mathf.MoveTowards(sonHelico.pitch, pitchMin, fadeVitesse * Time.fixedDeltaTime);
                if (sonHelico.volume <= 0.001f) sonHelico.Stop();
            }

            palesEnMarchePrev = palesEnMarche;
            return;
        }

        // Vol actif
        if (palesEnMarche)
        {
            // Démarre le son au moment où les pales s’activent
            if (sonHelico && !palesEnMarchePrev)
            {
                sonHelico.volume = 0f;
                sonHelico.pitch = pitchMin;
                sonHelico.Play();
            }

            // Consomme de l’essence
            essenceActuelle -= consoParSeconde * Time.fixedDeltaTime;
            essenceActuelle = Mathf.Max(0f, essenceActuelle);
            if (barreEssence) barreEssence.fillAmount = essenceActuelle / essenceMax;

            rigidHelico.useGravity = false;

            // Contrôles de vol
            float forceRotation = Input.GetAxis("Horizontal") * vitesseTourne;
            float forceMonte = Input.GetAxis("Vertical") * vitesseMonte;

            if (Input.GetKey(KeyCode.E) && vitesseAvance < vitesseAvanceMax)
                vitesseAvance += forceAcceleration;

            if (Input.GetKey(KeyCode.Q) && vitesseAvance > 0f)
                vitesseAvance -= forceAcceleration;

            rigidHelico.AddRelativeTorque(0f, forceRotation, 0f);
            rigidHelico.AddRelativeForce(0f, forceMonte, vitesseAvance);

            // Ajuste le son selon la vitesse de rotation des hélices
            float vitesseRot = LireVitesseRotation(heliceAvant, heliceArriere);
            float speed01 = vitesseRot >= 0f
                ? Mathf.InverseLerp(0f, Mathf.Max(0.0001f, vitesseRotationMax), vitesseRot)
                : Mathf.MoveTowards(rotorFactor, 1f, 1.25f * Time.fixedDeltaTime);

            float targetVol = Mathf.Clamp01(speed01);
            float targetPitch = Mathf.Lerp(pitchMin, pitchMax, speed01);

            if (sonHelico)
            {
                sonHelico.volume = Mathf.MoveTowards(sonHelico.volume, targetVol, fadeVitesse * Time.fixedDeltaTime);
                sonHelico.pitch = Mathf.MoveTowards(sonHelico.pitch, targetPitch, fadeVitesse * Time.fixedDeltaTime);
            }

            // Inclinaison visuelle lors des virages
            float inputHoriz = Input.GetAxis("Horizontal");
            float angleZCible = -inputHoriz * angleInclinaisonMax;

            float angleZActuel = transform.localEulerAngles.z;
            if (angleZActuel > 180f) angleZActuel -= 360f;
            angleZActuel = Mathf.SmoothDampAngle(angleZActuel, angleZCible, ref vitesseZ, 1f);

            transform.localEulerAngles = new Vector3(
                0f,
                transform.localEulerAngles.y,
                angleZActuel
            );

            // Fait légèrement suivre la caméra
            if (cameraSuivi != null)
            {
                float angleCam = angleZActuel * facteurInclinaisonCam;
                Vector3 rotCam = cameraSuivi.localEulerAngles;
                if (rotCam.z > 180f) rotCam.z -= 360f;
                rotCam.z = Mathf.Lerp(rotCam.z, angleCam, Time.fixedDeltaTime * vitesseLerpInclinaison * 0.5f);
                cameraSuivi.localEulerAngles = rotCam;
            }
        }
        else
        {
            // Chute sans moteur
            rigidHelico.useGravity = true;
            rotorFactor = Mathf.MoveTowards(rotorFactor, 0f, 1.25f * Time.fixedDeltaTime);

            if (sonHelico && sonHelico.isPlaying)
            {
                sonHelico.volume = Mathf.MoveTowards(sonHelico.volume, 0f, fadeVitesse * Time.fixedDeltaTime);
                sonHelico.pitch = Mathf.MoveTowards(sonHelico.pitch, pitchMin, fadeVitesse * Time.fixedDeltaTime);
                if (sonHelico.volume <= 0.001f) sonHelico.Stop();
            }
        }

        // Affiche la vitesse à l’écran
        if (affichageInfo != null)
            affichageInfo.text = rigidHelico.linearVelocity.magnitude.ToString("0.0");

        essencePrecedente = essenceActuelle;
        palesEnMarchePrev = palesEnMarche;
    }

    // Lecture de la vitesse de rotation des pales (si exposée par TournerHelice)
    float LireVitesseRotation(TournerHelice heliceAvant, TournerHelice heliceArriere)
    {
        var helice = heliceAvant ? heliceAvant : heliceArriere;
        if (helice == null) return -1f;
        return -1f; // à adapter selon le script TournerHelice
    }

    private void OnCollisionStay(Collision infosCollision)
    {
        if (infosCollision.gameObject.tag == "décor")
            auSol = true;
    }

    private void OnCollisionExit(Collision infosCollision)
    {
        if (infosCollision.gameObject.tag == "décor")
            auSol = false;
    }

    private void OnTriggerEnter(Collider infoCollision)
    {
        if (infoCollision.gameObject.tag == "bidon")
        {
            bool dejaPlein = essenceActuelle >= essenceMax - 0.0001f;

            // Remplissage d’essence
            if (!dejaPlein)
            {
                essenceActuelle = essenceMax;
                if (barreEssence) barreEssence.fillAmount = 1f;
                if (sonHelico && sonBidon) sonHelico.PlayOneShot(sonBidon);

                // Si on était en panne, restaure les paramètres physiques
                if (panneEssence)
                {
                    panneEssence = false;
                    relancePartiePlanifiee = false;

                    rigidHelico.mass = masseInitiale;
                    rigidHelico.linearDamping = linearDampingInitial;
                    rigidHelico.angularDamping = angularDampingInitial;
                    rigidHelico.constraints = contraintesInitiales;
                    masseBoostee = false;
                }
            }
            Destroy(infoCollision.gameObject);
        }
    }

    private void OnCollisionEnter(Collision infosCollision)
    {
        float vitesseDeplacement = rigidHelico.linearVelocity.magnitude;

        // Explosion si impact violent avec le décor
        if (infosCollision.gameObject.tag == "décor" && vitesseDeplacement > 0.25f && !finJeu && helicoptereActif)
            ExploserHelico();
    }

    private void ActiverHelicoptere()
    {
        if (helicoptereActif) return;

        helicoptereActif = true;

        var heliceAvant = refHeliceAvant ? refHeliceAvant.GetComponent<TournerHelice>() : null;
        var heliceArriere = refHeliceArriere ? refHeliceArriere.GetComponent<TournerHelice>() : null;
        if (heliceAvant) heliceAvant.enMarche = true;
        if (heliceArriere) heliceArriere.enMarche = true;
    }

    public void DemarrerPartie()
    {
        partieCommencee = true;
        if (objetMusiqueAmbiance != null)
            objetMusiqueAmbiance.SetActive(true);
    }

    public void remiseZero()
    {
        vitesseAvance = 0f;
    }

    void RelancerPartie()
    {
        Scene maSceneActuelle = SceneManager.GetActiveScene();
        SceneManager.LoadScene(maSceneActuelle.name);
    }

    public void DeclencherPanneEssence()
    {
        essenceActuelle = 0f;
    }

    // Déclenche une explosion visuelle et sonore
    public void ExploserHelico()
    {
        if (finJeu) return;
        finJeu = true;

        var heliceAvant = refHeliceAvant ? refHeliceAvant.GetComponent<TournerHelice>() : null;
        var heliceArriere = refHeliceArriere ? refHeliceArriere.GetComponent<TournerHelice>() : null;
        if (heliceAvant) heliceAvant.enMarche = false;
        if (heliceArriere) heliceArriere.enMarche = false;

        if (sonHelico && sonHelico.isPlaying)
            sonHelico.Stop();

        if (explosion != null)
        {
            explosion.SetActive(true);
            var ps = explosion.GetComponent<ParticleSystem>();
            if (ps) ps.Play();
            var s = explosion.GetComponent<AudioSource>();
            if (s) s.Play();
        }

        rigidHelico.useGravity = true;

        if (!masseBoostee)
        {
            rigidHelico.mass = masseLourde;
            masseBoostee = true;
        }

        rigidHelico.linearDamping = dragChute;
        rigidHelico.angularDamping = angularDragChute;
        rigidHelico.constraints = RigidbodyConstraints.None;

        if (gestionCameras != null)
            gestionCameras.SetActive(true);

        // Relance la scène après un délai
        Invoke(nameof(RelancerPartie), 6f);
    }
}