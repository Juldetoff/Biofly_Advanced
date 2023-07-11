#if UNITY_EDITOR

using System.ComponentModel;
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

namespace UnityEngine.Recorder.Examples
{
    /// <summary>
    /// This example shows how to set up a recording session via script, for an MP4 file.
    /// To use this example, add the MovieRecorderExample component to a GameObject.
    ///
    /// Enter the Play Mode to start the recording.
    /// The recording automatically stops when you exit the Play Mode or when you disable the component.
    ///
    /// This script saves the recording outputs in [Project Folder]/SampleRecordings.
    /// </summary>
    public class MovieRecorderExample : MonoBehaviour
    {
        public RecorderController m_RecorderController;
        public bool m_RecordAudio = true;
        public StartScript startScript;
        public int count;
        internal MovieRecorderSettings m_Settings = null;

        private string format = "mp4";
        private string quality = "high";
        private float fps = 60;
        public float startTime=0;
        public bool startVideo = false;

        public FileInfo OutputFile
        {
            get
            {
                var fileName = m_Settings.OutputFile +"."+ format;
                return new FileInfo(fileName);
            }
        }

        void OnEnable()
        {   
            //startScript = GameObject.Find("GameStartManager").GetComponent<StartScript>();
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
        private void Start() {
            this.gameObject.tag = "cam" + count;
        }

        void OnDisable()
        {
            DisableVideo();
        }
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
