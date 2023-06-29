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
public class StartScript : MonoBehaviour
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


    ////////////////////////////////////////
    //Objets et paramètres de génération

    public Terrain terrain = null; //le terrain dans lequel on va se balader (afin d'obtenir son y de surface)
    public CinemachineSmoothPath Prefabpath; //prefab chemin qu'on va instancier puis lui donner 11 points

    
    private Vector3 terrainPos = new Vector3(0, 0, 0); //la position du terrain afin de gérer des positionnements
    public float radius = 40; //rayon du cercle dans lequel on va générer les points pour le chemin ou les obstacles
    public GameObject cubePrefab = null;  
    public CubeManager cubeManager = null;
    public GameObject tigrePrefab = null;
    public GameObject taureauPrefab = null;
    public GameObject aiglePrefab = null; //prefab des obstacles à instancier
    public bool onlyCube = true; //si on veut que des cubes

    public TimelineAsset timeline; //la timeline qui gèrera les déplacements (un peu comme un film) (est un fichier)
    public PlayableDirector director; //relie la timeline au jeu

    public int camCount = 0; //compteur de cam pour les instantiates et les noms (est global)

    public SysCam prefabCam; //prefab de la paire cam physique cam virtuelle
    public GameObject flouPane; //objet en face de la caméra permettant de flouter l'image

    TrackAsset originalVirtualTrack; 
    TrackAsset originalPhysicalTrack; 

    public AnimationClip animClip; //pour pouvoir le duppliquer dans la timeline

    public NoiseSettings[] noiseSettings = new NoiseSettings[9]; //liste des types de bruits afin de pouvoir les associer à la caméra
    public string[] qualitySettings = new string[3]{"Low", "Medium", "High"}; //liste des qualités de vidéo
    public string[] formatSettings = new string[3]{"mp3", "mov", "webm"}; //liste des formats de vidéo
    public float offsetcam = 35f; //offset de la caméra par rapport au sol
    private MovieRecorderExample[] cams = null;
    private CinemachineVirtualCamera[] vcams = null;
    private GameObject generatedObject = null;
    private int count = 0;
    private bool repeat = false;
    private bool finished = false; //update lorsqu'une vcam envoie qu'elle a finit
    


    // Start is called before the first frame update
    void Start()
    {
        ////////////////////////////////////////
        // PARTIE EXTRACTION DU FICHIER CONFIG
        string path = "config.txt";
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
                this.bruitAmplitude = (float)Convert.ToDouble(line[1]);
            }
            else if (line[0] == "bruitFrequency"){
                this.bruitFrequency = (float)Convert.ToDouble(line[1]);
            }
            else if (line[0]=="bruitType"){
                this.noiseNumber = Convert.ToInt32(line[1]);
            }
            else if (line[0]=="flou"){
                //this.flouPane.SetActive(Convert.ToInt32(line[1])==1);
                //commenté car pas intéressant pour le moment
            }
            else if (line[0]=="videoType"){
                this.videoType = Convert.ToInt32(line[1]);
            }
            else if (line[0]=="videoQuality"){
                this.videoQuality = Convert.ToInt32(line[1]);
            }
            else if (line[0]=="fps"){
                this.videoFps = Convert.ToInt32(line[1]);
            }
            else if (line[0]=="jello"){
                //this.jello = Convert.ToInt32(line[1]);
                //commenté car pas intéressant pour le moment
            }
            else if (line[0]=="repeat"){
                this.repeat = Convert.ToInt32(line[1])==1;
            }
        }
        // Debug.Log("nBCamera = " + this.nBCamera); //nbr de paire de cam
        // Debug.Log("bruitAmplitude = " + this.bruitAmplitude); //la 2e cam est bruitée
        // Debug.Log("bruitFrequency = " + this.bruitFrequency); //same
        ////////////////////////////////////////


        //stockage et récupération de certaines infos
        //notamment car on a une track cam virtuelle et une track cam physique template à utiliser
        cams = new MovieRecorderExample[nBCamera*2];
        vcams = new CinemachineVirtualCamera[nBCamera*2];
        terrainPos = terrain.transform.position; //on récupère la position du terrain
        originalVirtualTrack = timeline.GetOutputTrack(0); // assuming the original virtual camera is the first track
        originalPhysicalTrack = timeline.GetOutputTrack(1); // assuming the original physical camera is the second track
        int k = 0;

        //
        //On supprime les tracks de la timeline sauf les 2 premières
        foreach (TrackAsset track in timeline.GetOutputTracks())
        {   
            if(k>1){
                timeline.DeleteTrack(track);
            }
            k++;
        }
        

        //////////////////////////////////////////
        //PARTIE INSTANCIATION DES PAIRES DE CAMERAS 
        for(int i=0; i<nBCamera; i++){ 
            //
            //GENERATION POINT DE DEPART
            point = new Vector3(
                UnityEngine.Random.Range(terrainPos.x+100,terrainPos.x + 900),
                terrainPos.y,
                UnityEngine.Random.Range(terrainPos.z+100,terrainPos.z + 900)
            ); //on génère un point de départ aléatoire
            float y = terrain.SampleHeight(point); //on récupère la hauteur du terrain à ce point
            point.y = y-offsetcam; //on met le point plus ou moins au dessus du sol

            //
            //GENERATION PATH A PARTIR DU POINT DE DEPART
            CinemachineSmoothPath Cpath = Instantiate(Prefabpath, new Vector3(0, 0, 0), Quaternion.identity);
            Cpath.m_Waypoints = new CinemachineSmoothPath.Waypoint[11];
            Cpath.m_Waypoints[0].position = point;
            for(int j=1; j<11; j++){ //on génère 10 points aléatoires de chemin
                point = GeneratePoint(point,radius);
                CheckTerrain(point); //au cas où on déborde sur un autre terrain
                y = terrain.SampleHeight(point); //on récupère la hauteur du terrain à ce point
                point.y = y-35; //on met le point au dessus du sol
                Cpath.m_Waypoints[j].position = point; //on lui associe la position
                //
                //Instanciation d'un objet aux environs du point généré

                Vector3 cubPoint = GeneratePoint(point, 10); //comme ça quand le cube apparait, il est autour du point ciblé
                CheckTerrain(cubPoint);
                y = terrain.SampleHeight(cubPoint); //on récupère la hauteur du terrain à ce point
                cubPoint.y = y; //on met le point au dessus du sol
                //cubeManager.CreateCube(point.x, point.y-4.5f, point.z);
                int prefabRand = UnityEngine.Random.Range(0, 4);
                if(prefabRand == 0 || onlyCube){
                    cubPoint.y = y-39.5f; //on met le point au dessus du sol
                    generatedObject=cubeManager.CreateCube(cubPoint.x, cubPoint.y, cubPoint.z, cubePrefab);
                    generatedObject.name = "cube" + count;
                    count++;
                }
                else if(prefabRand == 1 && !onlyCube){
                    cubPoint.y = y-40f; //on met le point au dessus du sol
                    generatedObject=cubeManager.CreateCube(cubPoint.x, cubPoint.y, cubPoint.z, tigrePrefab);
                    generatedObject.name = "tigre" + count;
                    count++;
                }
                else if(prefabRand == 2 && !onlyCube){
                    cubPoint.y = y-39.6f; //on met le point au dessus du sol
                    generatedObject=cubeManager.CreateCube(cubPoint.x, cubPoint.y, cubPoint.z, taureauPrefab);
                    generatedObject.name = "taureau" + count;
                    count++;
                }
                else if(prefabRand == 3 && !onlyCube){
                    cubPoint.y = y-37f; //on met le point au dessus du sol
                    generatedObject=cubeManager.CreateCube(cubPoint.x, cubPoint.y, cubPoint.z, aiglePrefab);
                    generatedObject.name = "aigle" + count;
                    count++;
                }
                else{
                    generatedObject=null;
                }
                if (generatedObject)
                {
                    RaycastHit hit; //permet d'avoir l'objet orienté selon la surface
                    var ray = new Ray(generatedObject.transform.position, Vector3.down); // check for slopes
                    if (terrain.GetComponent<Collider>().Raycast(ray, out hit, 1000))
                    {
                        generatedObject.transform.rotation = Quaternion.FromToRotation(
                            generatedObject.transform.up, hit.normal) * generatedObject.transform.rotation; // adjust for slopes
                    }
                }
            }
            /////////////////////////////////////////

            //
            //ASSOCIATION CAMERAS ET TIMELINE
            SysCam syscam = Instantiate(prefabCam, new Vector3(0, 0, 0), Quaternion.identity); //on instancie un SysCam qui contient une caméra physique et virtuelle
            cams[camCount/2] = syscam.cam.GetComponent<MovieRecorderExample>();
            vcams[camCount/2] = syscam.vcam;
            syscam.cam.name = "cam" + i*2; //on lui donne un nom
            syscam.vcam.GetCinemachineComponent<CinemachineTrackedDolly>().m_Path = Cpath; //on lui associe le chemin généré
            syscam.cam.transform.position = Cpath.m_Waypoints[0].position; //on place la caméra au début du chemin
            syscam.vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = bruitAmplitude;
            syscam.vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = bruitFrequency;
            syscam.vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_NoiseProfile = noiseSettings[noiseNumber];
            //ensuite on crée les tracks dans la timeline

            AnimationTrack newTrack = timeline.CreateTrack<AnimationTrack>("newVirtualTrack");
            TrackAsset physicalCameraTrack = timeline.CreateTrack(typeof(CinemachineTrack), null, "Physical Camera Track");
            //on récupère les tracks dans les positions suivantes dans la timeline
            AnimationTrack newVirtualTrack = (AnimationTrack)timeline.GetOutputTrack(2+4*i); // i étant le numéra de la new paire cam, on commence en 2 et on se déplace de 4 en 4
            TrackAsset newPhysicalTrack = timeline.GetOutputTrack(3+4*i); //à cause des cams bruitées

            //on associe l'animator de la virtual cam à la virtual track
            director.SetGenericBinding(newVirtualTrack, syscam.animator);
            //on associe le CinemachineBrain de la physical cam à la physical track   
            director.SetGenericBinding(newPhysicalTrack, syscam.brain);
            
            //on copie les clips de la track virtuelle template dans la nouvelle
            foreach (TimelineClip clip in originalVirtualTrack.GetClips())
            {
                TimelineClip newClip = newVirtualTrack.CreateClip(animClip);
                newClip.start = clip.start;
                newClip.duration = clip.duration;
                newClip.displayName = clip.displayName;   
            }


            //on copie les clips de la track physique template dans la nouvelle
            foreach (TimelineClip clip in originalPhysicalTrack.GetClips())
            {
                TimelineClip newClip = newPhysicalTrack.CreateDefaultClip();
                newClip.start = clip.start;
                newClip.duration = clip.duration;
                newClip.displayName = syscam.vcam.name;
                CinemachineShot cinemachineShot = (CinemachineShot)newClip.asset;
                cinemachineShot.VirtualCamera = new ExposedReference<CinemachineVirtualCameraBase>();
                ExposedReference<CinemachineVirtualCameraBase> virtualCameraReference = new ExposedReference<CinemachineVirtualCameraBase>();
                virtualCameraReference.defaultValue = syscam.vcam; //en gros un sorte de systeme e pointeur reliant le fichier timeline
                //a la virtual cam instancié en run (car elle n'existe pas hors runtime, donc faut l'associer comme ça)
                cinemachineShot.VirtualCamera.exposedName = UnityEditor.GUID.Generate().ToString();
                director.SetReferenceValue(cinemachineShot.VirtualCamera.exposedName, syscam.vcam);
            }
            syscam.cam.GetComponent<MovieRecorderExample>().startVideo = true;
            ///////////////////////////////////////// 


            //
            //CREATION DE LA CAMERA BRUITEE
            SysCam syscambruit = Instantiate(prefabCam, new Vector3(0, 0, 0), Quaternion.identity); 
            syscambruit.cam.name = "cambruit" + (i*2+1); //on lui donne un nom
            cams[camCount/2] = syscambruit.cam.GetComponent<MovieRecorderExample>();
            vcams[camCount/2] = syscambruit.vcam;
            syscambruit.vcam.GetCinemachineComponent<CinemachineTrackedDolly>().m_Path = Cpath; //on lui associe le chemin généré
            syscambruit.cam.transform.position = Cpath.m_Waypoints[0].position; //on place la caméra au début du chemin

            
            TrackAsset virtualCameraTrackbruit = timeline.CreateTrack<AnimationTrack>("newNoiseVirtualTrack");
            TrackAsset physicalCameraTrackbruit = timeline.CreateTrack(typeof(CinemachineTrack), null, "Physical Camera Track");
            //on récupère les tracks dans les positions suivantes dans la timeline
            AnimationTrack newVirtualTrackbruit = (AnimationTrack)timeline.GetOutputTrack(4+4*i); // i étant le numéra de la new paire cam, on commence en 4 et on se déplace de 4 en 4
            TrackAsset newPhysicalTrackbruit = timeline.GetOutputTrack(5+4*i); // de 4 en 4 à cause des cams bruitées

            //on associe l'animator de la virtual cam bruit à la virtual track
            director.SetGenericBinding(newVirtualTrackbruit, syscambruit.animator);
            //on associe le CinemachineBrain de la physical cam bruit à la physical track   
            director.SetGenericBinding(newPhysicalTrackbruit, syscambruit.brain);



            /////////////////////////////////////////////////
            //on associe à notre camera virtuelle les options de bruitage récupérée dans config
            syscambruit.vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = bruitAmplitude;
            syscambruit.vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = bruitFrequency;

            syscambruit.vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_NoiseProfile = noiseSettings[noiseNumber];


            
            //on copie les clips de la track virtuelle template dans la nouvelle
            foreach (TimelineClip clip in originalVirtualTrack.GetClips())
            {
                TimelineClip newClip = newVirtualTrackbruit.CreateClip(animClip);
                newClip.start = clip.start;
                newClip.duration = clip.duration;
                newClip.displayName = clip.displayName;           
            }


            //on copie les clips de la track physique template dans la nouvelle
            foreach (TimelineClip clip in originalPhysicalTrack.GetClips())
            {
                TimelineClip newClip = newPhysicalTrackbruit.CreateDefaultClip();
                newClip.start = clip.start;
                newClip.duration = clip.duration;
                newClip.displayName = syscambruit.vcam.name;
                CinemachineShot cinemachineShot = (CinemachineShot)newClip.asset;
                cinemachineShot.VirtualCamera = new ExposedReference<CinemachineVirtualCameraBase>();
                ExposedReference<CinemachineVirtualCameraBase> virtualCameraReference = new ExposedReference<CinemachineVirtualCameraBase>();
                virtualCameraReference.defaultValue = syscambruit.vcam; //en gros un sorte de systeme e pointeur reliant le fichier timeline
                //a la virtual cam instancié en run (car elle n'existe pas hors run, donc faut l'associer comme ça)
                cinemachineShot.VirtualCamera.exposedName = UnityEditor.GUID.Generate().ToString();
                director.SetReferenceValue(cinemachineShot.VirtualCamera.exposedName, syscambruit.vcam);
            }
            syscambruit.cam.GetComponent<MovieRecorderExample>().startVideo = true;
        }

        //Bidouillage car lorsque toute les caméras sont instanciées, le temps n'est plus à 0 et elles ne veulent pas bouger
        director.Stop();
        director.time = 0;
        director.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            // director.Stop();
            // director.time = 0;
            // director.Play(); //cool mais ça remet juste au point 0 en "remontant la timeline"
            //sinon test de parcourir chaque caméra et de disable
            Restart();
        }
        CheckFinished();
        if(finished && repeat){
            Restart();
        }
    }

    private void CheckFinished(){
        foreach(CinemachineVirtualCamera vcam in vcams){
            if(vcam.GetCinemachineComponent<CinemachineTrackedDolly>().m_PathPosition == 10){
                finished = true;
            }
        }
    }

    private void Restart()
    {
        foreach (MovieRecorderExample mov in cams)
        {
            mov.DisableVideo();
        }
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f);
        StartCoroutine(WaitAndRestartScene(waitForSeconds));
    }

    private IEnumerator WaitAndRestartScene(WaitForSeconds waitForSeconds)
    {
        yield return waitForSeconds;
        // Libérer les ressources associées à l'enregistreur de vidéo
        foreach(MovieRecorderExample mov in cams){
            Destroy(mov);
        }
        // Relancer la scène
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private Vector3 GeneratePoint(Vector3 previousPoint, float radius){
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere.normalized;
        Vector3 randomPoint = previousPoint + randomDirection * radius;
        return randomPoint;
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
