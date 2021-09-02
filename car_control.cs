using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class car_control : MonoBehaviour
{
    
    public float MotorForce, Steerforce, Brakeforce;
    public GameObject frontR, frontL, rearR, rearL, chassie;
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
    


    private Quaternion rotationVoiture;
    private Vector3 offsetRoueM = new Vector3(0f, -45f, 0f);
    private Vector3 roueAvanceL;
    private Vector3 roueAvanceR;




    // Start is called before the first frame update
    void Start()
    {
        //Mise de la friction sur les roues arrières
        glisseRight = RRW.sidewaysFriction;
        glisseRight.extremumSlip = 2f;
        glisseLeft = RLW.sidewaysFriction;
        glisseLeft.extremumSlip = 2f;

        //Enleve la friction sur les roues arrières
        NonglisseRight = RRW.sidewaysFriction;
        NonglisseRight.extremumSlip = 0.1f;
        NonglisseLeft = RRW.sidewaysFriction;
        NonglisseLeft.extremumSlip = 0.1f;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Prise de la rotation du chassis
        rotationVoiture = transform.rotation;



        //Pour que les roues arrières avance en fonctions de la vitesse
        float positionChassieL = chassie.transform.position.x * -100;
        float positionChassieR = chassie.transform.position.x * 100;
        roueAvanceL = new Vector3(positionChassieL, 0f, 0f);
        roueAvanceR = new Vector3(positionChassieR, 0f, 0f);
        rearR.transform.rotation = rotationVoiture * Quaternion.Euler(roueAvanceR);
        rearL.transform.rotation = rotationVoiture * Quaternion.Euler(roueAvanceL);



        //frontR.transform.rotation = rotationVoiture * Quaternion.Euler(roueAvance);
        //frontL.transform.rotation = rotationVoiture * Quaternion.Euler(roueAvance);

        /*
        float v = Input.GetAxis("Vertical") * MotorForce;
        float h = Input.GetAxis("Horizontal") * Steerforce;

        RRW.motorTorque = v;
        RLW.motorTorque = v;

        FLW.steerAngle = h;
        FRW.steerAngle = h;
        */



        if (Input.GetKey("z"))
        {
            RRW.motorTorque = 800;
            RLW.motorTorque = 800;
        }
        else
        {
            RRW.motorTorque = 0;
            RLW.motorTorque = 0;
        }


        if (Input.GetKey("s"))
        {

            FLW.brakeTorque = 6000;
            FRW.brakeTorque = 6000;
            RLW.brakeTorque = 4000;
            RLW.brakeTorque = 4000;

        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            RRW.motorTorque = -800;
            RLW.motorTorque = -800;
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            RRW.brakeTorque = 100;
            RLW.brakeTorque = 100;
            
            RRW.sidewaysFriction = glisseRight;
            RLW.sidewaysFriction = glisseLeft;
        }
        else
        {
            RRW.brakeTorque = 0;
            RLW.brakeTorque = 0;
            FRW.brakeTorque = 0;
            FLW.brakeTorque = 0;
            RRW.sidewaysFriction = NonglisseRight;
            RLW.sidewaysFriction = NonglisseLeft;
        }
       
        
        if (Input.GetKey("d"))
        {
            tourneD();
 
        }
        else if (Input.GetKey("q"))
        {
            tourneG();   
        }
        else
        {
            //initailisation du timer à 0
            timer = 0;

            //Ajout du deltatime(pause de 0.1s) au timer
            timerR += Time.deltaTime;

            if (timerR > waitTimeR) //a chaque 0.1s
            {
                
                //si l'angle de la roue est tourné vers la gauche, la roue se redresse
                if (angle < 0) 
                {
                    angle = angle + 1;
                }


                //Le modele de la roue tourne dans sa position standart et tourne vers l'avant
                Vector3 offsetRoue = new Vector3(0f, angle, 0f);
                offsetRoueM = offsetRoue + roueAvanceL;
                
                
                //si l'angle de la roue est tournée vers la droite, la roue se redresse
                if (angle > 0 )
                {
                    angle = angle - 1;
                }

                //les roues avant tournent
                FRW.steerAngle = angle;
                FLW.steerAngle = angle;

                //rotation des roues avant (gauche-droite)
                frontR.transform.rotation = rotationVoiture * Quaternion.Euler(offsetRoueM);
                frontL.transform.rotation = rotationVoiture * Quaternion.Euler(offsetRoueM);

            }
            else
            {
                //rotation des roues avant (gauche-droite)
                frontR.transform.rotation = rotationVoiture * Quaternion.Euler(roueAvanceR);
                frontL.transform.rotation = rotationVoiture * Quaternion.Euler(roueAvanceL);
            }
        }

        Debug.Log("angle : " + angle.ToString());

        

    }


    void tourneD()
    {
        

        timerR = 0;

        timer += Time.deltaTime;

        if (timer > waitTime) // a chaque 0.1s
        {
            if (angle < 45) // que si angle < 45
            {
                angle++;

            }
            Vector3 offsetRoue = new Vector3(0f, angle, 0f);
            offsetRoue = offsetRoue + roueAvanceR;
            FRW.steerAngle = angle;
            FLW.steerAngle = angle;
            frontR.transform.rotation = rotationVoiture * Quaternion.Euler(offsetRoue);
            frontL.transform.rotation = rotationVoiture * Quaternion.Euler(offsetRoue);

        }
    }


    void tourneG()
    {
        timerR = 0;
        offsetRoueM = offsetRoueM + roueAvanceL;


        timer += Time.deltaTime;

        if (timer > waitTime) // a chaque 0.1s
        {
            if (angle > -45)
            {
                
                angle--;

                //Debug.Log(angleG.ToString());

            }
            Vector3 offsetRoue = new Vector3(0f, angle, 0f);
            offsetRoueM = offsetRoue + roueAvanceL;
            FRW.steerAngle = angle;
            FLW.steerAngle = angle;
            frontR.transform.rotation = rotationVoiture * Quaternion.Euler(offsetRoueM);
            frontL.transform.rotation = rotationVoiture * Quaternion.Euler(offsetRoueM);


        }
            

    }



}









