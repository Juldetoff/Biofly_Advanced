using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class SoloDetectableScript : MonoBehaviour
{
    Camera camtrouvé;
    float currentTime;
    DateTime current;
    Camera cam;
    bool almostVisible=false;

    // Start is called before the first frame update

    public void setCam(Camera cam)
    {
        this.cam = cam;
    }
    public void setTimeStart(float timestart)
    {
        this.currentTime = timestart;
    }

    private void Start() {
        camtrouvé = cam;
    }

    private void Update() {
        currentTime += Time.deltaTime;

        //partie cam
        Vector3 screenPos = cam.WorldToViewportPoint(this.transform.position);
        Vector3 viewportPos = screenPos;

        if(viewportPos.x > 0 && viewportPos.x <= 1 &&
            viewportPos.y > 0 && viewportPos.y <= 1 && viewportPos.z > 0)
        {
            if(!almostVisible){
                string objectName =this.gameObject.name;

                string filePath = Application.dataPath + "/../Positions/"+ camtrouvé.name + ".txt";
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Objet : " + objectName + " vu après : " + ( currentTime ).ToString() + "s");
                }
            }
            almostVisible=true;
        }
        else
        {
            if(almostVisible){
                Debug.Log(gameObject.name +" invisible");
                string objectName =this.gameObject.name;

                string filePath = Application.dataPath + "/../Positions/"+ camtrouvé.name + ".txt";
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Objet : " + objectName + " plus vu après : " + ( currentTime ).ToString()+ "s");
                }
            }
            almostVisible=false;
        }

        // if(almostVisible && camtrouvé==null){   
        //     // Check if viewport position is within the camera's viewport
        //     if (viewportPos.x > 0 && viewportPos.x <= 1 &&
        //         viewportPos.y > 0 && viewportPos.y <= 1 && viewportPos.z > 0)
        //     {
        //         // Return the camera that is seeing the object
        //         camtrouvé = cam;
        //         string objectName =this.gameObject.name;

        //         string filePath = Application.dataPath + "/../Positions/"+ camtrouvé.name + ".txt";
        //         using (StreamWriter writer = new StreamWriter(filePath, true))
        //         {
        //             writer.WriteLine("Objet : " + objectName + " vu après : " + ( currentTime ).ToString() + "s");
        //         }
        //     }
        // }
    }

    // private void OnBecameVisible() 
    // {
    // Debug.Log("Objet "+ this.gameObject.name +" visible");
    // {
    //     almostVisible=true;
    //     // Convert object position to screen space
    //     //Vector3 screenPos = cam.WorldToScreenPoint(this.transform.position);
    //     Vector3 screenPos = cam.WorldToViewportPoint(this.transform.position);

    //     // Convert screen space to viewport space
    //     //Vector3 viewportPos = cam.ScreenToViewportPoint(screenPos);
    //     Vector3 viewportPos = screenPos;
    //     // print(viewportPos);

    //     // Check if viewport position is within the camera's viewport
    //     if (viewportPos.x > 0 && viewportPos.x <= 1 &&
    //         viewportPos.y > 0 && viewportPos.y <= 1 && viewportPos.z > 0)
    //     {
    //         // Return the camera that is seeing the object
    //         camtrouvé = cam;
    //         string objectName =this.gameObject.name;

    //         string filePath = Application.dataPath + "/../Positions/"+ camtrouvé.name + ".txt";
    //         using (StreamWriter writer = new StreamWriter(filePath, true))
    //         {
    //             writer.WriteLine("Objet : " + objectName + " vu après : " + ( currentTime ).ToString()+ "s");
    //         }
    //     }
    // }   
    // }

    // private void OnBecameInvisible() 
    // {
        
    //     if(camtrouvé){
    //         Debug.Log(gameObject.name +" invisible");
    //         string objectName =this.gameObject.name;

    //         string filePath = Application.dataPath + "/../Positions/"+ camtrouvé.name + ".txt";
    //         using (StreamWriter writer = new StreamWriter(filePath, true))
    //         {
    //             writer.WriteLine("Objet : " + objectName + " plus vu après : " + ( currentTime ).ToString()+ "s");
    //         }
    //     }
    //     camtrouvé=null;
    //     almostVisible=false;
    // }
}
