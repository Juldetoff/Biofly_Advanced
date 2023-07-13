using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///Cette classe étend la classe <c>StartDrone</c> afin de prendre en compte l'espace de la maison pour les contraintes de déplacement.
///</summary>
public class DroneCameraHouse : DroneCameraMovement
{    
    // Start est appelé avant la première frame 
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // SetOffsetBotHeight(-190.5f); //valeurs intéressantes pour la maison
        // SetOffsetTopHeight(-187.5f); 
    }

    // Update est appelé à chaque frame
    void Update()
    {
        Move();
        this.ClampHeight();        
    }

    ///<summary>
    ///Cette fonction est un override de la fonction <c>ClampHeight</c> de la classe <c>DroneCameraMovement</c>.
    ///Elle gère différemment la hauteur de la caméra, avec directement la hauteur du sol de la maison.
    ///</summary>
    public override void ClampHeight(){
        if(GetOffsetBotHeight() > transform.localPosition.y){
            transform.localPosition = new Vector3(transform.localPosition.x,
            Mathf.Clamp(GetOffsetBotHeight(), transform.localPosition.y, GetOffsetTopHeight()),
            transform.localPosition.z);
        }
        if(GetOffsetTopHeight() < transform.localPosition.y){
            transform.localPosition = new Vector3(transform.localPosition.x,
            GetOffsetTopHeight(),
            transform.localPosition.z);
        }
    }
}
