using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cette classe gère la génération de Cubes rouge à qr code aléatoire. Elle est attachée aux prefabs de Cubes rouge.
/// </summary>
public class QrCube : MonoBehaviour
{
    [Tooltip("Liste des objets points jaune affichables.")]public List<GameObject> dotList = new List<GameObject>();
    [Tooltip("Liste des QR code sur les faces du cube (plan).")]public List<MeshRenderer> FaceList = new List<MeshRenderer>();
    //public SpriteRenderer qrSprite = null;

    //Les variables suivantes correspondent aux 8 coins du cube (left, right, top, front, back, down)
    //la position de chacun dépend des autres, donc peut importe la position du premier coin choisit
    [Tooltip("coin left-top-front")]public Transform ltf;
    [Tooltip("coin left-top-bottom")]public Transform ltb;
    [Tooltip("coin left-down-front")]public Transform ldf;
    [Tooltip("coin left-down-bottom")]public Transform ldb;
    [Tooltip("coin right-top-front")]public Transform rtf;
    [Tooltip("coin right-top-bottom")]public Transform rtb;
    [Tooltip("coin right-down-front")]public Transform rdf;
    [Tooltip("coin right-down-bottom")]public Transform rdb;

    /// <summary>
    /// Cette fonction permet de fixer le matériel du QR code afin de l'afficher sur le cube.
    /// </summary>
    public void SetQrMaterial(Material sprite) // Set the sprite of the QR code
    {
        int num = UnityEngine.Random.Range(0, 6);
        for (int i = 0; i < FaceList.Count; i++)
        {
            if(i==num){
                FaceList[i].material = sprite;
                FaceList[i].enabled = true; 
            }
            else{
                FaceList[i].material = null;
                FaceList[i].enabled = false;
            }
        }
    }

    /// <summary>
    /// Cette fonction permet de récupérer le nombre de points jaune affichés sur le cube.
    /// </summary>
    public int GetDotCount() // Renvoi le nombre de points jaune
    {
        return dotList.Count;
    }

    /// <summary>
    /// Cette fonction permet de fixer la visibilité des points jaunes.
    /// </summary>
    public void SetDotVisible(List<bool> visibleList) // Set la visibilité des points jaunes
    {
        for (int i = 0; i < dotList.Count; i++)
        {
            dotList[i].SetActive(visibleList[i]);
        }
    }
}
