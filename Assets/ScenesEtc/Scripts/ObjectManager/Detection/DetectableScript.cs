using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Recorder.Examples;

public class DetectableScript : MonoBehaviour
{
    float currentTime;
    DateTime current;
    // bool almostVisible=false;
    public bool[] camSeen;
    public Transform[] smallMesh;

    
    private void Start() {
        this.gameObject.tag = "obstacle";
        camSeen = new bool[Camera.allCamerasCount];
        for(int i = 0; i<Camera.allCamerasCount; i++){
            camSeen[i] = false;
        }
    }

    private void Update()
    {
        //partie cam
        Seen();
        Where();
    }

    private void Seen() //on vérifie si l'objet est vu par une caméra. Il s'occupe également de remplir le tableau des bools
    {
        for (int i = 0; i < camSeen.Length; i++)
        {
            Vector3 scenePos = Camera.allCameras[i].WorldToViewportPoint(this.transform.position);
            Vector3 viewportPos = scenePos;
            if (viewportPos.x > 0 && viewportPos.x <= 1 &&
                viewportPos.y > 0 && viewportPos.y <= 1 && viewportPos.z > 0)
            {
                Debug.DrawLine(Camera.allCameras[i].transform.position, this.transform.position, Color.red, 1000f);
                Ray ray = new Ray(Camera.allCameras[i].transform.position, this.transform.position - Camera.allCameras[i].transform.position);
                RaycastHit hit;
                Physics.Raycast(ray, out hit);
                if (hit.collider.CompareTag("obstacle"))
                {
                    if (!camSeen[i])
                    { //Cas objet visible non caché
                        Debug.Log("Objet " + this.gameObject.name + " visible");
                        string objectName = this.gameObject.name;
    
                        string filePath = Application.dataPath + "/../Positions/" + Camera.allCameras[i].name + ".txt";
                        float temps = Time.time-Camera.allCameras[i].GetComponent<MovieRecorderExample>().startTime;
                        using (StreamWriter writer = new StreamWriter(filePath, true))
                        {
                            writer.WriteLine(objectName + " vu après : " + (temps).ToString() + "s");
                        }
                    }
                    camSeen[i] = true;
                }
                else
                {
                    if (camSeen[i])
                    { //Cas objet visible qui passe caché
                        Debug.Log(gameObject.name + " invisible");
                        string objectName = this.gameObject.name;
    
                        string filePath = Application.dataPath + "/../Positions/" + Camera.allCameras[i].name + ".txt";
                        float temps = Time.time-Camera.allCameras[i].GetComponent<MovieRecorderExample>().startTime;
                        using (StreamWriter writer = new StreamWriter(filePath, true))
                        {
                            writer.WriteLine( objectName + " plus vu après : " + (temps).ToString() + "s");
                        }
                    }
                    camSeen[i] = false;
                }
            }
            else
            {
                if (camSeen[i])
                { //Cas objet visible qui passe invisible
                    Debug.Log(gameObject.name + " invisible");
                    string objectName = this.gameObject.name;
    
                    string filePath = Application.dataPath + "/../Positions/" + Camera.allCameras[i].name + ".txt";
                    float temps = Time.time-Camera.allCameras[i].GetComponent<MovieRecorderExample>().startTime;
                    using (StreamWriter writer = new StreamWriter(filePath, true))
                    {
                        writer.WriteLine(objectName + " plus vu après : " + (temps).ToString() + "s");
                    }
                }
                camSeen[i] = false;
            }
        }
    }
    private void Where(){ //on cherche la position de l'objet dans le champ de vision des caméras qui l'ont vu
        for (int i = 0; i < camSeen.Length; i++)
        {
            if(camSeen[i]){
                Vector2 smallMeshToViewX = meshToViewPortMinX(smallMesh, Camera.allCameras[i]);
                Vector2 smallMeshToViewY = meshToViewPortMinY(smallMesh, Camera.allCameras[i]);

                Vector2 lefttop = new Vector2(smallMeshToViewX.x,smallMeshToViewY.y);
                Vector2 rightbot = new Vector2(smallMeshToViewX.y,smallMeshToViewY.x);

                string filePath = Application.dataPath + "/../Positions/" + "Start"+ Camera.allCameras[i].name +".txt";
                float temps = Time.time-Camera.allCameras[i].GetComponent<MovieRecorderExample>().startTime;
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Objet " + this.name + " vu en :" + lefttop.ToString() + "," + rightbot.ToString());
                }
            }
        }
    }

    public Vector2 meshToViewPortMinX(Transform[] mesh, Camera cam){
        float minx = 1;
        float maxx = -1;
        Vector3[] meshToViewPort = new Vector3[mesh.Length];
        for (int i = 0; i < mesh.Length; i++)
        {
            meshToViewPort[i] = cam.WorldToViewportPoint(mesh[i].position);
            if(meshToViewPort[i].x < minx){
                minx = meshToViewPort[i].x;
            }
            if(meshToViewPort[i].x > maxx){
                maxx = meshToViewPort[i].x;
            }
        }
        return new Vector2(minx,maxx);
    }
    public Vector2 meshToViewPortMinY(Transform[] mesh, Camera cam){
        float miny = 1;
        float maxy = -1;
        Vector3[] meshToViewPort = new Vector3[mesh.Length];
        for (int i = 0; i < mesh.Length; i++)
        {
            meshToViewPort[i] = cam.WorldToViewportPoint(mesh[i].position);
            if(meshToViewPort[i].y < miny){
                miny = meshToViewPort[i].y;
            }
            if(meshToViewPort[i].y > maxy){
                maxy = meshToViewPort[i].y;
            }
        }
        return new Vector2(miny,maxy);
    }


}