using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe de démarrage du drone en ville. Fonctionne avec la scène "M_City".
/// </summary>
public class StartCity : StartDrone
{
    private GameObject route = null;
    private MeshCollider meshRoute = null;
    private int objectCnt = 0;
    private GameObject objectGenerated = null;
    // Start is called before the first frame update
    [Tooltip("Liste des prefabs pouvant apparaître dans la scène.")]public GameObject[] prefabs; //fixé ) 36 dans ce cas car 0-30 voitures et 30-36 personnes
    [Tooltip("Nombre maximum d'essais de génération")]public int maxSpawnAttemps = 10;
    private int tryCnt = 0;
    [Tooltip("Nombre d'objets à générer à chaque génération.")]public int GenerateCount = 2;
    [Tooltip("Apparition ou non de voitures.")]public bool spawnCars = false; //prefabs de 0 à 30 (exclus, normalement si tout est bien configuré)
    [Tooltip("Apparition ou non de personnes.")]public bool spawnPeople = false; //prefabs de 30 à 36

    void Start()
    {
        ExtractConfig();
        route = CheckRoute(this.transform.position);
        meshRoute = route.GetComponent<MeshCollider>();

        if(jello){
            Cams.cam.gameObject.GetComponent<HDRP_RollingShutter>().enabled = true;
            VCams.cam.gameObject.GetComponent<HDRP_RollingShutter>().enabled = true; 
        }
    }

    void Update()
    {
        if(Time.time - lastTime > maxTime && Vector3.Distance(lastPos,Cams.cam.gameObject.transform.position) > distance){
            lastPos = Cams.cam.gameObject.transform.position; 
            for (int i = 0; i < GenerateCount; i++)
            {
                CreateObject();
                //Debug.Log("object generated :" + i);
            }
            lastTime = Time.time; 
        }

        noisecam.gameObject.transform.position = Cams.cam.gameObject.transform.position;
        noisecam.gameObject.transform.rotation = Cams.cam.gameObject.transform.rotation;

        //on met à jour le sol pour le mouvement :
        RaycastHit[] hits;
        hits = Physics.RaycastAll(Cams.cam.gameObject.transform.position, Vector3.down, 1000.0F);
        foreach (RaycastHit hit in hits)
        {
            if(hit.collider.gameObject.tag == "route"){
                route = hit.collider.gameObject;
                meshRoute = route.GetComponent<MeshCollider>();
            }
            if(hit.collider.GetComponent<Terrain>() != null){
                terrain = hit.collider.GetComponent<Terrain>();
                Cams.vcam.gameObject.GetComponent<DroneCameraMovement>().SetTerrain(terrain);
            }
        }
    }

    /// <summary>
    /// Fonction qui renvoie un booléen indiquant si la position donnée est au dessus de la route.
    /// </summary>
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

    /// <summary>
    /// Fonction qui génère un objet aléatoire sur la route.
    /// </summary>
    public void CreateObject(){
        Vector3 randomPoint = Vector3.zero;
        tryCnt = 0;

        while(randomPoint == Vector3.zero && tryCnt < maxSpawnAttemps){ //ici on tente d'obtenir un point aléatoire appartenant à la route
            randomPoint = RouteRandomPos(lastPos); 
            //randomPoint = Vector3.zero; //le temps de tester des draws
        }

        int prefabRand = UnityEngine.Random.Range(0, prefabs.Length+1); //ici on va s'occuper de la voiture qu'on génère (max+1 pour une chance de ne pas avoir d'objet qui apparaisse)
        if(spawnCars && !spawnPeople){
            prefabRand = UnityEngine.Random.Range(0, 30);
        }
        else if(spawnPeople && !spawnCars){
            prefabRand = UnityEngine.Random.Range(30, 36);
        }
        else if(!spawnCars && !spawnPeople){ //pas super opti comme méthode mais on fait avec ce qu'on a
            prefabRand = prefabs.Length+1; //cas pas d'objet du coup
        }

        if(prefabRand!=prefabs.Length && randomPoint != Vector3.zero){ 

            objectGenerated = cubeManager.CreateCube(randomPoint.x, randomPoint.y, randomPoint.z, prefabs[prefabRand]);
            if(prefabRand < 30){
                objectGenerated.name = "voiture" +objectCnt;
            }
            else{
                objectGenerated.name = "personne" +objectCnt;
                objectGenerated.transform.GetChild(1).gameObject.name = "personne" +objectCnt;
                // objectGenerated = objectGenerated.transform.GetChild(1).gameObject;
            }
            objectGenerated.tag = "obstacle";
            objectCnt++;

            objectGenerated.transform.position = new Vector3( //ici on positionne l'objet en fonction de sa hauteur
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
            if(prefabRand >= 30){ //on repositionne parce que la foule a du mal à se placer
                objectGenerated.transform.position = new Vector3(
                    objectGenerated.transform.position.x,
                    objectGenerated.transform.position.y - 1.15f,
                    objectGenerated.transform.position.z);
                objectGenerated = objectGenerated.transform.GetChild(1).gameObject;
            }
            objectGenerated.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            objectGenerated.GetComponent<SoloDetectableScript>().setCam(Cams.cam.gameObject.GetComponent<Camera>());
        }
    }

    /// <summary>
    /// Fonction qui renvoie un point aléatoire sur la route autour du point de départ.
    /// </summary>
    public Vector3 RouteRandomPos(Vector3 departPos){ //on cherche un point aléatoire sur la route autour du point de départ 
        if(tryCnt >= maxSpawnAttemps){ //on a essayé de trouver un point sur la route mais on a pas réussi
            Debug.Log("pas de point trouvé");
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
        return randomPoint;        
    }
}
