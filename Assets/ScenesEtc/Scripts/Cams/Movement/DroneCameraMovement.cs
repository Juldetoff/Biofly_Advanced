using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///Cette classe permet de gérer les mouvements manuels de la caméra. Elle peut être étendu afin de gérer de manière spécifique le déplacement,
///et des fonctions peuvent être ajoutés à l'extension afin de mieux gérer les contraintes de déplacements de la caméra (murs, sols, obstacles...)
///</summary>
public class DroneCameraMovement : MonoBehaviour
{
    [Header("Variables de déplacement de la caméra")]
    [SerializeField] private Vector2 acceleration = new Vector2(100, 100);
    [SerializeField] private float speed = 20.0f;
    [Header("Contraintes de déplacement")]
    [SerializeField]private float offsetTopHeight = 30;
    [SerializeField]private float offsetBotHeight = 25;
    private Terrain terrain = null;

    [HideInInspector]public Rigidbody rb;

    // Start est appelé avant la première frame
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update est appelé à chaque frame
    void Update()
    {
        //déplacement
        Move();
        ClampHeight();
    }

///<summary>
///Cette fonction permet de vérifier si l'utilisateur utilise les touches de mouvements de la caméra.
///<exemple> 
///Par exemple dans Update :
///<code> 
///void Update() {
///     Move();
///}
///</code>
///Cela permet à chaque frame de vérifier si l'utilisateur déplace la caméra à chaque frame.
///</exemple>
///</summary>
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

    ///<summary>
    ///Cette fonction permet de gérer la hauteur de la caméra en limitant la hauteur max et min par rapport au sol du terrain associé.
    ///Lorsque la classe est étendu afin de gérer un cas spécifique,
    ///cette fonction peut être override afin de gérer le cas souhaité.
    ///</summary>
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

    ///<summary>
    ///Cette fonction permet d'associer un terrain à l'objet.
    ///</summary>
    public void SetTerrain(Terrain terrain){
        this.terrain = terrain;
    }

    public void SetOffsetTopHeight(float offsetTopHeight){
        this.offsetTopHeight = offsetTopHeight;
    }

    public void SetOffsetBotHeight(float offsetBotHeight){
        this.offsetBotHeight = offsetBotHeight;
    }

    public float GetOffsetTopHeight(){
        return offsetTopHeight;
    }

    public float GetOffsetBotHeight(){
        return offsetBotHeight;
    }
}
