using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using Cinemachine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Recorder.Examples;
using UnityEngine.SceneManagement;

/// <summary>
/// Template de classe pour une scène automatique. Est une extension de la classe 'StartScript'. 
/// Les parties modifiables seront indiquées par des commentaires.
/// </summary>
public class TemplateAuto : StartScript
{
    //variables générales
    private int tryCnt2 = 0; //compteur d'essais pour la génération de points
    private Vector3 pointTemp = new Vector3(0,0,0); //point temporaire pour la génération de points
    
    private GameObject objectGenerated = null;
    private GameObject sol = null; //si on utilise un sol, on peut le stocker et le récupérer ici
    private MeshCollider meshSol = null; //si on utilise un sol et qu'il possède un mesh on peut le stocker et le récupérer ici
    [Tooltip("Nombre d'essais de génération maximum")]public int maxGenerationAttempts = 100; //nombre d'essais max pour la génération de points
    [Tooltip("Liste des prefabs d'objets à générer")]public GameObject[] prefabs = null; //liste des prefabs d'objets à générer




    // Start est appelé avant la première frame
    void Start()
    {
        ExtractConfig(); //on récupère les configs de la scène
        PrepareTimeline(); //on (ré)initialise la timeline

        for (int i = 0; i < nBCamera; i++) //pour chaque caméra on va générer un chemin, des objets sur le chemin, une caméra normale et une caméra bruitée
        {
            CinemachineSmoothPath Cpath = GeneratePath(); //on génère un chemin

            SysCam sysCam = GenerateCam(i, Cpath); //on génère un système de caméra associé à notre chemin
            GenerateNormalTrack(i, sysCam); //on génère une piste dans la timeline pour notre caméra normale
            sysCam.cam.GetComponent<MovieRecorderExample>().startVideo = true; //on indique à la caméra qu'elle peut enregistrer
            sysCam.cam.GetComponent<Camera>().depth = 2; //on indique à la caméra qu'elle est prioritaire sur les depths<2

            SysCam sysCamBruit = GenerateNoiseCam(i, Cpath); //on génère un système de caméra bruitée associé à notre chemin
            GenerateNoiseTrack(i, sysCamBruit); //on génère une piste dans la timeline pour notre caméra bruitée
            sysCamBruit.cam.GetComponent<MovieRecorderExample>().startVideo = true; //on indique à la caméra qu'elle peut enregistrer
            sysCamBruit.cam.GetComponent<Camera>().depth = 1; //on indique à la caméra que la caméra normale lui est prioritaire en affichage (depth<2)
        }

        //on remet à zéro le déroulement de la timeline et on la relance
        director.Stop();
        director.time = 0;
        director.Play();
    }

    // Update est appelé à chaque frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)){ //si on appuie sur espace on relance la scène (utile pour les tests)
            Restart();
        }
        CheckFinished(); //on vérifie si une caméra a fini son parcours
        if(finished){
            foreach (MovieRecorderExample item in cams)
            {
                item.DisableVideo(); //on arrête l'enregistrement des caméras
            }
        }
        if(finished && repeat){
            Restart(); //si on a fini et qu'on veut répéter, on relance la scène
        }
        if(Input.GetKeyDown(KeyCode.R)){ //si on appuie sur R on active/désactive la répétition
            repeat = !repeat;
            Debug.Log("repeat: " + repeat); //afin de visualiser si on répète ou non lorsqu'on appuie sur R
        }
    }

    /// <summary>
    /// Cette fonction template permet de générer un chemin pour une caméra.
    /// </summary>
    private CinemachineSmoothPath GeneratePath()
    {
        point = GetRandomPoint(); //on génère un point aléatoire dans la zone de travail
        point = GeneratePoint(point, radius); //à partir de ce point on cherche le premier point respectant nos restrictions (s'il y en a)
        float y = GetHeight(point); //on récupère la hauteur au niveau du sol aux coordonnées x,z
        point.y = y + offsetcam; //on ajoute un offset choisi manuellement afin de s'assurer que le chemin soit placé comme souhaité.

        CinemachineSmoothPath Cpath = Instantiate(Prefabpath, new Vector3(0, 0, 0), Quaternion.identity); //on instancie un chemin
        Cpath.m_Waypoints = new CinemachineSmoothPath.Waypoint[pointCnt]; //on initialise le nombre de points du chemin
        Cpath.m_Waypoints[0].position = point; //on place le premier point du chemin qui est celui qu'on a généré au dessus
        for (int i = 1; i < pointCnt; i++) //on boucle pour générer les points restants
        {
            pointTemp = GeneratePoint(point, radius); //on génère un point autour du point précédent (en respectant nos potentiels restrictions)
            y = GetHeight(pointTemp); //ici on récupère la hauteur du point (puisque la fonction précédente s'occupe d'assurer les restrictions)

            pointTemp.y = y + offsetcam; //on ajoute un offset choisi manuellement afin de s'assurer que le chemin soit placé comme souhaité.
            point = pointTemp; //le nouveau point précédent est le point qu'on vient de générer
            Cpath.m_Waypoints[i].position = point; //on place le point dans le chemin

            for (int j = 0; j < numObject; j++) //on boucle pour générer des objets sur le chemin autour du point généré
            {
                GenerateObjectOnPath(out Vector3 objPoint, out int prefabRand);
            }
        }
        return Cpath;
    }

    /// <summary>
    /// Cette fonction template permet de générer un objet sur le chemin. <br/>
    /// MODIFIABLE : on peut appliquer davantage de restriction sur la génération de l'objet. <br/>
    /// Par exemple pour la ville on a voulu forcer la génération des objets au dessus des routes.
    /// </summary>
    private void GenerateObjectOnPath(out Vector3 objPoint, out int prefabRand)
    {
        objPoint = GeneratePoint(point, radius); //on génère un point autour du point du chemin (en respectant nos potentiels restrictions)
        //modifiable selon le cas, par exemple si la génération d'objets doit respecter des conditions différentes de la génération de chemin.
        objPoint.y = GetHeight(objPoint) + offsetcam; //GeneratePoint assure nos resctrictions, il suffit donc de récupérer la hauteur et d'ajouter notre offset.
        prefabRand = UnityEngine.Random.Range(0, prefabs.Length); //on choisit un prefab aléatoire dans la liste de prefabs
        //modifiable par exemple si on veut ajouter le cas où on peut ne pas générer d'objet, dépend de la probabilité souhaitée.
        //Pour la ville, on a simplement ajouter un cas de non génération, ce qui faisait du 1/36 chance de ne pas générer un objet, ce
        //qui est anecdotique, mais montre que c'est faisable selon les besoins.
        if(prefabRand != prefabs.Length){
            objectGenerated = cubeManager.CreateCube(objPoint.x, objPoint.y, objPoint.z, prefabs[prefabRand]);
    
            //on peut gérer maintenant selon le prefab qui est instancié
            //par exemple pour la ville les prefabs 0 à 29 étaient les voitures, dans ce cas il suffisait de vérifier prefabRand
            //et de modifier l'objet en conséquence.
            if (prefabRand >= 0) //valeur de présentation
            {
                objectGenerated.name = "template_object" + count; //on nomme l'objet en plus du count afin de le distinguer des autres (id unique)
                //d'autres modifications liés à l'objet lors de son instanciation peuvent être faites ici
                //par exemple pour les voitures de la ville, on a récupéré celles en manuel, et on a modifié leurs scripts afin 
                //de les adapter au automatique.
            }
            objectGenerated.tag = "obstacle"; //on tag l'objet afin de savoir qu'il s'agit d'un obstacle lorsqu'on le croisera avec un raycast
            count++; //id unique, du coup à chaque génération on incrémente le compteur
            
            //Ensuite, si on utilise comme sol un objet qui contient un mesh (plan, terrain, navmesh, etc...), on peut utiliser ce mesh
            //afin de placer l'objet sur le sol, et de l'orienter en fonction de la pente du sol.
            if (meshSol)
            {
                RaycastHit hit; //le sol n'est pas exactement plat on oriente l'objet en fonction de la pente
                var ray = new Ray(objectGenerated.transform.position, Vector3.down); // vérifie les pentes
                Debug.DrawRay(objectGenerated.transform.position, Vector3.down, Color.blue, 1000);
                if (meshSol.Raycast(ray, out hit, 2000)) //on raycast vers le bas afin de détecter le croisement avec le sol
                {
                    objectGenerated.transform.rotation = Quaternion.FromToRotation(
                    objectGenerated.transform.up, hit.normal) * objectGenerated.transform.rotation; // on ajuste avec la normale au sol
                    objectGenerated.transform.position = hit.point + Vector3.up * 1.15f; // on positionne sur le sol
                }
                ray = new Ray(objectGenerated.transform.position, Vector3.up); // check for ground
                //Debug.DrawRay(objectGenerated.transform.position, Vector3.up, Color.yellow, 1000);
                //pratique pour tracer dans la scène lors du runtime les trajectoires des raycasts. Elles sont invisibles dans le play mode, mais
                //dans la scène cela permet de visualiser les tracés.
            }
        }
        else
        {
            objectGenerated = null;
        }
        if (objectGenerated)
        {
            if (prefabRand >= 0)
            { //on peut faire quelques retouches sur l'objet généré si besoin, par exemple le repositionner après l'avoir orienté
                objectGenerated.transform.position = new Vector3(
                    objectGenerated.transform.position.x,
                    objectGenerated.transform.position.y + 1.15f,
                    objectGenerated.transform.position.z);
            }
            objectGenerated.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            //pour orienter l'objet aléatoirement, et que tous ne soit pas orienté dans le même sens (modifiable)
        }
    }

    /// <summary>
    /// Cette fonction template permet à partir d'un point de générer un autre point à proximité. <br/>
    /// MODIFIABLE : on peut appliquer davantage de restriction sur la génération du point. <br/>
    /// Par exemple pour la ville on a voulu forcer la génération des points au dessus des routes, 
    /// tandis que pour la forêt il n'y avait pas besoin de restriction, il n'y a donc pas eu besoin "d'essayer de générer" un point.
    /// </summary>
    private Vector3 GeneratePoint(Vector3 point, float radius) 
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere.normalized * radius; //on génère un point aléatoire dans un rayon donné sur la sphère de rayon radius
        Vector3 randomPoint = point + randomDirection * radius; //et de centre point.
        if(CheckGround(randomPoint)){ //si le point respecte nos restriction, on le retourne et on remet le compteur d'essais à 0.
            tryCnt2 = 0;
            return randomPoint;
        }
        else{ //sinon dans le cas où on a des restrictions qui ne sont pas respéctés, on reessaye de générer un point.
            tryCnt2++;
            if(tryCnt2 < maxGenerationAttempts){
                return GeneratePoint(point, radius); //risque d'overflow si non limité en nombre d'essais.
            }
            else{ //si on ne trouve toujours pas de points, on retourne un point par défaut
                //ce point peut être le point non modifié, ou un point fixé en avance, ou le vecteur nul, etc...
                tryCnt2 = 0; //on oublie pas de remettre le compteur à 0
                //return Vector3.zero;
                return randomPoint; //modifiable selon le cas souhaité
            }
        }
    }

    /// <summary>
    /// Cette fonction template permet de générer un point aléatoire dans la scène. <br/>
    /// MODIFIABLE : on peut utiliser des marqueurs ou directement des coordonnées afin de connaître la zone 
    /// dans laquelle on veut générer des points aléatoires. <br/>
    /// Par exemple pour la ville on a utilisé deux marqueurs délimitant la zone en x et z, tandis que pour la maison 
    /// on a utilisé les coordonnées des coins de la maison afin de délimiter le quadrilatère délimitant la zone.
    /// </summary>
    private Vector3 GetRandomPoint()
    {   //exemple à modifier
        return new Vector3 ( 
            UnityEngine.Random.Range(-10, 10), // valeurs de présentation
            0, // dépend du cas et de comment récupérer cette coordonnée.
            // Pour un objet de la classe Terrain, SampleHeight(x,y,z) renverra le y correspondant au point (x,z) à la surface du terrain.
            // Pour un objet NavMesh ou un plan, mieux vaut mettre le point largement au dessus du sol, puis raycast vers le bas afin d'obtenir le sol.
            // (un raycast vers le haut peut marcher aussi sans avoir besoin de mettre le point largement au dessus, mais ça dépend du cas)
            UnityEngine.Random.Range(-10, 10) // valeurs de présentation
        );
    }

    /// <summary>
    /// Cette fonction template permet de récupérer la hauteur de la zone de travail. <br/>
    /// MODIFIABLE : on peut utiliser un raycast ou un terrain afin de récupérer la hauteur, et ajouter un offset si besoin pour assurer
    /// que les objets ne soient pas sous le sol. <br/>
    /// Par exemple pour la forêt on a utilisé le terrain, en faisant SampleHeight(x,y,z), tandis que pour la ville
    /// on a utilisé un raycast vers le bas à partir d'un point au dessus de la ville. <br/>
    /// ATTENTION : dans tout les cas il faut s'assurer de la présence du sol cherché, car si on raycast vers le bas sans rien trouver, 
    /// on risque une erreur NullReferenceException. Dans un cas possible comme celui ci il vaut mieux utiliser une fonction CheckGround() avec.
    /// </summary>
    private float GetHeight(Vector3 point)
    {
        return 0; //valeur de présentation
    }

    /// <summary>
    /// Cette fonction template permet de vérifier si le point est bien au dessus d'un sol. <br/>
    /// MODIFIABLE : on peut utiliser un raycast pour vérifier si on trouve bien un objet ou non,
    /// et ainsi vérifier si le point est bien au dessus d'un sol. <br/>
    /// Par exemple pour la ville on a lancé un raycast vers le bas en vérifiant si parmi les objets croisés, on trouvait un objet taggé "route". <br/>
    /// ATTENTION : avec l'utilisation de cette fonction, il faut prévoir quoi faire dans le cas où on ne trouve pas de sol. On peut 
    /// rechercher à nouveau un point, cependant cela risque de causer un OverflowException si on ne trouve pas de sol pour beaucoup d'essais.
    /// Il faudra donc ajouter un compteur d'essais et une limite d'essais. Dans le cas où le point recherché est obligatoire, on peut utiliser
    /// une valeur par défaut qui fonctionne, ou alors relancer la scène (SceneManager.LoadScene(SceneManager.GetActiveScene().name)).
    /// </summary>
    private bool CheckGround(Vector3 point)
    {
        return true; //valeur de présentation
        //on peut également vérifier si un sol différent existe à cet endroit, et le stocker dans sol et meshSol
        //dans city par exemple, la route de la ville est séparée en plusieurs morceaux, on peut donc
        //avoir un cas où le point passe au dessus d'une route différente, donc il faut changer le sol et le meshSol
    }
}
