#if UNITY_EDITOR

using System.ComponentModel;
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

namespace UnityEngine.Recorder.Examples
{
    public class MovieRecordManual : MonoBehaviour
    {
        RecorderController m_RecorderController;
        public bool m_RecordAudio = true;
        public StartDrone startDronescript;
        internal MovieRecorderSettings m_Settings = null;
        public string title = "TEST";
        private string format = "mp4";
        private string quality = "high";

        public FileInfo OutputFile
        {
            get
            {
                var fileName = "DroneView" + title + "." + format;
                return new FileInfo(fileName);
            }
        }
        void Awake()
        {
            Initialize();
        }

        internal void Initialize()
        {

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
            n.stringValue = "cam" + title;
            tagManager.ApplyModifiedProperties();

            this.gameObject.tag = "cam" + title;


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
            
            //m_Settings.ImageInputSettings = new GameViewInputSettings
            m_Settings.ImageInputSettings = new CameraInputSettings
            {
                OutputWidth = 1920,
                OutputHeight = 1080,
                Source = ImageSource.TaggedCamera,
                CameraTag = "cam" + title
            };
            
            // choose the camera to use for the rendering

            m_Settings.AudioInputSettings.PreserveAudio = m_RecordAudio;

            // Simple file name (no wildcards) so that FileInfo constructor works in OutputFile getter.
            // the name of the object is used as the file name
            m_Settings.OutputFile = mediaOutputFolder.FullName + "/" + "cam" + title;
            
            
            // Setup Recording
            controllerSettings.AddRecorderSettings(m_Settings);
            controllerSettings.SetRecordModeToManual();
            // choose the camera to use for the rendering
            
            controllerSettings.FrameRate = 60.0f;
            // setup the camera to use for the rendering
    
            RecorderOptions.VerboseMode = false;
            m_RecorderController.PrepareRecording();
            m_RecorderController.StartRecording();

            Debug.Log($"Started recording for file {OutputFile.FullName}");
        }
        private void Start() {
            this.gameObject.tag = "cam" + title;
        }

        void OnDisable()
        {
            m_RecorderController.StopRecording();
        }
    }
}
#endif