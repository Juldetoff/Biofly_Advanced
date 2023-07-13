using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

///<summary>
///Cette classe sert à stocker les différentes caméras du système. Dans un prefabs elle permet de directement lier les caméras à la classe.
///</summary>
public class SysCam : MonoBehaviour
{
    [Header("Caméras")]
    [Tooltip("Caméra réelle permettant de voir la scène")]public Camera cam;
    [Tooltip("Caméra virtuelle permettant de manipuler la caméra réelle")]public CinemachineVirtualCamera vcam;

    [Header("Autres")]
    [Tooltip("Permet de connecter la caméra à la Timeline dans le cas automatique")]public CinemachineBrain brain;

    [Tooltip("Permet d'animer la caméra dans le cas automatique")]public Animator animator;

    //public StartScript startScript;
}
