using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class DetectableScript : MonoBehaviour
{
    Camera camtrouvé;
    DateTime timestart ;
    // Start is called before the first frame update
    void Start()
    {
        timestart = DateTime.Now;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnBecameVisible() 
    {
    Debug.Log("Objet "+ this.gameObject.name +" visible");
     foreach (Camera cam in Camera.allCameras)
    {
        int n = cam.name.Length;
        if((int)cam.name[n-1] % 2 == 0)
        {// Convert object position to screen space
        Vector3 screenPos = cam.WorldToScreenPoint(this.transform.position);

        // Convert screen space to viewport space
        Vector3 viewportPos = cam.ScreenToViewportPoint(screenPos);

        // Check if viewport position is within the camera's viewport
        if (viewportPos.x > 0 && viewportPos.x < 1 &&
            viewportPos.y > 0 && viewportPos.y < 1)
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
        }}
    }   
    }

    private void OnBecameInvisible() 
    {
    Debug.Log("Objet invisible");
        string objectName =this.gameObject.name;
    DateTime currentTime = DateTime.Now;

    string filePath = Application.dataPath + "/../Positions/"+ camtrouvé.name + ".txt";
    using (StreamWriter writer = new StreamWriter(filePath, true))
    {
        writer.WriteLine("Objet : " + objectName + " plus vu à : " + ( currentTime - timestart).ToString());
    }
    }
}
