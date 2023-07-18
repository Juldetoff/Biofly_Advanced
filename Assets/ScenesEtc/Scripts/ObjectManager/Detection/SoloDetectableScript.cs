using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Recorder.Examples;

/// <summary>
/// Classe qui permet de savoir si un objet est vu par une caméra ou non 
/// et de savoir où il est dans le champ de vision de la caméra dans le cas où la scène est manuelle et qu'il n'y a qu'une seule caméra connue.
/// </summary>
public class SoloDetectableScript : MonoBehaviour
{
    Camera camtrouvé;
    [Tooltip("Caméra pourant détecter l'objet.")]public Camera cam;
    bool almostVisible=false;
    [Tooltip("Liste des points permettant de produire un carré les englobant dont les coordonnées seront enregistrés dans un txt.")]public Transform[] smallMesh;
    
    private GameObject lineRenderer = null;

    // Start is called before the first frame update

    /// <summary>
    /// Permet de changer la caméra utilisée pour la détection.
    /// </summary>
    public void setCam(Camera cam)
    {
        this.cam = cam;
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

    private void Start() {
        this.tag = "obstacle";
        camtrouvé = cam;
    }

    private void Update()
    {
        //partie cam
        Seen();
        Where();
    }

    /// <summary>
    /// Permet de vérifier si l'objet est vu par une caméra. Si c'est le cas, on vérifie si l'objet est caché par un autre objet. Si c'est le cas, on ne fait rien. Sinon, on enregistre le temps de début de visibilité de l'objet.
    /// Si l'objet passe invisible par une caméra, on enregistre le temps de fin de visibilité de l'objet.
    /// </summary>
    private void Seen() //on regarde à chaque instant si l'objet est vu par une des caméras
    {
        Vector3 scenePos = cam.WorldToViewportPoint(this.transform.position);
        Vector3 viewportPos = scenePos;

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

    /// <summary>
    /// Permet d'enregistrer la position de l'objet dans le champ de vision des caméras qui l'ont vu.
    /// </summary>
    private void Where(){
        if(almostVisible){
            Destroy(lineRenderer);
            lineRenderer = null;

            Vector2 smallMeshToViewX = meshToViewPortMinX(smallMesh);
            Vector2 smallMeshToViewY = meshToViewPortMinY(smallMesh);

            Vector2 lefttop = new Vector2(smallMeshToViewX.x,smallMeshToViewY.y);
            Vector2 rightbot = new Vector2(smallMeshToViewX.y,smallMeshToViewY.x);

            //pour tester, on va instancier un lineRenderer aux positions pour afficher un rectangle
            // lineRenderer = new GameObject();
            // lineRenderer.AddComponent<LineRenderer>();
            // LineRenderer lr = lineRenderer.GetComponent<LineRenderer>();
            // lr.material = new Material(Shader.Find("Sprites/Default"));
            // lr.startColor = Color.red;
            // lr.endColor = Color.red;
            // lr.startWidth = 0.1f;
            // lr.endWidth = 0.1f;
            // lr.positionCount = 5;
            // Debug.Log("lefttop : " + lefttop.ToString());
            // Debug.Log("rightbot : " + rightbot.ToString());
            // float z = Vector3.Distance(cam.transform.position, this.transform.position);
            // lr.SetPosition(0, cam.ViewportToWorldPoint(new Vector3(lefttop.x,lefttop.y,z)));
            // lr.SetPosition(1, cam.ViewportToWorldPoint(new Vector3(rightbot.x,lefttop.y,z)));
            // lr.SetPosition(2, cam.ViewportToWorldPoint(new Vector3(rightbot.x,rightbot.y,z)));
            // lr.SetPosition(3, cam.ViewportToWorldPoint(new Vector3(lefttop.x,rightbot.y,z)));
            // lr.SetPosition(4, cam.ViewportToWorldPoint(new Vector3(lefttop.x,lefttop.y,z)));

    
            string filePath = Application.dataPath + "/../Positions/" + "Start"+ camtrouvé.name +".txt";
            float temps = Time.time-cam.GetComponent<MovieRecordManual>().startTime; 
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine("Objet " + this.name + " vu en :" + lefttop.ToString() + "," + rightbot.ToString());
            }
        }
    }

    /// <summary>
    /// Permet de récupérer les coordonnées du carré englobant l'objet dans le champ de vision de la caméra.
    /// </summary>
    public Vector2 meshToViewPortMinX(Transform[] mesh){
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
    public Vector2 meshToViewPortMinY(Transform[] mesh){
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
