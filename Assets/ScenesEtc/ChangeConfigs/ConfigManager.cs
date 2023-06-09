using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using TMPro;

public class ConfigManager : MonoBehaviour
{
    public Slider sliderFreq;
    public TextMeshProUGUI textFreq;
    public Slider sliderAmpl;
    public TextMeshProUGUI textAmpl;
    public Slider sliderCam;
    public TextMeshProUGUI textCam;
    public Button buttonSubmit;
    public TextMeshProUGUI textQuality;
    public Slider sliderQuality;
    public TextMeshProUGUI textFPS;
    public Slider sliderFPS;

    public GameObject menuScene;
    public GameObject menuBruit; //contient également le nbcamera et probablement flou
    public GameObject menuVideo;

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


    // Start is called before the first frame update
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

    public void Next(){
        if(config == 0){
            menuScene.SetActive(false);
            menuBruit.SetActive(true);
            menuVideo.SetActive(false);
            config = 1;
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

    public void ChangeScene(int scene){
        this.scene = scene;
    }

    public void ChangeFreq(){
        freqBruit = (int)sliderFreq.value;
        textFreq.text = "Fréquence bruit : "+freqBruit;
    }

    public void ChangeAmpl(){
        amplBruit = (int)sliderAmpl.value;
        textAmpl.text = "Amplitude bruit : "+amplBruit;
    }

    public void ChangeCam(){
        nbCam = (int)sliderCam.value;
        textCam.text = "Nombre caméras : "+nbCam;
    }

    public void ChangeFlou(){
        flou = -flou+1;
    }

    public void ChangeTypeVideo(int type){ //0 pour mp4, 1 pour mov, 2 pour webm
        typeVideo = type;
    }

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

    public void ChangeBruit(int type){ //0 pour Shake6D, 1 pour Wobble6D, 2 pour Handheld_normal_extreme, 3 pour Handheld_normal_mild, 
    //4 pour Handheld_normal_strong, 5 pour Handheld_tele_mild, 6 pour Handheld_tele_strong, 7 pour Handheld_wideangle_mild, 
    //8 pour Handheld_wideangle_strong
        typeBruit = type; 
    }

    public void ChangeFPSVideo(){
        fpsvideo = (int)sliderFPS.value;
        textFPS.text = "FPS : "+fpsvideo;
    }

    public void OnSubmit(){
        string filePath = Application.dataPath + "./../config.txt";
        //StreamWriter sr = File.CreateText(filePath);
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("9");
            writer.WriteLine("scene,"+scene); //1 pour scene auto foret, 2 pour foret manuel,3,4,5... pour les préfaits (voir si on en fait)
            writer.WriteLine("nBCamera,"+nbCam); //nombre de caméra, certaines scènes fixent ça
            writer.WriteLine("bruitType,"+typeBruit); //0 pour aucun, 1 pour bruit blanc, 2 pour bruit rose, 3 pour bruit brownien...
            writer.WriteLine("bruitAmplitude,"+amplBruit); //amplitude du bruit
            writer.WriteLine("bruitFrequency,"+freqBruit); //fréquence du bruit
            writer.WriteLine("flou,"+flou); //flou ou non (0 non, 1 oui)
            writer.WriteLine("videoType,"+typeVideo); //0 pour mp4, 1 pour mov, 2 pour webm
            writer.WriteLine("videoQuality,"+qualiteVideo); //0 pour low, 1 pour medium, 2 pour high
            writer.WriteLine("fps,"+fpsvideo); //fps de la vidéo
            writer.Close();
        }
        //en dessous on gère le changement de scène, bien que cela rend inutile la ligne "scene,..." dans config.txt
        SceneManager.LoadScene(scene);
        
    }
}
