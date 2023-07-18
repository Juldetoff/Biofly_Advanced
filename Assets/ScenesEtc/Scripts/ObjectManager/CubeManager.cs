using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cette classe gère l'instanciation de prefabs.
/// </summary>
public class CubeManager : MonoBehaviour
{
    [Tooltip("Liste des images de QR code pouvant s'appliquer lors de la génération de cubes rouge")]public List<Material> qrList = new List<Material>();
    
    /// <summary>
    /// Instancie le prefab en paramètre à la position (x, y, z). Si c'est un cube rouge, il applique un QR code aléatoire.
    /// </summary>
    public GameObject CreateCube(float x, float y, float z, GameObject prefab){
        GameObject qrCube = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
        if(prefab.GetComponent<QrCube>() != null){
            ChangeSprite(qrCube);
        }
        return qrCube;
    }

    /// <summary>
    /// Change le QR code d'un cube rouge de manière aléatoire.
    /// </summary>
    public void ChangeSprite(GameObject qrCube){
        List<bool> visibleList = new List<bool>();
        qrCube.GetComponent<QrCube>().SetQrMaterial(qrList[Random.Range(0,qrList.Count)]); // Change the QR code
        for(int i = 0; i < qrCube.GetComponent<QrCube>().GetDotCount(); i++){
           if(Random.Range(0, 1) == 0){
               visibleList.Add(false);
           }
        }
    }
}
