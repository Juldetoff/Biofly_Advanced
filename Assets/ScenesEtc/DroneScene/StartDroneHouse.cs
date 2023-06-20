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
public class StartDroneHouse : StartDrone
{   ////////////////////////////////////////
    //cette classe sert pour le start en intérieur en manuel (car il faut gérer les murs etc lors de génération d'obstacles)
    //(et peu rentable à automatiser niveau difficulté, il faudrait détecter les murs, faire du pathfinding de caméras...)
    ////////////////////////////////////////
    private Vector3 pointdepart = new Vector3(0, 0, 0); //pointdepart de départ du chemin modifié à chaque fois

    public GameObject sol = null; //le sol dans lequel on va se balader

    private Vector3 solPos = new Vector3(0, 0, 0);
    // //la position du sol
    private int objectCnt = 0;

    private GameObject objectGenerated = null;


    // Start is called before the first frame update
    void Start()
    {
        ExtractConfig();

        solPos = sol.transform.position; //on récupère la position du sol
    }

    bool isInHouse(Vector3 pos){
        return( //fonction alambiquée pour vérifier si on est dans la maison
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

    private void CreateObject()
    {
        float r = radius;
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere.normalized;
        Vector3 randomPoint = lastPos + randomDirection * r;
        while (!isInHouse(randomPoint))
        {
            r -= 0.5f;
            randomDirection = UnityEngine.Random.insideUnitSphere.normalized;
            randomPoint = lastPos + randomDirection * radius;
        }
        randomPoint.y = 1.196f;
        int prefabRand = UnityEngine.Random.Range(0, 4);
        if (prefabRand != 0)
        {
            objectGenerated = cubeManager.CreateCube(randomPoint.x, randomPoint.y, randomPoint.z, cubePrefab);
            objectGenerated.name = "Objet" +objectCnt;
            objectGenerated.tag = "obstacle";
            objectCnt++;

            objectGenerated.GetComponent<Render_dist>().SetCam(Cams.cam.gameObject);
            objectGenerated.GetComponent<Render_dist>().SetDistance(render_dist);

            float size = UnityEngine.Random.Range(0.10f, 0.5f);
            objectGenerated.transform.localScale = new Vector3(size, size, size);
            objectGenerated.transform.position = new Vector3(
                objectGenerated.transform.position.x,
                 objectGenerated.transform.position.y + (size - 1f) * 0.5f,
                  objectGenerated.transform.position.z);
            objectGenerated.GetComponent<Renderer>().material.color = new Color(
                0.7264151f,
                 UnityEngine.Random.Range(0f, 0.25f),
                  UnityEngine.Random.Range(0f, 0.25f));

            // RaycastHit hit; //pas nécessaire, le sol étant plat
            // var ray = new Ray(objectGenerated.transform.position, Vector3.down); // check for slopes
            // if (sol.GetComponent<Collider>().Raycast(ray, out hit, 1000))
            // {
            //     objectGenerated.transform.rotation = Quaternion.FromToRotation(
            //     objectGenerated.transform.up, hit.normal) * objectGenerated.transform.rotation; // adjust for slopes
            // }
        }
        // else if(prefabRand == 1){
        //     randomPoint.y = y-40f; //on met le pointdepart au dessus du sol
        //     objectGenerated = cubeManager.CreateCube(randomPoint.x, randomPoint.y, randomPoint.z, tigrePrefab);
        // }
        // else if(prefabRand == 2){
        //     randomPoint.y = y-39.6f; //on met le pointdepart au dessus du sol
        //     objectGenerated = cubeManager.CreateCube(randomPoint.x, randomPoint.y, randomPoint.z, taureauPrefab);
        // }
        // else if(prefabRand == 3){
        //     randomPoint.y = y-37f; //on met le pointdepart au dessus du sol
        //     objectGenerated = cubeManager.CreateCube(randomPoint.x, randomPoint.y, randomPoint.z, aiglePrefab);
        // }
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
}
