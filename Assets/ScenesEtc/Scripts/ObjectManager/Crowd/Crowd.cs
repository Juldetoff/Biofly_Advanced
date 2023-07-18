using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cette classe gère l'animation des personnages de la foule. Elle est attachée à l'objet "Crowd" dans les prefabs du dossier "Addons/crowd/prefabs".
/// </summary>
public class Crowd : MonoBehaviour
{
    //variables privées qui serviront à gérer de manière interne l'animation du personnage
    float timer = 0;
    int rand = 0;
    bool crouched = false;
    bool fight = false;
    bool applause = false;
    bool lie = false;
    Animator anim = null;
    private string count = "0";

    // Start est appelé avant la première frame 
    void Start()
    {
        anim = GetComponent<Animator>();
        count = this.name.Substring(8);
        //count = this.name[this.name.Length-1]+"";
    }

    // Update est appelé à chaque frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer > 5){
            rand = Random.Range(0,10);
            if(rand==0 || rand==1){
                crouched = !crouched;
                anim.SetBool("crouch", crouched);
            }
            else if(rand==2 || rand==3){
                fight = !fight;
                anim.SetBool("fight", fight);
            }
            else if(rand==4||rand==5){
                applause = !applause;
                anim.SetBool("applause", applause);
            }
            else if(rand==6||rand==7){
                lie = !lie;
                anim.SetBool("lie", lie);
            }
            timer = 0;
        }
    }

    //LateUpdate est appelé après Update, ce qui évite le décalage entre l'animation et le nom de l'objet.
    private void LateUpdate() {
        this.name = GetState() + count;
        this.transform.GetChild(1).gameObject.name = GetState() + count; //on renomme également le child car c'est lui qui contient les colliders pour les raycasts (pour plus de facilité).
    }

    //<summary>
    //Renvoie l'état de l'animation du personnage. Utilisé afin de renommer l'objet.
    //</summary>
    public string GetState(){
        AnimatorStateInfo animState = anim.GetCurrentAnimatorStateInfo(0);
        //Access the Animation clip name
        string m_ClipName = animState.IsName("Idle") ? "idle" : animState.IsName("Crouch") ? "crouch" : animState.IsName("Fight") ? "fight" : animState.IsName("Applause") ? "applause" : animState.IsName("lie-down_anim") ? "lie" : "idle";
        return m_ClipName;
    }
}
