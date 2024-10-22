using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using Cinemachine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Recorder.Examples;
using UnityEngine.SceneManagement;

/// <summary>
/// Classe qui permet de générer des caméras et chemins automatiquement dans la scene "A_City". Elle étend la classe StartScript.
/// </summary>
public class AutoCityStart : StartScript
{
    [Header("Paramètres de cette classe")] //car on ne peut pas réorganiser les header dans le fils, ça se met à la fin
    [Tooltip("Coin de la carte 1")]public GameObject corner1 = null;
    public GameObject corner2 = null;
    public GameObject route = null;
    private MeshCollider meshRoute = null;
    private GameObject objectGenerated = null;
    [Tooltip("Prefabs pouvant apparaître dans la scènes en tant qu'objets.")]public GameObject[] prefabs; //est fixé à 36 pour l'instant, pour ajouter plus il faudra modifier en brut car 0-30 sont les voitures et 30-36 les personnes
    [Tooltip("Nombre d'essais maximum de génération.")]public int maxSpawnAttemps = 100; 
    private int tryCnt = 0;
    private int tryCnt2 = 0;
    [Tooltip("Apparition ou non de voitures.")]public bool spawnCars = false; //prefabs de 0 à 30 (exclus, normalement si tout est bien configuré)
    [Tooltip("Apparition ou non de personnages.")]public bool spawnPeople = false; //prefabs de 30 à 36
    private Vector3 pointTemp = new Vector3(0,0,0);


    // Start est appelé avant la première frame 
    void Start()
    {
        ExtractConfig();
        PrepareTimeline();
        
        for (int i = 0; i < nBCamera; i++)
        {
            CinemachineSmoothPath Cpath = GeneratePath();

            SysCam sysCam = GenerateCam(i, Cpath);
            GenerateNormalTrack(i, sysCam);
            sysCam.cam.GetComponent<MovieRecorderExample>().startVideo = true;
            sysCam.cam.GetComponent<Camera>().depth = 2;

            SysCam sysCamBruit = GenerateNoiseCam(i, Cpath);
            GenerateNoiseTrack(i, sysCamBruit);
            sysCamBruit.cam.GetComponent<MovieRecorderExample>().startVideo = true;
            sysCamBruit.cam.GetComponent<Camera>().depth = 1;
        }

        director.Stop();
        director.time = 0;
        director.Play();
    }

    // Update est appelé à chaque frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)){
            Restart();
        }
        CheckFinished();
        if(finished){
            foreach (MovieRecorderExample item in cams)
            {
                item.DisableVideo();
            }
        }
        if(finished && repeat){
            Restart();
        }
        if(Input.GetKeyDown(KeyCode.R)){
            repeat = !repeat;
            Debug.Log("repeat: " + repeat);
        }
    }

    /// <summary>
    /// GeneratePath ici génère un chemin de caméra aléatoire dans la zone définie par les deux coins de la carte dans la ville.
    private CinemachineSmoothPath GeneratePath()
    {
        point = GetRandomPoint();
        point = GeneratePoint(point, radius);
        point.y = point.y + 30;
        float y = getRoute(point);
        point.y = y + offsetcam;

        CinemachineSmoothPath Cpath = Instantiate(Prefabpath, new Vector3(0, 0, 0), Quaternion.identity);
        Cpath.m_Waypoints = new CinemachineSmoothPath.Waypoint[pointCnt];
        Cpath.m_Waypoints[0].position = point;
        for (int i = 1; i < pointCnt; i++)
        {
            pointTemp = GeneratePoint(point, radius);
            y = getRoute(pointTemp); //ici route assuré par generatepoint sinon rip

            pointTemp.y = y + offsetcam;
            point = pointTemp;
            Cpath.m_Waypoints[i].position = point;

            for (int j = 0; j < numObject; j++)
            {
                GenerateObjectOnPath(out Vector3 objPoint, out int prefabRand);
            }
        }
        return Cpath;
    }

    /// <summary>
    /// GenerateObjectOnPath génère un objet sur le chemin de caméra. Il est appelé par GeneratePath. Cette version adaptée à la ville
    /// permet de générer des personnes ou des voitures sur la route.
    /// </summary>
    private void GenerateObjectOnPath(out Vector3 objPoint, out int prefabRand)
    {
        objPoint = GeneratePoint(point, radius);
        objPoint.y = getRoute(objPoint) + offsetcam; //encore une fois, getroute assure la route sinon rip
        prefabRand = UnityEngine.Random.Range(0, prefabs.Length + 1); //ici on va s'occuper de la voiture qu'on génère (max+1 pour une chance de ne pas avoir d'objet qui apparaisse)
        if (spawnCars && !spawnPeople)
        {
            prefabRand = UnityEngine.Random.Range(0, 30);
        }
        else if (spawnPeople && !spawnCars)
        {
            prefabRand = UnityEngine.Random.Range(30, 36);
        }
        else if (!spawnCars && !spawnPeople)
        { //pas super opti comme méthode mais on fait avec ce qu'on a
            prefabRand = prefabs.Length + 1; //cas pas d'objet du coup
        }
        if (prefabRand >= 30 && prefabRand < 36)
        {
            objPoint.y += 0.5f;
        }
        if (prefabRand != prefabs.Length + 1 && objPoint != Vector3.zero)
        {
            objectGenerated = cubeManager.CreateCube(objPoint.x, objPoint.y, objPoint.z, prefabs[prefabRand]);

            if (prefabRand < 30)
            {
                objectGenerated.name = "voiture" + count;
                Destroy(objectGenerated.GetComponent<Render_dist>());
                objectGenerated.AddComponent<DetectableScript>();
                Transform[] transfoList = objectGenerated.GetComponent<SoloDetectableScript>().GetSmallMesh();
                objectGenerated.GetComponent<DetectableScript>().SetSmallMesh(transfoList);
                Destroy(objectGenerated.GetComponent<SoloDetectableScript>());
            }
            else
            {
                objectGenerated.name = "personne" + count;
                Destroy(objectGenerated.GetComponent<Render_dist>());
                objectGenerated.transform.GetChild(1).gameObject.name = "personne" + count;
                objectGenerated.transform.GetChild(1).gameObject.AddComponent<DetectableScript>();
                Transform[] transfoList = objectGenerated.transform.GetChild(1).gameObject.GetComponent<SoloDetectableScript>().GetSmallMesh();
                objectGenerated.transform.GetChild(1).gameObject.GetComponent<DetectableScript>().SetSmallMesh(transfoList);
                Destroy(objectGenerated.transform.GetChild(1).gameObject.GetComponent<SoloDetectableScript>());
                // objectGenerated = objectGenerated.transform.GetChild(1).gameObject;
            }
            objectGenerated.tag = "obstacle";
            count++;

            objectGenerated.transform.position = new Vector3( //ici on positionne l'objet en fonction de sa hauteur
                objectGenerated.transform.position.x,
                objectGenerated.transform.position.y + 0.5f,
                objectGenerated.transform.position.z);

            if (meshRoute)
            {
                RaycastHit hit; //le sol n'est pas exactement plat on oriente l'objet en fonction de la pente
                var ray = new Ray(objectGenerated.transform.position, Vector3.down); // check for slopes
                Debug.DrawRay(objectGenerated.transform.position, Vector3.down, Color.blue, 1000);
                if (meshRoute.Raycast(ray, out hit, 2000))
                {
                    objectGenerated.transform.rotation = Quaternion.FromToRotation(
                    objectGenerated.transform.up, hit.normal) * objectGenerated.transform.rotation; // adjust for slopes
                    objectGenerated.transform.position = hit.point + Vector3.up * 1.15f;
                }
                ray = new Ray(objectGenerated.transform.position, Vector3.up); // check for ground
                Debug.DrawRay(objectGenerated.transform.position, Vector3.up, Color.yellow, 1000);
                if (meshRoute.Raycast(ray, out hit, 2000))
                {
                    objectGenerated.transform.position = hit.point + Vector3.up * 1.5f;
                }
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                //cas rare où on n'a pas de route et l'algo n'arrive pas à en trouver, du coup il fait avec mais ça snowball sur le reste. On skip le problème en relançant la scène
            }
        }
        else
        {
            objectGenerated = null;
        }
        if (objectGenerated)
        {
            if (prefabRand >= 30)
            { //on repositionne parce que la foule a du mal à se placer
                objectGenerated.transform.position = new Vector3(
                    objectGenerated.transform.position.x,
                    objectGenerated.transform.position.y - 1.15f,
                    objectGenerated.transform.position.z);
                objectGenerated = objectGenerated.transform.GetChild(1).gameObject;
            }
            objectGenerated.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
        }
    }

    /// <summary>
    /// GeneratePoint génère un point aléatoire dans la zone définie par les deux coins de la carte dans la ville.
    /// Si le point n'est pas sur la route, on en génère un autre, jusqu'à atteindre le maximum d'essais.
    /// </summary>
    private Vector3 GeneratePoint(Vector3 point, float radius) //sachant point sur la route, il faut générer un point également sur la route à distance de plus ou moins radius
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere.normalized * radius;
        Vector3 randomPoint = point + randomDirection * radius;
        if(CheckRoute(randomPoint)){
            tryCnt2 = 0;
            return randomPoint;
        }
        else{
            tryCnt2++;
            if(tryCnt2 < maxSpawnAttemps){
                return GeneratePoint(point, radius);
            }
            else{
                tryCnt2 = 0;
                //return Vector3.zero;
                return randomPoint;
            }
        }
    }

    /// <summary>
    /// CheckRoute vérifie si le point donné est sur la route.
    /// </summary>
    private bool CheckRoute(Vector3 point)
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(point, Vector3.down, 100.0F);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.tag == "route")
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// GetRandomPoint génère un point aléatoire dans la zone définie par les deux coins de la carte dans la ville.
    /// </summary>
    private Vector3 GetRandomPoint()
    {
        float maxx = Mathf.Max(corner1.transform.position.x, corner2.transform.position.x);
        float minx = Mathf.Min(corner1.transform.position.x, corner2.transform.position.x);
        float maxz = Mathf.Max(corner1.transform.position.z, corner2.transform.position.z);
        float minz = Mathf.Min(corner1.transform.position.z, corner2.transform.position.z);
        return new Vector3(
            UnityEngine.Random.Range(minx, maxx),
            corner1.transform.position.y,
            UnityEngine.Random.Range(minz, maxz)
        );
    }

    /// <summary>
    /// getRoute récupère la route la plus proche du point donné.
    /// </summary>
    private float getRoute(Vector3 point)
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(point, Vector3.down, 1000.0F);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.tag == "route")
            {
                route = hit.collider.gameObject;
                meshRoute = route.GetComponent<MeshCollider>();
                tryCnt = 0;
                return hit.point.y;
            }
        }
        if(tryCnt < maxSpawnAttemps){ //on a pas trouvé de route, on réessaye
            tryCnt++;
            this.pointTemp = GeneratePoint(this.point, radius);
            return getRoute(this.pointTemp); //prions pour pas de boucle infinie parce qu'il faut une route
        }
        else{
            return 0;
        }
    }
}
