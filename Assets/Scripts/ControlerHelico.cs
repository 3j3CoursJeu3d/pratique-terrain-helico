using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class controlerHelico : MonoBehaviour
{
    [Header("Mouvements")]
    public float vitesseTourne;
    public float vitesseMonte;
    private float vitesseAvance;
    public float vitesseAvanceMax;
    public float forceAcceleration;

    public GameObject uneHelice;

    [Header("Références objets")]
    public GameObject refHeliceAvant;
    public GameObject refHeliceArriere;
    public GameObject gestionCameras;        // à activer post-explosion si souhaité
    public GameObject objetMusiqueAmbiance;  // objet "Musique" à activer en jeu
    public AudioClip sonBidon;
    public GameObject explosion;

    [Header("UI / Audio")]
    public TextMeshProUGUI affichageInfo;
    public Image barreEssence;

    [Header("Essence")]
    public float essenceActuelle;
    public float essenceMax;
    public float consoParSeconde = 0.2f;

    [Header("Relance après panne")]
    public float delaiRelanceApresPanneEssence = 2f;

    [Header("Paramètres de chute")]
    public float dragChute = 0.0f;          // damping linéaire (faible pour chute franche)
    public float angularDragChute = 0.05f;  // damping angulaire (faible)
    public float masseLourde = 1000f;       // 75 → 1000 en panne/explosion

    [Header("Inclinaison visuelle")]
    public float angleInclinaisonMax = 15f;
    public float vitesseLerpInclinaison = 6f;

    [Header("Caméra de suivi fluide")]
    public Transform cameraSuivi;
    public float facteurInclinaisonCam = 0.5f;

    // États
    public bool finJeu;
    private bool partieCommencee = false;
    private bool helicoptereActif = false;
    private bool auSol = false;
    private bool panneEssence = false;
    private bool relancePartiePlanifiee = false;

    // Internes
    private Rigidbody rigidHelico;
    private AudioSource sonHelico;
    private float essencePrecedente;
    private float vitesseZ = 0f;

    // Sauvegardes pour restaurer après panne
    private float masseInitiale;
    private float linearDampingInitial;
    private float angularDampingInitial;
    private RigidbodyConstraints contraintesInitiales;
    private bool masseBoostee = false;

    void Start()
    {
        rigidHelico = GetComponent<Rigidbody>();
        sonHelico = GetComponent<AudioSource>();

        // Sauvegarde des réglages d’origine
        masseInitiale = rigidHelico.mass;
        linearDampingInitial = rigidHelico.linearDamping;   // même nom que dans ton projet
        angularDampingInitial = rigidHelico.angularDamping;  // idem
        contraintesInitiales = rigidHelico.constraints;

        essenceActuelle = essenceMax;
        essencePrecedente = essenceActuelle;

        if (objetMusiqueAmbiance != null)
            objetMusiqueAmbiance.SetActive(false);

        if (sonHelico && sonHelico.isPlaying)
            sonHelico.Stop();
    }

    void Update()
    {
        // Activer l'hélico à la demande après démarrage partie
        if (partieCommencee && !helicoptereActif && !finJeu)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ActiverHelicoptere();
            }
        }
    }

    void FixedUpdate()
    {
        // Détection panne d’essence
        if (essenceActuelle <= 0f)
            panneEssence = true;

        if (panneEssence)
        {
            // Couper hélices + son, laisser tomber avec la gravité
            var hA = refHeliceAvant ? refHeliceAvant.GetComponent<TournerHelice>() : null;
            var hR = refHeliceArriere ? refHeliceArriere.GetComponent<TournerHelice>() : null;
            if (hA) hA.enMarche = false;
            if (hR) hR.enMarche = false;

            rigidHelico.useGravity = true;
            vitesseAvance = 0f;

            if (!masseBoostee)
            {
                rigidHelico.mass = masseLourde;                 // 75 → 1000
                rigidHelico.linearDamping = dragChute;          // faible frein
                rigidHelico.angularDamping = angularDragChute;
                rigidHelico.constraints = RigidbodyConstraints.None;
                masseBoostee = true;
            }

            if (sonHelico && sonHelico.isPlaying)
                sonHelico.Stop();

            // relance rapide après contact sol (si tu gardes ce flow)
            if (auSol && !relancePartiePlanifiee)
            {
                relancePartiePlanifiee = true;
                Invoke(nameof(RelancerPartie), delaiRelanceApresPanneEssence);
            }

            if (affichageInfo != null)
                affichageInfo.text = "Panne sèche !";

            essencePrecedente = essenceActuelle;
            return; // pas d'inputs en panne
        }

        // Bloc vol normal / attente d’activation
        if (!partieCommencee || !helicoptereActif || finJeu)
        {
            if (affichageInfo != null && partieCommencee)
                affichageInfo.text = helicoptereActif ? "Hélico actif" : "Appuyez sur Entrée";
            return;
        }

        // Vol actif si les hélices tournent
        var tournerAvant = refHeliceAvant ? refHeliceAvant.GetComponent<TournerHelice>() : null;
        if (tournerAvant != null && tournerAvant.enMarche)
        {
            essenceActuelle -= consoParSeconde * Time.fixedDeltaTime;
            essenceActuelle = Mathf.Max(0f, essenceActuelle);

            if (barreEssence) barreEssence.fillAmount = essenceActuelle / essenceMax;

            rigidHelico.useGravity = false;

            if (sonHelico != null && !sonHelico.isPlaying)
            {
                sonHelico.volume = 0f;
                sonHelico.Play();
            }

            float forceRotation = Input.GetAxis("Horizontal") * vitesseTourne;
            float forceMonte = Input.GetAxis("Vertical") * vitesseMonte;

            if (Input.GetKey(KeyCode.E) && vitesseAvance < vitesseAvanceMax)
                vitesseAvance += forceAcceleration;

            if (Input.GetKey(KeyCode.Q) && vitesseAvance > 0f)
                vitesseAvance -= forceAcceleration;

            rigidHelico.AddRelativeTorque(0f, forceRotation, 0f);
            rigidHelico.AddRelativeForce(0f, forceMonte, vitesseAvance);

            if (sonHelico.isPlaying && sonHelico.volume < 1f)
                sonHelico.volume += 0.1f;

            // Inclinaison visuelle (bank)
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

            // Caméra suit l’inclinaison
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
            // Hélices arrêtées → gravité
            rigidHelico.useGravity = true;

            if (sonHelico && sonHelico.isPlaying && sonHelico.volume > 0f)
                sonHelico.volume -= 0.01f;
            else if (sonHelico)
                sonHelico.Stop();
        }

        if (affichageInfo != null)
            affichageInfo.text = rigidHelico.linearVelocity.magnitude.ToString("0.0");

        essencePrecedente = essenceActuelle;
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
            // plein déjà ?
            bool dejaPlein = essenceActuelle >= essenceMax - 0.0001f;

            if (!dejaPlein)
            {
                // Remplir à fond
                essenceActuelle = essenceMax;
                if (barreEssence) barreEssence.fillAmount = 1f;

                // Son de pickup seulement si on a vraiment ravitaillé
                if (sonHelico && sonBidon) sonHelico.PlayOneShot(sonBidon);

                // Si on était en panne, restaurer l'état physique de base
                if (panneEssence)
                {
                    panneEssence = false;
                    relancePartiePlanifiee = false; // évite un reload programmé

                    // Restaure masse/dampings/contraintes d’origine
                    rigidHelico.mass = masseInitiale;
                    rigidHelico.linearDamping = linearDampingInitial;
                    rigidHelico.angularDamping = angularDampingInitial;
                    rigidHelico.constraints = contraintesInitiales;
                    masseBoostee = false;

                    // On n'allume pas automatiquement les hélices ici
                }
            }

            // Dans tous les cas, le bidon disparaît
            Destroy(infoCollision.gameObject);
        }
    }

    private void OnCollisionEnter(Collision infosCollision)
    {
        float vitesseDeplacement = rigidHelico.linearVelocity.magnitude;

        // Explosion uniquement à l'impact avec décor (si en jeu)
        if (infosCollision.gameObject.tag == "décor" && vitesseDeplacement > 0.25f && !finJeu && helicoptereActif)
        {
            ExploserHelico();
        }
    }

    private void ActiverHelicoptere()
    {
        helicoptereActif = true;

        var hA = refHeliceAvant ? refHeliceAvant.GetComponent<TournerHelice>() : null;
        var hR = refHeliceArriere ? refHeliceArriere.GetComponent<TournerHelice>() : null;
        if (hA) hA.enMarche = true;
        if (hR) hR.enMarche = true;
    }

    // Appelé par DemarrerJeu
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

    // Minuteur → simuler panne (sans explosion)
    public void DeclencherPanneEssence()
    {
        essenceActuelle = 0f; // FixedUpdate gère la suite
    }

    public void ExploserHelico()
    {
        if (finJeu) return;
        finJeu = true;

        var hA = refHeliceAvant ? refHeliceAvant.GetComponent<TournerHelice>() : null;
        var hR = refHeliceArriere ? refHeliceArriere.GetComponent<TournerHelice>() : null;
        if (hA) hA.enMarche = false;
        if (hR) hR.enMarche = false;

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
            rigidHelico.mass = masseLourde; // lourdeur post-impact (optionnel)
            masseBoostee = true;
        }

        rigidHelico.linearDamping = dragChute;
        rigidHelico.angularDamping = angularDragChute;
        rigidHelico.constraints = RigidbodyConstraints.None;

        if (gestionCameras != null)
            gestionCameras.SetActive(true);

        Invoke(nameof(RelancerPartie), 9f);
    }
}