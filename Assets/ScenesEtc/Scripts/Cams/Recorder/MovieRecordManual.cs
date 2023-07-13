#if UNITY_EDITOR

using System.ComponentModel;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using System.Collections;

namespace UnityEngine.Recorder.Examples
{
    /// <summary>
    ///Cette classe permet de mettre en place une session d'enregistrement via un script, pour un fichier MP4, MOV ou WEBM.
    ///Pour l'utiliser, il suffit d'ajouter le composant MovieRecorderManuale à un GameObject.
    /// 
    ///Lancer le mode lecture pour démarrer l'enregistrement.
    ///L'enregistrement s'arrête automatiquement lorsque vous quittez le mode lecture ou lorsque vous désactivez le composant.
    /// 
    ///Ce script enregistre les sorties d'enregistrement dans [Project Folder]/SampleRecordings.
    ///
    ///Cette classe est une modification de MovieRecorderExample.cs,
    ///elle permet de lancer l'enregistrement manuel et de se lier au Jello Effect dans le cas où il est activé.
    /// </summary>
    public class MovieRecordManual : MonoBehaviour
    {
        RecorderController m_RecorderController;
        internal MovieRecorderSettings m_Settings = null;
        [HideInInspector]public bool startvideo = false;

        [Tooltip("Script de lancement associé")]public StartDrone startDronescript;
        [Header("Paramètres d'enregistrement")]
        public bool m_RecordAudio = true;
        public bool recordEffect = false;
        [Tooltip("Permet ou non d'activer l'enregistrement pour une caméra")]public bool isRegister = false;

        [Header("Paramètres de la caméra")]
        [Tooltip("Nom donné à l'enregistrement")]public string title = "TEST";
        private string format = "mp4";
        private string quality = "high";
        public float fps = 60;
        [HideInInspector]public float startTime=0;

        [Header("Paramètres du Jello Effect")]
        [Tooltip("Jello effect à associer à la caméra afin de le prendre en compte")]public HDRP_RollingShutter rollingShutterEffect;
        private RenderTextureInputSettings renderTextureInputSettings;


        //awake se lance avant start
        void Awake()
        {
            startvideo = false; //on fixe startvideo à false afin que le script start puisse décider quand lancer l'enregistrement
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
            startDronescript.lastTime = startTime; //afin de bien synchroniser les temps de chacun
        }

        /// <summary>
        /// Permet de lancer l'enregistrement en récupérant tout les paramètres nécessaires.
        /// </summary>
        internal void Initialize()
        {
            if(startvideo){
                startvideo = false; //pour que start gère le lancement de la vidéo (donc + ou - synchro)
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
                    // RenderTexture inputTexture = rollingShutterEffect.GetRenderTexture();
                    // rollingShutterEffect.ApplyRollingShutterEffect(inputTexture, inputTexture);
                    // renderTextureInputSettings.RenderTexture = inputTexture;
                    StartCoroutine(WaitForTextureGeneration());
                }
                
                m_RecorderController = new RecorderController(controllerSettings);
                controllerSettings.FrameRate = fps;
                
                m_RecorderController.PrepareRecording();
                
                m_RecorderController.StartRecording();
                startTime=0;
            }
        }
        //Start se lance après awake et avant la première frame
        private void Start() {
            this.gameObject.tag = "cam" + title; //on associe le tag à la caméra
        }

        //OnDisable se lance lorsque le composant est désactivé
        void OnDisable()
        {
            DisableVideo();
        }

        /// <summary>
        /// Permet de stopper l'enregistrement.
        /// </summary>
        public void DisableVideo(){
            if(m_RecorderController!=null){
                m_RecorderController.StopRecording();
                string filePath = Application.dataPath + "/../Positions/Start"+name+".txt";
                int numberOfLinesToRemove = 2; 
                RemoveFirstLines(filePath, numberOfLinesToRemove); //afin de retirer les lignes excessives au début du fichier pour qu'il soit
                //plus facilement lisible par le script d'entraînement de l'IA (projet lié)
            }
            else{ //cas possible quand le script est modifié lors de l'enregistrement et enregistré, 
                //unity va reload les scripts et cela va entraîner une corruption des vidéos
                //que les scripts ne peuvent pas gérer
                Debug.Log("No recorder to stop");
                //dans ce cas par sécurité on va delete la vidéo
                File.Delete(Application.dataPath + "/../SampleRecordings/cam" + title + ".mp4"); 
            }
        }

        /// <summary>
        /// Permet de retirer les premières lignes d'un fichier. 
        /// </summary>
        static void RemoveFirstLines(string filePath, int numberOfLinesToRemove)
        {
            string[] lines = File.ReadAllLines(filePath);
            string[] remainingLines = new string[lines.Length - numberOfLinesToRemove];

            Array.Copy(lines, numberOfLinesToRemove, remainingLines, 0, remainingLines.Length);

            File.WriteAllLines(filePath, remainingLines);
        }

        //Update se lance à chaque frame
        void Update()
        {
            Initialize(); //initialize ne s'active correctement que si startvideo est à true, donc on le relance à chaque frame
            if (rollingShutterEffect != null && rollingShutterEffect.enabled && recordEffect)
            {
                // RenderTexture inputTexture = rollingShutterEffect.GetRenderTexture();
                // rollingShutterEffect.ApplyRollingShutterEffect(inputTexture, inputTexture);
                // renderTextureInputSettings.RenderTexture = inputTexture;
                StartCoroutine(WaitForTextureGeneration());
            }
            if(isRegister){
                string filePath2 = Application.dataPath + "/../Positions/Start"+name+".txt";
                using (StreamWriter writer = new StreamWriter(filePath2, true))
                {
                    writer.WriteLine("Temps :" + ( Time.time-startTime  - 1/30f ).ToString() + "s"); 
                }
            }

        }

        /// <summary>
        /// Permet d'attendre la récupération de la texture générée par le Jello Effect.
        /// </summary>
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

        // private MovieRecorderSettings CloneMovieRecorderSettings(MovieRecorderSettings originalSettings)
        // {
        //     var clonedSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        //     clonedSettings.name = originalSettings.name;
        //     clonedSettings.Enabled = originalSettings.Enabled;
        //     clonedSettings.OutputFormat = originalSettings.OutputFormat;
        //     clonedSettings.VideoBitRateMode = originalSettings.VideoBitRateMode;
        //     clonedSettings.FrameRate = originalSettings.FrameRate;
        //     clonedSettings.OutputFile = originalSettings.OutputFile;

        //     return clonedSettings;
        // }

    }
}
#endif