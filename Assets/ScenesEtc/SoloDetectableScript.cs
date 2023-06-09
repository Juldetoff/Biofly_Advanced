using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class SoloDetectableScript : MonoBehaviour
{
    Camera camtrouvé;
    DateTime timestart ;
    Camera cam;
    bool almostVisible=false;
    // Start is called before the first frame update

    public void setCam(Camera cam)
    {
        this.cam = cam;
    }
    public void setTimeStart(DateTime timestart)
    {
        this.timestart = timestart;
    }

    private void Update() {
        if(almostVisible && camtrouvé==null){
            Vector3 screenPos = cam.WorldToViewportPoint(this.transform.position);

            // Convert screen space to viewport space
            Vector3 viewportPos = screenPos;
            print(viewportPos);

            // Check if viewport position is within the camera's viewport
            if (viewportPos.x > 0 && viewportPos.x <= 1 &&
                viewportPos.y > 0 && viewportPos.y <= 1 && viewportPos.z > 0)
            {
                // Return the camera that is seeing the object
                camtrouvé = cam;
                string objectName =this.gameObject.name;
                DateTime currentTime = DateTime.Now;

                string filePath = Application.dataPath + "/../Positions/"+ camtrouvé.name + ".txt";
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Objet : " + objectName + " vu à : " + ( currentTime - timestart).ToString());
                }
            }
        }
    }

    private void OnBecameVisible() 
    {
    Debug.Log("Objet "+ this.gameObject.name +" visible");
    {
        almostVisible=true;
        // Convert object position to screen space
        //Vector3 screenPos = cam.WorldToScreenPoint(this.transform.position);
        Vector3 screenPos = cam.WorldToViewportPoint(this.transform.position);

        // Convert screen space to viewport space
        //Vector3 viewportPos = cam.ScreenToViewportPoint(screenPos);
        Vector3 viewportPos = screenPos;
        print(viewportPos);

        // Check if viewport position is within the camera's viewport
        if (viewportPos.x > 0 && viewportPos.x <= 1 &&
            viewportPos.y > 0 && viewportPos.y <= 1 && viewportPos.z > 0)
        {
            // Return the camera that is seeing the object
            camtrouvé = cam;
            string objectName =this.gameObject.name;
            DateTime currentTime = DateTime.Now;

            string filePath = Application.dataPath + "/../Positions/"+ camtrouvé.name + ".txt";
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine("Objet : " + objectName + " vu à : " + ( currentTime - timestart).ToString());
            }
        }
    }   
    }

    private void OnBecameInvisible() 
    {
        
        if(camtrouvé){
            Debug.Log("Objet invisible");
            string objectName =this.gameObject.name;
            DateTime currentTime = DateTime.Now;

            string filePath = Application.dataPath + "/../Positions/"+ camtrouvé.name + ".txt";
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine("Objet : " + objectName + " plus vu à : " + ( currentTime - timestart).ToString());
            }
        }
        camtrouvé=null;
        almostVisible=false;
    }
}
