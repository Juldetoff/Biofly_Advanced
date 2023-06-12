using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneCameraMovement : MonoBehaviour
{
    [SerializeField] private Vector2 sensibility;
    [SerializeField] private Vector2 acceleration;
    [SerializeField] private float inputLagPeriod;
    [SerializeField] private float maxTopVerticalAngleFromHorizon;
    [SerializeField] private float maxBottomVerticalAngleFromHorizon;
    [SerializeField] private float maxVelocity;

    public Terrain terrain;

    private Rigidbody rb;
    private Vector2 rotation;
    private Vector2 velocity;
    private Vector2 lastInputEvent;
    private float inputLagTimer;

    public float speed = 20.0f;

    // private float ClampVerticalAngle(float angle){ //pour simuler la caméra du drone (si elle peut bouger ça sera pas à 180° vertical)
    //     return Mathf.Clamp(angle, -maxBottomVerticalAngleFromHorizon, maxTopVerticalAngleFromHorizon);
    //}

    // private Vector2 GetInput(){ //pour simuler caméra du drone à la souris
    //     inputLagTimer += Time.deltaTime;
    //     Vector2 input = new Vector2();
    //     input.x = Input.GetAxis("Mouse X");
    //     input.y = Input.GetAxis("Mouse Y");
    //     if(!Mathf.Approximately(0, input.x) || !Mathf.Approximately(0, input.y) || inputLagTimer >= inputLagPeriod){
    //         lastInputEvent = input;
    //         inputLagTimer = 0;
    //     }
    //     return lastInputEvent;
    // }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //déplacement
        if (Input.GetKey(KeyCode.Keypad4))
             rb.AddForce(transform.rotation* Vector3.left*Time.deltaTime*acceleration.x*speed);
        if (Input.GetKey(KeyCode.Keypad6))
             rb.AddForce(transform.rotation* Vector3.right*Time.deltaTime*acceleration.x*speed);
        if (Input.GetKey(KeyCode.Keypad8))
             rb.AddForce(transform.rotation* Vector3.forward*Time.deltaTime*acceleration.y*speed);
        if (Input.GetKey(KeyCode.Keypad2))
             rb.AddForce(transform.rotation* Vector3.back*Time.deltaTime*acceleration.y*speed);

        if(Input.GetKey(KeyCode.UpArrow)) { 
             var currEulerAngles = transform.eulerAngles;
             print(currEulerAngles);
             currEulerAngles.x -= 5* speed * Time.deltaTime;
             
             //currEulerAngles.x = Mathf.Clamp(currEulerAngles.x, maxBottomVerticalAngleFromHorizon+360, maxTopVerticalAngleFromHorizon+360);
             transform.rotation = Quaternion.Euler(currEulerAngles);
        }
        if(Input.GetKey(KeyCode.DownArrow)) {
             var currEulerAngles = transform.eulerAngles;
             currEulerAngles.x += 5* speed * Time.deltaTime;
             
             //currEulerAngles.x = Mathf.Clamp(currEulerAngles.x, maxBottomVerticalAngleFromHorizon+360, maxTopVerticalAngleFromHorizon+360);
             transform.rotation = Quaternion.Euler(currEulerAngles);
        }
        if(Input.GetKey(KeyCode.LeftArrow)) {
            var currEulerAngles = transform.eulerAngles;
            currEulerAngles.y -= 5* speed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(currEulerAngles);
        }
        if(Input.GetKey(KeyCode.RightArrow)) {
            var currEulerAngles = transform.eulerAngles;
            currEulerAngles.y += 5* speed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(currEulerAngles);
        }


        // Vector2 wantedVelocity = GetInput() * sensibility;
        // velocity = new Vector2( 
        //     Mathf.Clamp(Mathf.MoveTowards(velocity.x, wantedVelocity.x, acceleration.x*Time.deltaTime), -maxVelocity, maxVelocity),
        //     Mathf.Clamp(Mathf.MoveTowards(velocity.y, wantedVelocity.y, acceleration.y*Time.deltaTime), -maxVelocity, maxVelocity));
        // rotation += velocity*Time.deltaTime;
        // rotation.y = ClampVerticalAngle(rotation.y);
        // this.gameObject.transform.eulerAngles = new Vector3(-rotation.y, rotation.x, 0);

        // Vector3 forwardMovement = gameObject.transform.forward * Input.GetAxis("Vertical");
        // Vector3 horizontalMovement = gameObject.transform.right * Input.GetAxis("Horizontal");
        // Vector3 movement = Vector3.ClampMagnitude(forwardMovement + horizontalMovement,1);
        // movement.y = 0;
        // transform.Translate(movement * speed * Time.deltaTime, Space.World); 
        
        if(Input.GetKey(KeyCode.Keypad5)){
            transform.Translate(Vector3.up * 10 * Time.deltaTime, Space.World);
        }

        if(Input.GetKey(KeyCode.Keypad0)){
            transform.Translate(Vector3.down * 10 * Time.deltaTime, Space.World);
        }
        
        if(terrain.SampleHeight(transform.position)+30 > transform.position.y){
            transform.position = new Vector3(transform.position.x,
            Mathf.Clamp(terrain.SampleHeight(transform.position)-37, transform.position.y, terrain.SampleHeight(transform.position)+30),
            transform.position.z);
        }
        if(terrain.SampleHeight(transform.position)+25 < transform.position.y){
            transform.position = new Vector3(transform.position.x,
            terrain.SampleHeight(transform.position)+25,
            transform.position.z);
        }
    }
}
