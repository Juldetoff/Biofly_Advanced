using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemplateManual : StartDrone
{
    [Tooltip("Liste des prefabs pouvant apparaître dans la scène.")]public GameObject[] prefabs;
    [Tooltip("Nombre d'objets à générer à chaque génération.")]public int GenerateCount = 2;
    [Tooltip("Nombre maximum d'essais de génération")]public int maxGenerationAttemps = 10; //en manuel on peut avoir moins d'essais car la génération ne se fait pas qu'une seule fois
    private GameObject sol = null;
    private MeshCollider meshSol = null;
    private int objectCnt = 0;
    private GameObject objectGenerated = null;
    private int tryCnt = 0;
    // Start est appelé avant la première frame
    void Start()
    {
        ExtractConfig(); //on extrait les configs du txt
        //on peut également setup ici ce qui doit être associé ou instancié avant le démarrage de la scène, comme associer le terrain aux caméras etc...
        //par exemple le sol peut être fixe ou à chercher avec un raycast
        sol = CheckGround(Cams.vcam.transform.position); //on récupère le sol sous la caméra (virtuelle, car c'est elle qui gère le mouvement)
        //sinon on peut fixer manuellement le sol s'il n'en existe qu'un seul (exemple pour la maison)

        if(jello){ //si on veut du jello
            Cams.cam.gameObject.GetComponent<HDRP_RollingShutter>().enabled = true;
            VCams.cam.gameObject.GetComponent<HDRP_RollingShutter>().enabled = true; 
        }
    }

    // Update est appelé à chaque frame
    void Update()
    {   //en gros on va générer des objets à chaque fois que la caméra aura parcouru une certaine distance
        //et après un certains temps (pour éviter de générer trop d'objets)
        if(Time.time - lastTime > maxTime && Vector3.Distance(lastPos,Cams.cam.gameObject.transform.position) > distance){
            lastPos = Cams.cam.gameObject.transform.position; 
            for (int i = 0; i < GenerateCount; i++)
            {
                CreateObject();
            }
            lastTime = Time.time; 
        }
        
        //on met à jour la position de la caméra bruitée sur la position de la caméra normale (l'une sur l'autre pour avoir le même parcours)
        noisecam.gameObject.transform.position = Cams.cam.gameObject.transform.position; 
        noisecam.gameObject.transform.rotation = Cams.cam.gameObject.transform.rotation;

        //on met à jour le sol pour le mouvement :
        RaycastHit[] hits;
        hits = Physics.RaycastAll(Cams.cam.gameObject.transform.position, Vector3.down, 1000.0F);
        foreach (RaycastHit hit in hits)
        {
            if(hit.collider.gameObject.GetComponent<MeshCollider>()){ //on récupère le sol sous la caméra à chaque frame
                sol = hit.collider.gameObject;
                meshSol = sol.GetComponent<MeshCollider>();
            }
            if(hit.collider.GetComponent<Terrain>() != null){ //si c'est un terrain on le récupère aussi (pour l'utiliser si besoin)
                terrain = hit.collider.GetComponent<Terrain>();
                Cams.vcam.gameObject.GetComponent<DroneCameraMovement>().SetTerrain(terrain);
            }
        }
    }

    /// <summary>
    /// Fonction template qui renvoie le gameobject du sol sous la position donnée.
    /// Cette version utilise un raycast vers le bas afin de chercher un objet qui pourra servir de sol (contenant un meshCollider) <br/>
    /// MODIFIABLE : la fonction peut être retirée si le sol est fixe ou si on le récpère autrement, ou si on ne veut pas de sol.
    /// On peut également utiliser une autre méthode qu'un raycast si besoin. On peut également adapter à tout type de colliders, 
    /// pas seulement des meshColliders, sans changer comment le collider sera utilisé.
    /// </summary>
    public GameObject CheckGround(Vector3 pos){ //on lance un raycast vers le bas pour trouver le box collider de la route
        RaycastHit[] hits;
        hits = Physics.RaycastAll(pos, Vector3.down, 1000.0F);
        foreach (RaycastHit hit in hits)
        {
            if(hit.collider.gameObject.GetComponent<MeshCollider>()){
                sol = hit.collider.gameObject;
                meshSol = sol.GetComponent<MeshCollider>();
                return hit.collider.gameObject;
            }
        }
        return null;
    }

    /// <summary>
    /// Fonction template qui permet de générer un objet autour de la dernière position de génération où était la caméra.
    /// Cette version utilise un compteur d'essais de génération, afin de pouvoir essayer de générer avec davantage de contraintes,
    /// Cependant cela peut être retiré s'il n'y a pas de contraintes de génération. <br/>
    /// MODIFIABLE :
    /// Par exemple dans la ville on génère des objets sur la route, ce qui nécessite un nombre d'essais limité pour éviter un overflow.
    /// </summary>
    public void CreateObject(){
        Vector3 randomPoint = Vector3.zero; //on initialise le point à zéro pour pouvoir tester si on a réussi à en trouver un
        tryCnt = 0; //on réinitialise le compteur d'essais (on peut le retirer si on ne veut pas de contraintes de génération)

        while(randomPoint == Vector3.zero && tryCnt < maxGenerationAttemps){ //ici on tente d'obtenir un point aléatoire respectant nos contraintes de générations
            randomPoint = RandomPos(lastPos); 
        }

        int prefabRand = UnityEngine.Random.Range(0, prefabs.Length); //on choisit un prefab aléatoire dans la liste
        //on peut également choisir aléatoirement ou non de générer un préfab, afin de pouvoir des fois ne rien générer.
        //on peut également choisir quel type de prefab générer avec des booléens, du genre SpawnCars, SpawnPeople, SpawnCubes...
        // if(!spawnCars && spawnPeople){
        //     //gérer comment faire uniquement spawn des personnes (ex: fixer un prefabRand parmi les indices des prefabs de personnes, ou autrement...)
        // }

        if(prefabRand!=prefabs.Length && randomPoint != Vector3.zero){ //cas liste non vide et point respectant les contraintes
            objectGenerated = cubeManager.CreateCube(randomPoint.x, randomPoint.y, randomPoint.z, prefabs[prefabRand]);
            //ensuite on peut gérer l'objet générer selon lequel il est (à partir du numéro de prefabRand on peut savoir quel objet de la liste de prefabs il est)
            if(prefabRand >= 0){
                objectGenerated.name = "personne" +objectCnt;
            }
            objectCnt++; //identifiant unique pour chaque objet généré
                  
            RaycastHit hit; //si le sol n'est pas exactement plat, on oriente l'objet en fonction de la pente
            var ray = new Ray(objectGenerated.transform.position, Vector3.down); // check pentes
            Debug.DrawRay(objectGenerated.transform.position, Vector3.down, Color.blue, 1000); //utile pour tracer et voir si les raycast fonctionnent comme souhaité
            if (meshSol.Raycast(ray, out hit, 2000))
            {
                objectGenerated.transform.rotation = Quaternion.FromToRotation(
                objectGenerated.transform.up, hit.normal) * objectGenerated.transform.rotation; // ajuste selon pentes
                objectGenerated.transform.position = hit.point+Vector3.up*1.15f; 
            }
            ray = new Ray(objectGenerated.transform.position, Vector3.up); // check vers le haut au cas où l'objet était sous le sol lorsqu'on a regardé en dessous de lui
            Debug.DrawRay(objectGenerated.transform.position, Vector3.up, Color.yellow, 1000);
            if (meshSol.Raycast(ray, out hit, 1000))
            {
                objectGenerated.transform.position = hit.point+Vector3.up*1.5f;
            }
        }
        else
        {
            objectGenerated = null;
        }
        if (objectGenerated)
        {
            objectGenerated.GetComponent<Render_dist>().SetCam(Cams.cam.gameObject); //on associe la caméra au script de render distance (supprime l'objet s'il est trop éloigné de la caméra)
            if(prefabRand >= 30){ //on peut remodifier un coup l'objet généré selon ce qu'il est si besoin, par exemple sa position ou rotation
                objectGenerated.transform.position = new Vector3(
                    objectGenerated.transform.position.x,
                    objectGenerated.transform.position.y + 1.15f, //on élève un peu l'objet pour qu'il ne soit pas à moitié dans le sol (modifiable)
                    objectGenerated.transform.position.z);
            }
            objectGenerated.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0); //rotation aléatoire pour que les objets ne soient pas tous tourné dans la même direction
            objectGenerated.GetComponent<SoloDetectableScript>().setCam(Cams.cam.gameObject.GetComponent<Camera>()); //on associe la caméra aux objets pour qu'ils soient détectables par la caméra
        }
    }

    /// <summary>
    /// Fonction template qui renvoie un point aléatoire respectant les contraintes, autour du point de départ donné en paramètre.
    /// Cette version utilise un compteur d'essais de génération, afin de s'assurer que les contraintes soient respéctées,
    /// et au bout de suffisamment d'essais on abandonne la génération de l'objet (étant donné qu'on en génère souvent on peut se permettre). <br/>
    /// MODIFIABLE : Selon les contraintes de génération, on peut modifier la fonction, mais on peut aussi ajouter un point par défaut si 
    /// le nombre max d'essais de génération est dépassé.
    /// </summary>
    public Vector3 RandomPos(Vector3 departPos){ //on cherche un point aléatoire sur la route autour du point de départ 
        if(tryCnt >= maxGenerationAttemps){ //on a essayé de trouver un point sur la route mais on a pas réussi
            Debug.Log("pas de point trouvé"); 
            //on reinitialise le tryCnt dans la fonction supèrieure (CreateObject) 
            //(attention aux boucles infinies, des fois elles causent un overflow qui stop direct, des fois non et il faut force close Unity)
            return Vector3.zero; //sinon on peut mettre un point par défaut ou autre, mais ici vecteur zero permet de tester si on a réussi ou non après
        }
        //on peut aussi utiliser divers moyen de prendre un point autour du point de départ, 
        //pour la forêt et la maison on utilise une sphère autour du pdd, auquel on récupère la hauteur du sol.
        //Pour la ville cependant, on prend un point au dessus du pdd, puis on prend un point sur un cercle autour de ce point avant de tracer
        //une droite à travers le pdd; le choix de cette méthode permettait d'augmenter les chances de croiser une route une fois à proximité,
        //car une simple sphère aura tendance à prendre un point trop loin.

        //Cette version template utilisera un cercle de rayon aléatoire <=radius suivi d'un raycast vers le bas pour trouver un point au sol
        Vector3 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
        Vector3 randomPoint = lastPos + randomDirection * Random.Range(1, radius);
        //on vérifie la présence d'un sol (on peut également modifier la fonction pour qu'elle renvoie un point au sol si elle le trouve)
        GameObject temp = CheckGround(randomPoint);
        if(temp){
            sol = temp;
            meshSol = sol.GetComponent<MeshCollider>();
            RaycastHit hit;
            var ray = new Ray(randomPoint, Vector3.down); //on regarde en bas
            Debug.DrawRay(randomPoint, Vector3.down, Color.blue, 1000);
            if (meshSol.Raycast(ray, out hit, 2000)) //si on trouve 
            {
                return hit.point;
            }
            else{ //aucun sol trouvé en bas
                ray = new Ray(randomPoint, Vector3.up); //on regarde en haut
                Debug.DrawRay(randomPoint, Vector3.up, Color.yellow, 1000);
                if (meshSol.Raycast(ray, out hit, 1000)) //si on trouve
                {
                    return hit.point;
                }
                else{ //aucun sol trouvé en haut, on recommence en incrémentant le compteur d'essais
                    tryCnt++;
                    return RandomPos(departPos);
                }
            }
        }
        else{ //aucun sol trouvé, on recommence en incrémentant le compteur d'essais
            tryCnt++;
            return RandomPos(departPos);
        }     //dupplicata évitable en renvoyant la position du point au sol avec le gameobject servant de sol à la place de checkGround.
    }
}
