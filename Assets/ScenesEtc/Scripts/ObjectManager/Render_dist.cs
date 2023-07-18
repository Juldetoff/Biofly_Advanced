using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cette classe gère la destruction d'un objet si il est trop loin de la caméra.
/// </summary>
public class Render_dist : MonoBehaviour
{
    [Tooltip("Caméra associée.")]public GameObject cam;
    [Tooltip("Distance max pouvant séparer l'objet à 'cam' avant sa suppression.")]public float distance = 100;

    // Update est appelé une fois par frame
    void Update()
    {
        if(Vector3.Distance(this.transform.position, cam.transform.position) > distance)
        {
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// Change la caméra associée.
    /// </summary>
    public void SetCam(GameObject cam)
    {
        this.cam = cam;
    }

    /// <summary>
    /// Change la distance max pouvant séparer l'objet à 'cam'.
    /// </summary>
    public void SetDistance(float distance)
    {
        this.distance = distance;
    }
}
