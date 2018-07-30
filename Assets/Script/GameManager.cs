using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour 
{
    public int curlsOfRound = 3;

    int playerScore = 0;
    int turnCount = 1;

	bool throwing = false;		//If the stone is flying
	bool turnStart = false;		//If the player is ready to throwing

	bool gameFinished = false;
	
	Vector3 originalCameraPos;
	Vector3 tempCameraPos;
	
	GameObject currentStone;	//The moving one
	GameObject stoneObject;
	
	List<GameObject>	finishedStones;

    private bool pressed;
    private bool released;

    private float playerMass = 70;
    private float playerAcceleration = 10;
    public float maxPlayerMass = 100.0f;
    public float maxPlayerAcceleration = 30.0f;
	
	//----------------------------------
	//Score zone define
	public GameObject scoreCenter;
	public float scoreZone1 = 0.3f;
	public int score1 = 40;
	public float scoreZone2 = 1.22f;
	public int score2 = 30;
	public float scoreZone3 = 2.44f;
	public int score3 = 20;
	public float scoreZone4 = 3.66f;
	public int score4 = 10;

	int width = Screen.width;
	int height = Screen.height;

    private bool fault;

	void Start () 
    {
		originalCameraPos = Camera.main.transform.position;
		tempCameraPos = originalCameraPos;

        stoneObject = (GameObject)Resources.Load("Pref_Stone",typeof(GameObject));
		
        finishedStones = new List<GameObject>();
	}
	
	void OnGUI() 
    {
        GUI.enabled = true;

    	string text;

        if ( fault  )
        {
            GUIStyle myStyle  = new GUIStyle(GUI.skin.button);
            myStyle.fontSize = 20;
            myStyle.normal.textColor = Color.red;

            if ( GUI.Button(new Rect(width/2 - 250,height/2,500,50),"Release the rock BEFORE the Hog Line!", myStyle) )
            {
                fault = false;
                FinishThrow();
            }

            return;
        }

        if(gameFinished)
        {

            GUIStyle myStyle  = new GUIStyle(GUI.skin.button);
            myStyle.fontSize = 20;

        	GUILayout.BeginArea(new Rect(width/2 - 100, height/2-50, 200, 100));
        	
            text = "Player Score: " + playerScore;
        	GUILayout.Box(text, myStyle);

        	if(GUILayout.Button("Start a New Game", myStyle)) newGame();
        	
            GUILayout.EndArea();
        	
            return;
        }
		
        GUILayout.BeginArea(new Rect(10, 10, 200, 150));

    	text = "Turn : " + turnCount + " of "+ curlsOfRound ;
    	GUILayout.Box(text);

    	text = "Player Score : " + playerScore;
    	GUILayout.Box(text);

        text = (pressed ? "PUSHING" : "RELEASED");
        GUILayout.Box(text);

    	GUILayout.EndArea();	

        if (throwing) GUI.enabled = false;
        GUILayout.BeginArea(new Rect(width-210, 10, 200, 150));

        GUILayout.Label("Player Mass: " + playerMass);
        playerMass = GUILayout.HorizontalSlider(playerMass, 0.0F, maxPlayerMass);

        GUILayout.Label("Player Acceleration: " + playerAcceleration);
        playerAcceleration = GUILayout.HorizontalSlider(playerAcceleration, 0.0F, maxPlayerAcceleration);

        GUILayout.EndArea();    
    }

	void FinishTurn()
	{
		if(turnCount>=curlsOfRound) FinishGame();
		turnCount++;
	}

	void StartThrow()
	{
		turnStart = true;

        Camera.main.transform.position = originalCameraPos;

		currentStone = (GameObject)Object.Instantiate(stoneObject);
		currentStone.GetComponent<Renderer>().materials[1].SetColor("_Color",Color.red);
	}
	
	void FinishThrow()
	{
		if(currentStone!=null) finishedStones.Add(currentStone);

		CalculateScore();
		
        throwing = false;
		turnStart = false;
        released = false;
        pressed = false;
        fault = false;
		
        FinishTurn();
	}

	void FinishGame()
	{
		gameFinished = true;
	}

	void newGame()
	{
        foreach(GameObject stone in finishedStones)
        {
            Object.Destroy(stone);
        }
        finishedStones.Clear();

		gameFinished = false;
		turnCount = 1;
		throwing = false;		//If the stone is flying
		turnStart = false;		//If the player is ready to throwing
		playerScore = 0;

        pressed = false;
        released = false;
        fault = false;
	}
	
	void CalculateScore()
	{
        int score = 0;

		foreach(GameObject obj in finishedStones)
		{
			float distance = Vector3.Distance(obj.transform.position, scoreCenter.transform.position);

            if ( distance < scoreZone1 )
            {
                score += score1;
            }
            else if ( distance < scoreZone2 )
            {
                score += score2;
            }
            else if ( distance < scoreZone3 )
            {
                score += score3;
            }
            else if ( distance < scoreZone4 )
            {
                score += score4;
            }
		}
        playerScore = score;
	}
	
	void Update () 
    {
		if(gameFinished) return;

        if (fault) return;
       
        pressed = false;
        if ( (Input.GetKey("up") || Input.GetKey(KeyCode.W) ) && !released )
        {
            pressed = true;
            float force = playerMass * playerAcceleration;
            currentStone.GetComponent<ThrowCurling>().mForce = force;
            currentStone.GetComponent<ThrowCurling>().Throw();
            if (!throwing) throwing = true;
        }
        if (Input.GetKeyUp("up") || Input.GetKeyUp(KeyCode.W) )
        {
            pressed = false;
            released = true;
        }
    
		if(!throwing)
		{
			if(!turnStart)
			{
                StartThrow();
				return;
			}

            if (Input.GetKey("left") || Input.GetKey(KeyCode.A))
			{
				Vector3 pos = currentStone.transform.position;
				if(currentStone.transform.position.z<3.5)
					currentStone.transform.position = new Vector3(pos.x,pos.y,pos.z+Time.deltaTime*2.0f);
			}
			if (Input.GetKey("right") || Input.GetKey(KeyCode.D))
			{
				Vector3 pos = currentStone.transform.position;
				if(currentStone.transform.position.z>-3.5)
					currentStone.transform.position = new Vector3(pos.x,pos.y,pos.z-Time.deltaTime*2.0f);
			}
		}
		else // Curling is moving!
		{
            if (currentStone.GetComponent<ThrowCurling>().GetCrossedLine() && pressed)
            {
                fault = true;
                Object.Destroy(currentStone);
            }
            
			//Check other stones
			for(int i=0;i<finishedStones.Count;i++)
			{
				if(finishedStones[i].transform.position.z>4.13 || finishedStones[i].transform.position.z<-4.13 || finishedStones[i].transform.position.x>43)
				{
					Object.Destroy(finishedStones[i]);
					finishedStones.RemoveAt(i);
				}
			}

			//Update the camera
			if(currentStone!=null && currentStone.GetComponent<Rigidbody>().velocity.magnitude>0.1 && currentStone.transform.position.z<4.13 && currentStone.transform.position.z>-4.13 && currentStone.transform.position.x<43) 
			{
				if(currentStone.transform.position.x - originalCameraPos.x > 7)
				{
					tempCameraPos.x = currentStone.transform.position.x - 7;
					Camera.main.transform.position = tempCameraPos;
				}
			}
			else
			{
				//To check if the throwing finished
				if(currentStone!=null && (currentStone.transform.position.z>4.13 || currentStone.transform.position.z<-4.13 || currentStone.transform.position.x>43) )
				{
					Object.Destroy(currentStone);
					currentStone = null;
				}
               				for(int i=0; i<finishedStones.Count;i++)
				{
					if(finishedStones[i].GetComponent<Rigidbody>().velocity.magnitude>0.1)
						return;
				}
				
                if ( !pressed )
                {
                    // Before the hog line
                    if(currentStone!=null && (currentStone.transform.position.x<20) )
                    {
                        Object.Destroy(currentStone);
                        currentStone = null;
                    }

                    FinishThrow();
                }
			}
		}
	}

}
