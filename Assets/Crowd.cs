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
    Animator anim = null;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer > 5){
            rand = Random.Range(0,10);
            Debug.Log(rand);
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
            timer = 0;
        }
    }
}
