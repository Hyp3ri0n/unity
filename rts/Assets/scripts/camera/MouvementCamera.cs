using UnityEngine;
using System.Collections;

/*Script de déplacement d'un objet de type "camera" sur Unity3D V4.5 :
 *-Permet d'avancer la camera avec le clavier (flèches) et la souris (bords de l'écran)
 *-Permet de Zoomer sur la map avec la molette 
 *-Permet de s'ajuster automatiquement (hauteur) par rapport au terrain
 *-Permet le rotation de la camera dans les deux axes (x, y)
 *-Permet de s'ajuster aux différentes limites des map (x, z)
 *_/!\_Attacher ce script à un "GameObject" (objet vide) qui contient la "MainCamera" au même endroit (transform, rotate)
 *_/!\_Il faut créer un gameobject de type "Terrain" dans votre scène et le "drag and drop" depuis la Hierarchy dans la variable "worldTerrain"
 */

//TODO :
    //DONE 1 : verification pour la gestion zoom (problem avec update "impacte.distance" !?)
    //DONE 2 : verification des barrières de la map (avec les flèches + clavier)
    //TODO 3 : verification des barrières de la map (avec la molette)
    //TODO 4 : System de follow de joueur selectionné (avec "F")
    // ->      + System de touche en general sur un nouveau script (avec "ESC")

public class MouvementCamera : MonoBehaviour
{

    #region Attributs

    //********************Attributs Globaux Privés********************
	private float vitesseMouvement = 60f;
	private float bordureSouris = 25f;
	public GameObject MainCamera;
    private GameObject AngleScroll;

    //****************attributs Limites Map************
    private float limiteXMin;
    private float limiteZMin;
    private float limiteXMax;
    private float limiteZMax;
    public Terrain worldTerrain;
    private float paddingTerrain = 20f;
    //*************************************************

	//****************attributs rotation****************
	private float sourisX;
	private float sourisY;
	public float verticalRotationMin = 0f;	//en degrés
	public float verticalRotationMax = 65f; //en degrés
	//*************************************************

	//***********attributs ajustement hauteur***********
	private float hauteurCamera;
	private float axeYCamera;
    public float hauteurMaxCamera = 300f;
    public float hauteurMinCamera = 15f;
    public float hauteurMaxAjustementAuto = 40f;
    private Ray rayon; //Création d'un rayon qui pointe vers le bas
    private RaycastHit impact;
    //*************************************************

    //***********Activation / Désactivation***********
    public bool activationDeplacementCamBordure = true;
    public bool activationLimiteMap = true;
    //*************************************************
    //*************************************************************

    #endregion

    #region Méthodes Principales

    // Use this for initialization
    void Start() {

        //Initialisation Caméra
        //MainCamera = this.gameObject.transform.Find("MainCamera").gameObject;

        //Placement de l'objet camera et de son parent
        this.transform.Rotate(new Vector3(0, 0, 0));
        MainCamera.transform.Translate(new Vector3(0, 0, 0));
        MainCamera.transform.Rotate(new Vector3(10, 0, 0));
        //********************************************

        //Pour que la variable est une valeur au début
        hauteurCamera = this.transform.position.y;

        //Initialisation LimiteTerrain
        limiteXMin = worldTerrain.transform.position.x + paddingTerrain;
        limiteZMin = worldTerrain.transform.position.z + paddingTerrain;
        limiteXMax = worldTerrain.terrainData.size.x - paddingTerrain;
        limiteZMax = worldTerrain.terrainData.size.z - paddingTerrain;
        //********************************************

        //Initialisation de l'angle pour le scroll
		AngleScroll = new GameObject();

	}


	// Update is called once per frame
	void LateUpdate () {

		//Gestion de la rotation de la camera
        if (Input.GetMouseButton(2)) // molette	
		    GestionRotation();

		//Gestion du zoom
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
		    GestionZoom();

        if (UtilisateurDeplaceCamera() || EstSurLesBorduresEcran())
		{
			Vector3 nouvellePosition = MouvementVoulu();

            //Vérification que la camera n'est pas en dehors de la map
            if (!(CameraEnDehorsLimites(nouvellePosition)))
            {
			    Vector3 positionAjustement = this.transform.position + nouvellePosition;	//Pour l'ajustement hauteur avec le terrain

			    UpdateAxeYCamera(positionAjustement);

			    //Déplacement de la camera
			    this.transform.Translate(nouvellePosition, Space.Self);
            }
		}

		GestionAjustementHauteurCamera();

		sourisX = Input.mousePosition.x;
        sourisY = Input.mousePosition.y;
	
	}

    #endregion

    #region Méthodes Secondaires

    /// <summary>
	/// Permet de savoir si l'utilisateur deplace la camera avec les flèches.
	/// </summary>
	/// <returns>Retourne true si touche presser sinon false</returns>
	public bool UtilisateurDeplaceCamera()
	{
		//Utilisateur déplace la camera avec le clavier
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
            Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            return true; else return false;
	}


	/// <summary>
	/// Permet de savoir si le pointeur est sur les limites de l'écran
	/// </summary>
	/// <returns>Retourne true si le pointeur est sur la bordure sinon false</returns>
	public bool EstSurLesBorduresEcran()
	{
        if (activationDeplacementCamBordure)
        {
            if ((Input.mousePosition.x > -5) && (Input.mousePosition.x < (Screen.width + 5)) && (Input.mousePosition.y > -5) && (Input.mousePosition.y < (Screen.height + 5)))
                return true;
            else return false;
        }
        else
            return false;
	}


	/// <summary>
	/// Permet de déplacer la camera
	/// </summary>
	/// <returns>Retourne un objet de type Vector3 avec les nouvelles coordonnée de la camera</returns>
	public Vector3 MouvementVoulu()
	{
		Vector3 translationVoulu = new Vector3();

		//Gestion des flèches et des touches du clavier
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.W))
			translationVoulu += Vector3.forward * (vitesseMouvement * Time.deltaTime);
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
			translationVoulu += Vector3.back * (vitesseMouvement * Time.deltaTime);
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A))
			translationVoulu += Vector3.left * (vitesseMouvement * Time.deltaTime);
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
			translationVoulu += Vector3.right * (vitesseMouvement * Time.deltaTime);

		if (!(Input.GetMouseButton(2)))
		{
			//Gestion de la souris
			if (Input.mousePosition.x < bordureSouris)
				translationVoulu += Vector3.left * (vitesseMouvement * Time.deltaTime);
			if (Input.mousePosition.x > (Screen.width - bordureSouris))
				translationVoulu += Vector3.right * (vitesseMouvement * Time.deltaTime);
			if (Input.mousePosition.y < bordureSouris)
				translationVoulu += Vector3.back * (vitesseMouvement * Time.deltaTime);
			if (Input.mousePosition.y > (Screen.height - bordureSouris))
				translationVoulu += Vector3.forward * (vitesseMouvement * Time.deltaTime);
		}

		return translationVoulu;
	}


	/// <summary>
	/// Permet de gérer la rotation de la camera avec la sourris (molette).
	/// </summary>
	public void GestionRotation()
	{
		
		float facteurRotation = 10f;

		// Mouvement de la camera (rotation) avec la molette appuyée.	
		if (Input.GetMouseButton(2)) // molette	
		{	
			//Horizontale
			if(Input.mousePosition.x != sourisX)
			{
				var cameraRotationY = (Input.mousePosition.x - sourisX) * facteurRotation * Time.deltaTime;
				this.transform.Rotate(0, cameraRotationY, 0, Space.World);
			}

			//vertical
			if(Input.mousePosition.y != sourisY)
			{
				var cameraRotationX = (sourisY - Input.mousePosition.y) * facteurRotation * Time.deltaTime;
				var rotationVoulu = MainCamera.transform.eulerAngles.x + cameraRotationX;

				if (rotationVoulu >= verticalRotationMin && rotationVoulu <= verticalRotationMax)
					MainCamera.transform.Rotate(cameraRotationX, 0, 0);
			}
		}
	}


	/// <summary>
	/// Permet de calculer la hauteur à modifier par rapport au terrain.
	/// </summary>
	/// <param name="ajustementPosition">Saisir un objet de type Vector3 de la position à ajuster</param>
	public void UpdateAxeYCamera(Vector3 ajustementPosition)
	{
		float limiteAjustement = 0.1f;

        if (Physics.Raycast(ajustementPosition, Vector3.down, out impact, Mathf.Infinity))
		{
            float nouvelleHauteur = hauteurCamera + impact.point.y;
			float hauteurDifference = nouvelleHauteur - axeYCamera;

			if (hauteurDifference > -limiteAjustement && hauteurDifference < limiteAjustement)
				return;	//Meme utilité qu'un "break;"

			if (nouvelleHauteur > hauteurMaxCamera)
				return;

			axeYCamera = nouvelleHauteur;
		}

		return;
	}


	/// <summary>
	/// Permet de gérer l'ajustement à appliquer à la hauteur de la camera.
	/// </summary>
	public void GestionAjustementHauteurCamera()
	{
		//Soit l'axe n'a pas d'ajustement à faire soit la camera vient d'etre loadé et donc = 0
		if (axeYCamera == transform.position.y || axeYCamera == 0)
			return;

        //Si l'impacte du rayon est supérieur à 80 (y) alors ne rien faire
        if (impact.distance > hauteurMaxAjustementAuto)
            return;

		float tempMouvement = 0.01f;
		float yVelocity = 0.0f;		//Mettre à zéro car il sera déterminer à chaque frame dans une fonction (smoothDamp)

		float nouvellePositionY = Mathf.SmoothDamp(this.transform.position.y, axeYCamera, ref yVelocity, tempMouvement);

        //Application des modifications de l'axe Y (hauteur) sur la camera si besoin
		if (nouvellePositionY < hauteurMaxCamera)
            this.transform.position = new Vector3(this.transform.position.x, nouvellePositionY, this.transform.position.z);

		return;
	}


	/// <summary>
	/// Permet de gérer le zoom avec la molette.
	/// </summary>
	public void GestionZoom()
	{
		float facteurVitesse = 20f;
		float valeurScrollMolette = Input.GetAxis("Mouse ScrollWheel") * -facteurVitesse;

		float angleXCamera = MainCamera.transform.eulerAngles.x;

		//Configuration du GameObject AngleScroll
		AngleScroll.transform.position = this.transform.position;
		AngleScroll.transform.eulerAngles = new Vector3(angleXCamera, this.transform.eulerAngles.y, this.transform.eulerAngles.z);
		AngleScroll.transform.Translate(Vector3.back * valeurScrollMolette);

		Vector3 scrollVoulu = AngleScroll.transform.position;

        

        //Vérification : Si l'impacte est dans la hauteur min ou max ne rien faire
        if ((valeurScrollMolette < 0) && (impact.distance < hauteurMinCamera))
            return;
        if ((valeurScrollMolette > 0) && (impact.distance > hauteurMaxCamera))
            return;

		//Modification pour l'ajustement automatique de la hauteur
		float differenceHauteur = scrollVoulu.y - this.transform.position.y;
		hauteurCamera += differenceHauteur;
		axeYCamera = scrollVoulu.y;

		//Mouvement de la camera
		this.transform.position = scrollVoulu;
    }


    /// <summary>
    /// Permet de savoir si l'utilisateur deplace la camera en dehors des limites.
    /// </summary>
    /// <returns>Retourne true si dépace sinon false</returns>
    public bool CameraEnDehorsLimites(Vector3 nouvellePosition)
    {

        Vector3 nouvellePositionLocal = this.transform.TransformPoint(nouvellePosition);

        if (activationLimiteMap)
        {
            bool verification = false;

            //Utilisateur déplace la camera en dehors des limites X
            if (nouvellePositionLocal.x > limiteXMax)
                verification = true;
            if (nouvellePositionLocal.x < limiteXMin)
                verification = true;

            //Utilisateur déplace la camera en dehors des limites Z
            if (nouvellePositionLocal.z > limiteZMax)
                verification = true;
            if (nouvellePositionLocal.z < limiteZMin)
                verification = true;

            return verification;
        }
        else
            return false;

    }

    #endregion

}