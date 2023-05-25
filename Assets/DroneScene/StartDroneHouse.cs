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
public class StartDroneHouse : MonoBehaviour
{
    int nBCamera =1;
    float bruitAmplitude;
    float bruitFrequency;

    public GameObject sol = null; //le sol dans lequel on va se balader

    private Vector3 point = new Vector3(0, 0, 0); //le point de départ non modif
    private Vector3 terrainPos = new Vector3(0, 0, 0);
    //la position du sol

    public float radius = 10; //rayon du cercle dans lequel on va générer les points
    
    public GameObject cubePrefab = null;
    public CubeManager cubeManager = null;

    public SysCam Cams;

    public NoiseSettings Shake6D = null;
    public NoiseSettings Wobble6D = null;
    public NoiseSettings Handheld_normal_extreme = null;
    public NoiseSettings Handheld_normal_mild = null;
    public NoiseSettings Handheld_normal_strong = null;
    public NoiseSettings Handheld_tele_mild = null;
    public NoiseSettings Handheld_tele_strong = null;
    public NoiseSettings Handheld_wideangle_mild = null;
    public NoiseSettings Handheld_wideangle_strong = null;
    
    public Vector3 lastPos = new Vector3(0, 0, 0);
    public float lastTime = 0;
    public float maxTime = 5;
    public float distance = 100;

    private GameObject generatedObject = null;

    public CinemachineVirtualCamera noisecam;

    // Start is called before the first frame update
    void Start()
    {
        ////////////////////////////////////////
        // PARTIE EXTRACTION DU FICHIER CONFIG
        string path = "./config.txt";
        StreamReader reader = new StreamReader(path);
        // n the number of lines in the file
        int n = int.Parse(reader.ReadLine());
        // Read the lines
        for (int i = 1 ;i<n+1;i++){
            string[] line = reader.ReadLine().Split(',');
            if(line[0] == "scene"){
                //Nothing
            }
            else if (line[0] == "nBCamera"){
                this.nBCamera = Convert.ToInt32(line[1]);
            }
            else if (line[0] == "bruitAmplitude"){
                Cams.vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = (float)Convert.ToDouble(line[1]);
            }
            else if (line[0] == "bruitFrequency"){
                Cams.vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = (float)Convert.ToDouble(line[1]);
            }
        }
        //Cams.vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_NoiseProfile = Shake6D;
        ////////////////////////////////////////

        //stockage et récupération de certaines infos
        terrainPos = sol.transform.position; //on récupère la position du sol
        lastPos = Cams.cam.gameObject.transform.position - new Vector3(0, 500, 0);
        lastTime = 0;
    }

    bool isInHouse(Vector3 pos){
        return(
            (pos.x < 1.39f && pos.x > -2.75f &&
            pos.z < -2.32 && pos.z > -10.40f) ||
            (pos.x < -2.75f && pos.x > -7.63f &&
            pos.z < -4.69f && pos.z > -10.40f) ||
            (pos.x < -7.63f && pos.x > -10.18f &&
            pos.z < -4.69f && pos.z > -11.04f) ||
            (pos.x < -10.18f && pos.x > -15.88f &&
            pos.z < -2.52f && pos.z > -11.04f) 
        );
    }

    // Update is called once per frame
    void Update()
    {
        lastTime += Time.deltaTime;
        if(lastTime > maxTime && Vector3.Distance(lastPos,Cams.cam.gameObject.transform.position) > distance){
            lastPos = Cams.cam.gameObject.transform.position;
            for (int i = 0; i < 4; i++)
            {
                float r = radius;
                Vector3 randomDirection = UnityEngine.Random.insideUnitSphere.normalized;
                Vector3 randomPoint = lastPos + randomDirection * r;
                while(!isInHouse(randomPoint))
                {
                    r-=0.5f;
                    randomDirection = UnityEngine.Random.insideUnitSphere.normalized;
                    randomPoint = lastPos + randomDirection * radius;
                }
                randomPoint.y = 1.196f;
                int prefabRand = UnityEngine.Random.Range(0, 4);
                if(prefabRand != 0){
                    generatedObject = cubeManager.CreateCube(randomPoint.x, randomPoint.y, randomPoint.z, cubePrefab);

                    generatedObject.GetComponent<Render_dist>().SetCam(Cams.cam.gameObject);
                    generatedObject.GetComponent<Render_dist>().SetDistance(distance);

                    float size = UnityEngine.Random.Range(0.10f, 0.5f);
                    generatedObject.transform.localScale = new Vector3(size, size, size);
                    generatedObject.transform.position = new Vector3(
                        generatedObject.transform.position.x,
                         generatedObject.transform.position.y + (size-1f)*0.5f,
                          generatedObject.transform.position.z);
                    generatedObject.GetComponent<Renderer>().material.color = new Color(
                        0.7264151f,
                         UnityEngine.Random.Range(0f, 0.25f),
                          UnityEngine.Random.Range(0f, 0.25f));
                    
                    //TODO: verifier si ça en dessous marche, sinon supprimer le code
                    RaycastHit hit;
                    var ray = new Ray (generatedObject.transform.position, Vector3.down); // check for slopes
                    if (sol.GetComponent<Collider>().Raycast(ray, out hit, 1000)) {
                        generatedObject.transform.rotation = Quaternion.FromToRotation(
                        generatedObject.transform.up, hit.normal)*generatedObject.transform.rotation; // adjust for slopes
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
                else{
                    generatedObject = null;
                }
            }
            lastTime = 0; 
        }

        noisecam.gameObject.transform.position = Cams.cam.gameObject.transform.position;
        noisecam.gameObject.transform.rotation = Cams.cam.gameObject.transform.rotation;
    }

}
