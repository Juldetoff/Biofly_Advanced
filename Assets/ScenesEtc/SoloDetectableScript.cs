using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Recorder.Examples;

public class SoloDetectableScript : MonoBehaviour
{
    Camera camtrouvé;
    float currentTime;
    DateTime current;
    public Camera cam;
    bool almostVisible=false;

    // Start is called before the first frame update

    public void setCam(Camera cam)
    {
        this.cam = cam;
    }
    public void setTimeStart(float timestart)
    {
    }

    private void Start() {
        camtrouvé = cam;
        Seen();
    }

    private void Update()
    {
        //partie cam
        Seen();

    }

    private void Seen()
    {
        Vector3 screenPos = cam.WorldToViewportPoint(this.transform.position);
        Vector3 viewportPos = screenPos;

        if (viewportPos.x > 0 && viewportPos.x <= 1 &&
            viewportPos.y > 0 && viewportPos.y <= 1 && viewportPos.z > 0)
        {
            Ray ray = new Ray(cam.transform.position, this.transform.position - cam.transform.position);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            Debug.DrawRay(cam.transform.position, this.transform.position - cam.transform.position, Color.red); //trop cool ça trace le rayon
            //Vector3 hitPos = hit.point;
            // print(hit.collider.gameObject.name);
            if (hit.collider.CompareTag("obstacle"))
            {
                if (!almostVisible)
                { //Cas objet visible non caché
                    Debug.Log("Objet " + this.gameObject.name + " visible");
                    string objectName = this.gameObject.name;

                    string filePath = Application.dataPath + "/../Positions/" + camtrouvé.name + ".txt";
                    float temps = Time.time-cam.GetComponent<MovieRecordManual>().startTime;
                    using (StreamWriter writer = new StreamWriter(filePath, true))
                    {
                        writer.WriteLine("Objet : " + objectName + " vu après : " + (temps).ToString() + "s");
                    }
                }
                almostVisible = true;
            }
            else
            {
                if (almostVisible)
                { //Cas objet visible qui passe caché
                    Debug.Log(gameObject.name + " invisible");
                    string objectName = this.gameObject.name;

                    string filePath = Application.dataPath + "/../Positions/" + camtrouvé.name + ".txt";
                    float temps = Time.time-cam.GetComponent<MovieRecordManual>().startTime;
                    using (StreamWriter writer = new StreamWriter(filePath, true))
                    {
                        writer.WriteLine("Objet : " + objectName + " plus vu après : " + (temps).ToString() + "s");
                    }
                }
                almostVisible = false;
            }
        }
        else
        {
            if (almostVisible)
            { //Cas objet visible qui passe invisible
                Debug.Log(gameObject.name + " invisible");
                string objectName = this.gameObject.name;

                string filePath = Application.dataPath + "/../Positions/" + camtrouvé.name + ".txt";
                float temps = Time.time-cam.GetComponent<MovieRecordManual>().startTime;
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Objet : " + objectName + " plus vu après : " + (temps).ToString() + "s");
                }
            }
            almostVisible = false;
        }
    }

}
