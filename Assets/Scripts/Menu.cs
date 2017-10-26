using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {
	public static Menu GameControl;

	public Timeline[] saves;
	int levelCount = 10;

	public int currentLevel = 0;
	// Use this for initialization

	void Awake(){
		if(GameControl == null){
			DontDestroyOnLoad (gameObject);
			GameControl = this;
		} else if (GameControl != this)
			Destroy(gameObject);

	}

	void Start () {
		saves = new Timeline[levelCount];
		for (int i = 0; i < levelCount; i++) {
			saves[i] = gameObject.AddComponent<Timeline>();
		}

	}
	
	// Update is called once per frame
	void Update () {
	}

	public void loadLevel (int level){
		Application.LoadLevel (level);
		currentLevel = level;
	}

	public void saveLevel(){
		saves[currentLevel].copy (GameObject.FindWithTag ("Player").GetComponent<TimeManager>().playerTimeline);
	}

	public void quitToMenu(){
		Application.LoadLevel (0);
		currentLevel = 0;
	}

	void OnGUI(){
		if (currentLevel != 0) {
			if (GUI.Button (new Rect (1, 1, 100, 30), "Return to Menu")) {
				saveLevel ();
				quitToMenu ();
			}
		} else {
			if(GUI.Button (new Rect(10, 10, 200, 40), "Level 1")){
				loadLevel (1);
			}
			for(int i = 2; i < levelCount; i++){
				if(saves[i-1].complete && !saves[i-1].paradox (saves[i-1].getLength ()-1)){
					if(GUI.Button (new Rect(10, -40+50*i, 200, 40), "Level " + i)){
						loadLevel (i);
					}
				} else
					break;
			}
		}
	}
}
