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
    public Button buttonAuto;
    public Button buttonSubmit;
    public Button buttonManual;  

    private bool isAuto;


    // Start is called before the first frame update
    void Start()
    {
        isAuto = true;
        sliderFreq.value = 3;
        textFreq.text = "Fréquence bruit : 3";
        sliderAmpl.value = 3;
        textAmpl.text = "Amplitude bruit : 3";
        sliderCam.value = 1;
        textCam.text = "Nombre : 1";
        buttonAuto.interactable = false;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeFreq(){
        textFreq.text = "Fréquence bruit : "+sliderFreq.value;
    }

    public void ChangeAmpl(){
        textAmpl.text = "Amplitude bruit : "+sliderAmpl.value;
    }

    public void ChangeCam(){
        textCam.text = "Nombre : "+sliderCam.value;
    }

    public void Auto(){
        isAuto = true;
        buttonAuto.interactable = false;
        buttonManual.interactable = true;
        sliderAmpl.interactable = true;
        sliderFreq.interactable = true;
        sliderCam.interactable = true;
    }

    public void Manual(){
        isAuto = false;
        buttonAuto.interactable = true;
        buttonManual.interactable = false;
        sliderCam.value = 1;
        textCam.text = "Nombre : 1";
        sliderFreq.value = 0;
        textFreq.text = "Fréquence bruit : 0";
        sliderAmpl.value = 0;
        textAmpl.text = "Amplitude bruit : 0";
        sliderAmpl.interactable = true;
        sliderFreq.interactable = true;
        sliderCam.interactable = false;
    }

    public void OnSubmit(){
        string filePath = Application.dataPath + "/../config.txt";
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("4");
            writer.WriteLine("scene,NULL");
            writer.WriteLine("nBCamera,"+sliderCam.value);
            writer.WriteLine("bruitAmplitude,"+sliderAmpl.value);
            writer.WriteLine("bruitFrequency,"+sliderFreq.value);
        }
        if(isAuto){
            SceneManager.LoadScene(1);
        }
        else{
            SceneManager.LoadScene(2);
        }
    }
}
