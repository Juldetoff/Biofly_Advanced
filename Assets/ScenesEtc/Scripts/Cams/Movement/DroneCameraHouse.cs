using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneCameraHouse : DroneCameraMovement
{
    // [SerializeField] private Vector2 sensibility;
    // [SerializeField] private Vector2 acceleration;
    // [SerializeField] private float inputLagPeriod;
    // [SerializeField] private float maxTopVerticalAngleFromHorizon;
    // [SerializeField] private float maxBottomVerticalAngleFromHorizon;
    // [SerializeField] private float maxVelocity;

    // [SerializeField] private float speed = 20.0f;
   
    // private Vector2 rotation;
    public GameObject sol;

    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        this.ClampHeight();        
    }

    public override void ClampHeight(){
        if(-190.5f > transform.localPosition.y){
            transform.localPosition = new Vector3(transform.localPosition.x,
            Mathf.Clamp(-190.5f, transform.localPosition.y, -187.5f),
            transform.localPosition.z);
        }
        if(-187.5f < transform.localPosition.y){
            transform.localPosition = new Vector3(transform.localPosition.x,
            -187.5f,
            transform.localPosition.z);
        }
    }
}
