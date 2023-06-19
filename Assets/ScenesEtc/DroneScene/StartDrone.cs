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
public class StartDrone : MonoBehaviour
{
    ////////////////////////////////////////
    //Variable de config
    public int nBCamera =1;
    public float bruitAmplitude=0;
    public float bruitFrequency=0;
    public int noiseNumber = 0;
    public int videoType = 0;
    public int videoQuality = 0;
    public int videoFps = 60;
    private Vector3 point = new Vector3(0, 0, 0); //point de départ du chemin modifié à chaque fois
    public float start=0f;//date de début de la vidéo

    ////////////////////////////////////////
    //Objets et paramètres de génération

    public Terrain terrain = null; //le terrain dans lequel on va se balader (afin d'obtenir son y de surface)
    private Vector3 terrainPos = new Vector3(0, 0, 0);//la position du terrain

    public float radius = 40; //rayon du cercle dans lequel on va générer les points
    
    public GameObject cubePrefab = null;
    public CubeManager cubeManager = null;
    public GameObject tigrePrefab = null;
    public GameObject taureauPrefab = null;
    public GameObject aiglePrefab = null;
    private int objectCount = 0;

    public SysCam Cams;
    public GameObject flouPane;

    public NoiseSettings[] noiseSettings = new NoiseSettings[9]; //liste des types de bruits afin de pouvoir les associer à la caméra
    public string[] qualitySettings = new string[3]{"Low", "Medium", "High"}; //liste des qualités de vidéo
    public string[] formatSettings = new string[3]{"mp3", "mov", "webm"}; //liste des formats de vidéo
    
    public Vector3 lastPos = new Vector3(0, 0, 0);
    public float lastTime = 0;
    public float maxTime = 5;
    public float distance = 100;
    public float render_dist = 100;
    public float offset = 39.3f;

    private GameObject generatedObject = null;

    public CinemachineVirtualCamera noisecam;
    public MovieRecordManual[] movieRecordManuals;

    // Start is called before the first frame update
    void Start()
    {
        //ExtractConfig(); //on déplace dans Awake afin de pouvoir affecter les caméras ?

        terrainPos = terrain.transform.position; //on récupère la position du terrain
        start = 0;
    }

    private void Awake() {
        ExtractConfig();
    }

    public void ExtractConfig()
    {
        ////////////////////////////////////////
        // PARTIE EXTRACTION DU FICHIER CONFIG
        string path = "./config.txt";
        StreamReader reader = new StreamReader(path);
        // n the number of lines in the file
        int n = int.Parse(reader.ReadLine());
        // Read the lines
        for (int i = 1; i < n + 1; i++)
        {
            string[] line = reader.ReadLine().Split(',');
            if (line[0] == "scene")
            {
                //Nothing
            }
            else if (line[0] == "nBCamera")
            {
                this.nBCamera = 1; //manuel donc 1
            }
            else if (line[0] == "bruitAmplitude")
            {
                this.bruitAmplitude = (float)Convert.ToDouble(line[1]);
            }
            else if (line[0] == "bruitFrequency")
            {
                this.bruitFrequency = (float)Convert.ToDouble(line[1]);
            }
            else if (line[0] == "bruitType")
            {
                this.noiseNumber = Convert.ToInt32(line[1]);
            }
            else if (line[0] == "flou")
            {
                this.flouPane.SetActive(Convert.ToInt32(line[1]) == 1);
            }
            else if (line[0] == "videoType")
            {
                this.videoType = Convert.ToInt32(line[1]);
            }
            else if (line[0] == "videoQuality")
            {
                this.videoQuality = Convert.ToInt32(line[1]);
            }
            else if (line[0] == "fps")
            {
                this.videoFps = Convert.ToInt32(line[1]);
            }
            else if (line[0] == "jello")
            {
                //this.jello = Convert.ToInt32(line[1]);
            }
        }
        lastPos = Cams.cam.gameObject.transform.position - new Vector3(0, 500, 0);
        lastTime = 0;

        noisecam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = bruitAmplitude;
        noisecam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = bruitFrequency;
        noisecam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_NoiseProfile = noiseSettings[noiseNumber];
        foreach (MovieRecordManual mov in movieRecordManuals)
        {
            mov.startvideo = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        start += Time.deltaTime;
        lastTime += Time.deltaTime;
        
        if(lastTime > maxTime && Vector3.Distance(lastPos,Cams.cam.gameObject.transform.position) > distance){
            lastPos = Cams.cam.gameObject.transform.position;
            for (int i = 0; i < 4; i++)
            {
                CreateObject();
            }
            lastTime = 0; 
        }

        noisecam.gameObject.transform.position = Cams.cam.gameObject.transform.position;
        noisecam.gameObject.transform.rotation = Cams.cam.gameObject.transform.rotation;
    }

    private void CreateObject()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere.normalized;
        Vector3 randomPoint = lastPos + randomDirection * radius;
        //fonction de détection du terrain
        CheckTerrain(randomPoint);
        //on récupère la hauteur du terrain à ce point
        randomPoint.y = terrain.SampleHeight(randomPoint);
        float y = randomPoint.y;
        int prefabRand = UnityEngine.Random.Range(0, 4);
        if (prefabRand != 0)
        {
            randomPoint.y = y - offset; //on met le point au dessus du sol
            generatedObject = cubeManager.CreateCube(randomPoint.x, randomPoint.y, randomPoint.z, cubePrefab);
            generatedObject.name = "Objet" + objectCount;
            generatedObject.tag = "obstacle";
            generatedObject.GetComponent<SoloDetectableScript>().setTimeStart(start);
            objectCount++;

            float size = UnityEngine.Random.Range(1f, 3f);
            generatedObject.transform.localScale = new Vector3(size, size, size);
            generatedObject.transform.position = new Vector3(
                generatedObject.transform.position.x,
                 generatedObject.transform.position.y + (size - 1f) * 0.5f,
                  generatedObject.transform.position.z);
            generatedObject.GetComponent<Renderer>().material.color = new Color(
                0.7264151f,
                 UnityEngine.Random.Range(0f, 0.25f),
                  UnityEngine.Random.Range(0f, 0.25f));

            RaycastHit hit; //permet d'avoir l'objet orienté selon la surface
            var ray = new Ray(generatedObject.transform.position, Vector3.down); // check for slopes
            if (terrain.GetComponent<Collider>().Raycast(ray, out hit, 1000))
            {
                generatedObject.transform.rotation = Quaternion.FromToRotation(
                    generatedObject.transform.up, hit.normal) * generatedObject.transform.rotation; // adjust for slopes
            }

        }
        // else if(prefabRand == 1){
        //     randomPoint.y = y-40f; //on met le point au dessus du sol
        //     generatedObject = cubeManager.CreateCube(randomPoint.x, randomPoint.y, randomPoint.z, tigrePrefab);
        // }
        // else if(prefabRand == 2){
        //     randomPoint.y = y-39.6f; //on met le point au dessus du sol
        //     generatedObject = cubeManager.CreateCube(randomPoint.x, randomPoint.y, randomPoint.z, taureauPrefab);
        // }
        // else if(prefabRand == 3){
        //     randomPoint.y = y-37f; //on met le point au dessus du sol
        //     generatedObject = cubeManager.CreateCube(randomPoint.x, randomPoint.y, randomPoint.z, aiglePrefab);
        // }
        else
        {
            generatedObject = null;
        }
        if (generatedObject)
        {
            generatedObject.GetComponent<Render_dist>().SetCam(Cams.cam.gameObject);
            generatedObject.GetComponent<SoloDetectableScript>().setCam(Cams.cam.gameObject.GetComponent<Camera>());
        }
    }

    private void CheckTerrain(Vector3 randomPoint)
    {
        //on envoi un raycast vers le bas pour vérifier si c'est toujours le même terrain
        //si ce n'est pas le cas, on change le terrain
        RaycastHit hit;
        var ray = new Ray(randomPoint, Vector3.down); 
        if (terrain.GetComponent<Collider>().Raycast(ray, out hit, 1000))
        {
            if (hit.collider.gameObject != terrain)
            {
                terrain = hit.collider.gameObject.GetComponent<Terrain>();
                terrainPos = terrain.transform.position;
            }
        }
        ray = new Ray(randomPoint, Vector3.up);
        if (terrain.GetComponent<Collider>().Raycast(ray, out hit, 1000))
        {
            if (hit.collider.gameObject != terrain)
            {
                terrain = hit.collider.gameObject.GetComponent<Terrain>();
                terrainPos = terrain.transform.position;
            }
        }
    }
}
