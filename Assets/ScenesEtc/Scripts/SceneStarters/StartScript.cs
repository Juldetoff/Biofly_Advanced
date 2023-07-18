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
/// Cette classe permet de mettre en place automatiquement la génération de caméra et de chemin pour une scène ainsi que la génération d'objets.
/// Elle est associée à la scene "A_Forest", mais peut être étendu afin de gérer automatiquement d'autres scènes (comme AutoStartCity.cs qui gère la scène "A_City").
/// Il suffit pour cela d'override les fonctions devant être changées.
/// </summary>
public class StartScript : MonoBehaviour
{
    [Header("Paramètres globaux")]
    [Tooltip("Terrain que vont parcourir les caméras.")]public Terrain terrain = null; //le terrain dans lequel on va se balader (afin d'obtenir son y de surface)
    private Vector3 terrainPos = new Vector3(0, 0, 0); //la position du terrain afin de gérer des positionnements
    [HideInInspector]public bool repeat = false;
    [HideInInspector]public bool finished = false; //update lorsqu'une vcam envoie qu'elle a finit

    [Header("Paramètres génération de caméras")]
    public SysCam prefabCam; //prefab de la paire cam physique cam virtuelle
    public float offsetcam = 35f; //offset de la caméra par rapport au sol
    [HideInInspector]public MovieRecorderExample[] cams = null;
    [HideInInspector]public CinemachineVirtualCamera[] vcams = null;
    [HideInInspector]public int nBCamera =1;
    [HideInInspector]public float bruitAmplitude=0;
    [HideInInspector]public float bruitFrequency=0;
    [HideInInspector]public int noiseNumber = 0;
    [HideInInspector]public int camCount = 0; //compteur de cam pour les instantiates et les noms (est global)


    [Header("Paramètres de génération du chemin")]
    [Tooltip("Prefab de chemin qui sera instancié.")]public CinemachineSmoothPath Prefabpath; //prefab chemin qu'on va instancier puis lui donner 11 points
    [Tooltip("Rayon autour des points générés permettant de générer d'autres points.")]public float radius = 40; //rayon du cercle dans lequel on va générer les points pour le chemin ou les obstacles
    [Tooltip("Nombre de points à générer sur le chemin.")]public int pointCnt = 11; //nombre de points du chemin
    [Tooltip("Nombre d'objets à générer.")]public int numObject = 1; //nombre d'objets à générer
    [HideInInspector]public Vector3 point = new Vector3(0, 0, 0); //point de départ du chemin modifié à chaque fois


    [Header("Objets Prefabs")]
    [Tooltip("Objet CubeManager dans la scène gérant l'apparition des prefabs.")]public CubeManager cubeManager = null;
    [SerializeField][Tooltip("Permet de ne générer que des cubePrefabs.")]private bool onlyCube = true; //si on veut que des cubes
    [SerializeField][Tooltip("Prefab de cube rouge.")]private GameObject cubePrefab = null;  
    [SerializeField][Tooltip("Prefab de tigre.")]private GameObject tigrePrefab = null;
    [SerializeField][Tooltip("Prefab de taureau.")]private GameObject taureauPrefab = null;
    [SerializeField][Tooltip("Prefab d'aigle.")]private GameObject aiglePrefab = null; //prefab des obstacles à instancier
    private GameObject generatedObject = null;
    [HideInInspector]public int count = 0;

    [Header("Paramètres enregistrement vidéo")]
    [Tooltip("Liste des NoiseSettings (bruits possibles parmi ceux de Cinemachine).")]public NoiseSettings[] noiseSettings = new NoiseSettings[9]; //liste des types de bruits afin de pouvoir les associer à la caméra
    //afin de moduler et ajouter ses propres bruits, il faudra faire attention aux erreurs d'index (voulu de taille 9 à la base)
    [Tooltip("Timeline associée à la scène.")]public TimelineAsset timeline; //la timeline qui gèrera les déplacements (un peu comme un film) (est un fichier!)
    [Tooltip("Director qui déroulera la timeline.")]public PlayableDirector director; //relie la timeline au jeu
    [Tooltip("Clip d'animation dans la timeline, sert de modèle à duppliquer.")]public AnimationClip animClip; //pour pouvoir le duppliquer dans la timeline
    private TrackAsset originalVirtualTrack; 
    private TrackAsset originalPhysicalTrack; 
    [HideInInspector]public int videoType = 0;
    [HideInInspector]public int videoQuality = 0;
    [HideInInspector]public int videoFps = 60;
    [HideInInspector]public string[] qualitySettings = new string[3]{"Low", "Medium", "High"}; //liste des qualités de vidéo
    [HideInInspector]public string[] formatSettings = new string[3]{"mp3", "mov", "webm"}; //liste des formats de vidéo


    [Header("Autres")]
    [Tooltip("Objet venant du prefab 'Floupane' à mettre devant la caméra.")]public GameObject flouPane; //objet en face de la caméra permettant de flouter l'image

    // Start est appelé avant la première frame 
    void Start()
    {
        ExtractConfig();

        //stockage et récupération de certaines infos
        //notamment car on a une track cam virtuelle et une track cam physique template à utiliser
        PrepareTimeline();        
        terrainPos = terrain.transform.position; //on récupère la position du terrain

        for (int i = 0; i < nBCamera; i++)
        {
            CinemachineSmoothPath Cpath = GeneratePath();

            //ASSOCIATION CAMERAS ET TIMELINE
            SysCam syscam = GenerateCam(i, Cpath);
            //ensuite on crée les tracks dans la timeline

            GenerateNormalTrack(i, syscam);
            syscam.cam.GetComponent<MovieRecorderExample>().startVideo = true;
            syscam.cam.GetComponent<Camera>().depth = 2; //on met 2 pour les caméras normales afin qu'elles soient prioritaires pour le rendu

            SysCam syscambruit = GenerateNoiseCam(i, Cpath);
            GenerateNoiseTrack(i, syscambruit);
            syscambruit.cam.GetComponent<MovieRecorderExample>().startVideo = true;
            syscambruit.cam.GetComponent<Camera>().depth = 1; //1 ici car on ne veut pas que l'affichage du runtime soit bruité (dérangeant)
        }

        //Bidouillage car lorsque toute les caméras sont instanciées, le temps n'est plus à 0 et elles ne veulent pas bouger
        director.Stop();
        director.time = 0;
        director.Play();
    }


    /// <summary>
    /// Permet de générer une track bruitée à partir d'un système de caméra donné et d'un identifiant.
    /// </summary>
    public void GenerateNoiseTrack(int i, SysCam syscambruit)
    {
        TrackAsset virtualCameraTrackbruit = timeline.CreateTrack<AnimationTrack>("newNoiseVirtualTrack");
        TrackAsset physicalCameraTrackbruit = timeline.CreateTrack(typeof(CinemachineTrack), null, "Physical Camera Track");
        //on récupère les tracks dans les positions suivantes dans la timeline
        AnimationTrack newVirtualTrackbruit = (AnimationTrack)timeline.GetOutputTrack(4 + 4 * i); // i étant le numéra de la new paire cam, on commence en 4 et on se déplace de 4 en 4
        TrackAsset newPhysicalTrackbruit = timeline.GetOutputTrack(5 + 4 * i); // de 4 en 4 à cause des cams bruitées

        //on associe l'animator de la virtual cam bruit à la virtual track
        director.SetGenericBinding(newVirtualTrackbruit, syscambruit.animator);
        //on associe le CinemachineBrain de la physical cam bruit à la physical track   
        director.SetGenericBinding(newPhysicalTrackbruit, syscambruit.brain);

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
    }

    /// <summary>
    /// Permet de générer une caméra bruitée à partir d'un chemin donné et d'un identifiant.
    /// </summary>
    public SysCam GenerateNoiseCam(int i, CinemachineSmoothPath Cpath)
    {
        SysCam syscambruit = Instantiate(prefabCam, new Vector3(0, 0, 0), Quaternion.identity);
        syscambruit.cam.name = "cambruit" + (i * 2 + 1); //on lui donne un nom
        cams[camCount / 2] = syscambruit.cam.GetComponent<MovieRecorderExample>();
        vcams[camCount / 2] = syscambruit.vcam;
        syscambruit.vcam.GetCinemachineComponent<CinemachineTrackedDolly>().m_Path = Cpath; //on lui associe le chemin généré
        syscambruit.cam.transform.position = Cpath.m_Waypoints[0].position; //on place la caméra au début du chemin
        syscambruit.vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = bruitAmplitude;
        syscambruit.vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = bruitFrequency;

        syscambruit.vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_NoiseProfile = noiseSettings[noiseNumber];
        return syscambruit;
    }

    /// <summary>
    /// Permet de générer une track normale à partir d'un système de caméra donné et d'un identifiant.
    /// </summary>
    public void GenerateNormalTrack(int i, SysCam syscam)
    {
        AnimationTrack newTrack = timeline.CreateTrack<AnimationTrack>("newVirtualTrack");
        TrackAsset physicalCameraTrack = timeline.CreateTrack(typeof(CinemachineTrack), null, "Physical Camera Track");
        //on récupère les tracks dans les positions suivantes dans la timeline
        AnimationTrack newVirtualTrack = (AnimationTrack)timeline.GetOutputTrack(2 + 4 * i); // i étant le numéra de la new paire cam, on commence en 2 et on se déplace de 4 en 4
        TrackAsset newPhysicalTrack = timeline.GetOutputTrack(3 + 4 * i); //à cause des cams bruitées

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
    }

    /// <summary>
    /// Permet de générer une caméra physique et virtuelle à partir d'un chemin donné.
    /// </summary>
    public SysCam GenerateCam(int i, CinemachineSmoothPath Cpath)
    {
        SysCam syscam = Instantiate(prefabCam, new Vector3(0, 0, 0), Quaternion.identity); //on instancie un SysCam qui contient une caméra physique et virtuelle
        cams[camCount / 2] = syscam.cam.GetComponent<MovieRecorderExample>();
        vcams[camCount / 2] = syscam.vcam;
        syscam.cam.name = "cam" + i * 2; //on lui donne un nom
        syscam.vcam.GetCinemachineComponent<CinemachineTrackedDolly>().m_Path = Cpath; //on lui associe le chemin généré
        syscam.cam.transform.position = Cpath.m_Waypoints[0].position; //on place la caméra au début du chemin
        syscam.vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = bruitAmplitude;
        syscam.vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = bruitFrequency;
        syscam.vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_NoiseProfile = null; //on veut pas de bruit sur la cam normale
        return syscam;
    }

    /// <summary>
    /// Permet de générer un chemin à partir d'un point de départ aléatoire.
    /// </summary>
    private CinemachineSmoothPath GeneratePath()
    {
        //GENERATION POINT DE DEPART
        point = new Vector3(
            UnityEngine.Random.Range(terrainPos.x + 100, terrainPos.x + 900),
            terrainPos.y,
            UnityEngine.Random.Range(terrainPos.z + 100, terrainPos.z + 900)
        ); //on génère un point de départ aléatoire
        float y = terrain.SampleHeight(point); //on récupère la hauteur du terrain à ce point
        point.y = y - offsetcam; //on met le point plus ou moins au dessus du sol

        //
        //GENERATION PATH A PARTIR DU POINT DE DEPART
        CinemachineSmoothPath Cpath = Instantiate(Prefabpath, new Vector3(0, 0, 0), Quaternion.identity);
        Cpath.m_Waypoints = new CinemachineSmoothPath.Waypoint[pointCnt];
        Cpath.m_Waypoints[0].position = point;
        for (int j = 1; j < pointCnt; j++)
        { //on génère 10 points aléatoires de chemin
            point = GeneratePoint(point, radius);
            CheckTerrain(point); //au cas où on déborde sur un autre terrain
            y = terrain.SampleHeight(point); //on récupère la hauteur du terrain à ce point
            point.y = y - offsetcam; //on met le point au dessus du sol
            Cpath.m_Waypoints[j].position = point; //on lui associe la position
                                                   //
                                                   //Instanciation d'un objet aux environs du point généré

            for (int i = 0; i < numObject; i++)
            {
                y = GenerateObjectOnPath();
            }
        }

        return Cpath;
    }

    /// <summary>
    /// Permet de générer un ou plusieurs objets aux environs d'un point donné.
    /// </summary>
    private float GenerateObjectOnPath()
    {
        float y;
        Vector3 cubPoint = GeneratePoint(point, 10); //comme ça quand le cube apparait, il est autour du point ciblé
        CheckTerrain(cubPoint);
        y = terrain.SampleHeight(cubPoint); //on récupère la hauteur du terrain à ce point
        cubPoint.y = y; //on met le point au dessus du sol
                        //cubeManager.CreateCube(point.x, point.y-4.5f, point.z);
        int prefabRand = UnityEngine.Random.Range(0, 4);
        if (prefabRand == 0 || onlyCube)
        {
            cubPoint.y = y - 39.5f; //on met le point au dessus du sol
            generatedObject = cubeManager.CreateCube(cubPoint.x, cubPoint.y, cubPoint.z, cubePrefab);
            generatedObject.name = "cube" + count;
            count++;
        }
        else if (prefabRand == 1 && !onlyCube)
        {
            cubPoint.y = y - 40f; //on met le point au dessus du sol
            generatedObject = cubeManager.CreateCube(cubPoint.x, cubPoint.y, cubPoint.z, tigrePrefab);
            generatedObject.name = "tigre" + count;
            count++;
        }
        else if (prefabRand == 2 && !onlyCube)
        {
            cubPoint.y = y - 39.6f; //on met le point au dessus du sol
            generatedObject = cubeManager.CreateCube(cubPoint.x, cubPoint.y, cubPoint.z, taureauPrefab);
            generatedObject.name = "taureau" + count;
            count++;
        }
        else if (prefabRand == 3 && !onlyCube)
        {
            cubPoint.y = y - 37f; //on met le point au dessus du sol
            generatedObject = cubeManager.CreateCube(cubPoint.x, cubPoint.y, cubPoint.z, aiglePrefab);
            generatedObject.name = "aigle" + count;
            count++;
        }
        else
        {
            generatedObject = null;
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

        return y;
    }

    /// <summary>
    /// Permet de préparer la timeline en supprimant les tracks inutiles et en fixant les deux premières tracks comme les tracks templates.
    /// </summary>
    public void PrepareTimeline()
    {
        cams = new MovieRecorderExample[nBCamera * 2];
        vcams = new CinemachineVirtualCamera[nBCamera * 2];
        originalVirtualTrack = timeline.GetOutputTrack(0); // assuming the original virtual camera is the first track
        originalPhysicalTrack = timeline.GetOutputTrack(1); // assuming the original physical camera is the second track
        int k = 0;
        //
        //On supprime les tracks de la timeline sauf les 2 premières qui sont les templates
        foreach (TrackAsset track in timeline.GetOutputTracks())
        {
            if (k > 1)
            {
                timeline.DeleteTrack(track);
            }
            k++;
        }
    }

    /// <summary>
    /// Permet d'extraire les paramètres de la scène à partir d'un fichier de configuration.
    /// </summary>
    public void ExtractConfig()
    {
        string path = "config.txt";
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
                this.nBCamera = Convert.ToInt32(line[1]);
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
                //this.flouPane.SetActive(Convert.ToInt32(line[1])==1);
                //commenté car pas intéressant pour le moment en automatique (pas testé non plus, floupane n'étant pas généré en automatique)
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
                //commenté car pas intéressant pour le moment en automatique (pas testé non plus)
            }
            else if (line[0] == "repeat")
            {
                this.repeat = Convert.ToInt32(line[1]) == 1;
            }
        }
    }

    // Update est appelé à chaque frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Restart();
        }
        CheckFinished();
        if(finished && repeat){
            Restart();
        }
        if(Input.GetKeyDown(KeyCode.R)){
            repeat = !repeat;
            Debug.Log("repeat: " + repeat);
        }
    }

    /// <summary>
    /// Permet de vérifier si au moins une caméra a fini son chemin.
    /// </summary>
    public void CheckFinished(){
        foreach(CinemachineVirtualCamera vcam in vcams){
            if(vcam.GetCinemachineComponent<CinemachineTrackedDolly>().m_PathPosition > 9.9){
                finished = true;
            }
        }
    }

    /// <summary>
    /// Permet de relancer la scène en désactivant les enregistrements.
    /// </summary>
    public void Restart()
    {
        foreach (MovieRecorderExample mov in cams)
        {
            mov.DisableVideo();
        }
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f);
        StartCoroutine(WaitAndRestartScene(waitForSeconds));
    }

    /// <summary>
    /// Coroutine permettant de relancer la scène après un certain temps, en s'assurant que les caméras n'enregistrent plus.
    /// </summary>
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

    /// <summary>
    /// Permet de générer un point aléatoire autour d'un point donné.
    /// </summary>
    private Vector3 GeneratePoint(Vector3 previousPoint, float radius){
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere.normalized;
        Vector3 randomPoint = previousPoint + randomDirection * radius;
        return randomPoint;
    }

    /// <summary>
    /// Permet de vérifier si le terrain est toujours le même. S'il ne l'est pas, il le change.
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
