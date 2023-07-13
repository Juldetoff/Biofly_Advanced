#if UNITY_EDITOR

using System.ComponentModel;
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

namespace UnityEngine.Recorder.Examples
{
    /// <summary>
    ///Cette classe permet de mettre en place une session d'enregistrement via un script, pour un fichier MP4, MOV ou WEBM.
    ///Pour l'utiliser, il suffit d'ajouter le composant MovieRecorderExample à un GameObject.
    /// 
    ///Lancer le mode lecture pour démarrer l'enregistrement.
    ///L'enregistrement s'arrête automatiquement lorsque vous quittez le mode lecture ou lorsque vous désactivez le composant.
    /// 
    ///Ce script enregistre les sorties d'enregistrement dans [Project Folder]/SampleRecordings.
    ///
    /// </summary>
    public class MovieRecorderExample : MonoBehaviour
    {
        public RecorderController m_RecorderController;
        internal MovieRecorderSettings m_Settings = null;
        [HideInInspector]public bool startVideo = false;
        [Tooltip("Script de lancement associé")]public StartScript startScript;
        [Header("Paramètres d'enregistrement")]
        public bool m_RecordAudio = true;
        //[Header("Paramètres de la caméra")]
        private string format = "mp4";
        private string quality = "high";
        private float fps = 60;
        [HideInInspector]public float startTime=0;
        private int count;

        public FileInfo OutputFile
        {
            get
            {
                var fileName = m_Settings.OutputFile +"."+ format;
                return new FileInfo(fileName);
            }
        }

        //OnEnable est appelé lorsque le script s'active
        void OnEnable()
        {   //on initialise le tag et on associe les variables nécessaires
            startScript = GameObject.FindObjectOfType<StartScript>();
            count = startScript.camCount;
            startScript.camCount++;
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
            n.stringValue = "cam" + count;
            tagManager.ApplyModifiedProperties();

            this.gameObject.tag = "cam" + count;
            Initialize();
        }

        /// <summary>
        /// Initialise l'enregistrement et le lance.
        /// </summary>
        internal void Initialize()
        {   
            if(startVideo){ //permet d'attendre un signal du StartScript pour savoir quand démarrer (permet l'attente des configs)
                format = startScript.formatSettings[startScript.videoType];
                quality = startScript.qualitySettings[startScript.videoQuality];
                fps = startScript.videoFps;
    
                
    
    
                var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
                m_RecorderController = new RecorderController(controllerSettings);
    
                var mediaOutputFolder = new DirectoryInfo(Path.Combine(Application.dataPath, "..", "SampleRecordings"));
    
                // Video
                m_Settings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
                m_Settings.name = "TEST";
                m_Settings.Enabled = true;
    
                // This example performs an MP4 recording
                if(format=="mp4"){
                    m_Settings.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MP4;
                }
                else if(format=="webm"){
                    m_Settings.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.WebM;
                }
                else if(format=="mov"){
                    m_Settings.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MOV;
                }
                if(quality=="low"){
                    m_Settings.VideoBitRateMode = VideoBitrateMode.Low;
                }
                else if(quality=="medium"){
                    m_Settings.VideoBitRateMode = VideoBitrateMode.Medium;
                }
                else if(quality=="high"){
                    m_Settings.VideoBitRateMode = VideoBitrateMode.High;
                }
                m_Settings.FrameRate = fps;
                
                //m_Settings.ImageInputSettings = new GameViewInputSettings
                m_Settings.ImageInputSettings = new CameraInputSettings
                {
                    OutputWidth = 1920,
                    OutputHeight = 1080,
                    Source = ImageSource.TaggedCamera,
                    CameraTag = "cam" + count
                };
                
                // choose the camera to use for the rendering
    
                m_Settings.AudioInputSettings.PreserveAudio = m_RecordAudio;
    
                // Simple file name (no wildcards) so that FileInfo constructor works in OutputFile getter.
                // the name of the object is used as the file name
                m_Settings.OutputFile = mediaOutputFolder.FullName + "/" + "cam" + count;
                
                
                // Setup Recording
                controllerSettings.AddRecorderSettings(m_Settings);
                controllerSettings.SetRecordModeToManual();
                // choose the camera to use for the rendering
                
                // setup the camera to use for the rendering
                controllerSettings.FrameRate = fps;
        
                RecorderOptions.VerboseMode = false;
                m_RecorderController.PrepareRecording();
                m_RecorderController.StartRecording();
                startTime = 0;
                startVideo = false;
    
                Debug.Log($"Started recording for file {OutputFile.FullName}");
                startVideo = false;

                //on crée un txt de détection vide
                string filePath = Application.dataPath + "/../Positions/"+ gameObject.name + ".txt";
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Détection des objets de la caméra à " + ( startTime ).ToString() + "s");
                }
                //startDronescript.start = currentTime;
                string filePath2 = Application.dataPath + "/../Positions/Start"+ name + ".txt";
                using (StreamWriter writer = new StreamWriter(filePath2))
                {
                    writer.WriteLine("Détection des objets de la caméra à " + ( startTime ).ToString() + "s");
                }
            }
        }

        //Start se lance avant la première frame
        private void Start() {
            this.gameObject.tag = "cam" + count;
        }

        //OnDisable est appelé lorsque le script se désactive
        void OnDisable()
        {
            DisableVideo();
        }

        /// <summary>
        /// Arrête l'enregistrement.
        /// </summary>
        public void DisableVideo(){
            if(m_RecorderController!=null){
                m_RecorderController.StopRecording();
                Debug.Log($"Stopped recording for file {OutputFile.FullName}");
            }
            else{
                Debug.Log("No recorder to stop for"+gameObject.tag +".mp4");
                //dans ce cas par sécurité pour le moment on va delete la vidéo (parce que sinon le programme ne tourne plus)
                File.Delete(Application.dataPath + "/../SampleRecordings/cam" + (count-2) + ".mp4"); 
            }
        }

        //Update se lance à chaque frame
        private void Update() {
            Initialize();
            string filePath2 = Application.dataPath + "/../Positions/Start"+name+".txt";
            using (StreamWriter writer = new StreamWriter(filePath2, true))
            {
                writer.WriteLine("Temps : " + ( Time.time-startTime ).ToString() + "s");
            }
        }
    }
}

#endif
