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
    [SerializeField] private float speed = 20.0f;
    //[SerializeField] private bool useTerrain = true;

    public Terrain terrain;
    public int offsetTopHeight = 30;
    public int offsetBotHeight = 25;

    public Rigidbody rb;
    private Vector2 rotation;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //déplacement
        Move();
        ClampHeight();
    }

    public void Move()
    {
        //Déplacement
        if (Input.GetKey(KeyCode.Keypad4))
        {
            Vector3 force = transform.rotation * Vector3.left * Time.deltaTime * acceleration.x * speed;
            force.y = 0; //afin que le déplacement en "avant" n'impacte pas la hauteur du drone
            rb.AddForce(force);
        }
        if (Input.GetKey(KeyCode.Keypad6))
        {
            Vector3 force = transform.rotation * Vector3.right * Time.deltaTime * acceleration.x * speed;
            force.y = 0;
            rb.AddForce(force);
        }
        if (Input.GetKey(KeyCode.Keypad8))
        {
            Vector3 force = transform.rotation * Vector3.forward * Time.deltaTime * acceleration.y * speed;
            force.y = 0;
            rb.AddForce(force);
        }
        if (Input.GetKey(KeyCode.Keypad2))
        {
            Vector3 force = transform.rotation * Vector3.back * Time.deltaTime * acceleration.y * speed;
            force.y = 0;
            rb.AddForce(force);
        }

        //Caméra
        if (Input.GetKey(KeyCode.UpArrow))
        {
            var currEulerAngles = transform.eulerAngles;
            currEulerAngles.x -= 5 * speed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(currEulerAngles);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            var currEulerAngles = transform.eulerAngles;
            currEulerAngles.x += 5 * speed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(currEulerAngles);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            var currEulerAngles = transform.eulerAngles;
            currEulerAngles.y -= 5 * speed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(currEulerAngles);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            var currEulerAngles = transform.eulerAngles;
            currEulerAngles.y += 5 * speed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(currEulerAngles);
        }

        //Hauteur
        if (Input.GetKey(KeyCode.Keypad5))
        {
            transform.Translate(Vector3.up * 10 * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.Keypad0))
        {
            transform.Translate(Vector3.down * 10 * Time.deltaTime, Space.World);
        }
    }

    public virtual void ClampHeight(){
        if(terrain.SampleHeight(transform.position)+ offsetBotHeight > transform.position.y){
            transform.position = new Vector3(transform.position.x,
            //terrain.SampleHeight(transform.position),
            Mathf.Clamp(terrain.SampleHeight(transform.position)+offsetBotHeight, transform.position.y, terrain.SampleHeight(transform.position)+offsetTopHeight),
            transform.position.z);
        }
        if(terrain.SampleHeight(transform.position)+offsetTopHeight < transform.position.y){
            transform.position = new Vector3(transform.position.x,
            terrain.SampleHeight(transform.position)+offsetTopHeight,
            transform.position.z);
        }
    }
}
