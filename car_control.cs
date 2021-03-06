using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class car_control : MonoBehaviour
{
    
    public float MotorForce, Steerforce, Brakeforce, CoefAcceleration, Speed, MaxSpeed, CH;
    public Transform frontR, frontL, rearR, rearL, chassie;
    public GameObject BackLight, FrontLight, StopLight;
    public Rigidbody rb;
    public AudioSource enSound;
    public WheelCollider FRW, FLW, RRW, RLW;
 
    //initialisation de la frictino de la roue
    private WheelFrictionCurve glisseRight;
    private WheelFrictionCurve glisseLeft;
    private WheelFrictionCurve NonglisseRight;
    private WheelFrictionCurve NonglisseLeft;

    //Timer pour la direction
    private float waitTime = 0.1f;
    private float timer = 0.0f;
    
    //time lors du retour de la direction 
    private float waitTimeR = 0.01f;
    private float timerR = 0.0f;
  
    //initialisation de l'angle des roues
    private float angle = 0f;

    //D?claration du texte de l'UI
    public Text TxtSpeed;
    public Text Txtgear;

    //D?claration du bool pour les phares avant
    private bool phare;

    //D?claration de la direction assist?
    float DA;
    float DAmax = 40;
    float DAmin = 10;

    //D?claration de la vitesse(gear)
    int gear = 1;

    //Boost de puissance selon la vitesse
    int BoostGear;





    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Lanc?");

        //D?claration du component RigideBody de la voiture
        rb = GetComponent<Rigidbody>();

        //D?claration du son de la voiture
        enSound = GetComponent<AudioSource>();

        //Mise de la friction sur les roues arri?res
        //glisseRight = RRW.sidewaysFriction;
        //glisseRight.extremumSlip = 2f;
        //glisseRight.stiffness = 0.5f;
        //glisseLeft = RLW.sidewaysFriction;
        //glisseLeft.extremumSlip = 2f;
        //glisseLeft.stiffness = 0.5f;

        //Enleve la friction sur les roues arri?res
        //NonglisseRight = RRW.sidewaysFriction;
        //NonglisseRight.extremumSlip = 0.1f;
        //NonglisseRight.stiffness = 0.8f;
        //NonglisseLeft = RRW.sidewaysFriction;
        //NonglisseLeft.extremumSlip = 0.1f;
        //NonglisseLeft.stiffness = 0.8f;

        //Changement du centre des masses de la voitures
        rb.centerOfMass = new Vector3(0f, 0.4f, 0.2f);

        

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Calcule de la direction assist?
        DirectionAssiste();

        //reception de l'angle de la roue
        angle = Input.GetAxis("Horizontal") * DA;

        //Pour que les roues avance(du collider au mesh) en fonctions de la vitesse
        rearR.transform.Rotate(RRW.rpm / 60 * 360 * Time.deltaTime, 0, 0);
        rearL.transform.Rotate(RLW.rpm / 60 * 360 * Time.deltaTime, 0, 0);
        frontR.transform.Rotate(FRW.rpm / 60 * 360 * Time.deltaTime, 0, 0);
        frontL.transform.Rotate(FLW.rpm / 60 * 360 * Time.deltaTime, 0, 0);

        //Pour que le mesh applique la rotation(gauche-droite) par rapport aux colliders
        frontR.localEulerAngles = new Vector3(frontR.localEulerAngles.x, FRW.steerAngle - frontR.localEulerAngles.z, frontR.localEulerAngles.z);
        frontL.localEulerAngles = new Vector3(frontL.localEulerAngles.x, FLW.steerAngle - frontL.localEulerAngles.z, frontL.localEulerAngles.z);

        //Calcule de la vitesse
        Speed = rb.velocity.magnitude * 3.6f;

        //Affichage de la vitesse ? l'UI
        TxtSpeed.text = "Vitesse : " + (int)Speed;
        Txtgear.text = "Rapport : " + gear;


        if (Input.GetKey("z") && Speed < MaxSpeed)
        {
            acceleration();
        }
        else if(RRW.rpm > 0)
        {
            deAcceleration();

        }
        else
        {
            roueLibre();


        }

        if (Input.GetKey("s"))
        {
            //Freine les roues
            freinage();

            //Allumage des feux avant
            BackLight.SetActive(true);
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            //Fait reculer la voiture
            recule();

            //Allume le feu de recule
            StopLight.SetActive(true);
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            RRW.brakeTorque = 100;
            RLW.brakeTorque = 100;
            
            //RRW.sidewaysFriction = glisseRight;
            //RLW.sidewaysFriction = glisseLeft;
        }
        else
        {
            RRW.brakeTorque = 0;
            RLW.brakeTorque = 0;
            FRW.brakeTorque = 0;
            FLW.brakeTorque = 0;

            //Extinction des feux arri?re
            BackLight.SetActive(false);
            //Extinction du feu de recule
            StopLight.SetActive(false);

            //RRW.sidewaysFriction = NonglisseRight;
            //RLW.sidewaysFriction = NonglisseLeft;
        }

        tourne(Input.GetAxis("Horizontal") * DA);

        //Allume et ?teint les feux avants
        if (Input.GetKeyUp(KeyCode.L))
        {
            Debug.Log(phare);
            phare = !phare;

            if(phare)
            {
                FrontLight.SetActive(true);
            }
            else
            {
                FrontLight.SetActive(false);
            }         
        }


        //Systeme de vitesse
        BoiteDeVitesse(Speed, MaxSpeed);
        







        //ZONE DE DEBUG

        //Debug.Log("FR RPM : " + FRW.rpm + "FL RPM : " + FLW.rpm + "RR RPM : " + RRW.rpm + "RL RPM : " + RLW.rpm);
        //Debug.Log("angle : " + angle.ToString());
        //Debug.Log("FRW angle : " + frontR.localEulerAngles );
        //Debug.Log("Axis : " + Input.GetAxis("Horizontal"));
        Debug.Log(gear);

    }


    void tourne(float sAngle)
    {
        FRW.steerAngle = sAngle;
        FLW.steerAngle = sAngle;
    }

    /* redressement
    void redressement()
    {
        //initailisation du timer ? 0
        timer = 0;

        //Ajout du deltatime(pause de 0.1s) au timer
        timerR += Time.deltaTime;

        if (timerR > waitTimeR) //a chaque 0.1s
        {

            //si l'angle de la roue est tourn? vers la gauche, la roue se redresse
            if (angle < 0)
            {
                angle = angle + 1;
            }

            //si l'angle de la roue est tourn?e vers la droite, la roue se redresse
            if (angle > 0)
            {
                angle = angle - 1;
            }

            //application de l'angle au collider de la roue
            FRW.steerAngle = angle;
            FLW.steerAngle = angle;

        }
    }
    */
    void freinage()
    {
        //Freinnage roue avant droite
        if (FRW.rpm > 1)
        {
            FRW.motorTorque = -(Input.GetAxis("Vertical") * 500);
        }
        else
        {
            FRW.motorTorque = 0;
            FRW.brakeTorque = 10;
        }
        //Freinnage roue avant gauche
        if (FLW.rpm > 1)
        {
            FLW.motorTorque = -800;
        }
        else
        {
            FLW.motorTorque = 0;
            FLW.brakeTorque = 10;
        }
        //Freinnage roue arri?re droite
        if (RRW.rpm > 1)
        {
            RRW.motorTorque = -800;
        }
        else
        {
            RRW.motorTorque = 0;
            RRW.brakeTorque = 10;
        }
        //Freinnage roue arri?re gauche
        if (RLW.rpm > 1)
        {
            RLW.motorTorque = -800;
        }
        else
        {
            RLW.motorTorque = 0;
            RLW.brakeTorque = 10;
        }
    }

    void deAcceleration()
    {
        RRW.motorTorque = -100;
        RLW.motorTorque = -100;
        FRW.motorTorque = -100;
        FLW.motorTorque = -100;
    }
    
    void roueLibre()
    {
        RRW.motorTorque = 0;
        RLW.motorTorque = 0;
        FRW.motorTorque = 0;
        FLW.motorTorque = 0;
    }

    void acceleration()
    {
        float puissance = Input.GetAxis("Vertical") * CoefAcceleration * CH + BoostGear;
        RRW.motorTorque = puissance;
        RLW.motorTorque = puissance;
        
    }

    void recule()
    {
        RRW.motorTorque = -800;
        RLW.motorTorque = -800;
    }


    void DirectionAssiste()
    {
        DA = -((DAmax - DAmin) / MaxSpeed) * Speed + DAmax;
    }


    void BoiteDeVitesse(float vitesse , float maxVitesse)
    {
        if ((int)vitesse > 124)
        {
            gear = 5;
            BoostGear = -100;
            enSound.pitch = Mathf.Clamp((vitesse / maxVitesse) * 2f + 0.5f, 1f, 3f);


        }
        else if ((int)vitesse > 84)
        {
            gear = 4;
            BoostGear = -50;
            enSound.pitch = Mathf.Clamp((vitesse / maxVitesse) * 2.5f + 0.5f, 1f, 3f);
        }
        else if ((int)vitesse > 56)
        {
            gear = 3;
            BoostGear = 0;
            enSound.pitch = Mathf.Clamp((vitesse / maxVitesse) * 3f + 0.5f, 1f, 3f);
        }
        else if ((int)vitesse > 21)
        {
            gear = 2;
            BoostGear = 50;
            enSound.pitch = Mathf.Clamp((vitesse / maxVitesse) * 4f + 0.5f, 1f, 3f);
        }
        else
        {
            gear = 1;
            BoostGear = 70;
            enSound.pitch = Mathf.Clamp((vitesse / maxVitesse) * 5f + 1f, 1f, 3f);


        }
    }


    


}









