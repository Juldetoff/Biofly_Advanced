using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartCity : StartDrone
{
    public GameObject route = null;
    private MeshCollider meshRoute = null;
    private int objectCnt = 0;
    private GameObject objectGenerated = null;
    // Start is called before the first frame update
    public GameObject[] prefabs;
    public int maxSpawnAttemps = 10;
    private int tryCnt = 0;

    void Start()
    {
        ExtractConfig();
        route = CheckRoute(this.transform.position);
        meshRoute = route.GetComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - lastTime > maxTime && Vector3.Distance(lastPos,Cams.cam.gameObject.transform.position) > distance){
            lastPos = Cams.cam.gameObject.transform.position; 
            for (int i = 0; i < 2; i++)
            {
                CreateObject();
            }
            lastTime = Time.time; 
        }

        noisecam.gameObject.transform.position = Cams.cam.gameObject.transform.position;
        noisecam.gameObject.transform.rotation = Cams.cam.gameObject.transform.rotation;
    }

    public GameObject CheckRoute(Vector3 pos){ //on lance un raycast vers le bas pour trouver le box collider de la route
        RaycastHit[] hits;
        hits = Physics.RaycastAll(pos, Vector3.down, 1000.0F);
        foreach (RaycastHit hit in hits)
        {
            if(hit.collider.gameObject.tag == "route"){
                route = hit.collider.gameObject;
                meshRoute = route.GetComponent<MeshCollider>();
                return hit.collider.gameObject;
            }
        }
        return null;
    }

    public void CreateObject(){
        Vector3 randomPoint = Vector3.zero;
        tryCnt = 0;

        while(randomPoint == Vector3.zero && tryCnt < maxSpawnAttemps){ //ici on tente d'obtenir un point aléatoire appartenant à la route
            randomPoint = RouteRandomPos(lastPos); 
            //randomPoint = Vector3.zero; //le temps de tester des draws
        }

        int prefabRand = UnityEngine.Random.Range(0, prefabs.Length+1); //ici on va s'occuper de la voiture qu'on génère

        if(prefabRand!=prefabs.Length && randomPoint != Vector3.zero){ 
            objectGenerated = cubeManager.CreateCube(randomPoint.x, randomPoint.y, randomPoint.z, prefabs[prefabRand]);
            objectGenerated.name = "voiture" +objectCnt;
            objectGenerated.tag = "obstacle";
            objectCnt++;

            objectGenerated.GetComponent<Render_dist>().SetCam(Cams.cam.gameObject);
            objectGenerated.GetComponent<Render_dist>().SetDistance(render_dist);

            // float size = UnityEngine.Random.Range(0.10f, 0.5f);
            // objectGenerated.transform.localScale = new Vector3(size, size, size); //voiture a taille fixe
            // objectGenerated.transform.position = new Vector3(
            //     objectGenerated.transform.position.x,
            //      objectGenerated.transform.position.y + (size - 1f) * 0.5f,
            //       objectGenerated.transform.position.z);
            objectGenerated.transform.position = new Vector3( //ici on positionne la voiture en fonction de sa hauteur
                objectGenerated.transform.position.x,
                 objectGenerated.transform.position.y + 0.5f,
                  objectGenerated.transform.position.z);

            RaycastHit hit; //le sol n'est pas exactement plat on oriente l'objet en fonction de la pente
            var ray = new Ray(objectGenerated.transform.position, Vector3.down); // check for slopes
            Debug.DrawRay(objectGenerated.transform.position, Vector3.down, Color.blue, 1000);
            if (meshRoute.Raycast(ray, out hit, 2000))
            {
                objectGenerated.transform.rotation = Quaternion.FromToRotation(
                objectGenerated.transform.up, hit.normal) * objectGenerated.transform.rotation; // adjust for slopes
                objectGenerated.transform.position = hit.point+Vector3.up*1.15f;
            }
            ray = new Ray(objectGenerated.transform.position, Vector3.up); // check for ground
            Debug.DrawRay(objectGenerated.transform.position, Vector3.up, Color.yellow, 1000);
            if (meshRoute.Raycast(ray, out hit, 1000))
            {
                objectGenerated.transform.position = hit.point+Vector3.up*1.5f;
            }
        }
        else
        {
            objectGenerated = null;
        }
        if (objectGenerated)
        {
            objectGenerated.GetComponent<Render_dist>().SetCam(Cams.cam.gameObject);
            objectGenerated.GetComponent<SoloDetectableScript>().setCam(Cams.cam.gameObject.GetComponent<Camera>());
        }
    }

    public Vector3 RouteRandomPos(Vector3 departPos){ //on cherche un point aléatoire sur la route autour du point de départ 
        if(tryCnt >= maxSpawnAttemps){ //on a essayé de trouver un point sur la route mais on a pas réussi
            return Vector3.zero;
        }

        int dist = Random.Range(7, 10);     
        int radius = Random.Range(20, 35); 
        Vector3 randomPoint = Vector3.zero;
        //droite vers le haut
        Vector3 spherePos = departPos + new Vector3(0, dist, 0); 
        Debug.DrawLine(departPos, spherePos, Color.red, 2000);

	    //sphere pour point random sphere, ou plutot cercle pour pas avoir de points au dessus ou en dessous?
        //Vector3 randomDir = Random.insideUnitSphere.normalized;
        Vector3 randomDir = Random.insideUnitCircle.normalized; //fait un cercle sur le plan xy (on veut zx)
        randomDir.z = randomDir.y;
        randomDir.y = 0;
        Vector3 randomA = spherePos + randomDir * radius;
        Debug.DrawLine(spherePos, randomA, Color.blue, 2000);

	    //raycast avec pour vecteur point->cam + parcourt en cherchant route
        RaycastHit[] hits;
        hits = Physics.RaycastAll(randomA, Cams.cam.gameObject.transform.position - randomA, 1000.0F);
        foreach (RaycastHit hit in hits)
        {
            if(hit.collider.gameObject.tag == "route" && hit.collider.GetType() == typeof(MeshCollider)){
                CheckRoute(hit.point);
                if(meshRoute!=null && !meshRoute.bounds.Contains(hit.point)){
                    Debug.DrawLine(randomA, hit.point, Color.green, 2000);
                    Debug.DrawLine(hit.point-Vector3.up, hit.point+Vector3.up, Color.green, 4);
                    Debug.DrawLine(hit.point-Vector3.right, hit.point+Vector3.right, Color.green, 4);
                    Debug.DrawLine(hit.point-Vector3.forward, hit.point+Vector3.forward, Color.green, 4);
                    randomPoint = hit.point;
                }
                else{
                    Debug.DrawLine(randomA, hit.point, Color.red, 2000);
                    Debug.DrawLine(hit.point-Vector3.up, hit.point+Vector3.up, Color.red, 4);
                    Debug.DrawLine(hit.point-Vector3.right, hit.point+Vector3.right, Color.red, 4);
                    Debug.DrawLine(hit.point-Vector3.forward, hit.point+Vector3.forward, Color.red, 4);
                    randomPoint = hit.point;
                }
            }
            else{ //sinon rince and repeat  
                tryCnt++;
                randomPoint = RouteRandomPos(departPos);
            }
            if(randomPoint != Vector3.zero){
                break;
            }
        }

        // float r = UnityEngine.Random.Range(5,radius);
        // Vector3 randomDirection = UnityEngine.Random.insideUnitSphere.normalized;
        // randomPoint = lastPos + randomDirection * r;
        // Debug.DrawLine(departPos, randomPoint, Color.green, 2000);
        // CheckRoute(randomPoint);
        

        // if(!meshRoute.bounds.Contains(randomPoint)){ //pn essaye de forcer le point à être sur la route
        //     Debug.Log("out of bounds");
        //     Debug.DrawLine(randomPoint, meshRoute.bounds.ClosestPoint(randomPoint), Color.blue, 2000);
        //     randomPoint = meshRoute.bounds.ClosestPoint(randomPoint);
        // }
        // else{
        //     RaycastHit hit;
        //     if (Physics.Raycast(randomPoint, Vector3.down, out hit))
        //     {
        //         Debug.DrawLine(randomPoint, hit.point, Color.red, 2000);
        //         randomPoint.y = hit.point.y;
        //     }
        //     else if (Physics.Raycast(randomPoint, Vector3.up, out hit))
        //     {
        //         Debug.DrawLine(randomPoint, hit.point, Color.red, 2000);
        //         randomPoint.y = hit.point.y;
        //     }
        //     else{
        //         randomPoint = RouteRandomPos(departPos);
        //     }
        // }
        // randomPoint.x = Random.Range(meshRoute.bounds.min.x, meshRoute.bounds.max.x);
        // randomPoint.z = Random.Range(meshRoute.bounds.min.z, meshRoute.bounds.max.z);

        return randomPoint;        
    }
}
