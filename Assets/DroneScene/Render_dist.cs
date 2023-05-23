using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Render_dist : MonoBehaviour
{
    public GameObject cam;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(this.transform.position, cam.transform.position) > 100)
        {
            Destroy(this.gameObject);
        }
    }

    public void SetCam(GameObject cam)
    {
        this.cam = cam;
    }
}
