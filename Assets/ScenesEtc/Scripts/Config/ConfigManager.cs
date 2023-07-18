using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using TMPro;

/// <summary>
/// Cette classe gère les menus de configuration. Il est attaché à l'objet "ConfigManager" dans la scène "Menu".
/// </summary>
public class ConfigManager : MonoBehaviour
{
    //les variables suivantes servent dans Unity à associer les différents éléments de l'interface au script
    [Header("Menu Scene")]
    public GameObject menuScene;

    [Header("Menu Bruit")]
    public GameObject menuBruit; //contient également le nbcamera et probablement flou
    public Slider sliderFreq;
    public TextMeshProUGUI textFreq;
    public Slider sliderAmpl;
    public TextMeshProUGUI textAmpl;
    public Slider sliderCam;
    public TextMeshProUGUI textCam;

    [Header("Menu Video")]
    public GameObject menuVideo;
    public Button buttonSubmit;
    public TextMeshProUGUI textQuality;
    public Slider sliderQuality;
    public TextMeshProUGUI textFPS;
    public Slider sliderFPS;

    

    //les variables suivantes servent à stocker les valeurs des menus avant d'être enregistrées dans un txt de config
    private int scene;
    private int config;
    private int nbCam;
    private int typeBruit;
    private int amplBruit;
    private int freqBruit;
    private int flou;
    private int typeVideo;
    private int qualiteVideo;
    private int fpsvideo;
    private int jello;
    private bool repetition;


    // Start est appelé avant la première frame
    void Start()
    {
        //on initialise les valeurs
        scene = 1;
        config = 0; //on va sur le premier menu des configs
        nbCam = 1;
        typeBruit = 0;
        amplBruit = 0;
        freqBruit = 0;
        flou = 0;
        typeVideo = 0;
        qualiteVideo = 2;
        fpsvideo = 60;
        jello = 0;
        repetition = false;
        sliderFreq.value = 3;
        textFreq.text = "Fréquence bruit : 3";
        sliderAmpl.value = 3;
        textAmpl.text = "Amplitude bruit : 3";
        sliderCam.value = 1;
        textCam.text = "Nombre caméras : 1";

        //on affiche ou non les menus
        menuScene.SetActive(true);
        menuBruit.SetActive(false);
        menuVideo.SetActive(false);
    }

    /// <summary>
    /// Cette fonction est appelée quand on clique sur le bouton "Next" d'un menu. Gère simplement les transitions entre les menus.
    /// </summary>
    public void Next(){ 
        if(config == 0){
            menuScene.SetActive(false);
            menuBruit.SetActive(true);
            menuVideo.SetActive(false);
            config = 1;
            if(scene==2 || scene==3){
                sliderCam.value = 1;
                sliderCam.interactable = false;
            }
            else{
                sliderCam.interactable = true;
            }
        }
        else if(config == 1){
            menuScene.SetActive(false);
            menuBruit.SetActive(false);
            menuVideo.SetActive(true);
            config = 2;
        }
        else if(config==2){
            OnSubmit();
        }
    }

    /// <summary>
    /// Cette fonction est appelée quand on clique sur une scène dans le choix des scènes. Cela associe la scène à la variable scene.
    /// </summary>
    public void ChangeScene(int scene){
        this.scene = scene;
    }

    /// <summary>
    /// Cette fonction est appelée quand on utilise le slider de fréquence du bruit. Elle change la valeur de la variable freqBruit et met à jour le texte.
    /// </summary>
    public void ChangeFreq(){
        freqBruit = (int)sliderFreq.value;
        textFreq.text = "Fréquence bruit : "+freqBruit;
    }

    /// <summary>
    /// Cette fonction est appelée quand on utilise le slider d'amplitude du bruit. Elle change la valeur de la variable amplBruit et met à jour le texte.
    /// </summary>
    public void ChangeAmpl(){
        amplBruit = (int)sliderAmpl.value;
        textAmpl.text = "Amplitude bruit : "+amplBruit;
    }

    /// <summary>
    /// Cette fonction est appelée quand on utilise le slider du nombre de caméras. Elle change la valeur de la variable nbCam et met à jour le texte.
    /// </summary>
    public void ChangeCam(){
        nbCam = (int)sliderCam.value;
        textCam.text = "Nombre caméras : "+nbCam;
    }

    /// <summary>
    /// Cette fonction est appelée quand on sur le bouton à cocher pour le flou. Active ou non le flou pour la scène (si supporté).
    /// </summary>
    public void ChangeFlou(){
        flou = -flou+1;
    }

    /// <summary>
    /// Cette fonction est appelée quand on sur le bouton à cocher pour le jello. Active ou non le jello pour la scène (si supporté).
    /// </summary>
    public void ChangeJello(){
        jello = -jello+1;
    }

    /// <summary>
    /// Cette fonction est appelée quand on change le type de vidéo. Elle change la valeur de la variable typeVideo.
    /// </summary>
    public void ChangeTypeVideo(int type){ //0 pour mp4, 1 pour mov, 2 pour webm
        typeVideo = type;
    }

    /// <summary>
    /// Cette fonction est appelée quand on change le slider de la qualité de la vidéo. Elle change la valeur de la variable qualiteVideo et met à jour le texte.
    /// </summary>
    public void ChangeQualiteVideo(){ //0 pour low, 1 pour medium, 2 pour high
        qualiteVideo = (int)sliderQuality.value;
        if(qualiteVideo == 0){
            textQuality.text = "Qualité : low";
        }
        else if(qualiteVideo == 1){
            textQuality.text = "Qualité : medium";
        }
        else if(qualiteVideo == 2){
            textQuality.text = "Qualité : high";
        }
    }

    /// <summary>
    /// Cette fonction est appelée quand on change le type de bruit. Elle change la valeur de la variable typeBruit.
    /// </summary>
    public void ChangeBruit(int type){ //0 pour Shake6D, 1 pour Wobble6D, 2 pour Handheld_normal_extreme, 3 pour Handheld_normal_mild, 
    //4 pour Handheld_normal_strong, 5 pour Handheld_tele_mild, 6 pour Handheld_tele_strong, 7 pour Handheld_wideangle_mild, 
    //8 pour Handheld_wideangle_strong
        typeBruit = type; 
    }

    /// <summary>
    /// Cette fonction est appelée quand on change le slider du fps de la vidéo. Elle change la valeur de la variable fpsvideo et met à jour le texte.
    /// </summary>
    public void ChangeFPSVideo(){
        fpsvideo = (int)sliderFPS.value;
        textFPS.text = "FPS : "+fpsvideo;
    }

    /// <summary>
    /// Cette fonction est appelée quand on clique sur le bouton à cocher pour la répétition. Active ou non la répétition pour la scène.
    /// </summary>
    public void ChangeRepeat(){
        repetition = !repetition;
    }

    /// <summary>
    /// Cette fonction est appelée quand on clique sur le bouton "Submit" du menu vidéo. Elle écrit les valeurs dans un fichier config.txt et lance la scène.
    /// </summary>
    public void OnSubmit(){
        string filePath = Application.dataPath + "./../config.txt";
        //StreamWriter sr = File.CreateText(filePath);
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("11");
            writer.WriteLine("scene,"+scene); //1 pour scene auto foret, 2 pour foret manuel,3,4,5... pour les préfaits (voir si on en fait)
            writer.WriteLine("nBCamera,"+nbCam); //nombre de caméra, certaines scènes fixent ça
            writer.WriteLine("bruitType,"+typeBruit); //0 pour aucun, 1 pour bruit blanc, 2 pour bruit rose, 3 pour bruit brownien...
            writer.WriteLine("bruitAmplitude,"+amplBruit); //amplitude du bruit
            writer.WriteLine("bruitFrequency,"+freqBruit); //fréquence du bruit
            writer.WriteLine("flou,"+flou); //flou ou non (0 non, 1 oui)
            writer.WriteLine("videoType,"+typeVideo); //0 pour mp4, 1 pour mov, 2 pour webm
            writer.WriteLine("videoQuality,"+qualiteVideo); //0 pour low, 1 pour medium, 2 pour high
            writer.WriteLine("fps,"+fpsvideo); //fps de la vidéo
            writer.WriteLine("jello,"+jello); //jello ou non (0 non, 1 oui)
            writer.WriteLine("repeat,"+repetition); //si se repète ou non en auto (0 non, 1 oui)
            writer.Close();
        }
        //en dessous on gère le changement de scène, bien que cela rend inutile la ligne "scene,..." dans config.txt
        SceneManager.LoadScene(scene);
        
    }
}
