using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QrCube : MonoBehaviour
{
    public List<GameObject> dotList = new List<GameObject>();
    public List<MeshRenderer> FaceList = new List<MeshRenderer>();
    //public SpriteRenderer qrSprite = null;

    public void SetQrMaterial(Material sprite) // Set the sprite of the QR code
    {
        int num = UnityEngine.Random.Range(0, 6);
        for (int i = 0; i < FaceList.Count; i++)
        {
            if(i==num){
                FaceList[i].material = sprite;
            }
            else{
                FaceList[i].material = null;
            }
        }
    }

    public int GetDotCount() // Get the number of dots
    {
        return dotList.Count;
    }

    public void SetDotVisible(List<bool> visibleList) // Set the visibility of the dots
    {
        for (int i = 0; i < dotList.Count; i++)
        {
            dotList[i].SetActive(visibleList[i]);
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
