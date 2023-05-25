using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeManager : MonoBehaviour
{
    public List<Material> qrList = new List<Material>();
    public GameObject qrCubePrefab = null;

    public GameObject CreateCube(float x, float y, float z, GameObject prefab){
        GameObject qrCube = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
        if(prefab.GetComponent<QrCube>() != null){
            ChangeSprite(qrCube);
        }
        return qrCube;
    }

    public void ChangeSprite(GameObject qrCube){
        List<bool> visibleList = new List<bool>();
        qrCube.GetComponent<QrCube>().SetQrMaterial(qrList[Random.Range(0,qrList.Count)]); // Change the QR code
        for(int i = 0; i < qrCube.GetComponent<QrCube>().GetDotCount(); i++){
           if(Random.Range(0, 1) == 0){
               visibleList.Add(false);
           }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
