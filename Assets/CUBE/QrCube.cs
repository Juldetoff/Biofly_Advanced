using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QrCube : MonoBehaviour
{
    public List<GameObject> dotList = new List<GameObject>();
    public List<SpriteRenderer> FaceList = new List<SpriteRenderer>();
    //public SpriteRenderer qrSprite = null;

    public void SetQrSprite(Sprite sprite) // Set the sprite of the QR code
    {
        foreach (SpriteRenderer face in FaceList)
        {
            face.sprite = sprite;
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
