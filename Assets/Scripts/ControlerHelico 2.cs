using UnityEngine;

public class ControlerHelico : MonoBehaviour
{
    public float vitesseTourne;
    public float vitesseMonte;
    float vitesseAvance;

    public float vitesseAvanceMax;
    public float forceAcceleration;

    public GameObject uneHelice;
    public Rigidbody rigidHelico;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidHelico = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (uneHelice.GetComponent<TournerHelice>().enMarche)
        {
            rigidHelico.useGravity = false;

            float forceRotation = Input.GetAxis("Horizontal") * vitesseTourne;
            float forceMonte = Input.GetAxis("Vertical") * vitesseMonte;


            if (Input.GetKey(KeyCode.E) && vitesseAvance < vitesseAvanceMax)
            {
                vitesseAvance += forceAcceleration;
            }

            if (Input.GetKey(KeyCode.Q) && vitesseAvance > 0f)
            {
                vitesseAvance -= forceAcceleration;
            }

            GetComponent<Rigidbody>().AddRelativeTorque(0f, forceRotation, 0f);
            GetComponent<Rigidbody>().AddRelativeForce(0f, forceMonte, vitesseAvance);

            transform.localEulerAngles = new Vector3(0f, transform.localEulerAngles.y, 0f);
        }
        else
        {
            rigidHelico.useGravity = true;
        }
    }
}
