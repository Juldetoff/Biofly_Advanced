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
/// Classe permettant de gérer la scène 'M_Forest'. Sert à gérer manuellement la caméra et crée des obstacles autour d'elle. 
/// Peut être étendue pour générer différemment des objets ou gérer autrement la position de la caméra.
/// </summary>
public class StartDrone : MonoBehaviour
{
    [Header("Paramètres globaux")]
    [Tooltip("Terrain que vont parcourir les caméras.")]public Terrain terrain = null; //le terrain dans lequel on va se balader (afin d'obtenir son y de surface)
    private Vector3 terrainPos = new Vector3(0, 0, 0);//la position du terrain
    [Tooltip("Dernière position de la caméra où des objets ont été généré.")]public Vector3 lastPos = new Vector3(0, 0, 0);
    [HideInInspector]public float lastTime = 0;
    [Tooltip("Temps à attendre avant de pouvoir regénérer des objets si la caméra est suffisamment loin de lastPos.")]public float maxTime = 5;

    [Header("Paramètres de la caméra")]
    [Tooltip("SysCam normal.")]public SysCam Cams;
    [Tooltip("SysCam bruité.")]public SysCam VCams;
    [HideInInspector]public float bruitAmplitude=0;
    [HideInInspector]public float bruitFrequency=0;
    [HideInInspector]public int noiseNumber = 0;
    [Tooltip("Liste des NoiseSettings (bruits possibles parmi ceux de Cinemachine).")]public NoiseSettings[] noiseSettings = new NoiseSettings[9]; //liste des types de bruits afin de pouvoir les associer à la caméra
    [Tooltip("Caméra virtuelle du SysCam bruité.")]public CinemachineVirtualCamera noisecam;
    [Tooltip("Liste des Caméras d'enregistrement.")]public MovieRecordManual[] movieRecordManuals;
    private int nBCamera =1;

    [Header("Prefabs")]
    [Tooltip("Objet CubeManager dans la scène gérant l'apparition des prefabs.")]public CubeManager cubeManager = null;
    [Tooltip("Rayon du cercle de génération d'objets autour de la caméra.")]public float radius = 40; //rayon du cercle dans lequel on va générer les objets    
    [SerializeField][Tooltip("Prefab de cube rouge pour manuel.")]public GameObject cubePrefab = null;
    [HideInInspector]private int objectCount = 0;
    [Tooltip("Offset de positionnement de l'objet")]public float offset = 39.3f;
    [Tooltip("Distance minimum entre chaque instanciation d'objets.")]public float distance = 100;
    [Tooltip("Distance maximum avant suppression de l'objet éloigné.")]public float render_dist = 100;
    private GameObject generatedObject = null;


    //[Header("Paramètres de la vidéo")]
    [HideInInspector]public int videoType = 0;
    [HideInInspector]public int videoQuality = 0;
    [HideInInspector]public int videoFps = 60;
    [HideInInspector]public string[] qualitySettings = new string[3]{"Low", "Medium", "High"}; //liste des qualités de vidéo
    [HideInInspector]public string[] formatSettings = new string[3]{"mp3", "mov", "webm"}; //liste des formats de vidéo

    [Header("Autres")]
    [Tooltip("Objet venant du prefab 'Floupane' à mettre devant la caméra.")]public GameObject flouPane;
    [HideInInspector]public bool jello = false;


    // Start est appelé avant la première frame
    void Start()
    {
        //ExtractConfig(); //on déplace dans Awake afin de pouvoir affecter les caméras ?
        lastTime = Time.time;

        terrainPos = terrain.transform.position; //on récupère la position du terrain

        Cams.vcam.gameObject.GetComponent<DroneCameraMovement>().SetTerrain(terrain); //on associe le terrain à la caméra pour la fonction "ClampHeight"

        if(jello){
            Cams.cam.gameObject.GetComponent<HDRP_RollingShutter>().enabled = true;
            VCams.cam.gameObject.GetComponent<HDRP_RollingShutter>().enabled = true; 
        }
    }

    // Awake est appelé avant Start
    private void Awake() {
        ExtractConfig();
    }

    /// <summary>
    /// Fonction permettant d'extraire les paramètres de la scène depuis le fichier de configuration. Elle met également en place le jello effect si activé.
    /// </summary>
    public void ExtractConfig()
    {
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
                this.jello = Convert.ToInt32(line[1]) == 1;
            }
        }
        lastPos = Cams.cam.gameObject.transform.position - new Vector3(0, 500, 0);
        lastTime = Time.time;

        if(jello){ //dans ce cas, pas de bruits et on échange les depths
            Cams.cam.gameObject.GetComponent<HDRP_RollingShutter>().enabled = true;
            noisecam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0f;
            noisecam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0f;
            noisecam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_NoiseProfile = null;
            VCams.cam.gameObject.GetComponent<Camera>().depth = 2;
            Cams.cam.gameObject.GetComponent<Camera>().depth = 1;
        }
        else{
            Cams.cam.gameObject.GetComponent<HDRP_RollingShutter>().enabled = false;
            noisecam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = bruitAmplitude;
            noisecam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = bruitFrequency;
            noisecam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_NoiseProfile = noiseSettings[noiseNumber];
        }
        foreach (MovieRecordManual mov in movieRecordManuals)
        {
            mov.startvideo = true;
        }
    }

    // Update est appelé à chaque frame
    void Update()
    {
        
        if(Time.time - lastTime > maxTime && Vector3.Distance(lastPos,Cams.cam.gameObject.transform.position) > distance){
            lastPos = Cams.cam.gameObject.transform.position;
            for (int i = 0; i < 4; i++)
            {
                CreateObject();
            }
            lastTime = Time.time; 
        }

        //on positionne la caméra bruitée au même endroit que la caméra normale à chaque frame
        noisecam.gameObject.transform.position = Cams.cam.gameObject.transform.position;
        noisecam.gameObject.transform.rotation = Cams.cam.gameObject.transform.rotation;
    }

    /// <summary>
    /// Fonction permettant de créer un objet à une position aléatoire autour de la caméra.
    /// </summary>
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
            generatedObject.name = "cube" + objectCount;
            generatedObject.tag = "obstacle";
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

    /// <summary>
    /// Fonction permettant de vérifier si le terrain est toujours le même que celui de la dernière fois.
    /// </summary>
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
