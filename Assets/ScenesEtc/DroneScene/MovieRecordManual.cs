#if UNITY_EDITOR

using System.ComponentModel;
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using System.Collections;

namespace UnityEngine.Recorder.Examples
{
    public class MovieRecordManual : MonoBehaviour
    {
        RecorderController m_RecorderController;
        public bool m_RecordAudio = true;
        public StartDrone startDronescript;
        public bool recordEffect = false;
        internal MovieRecorderSettings m_Settings = null;
        public string title = "TEST";
        private string format = "mp4";
        private string quality = "high";
        private float fps = 60;
        public float startTime=0;
        public HDRP_RollingShutter rollingShutterEffect;
        private RenderTextureInputSettings renderTextureInputSettings;
        public bool startvideo = false;
        public bool isRegister = false;

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
            //on crée un tag pour la caméra (pour pouvoir la détecter
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            int count = tagsProp.arraySize;
            tagsProp.InsertArrayElementAtIndex(count);
            SerializedProperty n = tagsProp.GetArrayElementAtIndex(count);
            n.stringValue = "cam" + title;
            tagManager.ApplyModifiedProperties();
            this.gameObject.tag = "cam" + title;

            Initialize();
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
            startDronescript.lastTime = startTime;
        }

        internal void Initialize()
        {
            if(startvideo){
                format = startDronescript.formatSettings[startDronescript.videoType];
                quality = startDronescript.qualitySettings[startDronescript.videoQuality];
                fps = startDronescript.videoFps;

                var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();

                var mediaOutputFolder = new DirectoryInfo(Path.Combine(Application.dataPath, "..", "SampleRecordings"));

                // Video
                m_Settings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
                m_Settings.name = "TEST";
                m_Settings.Enabled = true;
                controllerSettings.AddRecorderSettings(m_Settings);

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
                // Définir le framerate de capture
                
                // choose the camera to use for the rendering
                if (rollingShutterEffect != null && rollingShutterEffect.enabled && recordEffect)
                {
                    StartCoroutine(WaitForTextureGeneration());
                }
                else{
                    m_Settings.ImageInputSettings = new CameraInputSettings
                    {
                        OutputWidth = 1920,
                        OutputHeight = 1080,
                        Source = ImageSource.TaggedCamera,
                        CameraTag = "cam" + title
                    };
                }
                
                // choose the camera to use for the rendering

                m_Settings.AudioInputSettings.PreserveAudio = m_RecordAudio;

                // Simple file name (no wildcards) so that FileInfo constructor works in OutputFile getter.
                // the name of the object is used as the file name
                m_Settings.OutputFile = mediaOutputFolder.FullName + "/" + "cam" + title;
                
                controllerSettings.SetRecordModeToManual();
                
                // choose the camera to use for the rendering
                // setup the camera to use for the rendering
        
                RecorderOptions.VerboseMode = false;

                if (rollingShutterEffect != null && rollingShutterEffect.enabled && recordEffect)
                {
                    RenderTexture inputTexture = rollingShutterEffect.GetRenderTexture();
                    rollingShutterEffect.ApplyRollingShutterEffect(inputTexture, inputTexture);
                    renderTextureInputSettings.RenderTexture = inputTexture;
                }
                
                m_RecorderController = new RecorderController(controllerSettings);
                controllerSettings.FrameRate = fps;
                
                m_RecorderController.PrepareRecording();
                
                m_RecorderController.StartRecording();
                startTime=0;

                //Debug.Log($"Started recording for file {OutputFile.FullName}");
                startvideo = false; //pour que start gère le lancement de la vidéo (donc + ou - synchro)
            }
        }
        private void Start() {
            this.gameObject.tag = "cam" + title;
        }

        void OnDisable()
        {
            m_RecorderController.StopRecording();
        }

        void Update()
        {
            Initialize();
            if (rollingShutterEffect != null && rollingShutterEffect.enabled && recordEffect)
            {
                RenderTexture inputTexture = rollingShutterEffect.GetRenderTexture();
                rollingShutterEffect.ApplyRollingShutterEffect(inputTexture, inputTexture);
                renderTextureInputSettings.RenderTexture = inputTexture;
            }
            if(isRegister){
                string filePath2 = Application.dataPath + "/../Positions/Start"+name+".txt";
                using (StreamWriter writer = new StreamWriter(filePath2, true))
                {
                    writer.WriteLine("Temps :" + ( Time.time-startTime ).ToString() + "s");
                }
            }

        }

        private IEnumerator WaitForTextureGeneration()
        {
            // Attendez que la texture soit générée
            while (rollingShutterEffect.GetRenderTexture() == null)
            {
                yield return null;
            }

            // Une fois que la texture est générée, configurez renderTextureInputSettings
            renderTextureInputSettings = new RenderTextureInputSettings
            {
                OutputWidth = rollingShutterEffect.sceneCamera.pixelWidth,
                OutputHeight = rollingShutterEffect.sceneCamera.pixelHeight,
                RenderTexture = rollingShutterEffect.GetRenderTexture()
            };

            // Assignez renderTextureInputSettings à m_Settings.ImageInputSettings
            m_Settings.ImageInputSettings = renderTextureInputSettings;
        }

        private MovieRecorderSettings CloneMovieRecorderSettings(MovieRecorderSettings originalSettings)
        {
            var clonedSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            clonedSettings.name = originalSettings.name;
            clonedSettings.Enabled = originalSettings.Enabled;
            clonedSettings.OutputFormat = originalSettings.OutputFormat;
            clonedSettings.VideoBitRateMode = originalSettings.VideoBitRateMode;
            clonedSettings.FrameRate = originalSettings.FrameRate;
            clonedSettings.OutputFile = originalSettings.OutputFile;

            return clonedSettings;
        }

    }
}
#endif