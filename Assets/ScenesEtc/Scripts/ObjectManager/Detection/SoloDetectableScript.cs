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
    public Transform[] smallMesh;
    
    private GameObject lineRenderer = null;

    // Start is called before the first frame update

    public void setCam(Camera cam)
    {
        this.cam = cam;
    }
    public void SetSmallMesh(Transform[] transfo){
        this.smallMesh = transfo;
    }

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

    private void Where(){
        if(almostVisible){
            Destroy(lineRenderer);
            lineRenderer = null;

            Vector2 smallMeshToViewX = meshToViewPortMinX(smallMesh);
            Vector2 smallMeshToViewY = meshToViewPortMinY(smallMesh);
            // Vector2 lefttop = new Vector2(Minx(smallMesh),Maxy(smallMesh));
            // Vector2 righttop = new Vector2(Maxx(smallMesh),Maxy(smallMesh));
            // Vector2 leftbot = new Vector2(Minx(smallMesh),Miny(smallMesh));
            // Vector2 rightbot = new Vector2(Maxx(smallMesh),Miny(smallMesh));

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
