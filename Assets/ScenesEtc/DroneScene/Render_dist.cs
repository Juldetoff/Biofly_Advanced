using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Render_dist : MonoBehaviour
{
    public GameObject cam;
    public float distance = 100;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(this.transform.position, cam.transform.position) > distance)
        {
            Destroy(this.gameObject);
        }
    }

    public void SetCam(GameObject cam)
    {
        this.cam = cam;
    }

    public void SetDistance(float distance)
    {
        this.distance = distance;
    }
}
