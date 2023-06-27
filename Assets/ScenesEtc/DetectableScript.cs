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
    bool almostVisible=false;
    public bool[] camSeen;
    //TODO: retirer les deux en dessous
    Camera camtrouvé;
    public Camera cam;

    // Start is called before the first frame update

    
    private void Start() {
        camSeen = new bool[Camera.allCamerasCount];
        //on nettoie le txt de positions puis on cherche la position au cas où on apparaisse déjà dans le champ de vision
        string filePath = Application.dataPath + "/../Positions/" + "detections.txt";
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            writer.WriteLine("Positions des objets :");
        }
        Where();
        //je dirai d'abord de faire une liste de boolean, pour chaque caméra afin de savoir laquelle voit l'objet au temps t
        //puis on fait une liste de string pour chaque caméra, qui contient les positions de l'objet au temps t
        //vérifier dans update chaque caméra, si elle voit l'objet
        //je pense pour les positions, il faut faire un fichier txt par objet par caméra
        //donc pour n objets et m caméras, n*m fichiers txt en théorie
        
    //     foreach (Camera cam in Camera.allCameras)
    // {
    //     int n = cam.name.Length;
    //     if((int)cam.name[n-1] % 2 == 0)
    //     {// Convert object position to screen space
    //     Vector3 screenPos = cam.WorldToScreenPoint(this.transform.position);

    //     // Convert screen space to viewport space
    //     Vector3 viewportPos = cam.ScreenToViewportPoint(screenPos);

    //     // Check if viewport position is within the camera's viewport
    //     if (viewportPos.x > 0 && viewportPos.x < 1 &&
    //         viewportPos.y > 0 && viewportPos.y < 1)
    //     {
    //         // Return the camera that is seeing the object
    //         camtrouvé = cam;
    //         string objectName =this.gameObject.name;
    //         DateTime currentTime = DateTime.Now;

    //         string filePath = Application.dataPath + "/../Positions/"+ camtrouvé.name + ".txt";
    //         using (StreamWriter writer = new StreamWriter(filePath, true))
    //         {
    //             writer.WriteLine("Objet : " + objectName + " vu à : " + ( currentTime - timestart).ToString());
    //         }
    //     }}
    }

    private void Update()
    {
        //partie cam
        Seen();
        Where();
    }

    private void Seen() //on vérifie si l'objet est vu par une caméra. Il s'occupe également de remplir le tableau des bools
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
                        writer.WriteLine(objectName + " vu après : " + (temps).ToString() + "s");
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
                        writer.WriteLine( objectName + " plus vu après : " + (temps).ToString() + "s");
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
    private void Where(){ //on cherche la position de l'objet dans le champ de vision des caméras qui l'ont vu
        if(almostVisible){
            Vector3 ltf = cam.WorldToViewportPoint(this.GetComponent<QrCube>().ltf.position);
            Vector3 ltb = cam.WorldToViewportPoint(this.GetComponent<QrCube>().ltb.position);
            Vector3 ldf = cam.WorldToViewportPoint(this.GetComponent<QrCube>().ldf.position);
            Vector3 ldb = cam.WorldToViewportPoint(this.GetComponent<QrCube>().ldb.position);
            Vector3 rtf = cam.WorldToViewportPoint(this.GetComponent<QrCube>().rtf.position);
            Vector3 rtb = cam.WorldToViewportPoint(this.GetComponent<QrCube>().rtb.position);
            Vector3 rdf = cam.WorldToViewportPoint(this.GetComponent<QrCube>().rdf.position);
            Vector3 rdb = cam.WorldToViewportPoint(this.GetComponent<QrCube>().rdb.position);
    
            Vector2 lefttop =new Vector2(Minhuit(ltf.x,ltb.x,ldf.x,ldb.x,rtf.x,rtb.x,rdf.x,rdb.x),Maxhuit(ltf.y,ltb.y,ldf.y,ldb.y,rtf.y,rtb.y,rdf.y,rdb.y));
            Vector2 righttop = new Vector2(Maxhuit(ltf.x,ltb.x,ldf.x,ldb.x,rtf.x,rtb.x,rdf.x,rdb.x),Maxhuit(ltf.y,ltb.y,ldf.y,ldb.y,rtf.y,rtb.y,rdf.y,rdb.y));
            Vector2 leftbot = new Vector2(Minhuit(ltf.x,ltb.x,ldf.x,ldb.x,rtf.x,rtb.x,rdf.x,rdb.x),Minhuit(ltf.y,ltb.y,ldf.y,ldb.y,rtf.y,rtb.y,rdf.y,rdb.y));
            Vector2 rightbot = new Vector2(Maxhuit(ltf.x,ltb.x,ldf.x,ldb.x,rtf.x,rtb.x,rdf.x,rdb.x),Minhuit(ltf.y,ltb.y,ldf.y,ldb.y,rtf.y,rtb.y,rdf.y,rdb.y));
    
            string filePath = Application.dataPath + "/../Positions/" + this.name + ".txt";
            float temps = Time.time-cam.GetComponent<MovieRecordManual>().startTime;
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(this.name +" vu à : " + (temps).ToString() + "s");
                writer.WriteLine("lefttop : " + lefttop.ToString());
                writer.WriteLine("righttop : " + righttop.ToString());
                writer.WriteLine("leftbot : " + leftbot.ToString());
                writer.WriteLine("rightbot : " + rightbot.ToString());
                writer.WriteLine("/////");
            }
        }
    }

    public float Minhuit(float a, float b, float c, float d, float e, float f, float g, float h){
        return Math.Min(Math.Min(Math.Min(Math.Min(Math.Min(Math.Min(Math.Min(a,b),c),d),e),f),g),h);
    }
    public float Maxhuit(float a, float b, float c, float d, float e, float f, float g, float h){
        return Math.Max(Math.Max(Math.Max(Math.Max(Math.Max(Math.Max(Math.Max(a,b),c),d),e),f),g),h);
    }

}