using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Recorder.Examples;

/// <summary>
/// Classe qui permet de savoir si un objet est vu par une caméra ou non et de savoir où il est dans le champ de vision de la caméra dans le cas où la scène est automatique.
/// </summary>
public class DetectableScript : MonoBehaviour
{
    private bool[] camSeen;
    [Tooltip("Liste des points permettant de produire un carré les englobant dont les coordonnées seront enregistrés dans un txt.")]public Transform[] smallMesh;

    
    private void Start() {
        this.gameObject.tag = "obstacle";
        camSeen = new bool[Camera.allCamerasCount];
        for(int i = 0; i<Camera.allCamerasCount; i++){
            camSeen[i] = false; //initialise le tableau à faux
        }
    }

    private void Update()
    {
        //partie cam
        Seen();
        Where();
    }

    /// <summary>
    /// Permet de changer le tableau des positions des points. Utilisés afin d'adapter les points utilisé dans SoloDetectableScript ici.
    /// </summary>
    public void SetSmallMesh(Transform[] transfo){
        this.smallMesh = transfo;
    }

    /// <summary>
    /// Permet de récupérer le tableau des positions des points. 
    /// </summary>
    public Transform[] GetSmallMesh(){
        return smallMesh;
    }

    /// <summary>
    /// Permet de vérifier si l'objet est vu par une caméra. Si c'est le cas, on vérifie si l'objet est caché par un autre objet. Si c'est le cas, on ne fait rien. Sinon, on enregistre le temps de début de visibilité de l'objet.
    /// Si l'objet passe invisible par une caméra, on enregistre le temps de fin de visibilité de l'objet.
    /// </summary>
    private void Seen() //on vérifie si l'objet est vu par une caméra. Il s'occupe également de remplir le tableau des bools
    {
        for (int i = 0; i < camSeen.Length; i++)
        {
            Vector3 scenePos = Camera.allCameras[i].WorldToViewportPoint(this.transform.position);
            Vector3 viewportPos = scenePos;
            if (viewportPos.x > 0 && viewportPos.x <= 1 &&
                viewportPos.y > 0 && viewportPos.y <= 1 && viewportPos.z > 0)
            {
                //Debug.DrawLine(Camera.allCameras[i].transform.position, this.transform.position, Color.red, 1000f);
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

    /// <summary>
    /// Permet d'enregistrer la position de l'objet dans le champ de vision des caméras qui l'ont vu.
    /// </summary>
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

    /// <summary>
    /// Permet de récupérer les coordonnées du carré englobant l'objet dans le champ de vision de la caméra.
    /// </summary>
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

    /// <summary>
    /// Permet de récupérer les coordonnées du carré englobant l'objet dans le champ de vision de la caméra.
    /// </summary>
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