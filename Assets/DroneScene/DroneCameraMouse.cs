using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneCameraMouse : MonoBehaviour
{
    [SerializeField] private Vector2 sensibility;
    [SerializeField] private Vector2 acceleration;
    [SerializeField] private float inputLagPeriod;
    [SerializeField] private float maxTopVerticalAngleFromHorizon;
    [SerializeField] private float maxBottomVerticalAngleFromHorizon;
    [SerializeField] private float maxVelocity;

    public Terrain terrain;

    private Vector2 rotation;
    private Vector2 velocity;
    private Vector2 lastInputEvent;
    private float inputLagTimer;

    public float speed = 20.0f;

    private float ClampVerticalAngle(float angle){
        return Mathf.Clamp(angle, -maxBottomVerticalAngleFromHorizon, maxTopVerticalAngleFromHorizon);
    }

    private Vector2 GetInput(){
        inputLagTimer += Time.deltaTime;
        Vector2 input = new Vector2();
        input.x = Input.GetAxis("Mouse X");
        input.y = Input.GetAxis("Mouse Y");
        if(!Mathf.Approximately(0, input.x) || !Mathf.Approximately(0, input.y) || inputLagTimer >= inputLagPeriod){
            lastInputEvent = input;
            inputLagTimer = 0;
        }
        return lastInputEvent;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 wantedVelocity = GetInput() * sensibility;
        velocity = new Vector2( 
            Mathf.Clamp(Mathf.MoveTowards(velocity.x, wantedVelocity.x, acceleration.x*Time.deltaTime), -maxVelocity, maxVelocity),
            Mathf.Clamp(Mathf.MoveTowards(velocity.y, wantedVelocity.y, acceleration.y*Time.deltaTime), -maxVelocity, maxVelocity));
        rotation += velocity*Time.deltaTime;
        rotation.y = ClampVerticalAngle(rotation.y);
        this.gameObject.transform.eulerAngles = new Vector3(-rotation.y, rotation.x, 0);

        Vector3 forwardMovement = gameObject.transform.forward * Input.GetAxis("Vertical");
        Vector3 horizontalMovement = gameObject.transform.right * Input.GetAxis("Horizontal");
        Vector3 movement = Vector3.ClampMagnitude(forwardMovement + horizontalMovement,1);
        movement.y = 0;
        transform.Translate(movement * speed * Time.deltaTime, Space.World); 
        
        if(Input.GetKey(KeyCode.Space)){
            transform.Translate(Vector3.up * 10 * Time.deltaTime, Space.World);
        }

        if(Input.GetKey(KeyCode.LeftShift)){
            transform.Translate(Vector3.down * 10 * Time.deltaTime, Space.World);
        }
        
        if(terrain.SampleHeight(transform.position)+30 > transform.position.y){
            transform.position = new Vector3(transform.position.x,
            Mathf.Clamp(terrain.SampleHeight(transform.position)-30, transform.position.y, terrain.SampleHeight(transform.position)+30),
            transform.position.z);
        }
        if(terrain.SampleHeight(transform.position)+25 < transform.position.y){
            transform.position = new Vector3(transform.position.x,
            terrain.SampleHeight(transform.position)+25,
            transform.position.z);
        }
    }
}
