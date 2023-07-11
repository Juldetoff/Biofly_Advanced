using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Crowd : MonoBehaviour
{
    float timer = 0;
    int rand = 0;
    bool crouched = false;
    bool fight = false;
    bool applause = false;
    bool lie = false;
    Animator anim = null;
    private string count = "0";
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        count = this.name[this.name.Length-1]+"";
    }

    // Update is called once per frame
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

            this.name = GetState() + count;
        }
    }

    public string GetState(){
        AnimatorStateInfo animState = anim.GetCurrentAnimatorStateInfo(0);
        //Access the Animation clip name
        string m_ClipName = animState.IsName("Idle") ? "idle" : animState.IsName("Crouch") ? "crouch" : animState.IsName("Fight") ? "fight" : animState.IsName("Applause") ? "applause" : animState.IsName("lie-down_anim") ? "lie" : "idle";
        return m_ClipName;
    }
}
