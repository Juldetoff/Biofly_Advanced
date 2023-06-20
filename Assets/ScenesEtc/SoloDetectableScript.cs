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
        //on nettoie le txt de positions puis on cherche la position au cas où on apparaisse déjà dans le champ de vision
        string filePath = Application.dataPath + "/../Positions/" + this.name + ".txt";
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            writer.WriteLine("Positions de l'objet " + this.name + " :");
        }
        Where();
    }

    private void Update()
    {
        //partie cam
        Seen();
        Where();
    }

    private void Seen()
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
                writer.WriteLine("Objet vu à : " + (temps).ToString() + "s");
                writer.WriteLine("lefttop : " + lefttop.ToString());
                writer.WriteLine("righttop : " + righttop.ToString());
                writer.WriteLine("leftbot : " + leftbot.ToString());
                writer.WriteLine("rightbot : " + rightbot.ToString());
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
