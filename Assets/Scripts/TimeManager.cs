using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.ImageEffects;
public class TimeManager : MonoBehaviour {
	
	public float scrubSmoothness;

	public EdgeDetection Edges;
	public VignetteAndChromaticAberration paradoxEffect;
	public NoiseAndScratches paradoxEffect2;
	public AudioDistortionFilter distortion;
	//public GlitchEffect paradoxEffect3;

	public Light sound;

	public Text timeCounter;
	public Text timeCounter2;
	public Text[] milestones;
	public Slider timeSlider;
	public Slider sliderMax;
	FirstPersonController controller;

	public GameObject endMarkerPrefab;

	private GameObject endMarker;

	private Guard spotGuard;

	private Guard[] enemies;
	private GameObject[] sensors;

	public float hz = 15f;
	public float MAX_DURATION = 30f;
	enum State{recording, scrubbing, optimizing, playing};
    public Camera m_Camera;
	State playerState = State.recording;

	public Timeline playerTimeline;
	//Quaternion initRotation;

	private int optimizeStart;
	private int optimizeEnd;
	public Timeline templine;

	private int blurIndex;
	bool p = false; 
	bool freeLook = false;

    private Door[] doors;

	private float prevTime = 0;
	private float deltaTime = 0;

    // Use this for initialization
    void Start () {
        distortion.distortionLevel = 0f;
		GetComponentInChildren<Camera> ().enabled = false;
		blurIndex = 1;
		paradoxEffect.enabled = false;
		paradoxEffect2.enabled = false;
		//paradoxEffect3.enabled = false;
		GameObject[] temp = GameObject.FindGameObjectsWithTag ("Enemy");
		enemies = new Guard[temp.Length];
		for(int i = 0; i < temp.Length; i++) {
			enemies[i] = temp[i].GetComponent<Guard>();
		}
		sensors = GameObject.FindGameObjectsWithTag ("Obstacle");
        temp = GameObject.FindGameObjectsWithTag("Door");
        doors = new Door[temp.Length];
        for (int i = 0; i < temp.Length; i++)
        {
            doors[i] = temp[i].GetComponent<Door>();
        }
        //initRotation = transform.rotation;
        controller = GetComponent<FirstPersonController> ();
		sound = GetComponent<Light> ();

		if (Menu.GameControl != (null)) {
			load (Menu.GameControl.saves [Menu.GameControl.currentLevel]);
			blurIndex = playerTimeline.getLength ();
			if(Menu.GameControl.currentLevel > 1){
				int entrance = Menu.GameControl.saves[Menu.GameControl.currentLevel-1].exit;
				transform.position = GameObject.FindGameObjectsWithTag ("Respawn")[entrance].transform.position;
				if (playerTimeline.getLength() > 0){
					if(entrance != playerTimeline.entrance){
						playerTimeline.change (0, transform.position, Camera.main.transform.position, Camera.main.transform.localRotation, false, playerTimeline.time (0), transform.rotation, 0f, controller.getVelocity()); 
						createParadox(1);
					}else
						check (0);
				}
				playerTimeline.entrance = entrance;
			}
		}
		if (playerTimeline.getLength () > 1)
			startScrub ();
	}

	float updateTimer = 0f;
	float observedTime = 0f;
	int observedIndex = 0;

	// Update is called once per frame
	void Update () {
		getInput ();
		runTime ();
		updateTimeMarkers ();
		moveEnemies ();
	}

	void updateTimeMarkers(){
		timeCounter.text = (Mathf.Floor (observedTime*10f)/10f).ToString();
		if (sliderMax.value < timeSlider.value + 0.08f)
			timeCounter2.text = "";
		else
			timeCounter2.text = (Mathf.Floor (playerTimeline.time ((int)(sliderMax.value*(float)playerTimeline.getLength ()))*10f)/10f).ToString();

		for (int i = 0; i < milestones.Length-1; i++) {
			milestones[i].text = (Mathf.Floor (playerTimeline.time ((int)(((float)i+0.9f)/(float)milestones.Length*(float)playerTimeline.getLength ()))*10f)/10f).ToString();
		}
		milestones [milestones.Length - 1].text = (Mathf.Floor (playerTimeline.duration () * 10f) / 10f).ToString ();
	}

	void getInput(){
		if (Input.GetKeyDown (KeyCode.R)) {
			if (playerState != State.scrubbing) {
				endRecord ();
			} else if (!decide) {
				clear ();
				startRecord ();
			}
		} else if (!decide && Input.GetKeyDown (KeyCode.O) && playerState == State.scrubbing) {
			startOptimize ();
		} else if (!decide && Input.GetKeyDown (KeyCode.P)) {
			if (playerState == State.scrubbing)
				startPlay ();
			else if (playerState == State.playing)
				startScrub ();
		} 
//			else if (!decide && Input.GetKeyDown (KeyCode.C))
//			clear ();
		else if (Input.GetKeyDown (KeyCode.Mouse1)) {
				if (playerState == State.scrubbing) {
					startFreeLook ();
				}
			} else if (Input.GetKeyUp (KeyCode.Mouse1)) {
				if (playerState == State.scrubbing) {
					endFreeLook ();
				}
			} else if (!decide && Input.GetKey (KeyCode.B) && playerState == State.scrubbing) {
				playerTimeline.blur (1);
				playerTimeline.draw ();
			} 
//			else if (Input.GetKeyDown (KeyCode.U) && playerState == State.scrubbing) {
//			startView();

//		} else if (Input.GetKeyUp (KeyCode.U) && playerState == State.scrubbing) {
//			endView ();
//
//		}


	}

	void runTime(){
		//transform.rotation = initRotation;

		if (playerState == State.recording) {
			record ();
		} else if (playerState == State.scrubbing) {
			scrub ();
		} else if (playerState == State.optimizing) {
			optimize ();
		} else if (playerState == State.playing){
			play ();
		}

		if (observedIndex > 1) {
			if(playerState != State.optimizing){
				float targetRange = 12-(playerTimeline.volume(observedIndex)*5+2.5f);
				sound.range = (Mathf.Lerp (sound.range, targetRange, Mathf.Abs (sound.range-targetRange)/1000f + 0.1f));
			} else {

			}
		}
		if (p)
			distortion.distortionLevel = Mathf.Lerp (distortion.distortionLevel, 0.6f, 0.1f);
		else
			distortion.distortionLevel = Mathf.Lerp (distortion.distortionLevel, 0.0f, 0.1f);

		paradoxEffect.enabled = (p || playerState == State.scrubbing);
        //paradoxEffect.blur = ((playerState == State.scrubbing)? 0:(p?88:44));
		paradoxEffect2.enabled = p;
		Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60f + Mathf.Atan(deltaTime) * 5, 1/Mathf.Abs(Camera.main.fieldOfView-60f + Mathf.Exp(Mathf.Abs(deltaTime)) * 5));
		//sound.color = Color.Lerp (sound.color, p?new Color(253f/255f, 255f/255f, 200f/255f):Color.white, 0.1f);
		//paradoxEffect3.enabled = p;

		deltaTime = (observedTime-prevTime)/Time.deltaTime;
		prevTime = observedTime;
	}


	void startRecord(){
		if(!p){
			playerTimeline.erase();
			blurIndex = playerTimeline.getLength();
			timeSlider.value = 1;
			updateTimer = 0;
			scrub ();
			controller.setLook (Camera.main.transform.parent.localRotation, transform.rotation);
			playerState = State.recording;
			controller.on = true;
			controller.setVelocity (playerTimeline.yVelocity (observedIndex));
			controller.volume = playerTimeline.volume (observedIndex);
            foreach (Door door in doors)
            {
                door.erase(observedTime, 9999999);
            }
        }
	}

	void record(){
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		timeSlider.value = 1;
		observedTime = playerTimeline.duration ();
		observedIndex = playerTimeline.getLength();
		updateTimer += Time.deltaTime;
		if (hz >= 60 ||updateTimer >= 1f / hz) {
			playerTimeline.add (transform.position-(controller.m_isCrouched?Vector3.up/2:Vector3.zero), m_Camera.transform.localPosition+Camera.main.transform.localPosition, m_Camera.transform.localRotation * Camera.main.transform.localRotation, p, playerTimeline.duration () + updateTimer, transform.rotation, controller.volume, controller.getVelocity ());
			if(!p && playerTimeline.getLength() > 0)
				p = checkGuards(playerTimeline.duration () + updateTimer, playerTimeline.position (observedIndex), controller.volume);
			if(p){
				//playerTimeline.setParadox (observedIndex, true);
				if(spotGuard){
                    Quaternion tmp = m_Camera.transform.rotation;
					m_Camera.transform.LookAt (spotGuard.transform.position);
					playerTimeline.add (transform.position-(controller.m_isCrouched?Vector3.up/2:Vector3.zero), m_Camera.transform.localPosition + Camera.main.transform.localPosition, m_Camera.transform.localRotation * Camera.main.transform.localRotation, true, playerTimeline.duration () + updateTimer, transform.rotation, controller.volume, controller.getVelocity ());
                    m_Camera.transform.rotation = tmp;
                }
				endRecord ();
			}
			updateTimer = 0;
		}

		if(Input.GetKeyDown(KeyCode.Mouse0)){
			RaycastHit hit;
			if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit)){
				print(hit.collider.name);
				if(hit.collider.tag == "Door" && hit.distance < 500)
					hit.collider.GetComponent<Door>().toggle(observedTime, false);
			}
		}
	}

	void endRecord(){
        if (!p)
        {
            playerTimeline.setVolume(playerTimeline.getLength() - 1, controller.volume);
            p = checkGuards(playerTimeline.duration(), playerTimeline.position(playerTimeline.getLength() - 1), controller.volume);
            playerTimeline.setParadox(playerTimeline.getLength() - 1, p);
        }
        playerTimeline.blur (blurIndex);
		blurIndex = observedIndex;
		observedIndex = playerTimeline.getLength ();
		startScrub ();
	}

	bool checkGuards(float time, Vector3 position, float speed){
		bool spotted = false;
		foreach (Guard enemy in enemies) {
			if(enemy.canSee (time, position, speed)){
				spotted = true;
				spotGuard = enemy;
				break;
			}
		}
		foreach (GameObject sensor in sensors) {
			if(!spotted && sensor.GetComponent<BoxCollider>().bounds.Contains (position)){
				spotted = true;
				spotGuard = null;
				break;
			}
		}
        foreach (Door door in doors)
        {
        	if(playerState == State.optimizing)
        		door.setPositionOpt(time);
        	else
        		door.setPosition(time);
            if (!spotted && door.GetComponent<BoxCollider>().bounds.Contains(position))
            {
                spotted = true;
                spotGuard = null;
                break;
            }
        }
        return spotted;
	}

	void startPlay(){ 
		sliderMax.value = 1;
		playerState = State.playing;
		if (observedIndex <=0)
			observedIndex = 1;
		updateTimer = playerTimeline.time (observedIndex-1);
		//controller.enabled = true;
		//print (observedIndex);
		timeSlider.value = (float)observedIndex / (float)playerTimeline.getLength ();
	}

	void play(){
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		if (observedIndex >= playerTimeline.getLength () - 1) {
			startScrub ();
			return;
		}
		int old = observedIndex;
		updateTimer += Time.deltaTime;
		float lval = (updateTimer - playerTimeline.time (observedIndex - 1)) / (playerTimeline.time (observedIndex) - playerTimeline.time (observedIndex - 1));

		transform.position = Vector3.Lerp (playerTimeline.position (observedIndex - 1), playerTimeline.position (observedIndex), lval);
		m_Camera.transform.localPosition = Vector3.Lerp(playerTimeline.position2 (observedIndex-1), playerTimeline.position2(observedIndex), lval);
		transform.rotation = Quaternion.Slerp (playerTimeline.rotation2 (observedIndex - 1), playerTimeline.rotation2 (observedIndex), lval);
        m_Camera.transform.localRotation = Quaternion.Slerp (playerTimeline.rotation (observedIndex - 1), playerTimeline.rotation (observedIndex), lval);
		p = playerTimeline.paradox (observedIndex);
		observedTime = updateTimer;
		while(updateTimer >= playerTimeline.time(observedIndex)){
			observedIndex++;
			timeSlider.value = (float)observedIndex / (float)playerTimeline.getLength ();
		}
		if (playerTimeline.volume(observedIndex) > playerTimeline.volume(old))
			controller.PlayFootStepAudio();

        //		if (old > observedIndex && playerTimeline.volume(observedIndex) < playerTimeline.volume(old))
        //			controller.PlayFootStepAudio();
    }

	void startScrub(){
		timeSlider.value = (float)observedIndex / (float)playerTimeline.getLength ();
		playerTimeline.draw();
		playerState = State.scrubbing;
		Destroy(endMarker);
		controller.on = false;
		endMarker = Instantiate (endMarkerPrefab, playerTimeline.position (optimizeEnd), playerTimeline.rotation (optimizeEnd)) as GameObject;
    }

	void startFreeLook(){
		controller.on = true;
		controller.setLook (Camera.main.transform.parent.localRotation, transform.rotation);
		freeLook = true;
	}

	void endFreeLook(){

		controller.on = false;
		controller.setLook (playerTimeline.rotation (observedIndex), playerTimeline.rotation2 (observedIndex));
		freeLook = false;
	}

    private float sc = 1;

	void scrub(){
        controller.setVelocity (0f);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		int old = observedIndex;

        if (Input.GetKey(KeyCode.W))
        {
            if (Input.GetKey(KeyCode.LeftShift))
                timeSlider.value += 2 * sc / (float)playerTimeline.getLength();
            else
                timeSlider.value += sc / (float)playerTimeline.getLength();
            sc += 0.002f;
            //if (observedIndex > 0 && playerTimeline.volume(observedIndex) > playerTimeline.volume(observedIndex-1))
                //controller.PlayFootStepAudio();
        }
        if (Input.GetKey(KeyCode.S))
        {
            if(Input.GetKey(KeyCode.LeftShift))
                timeSlider.value -= 2*sc / (float)playerTimeline.getLength();
            else
                timeSlider.value -= sc / (float)playerTimeline.getLength();
            sc += 0.002f;
        }
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S))
            sc = 1;
        observedIndex = (Mathf.RoundToInt(timeSlider.value * (float)playerTimeline.getLength()));

        if (Input.GetKey(KeyCode.D))
        {
            sliderMax.value += 1 / (float)playerTimeline.getLength();
        }
        if (Input.GetKey(KeyCode.A))
        {
            sliderMax.value -= 1 / (float)playerTimeline.getLength(); ;
        }
        //if(observedIndex == 0)
        //observedIndex++;
        if (observedIndex == playerTimeline.getLength ())
			observedIndex--;
		if (observedIndex >= playerTimeline.getLength ())
			observedIndex = playerTimeline.getLength() - 1;
		if (observedIndex < 0)
			observedIndex = 0;
		endMarker.transform.position = playerTimeline.position (Mathf.RoundToInt(sliderMax.value*playerTimeline.getLength()));
		
		
		if (sliderMax.value < timeSlider.value) {
			sliderMax.value = timeSlider.value;
		}
		if (sliderMax.value == 1f || sliderMax.value == timeSlider.value) {
			//sliderMax.value = 0.99f;
			endMarker.transform.position = new Vector3(100, 100, 100);
		}

		if (scrubSmoothness < 1 && scrubSmoothness >0 && Mathf.Abs (observedIndex-old) < 10) {
			transform.position = Vector3.Lerp (transform.position, playerTimeline.position (observedIndex), scrubSmoothness);
            m_Camera.transform.localPosition = Vector3.Lerp (m_Camera.transform.localPosition, playerTimeline.position2 (observedIndex), scrubSmoothness);
			if (!freeLook) {
				transform.rotation = Quaternion.Slerp (transform.rotation, playerTimeline.rotation2 (observedIndex), scrubSmoothness);
				m_Camera.transform.localRotation = Quaternion.Slerp (m_Camera.transform.localRotation, playerTimeline.rotation (observedIndex), scrubSmoothness);
			}
		} else {
			transform.position = playerTimeline.position (observedIndex);
            m_Camera.transform.localPosition = playerTimeline.position2 (observedIndex);
			if (!freeLook) {
				transform.rotation = playerTimeline.rotation2 (observedIndex);
                m_Camera.transform.localRotation = playerTimeline.rotation (observedIndex);
			}
		}

		observedTime = playerTimeline.time (observedIndex);

//		if (old < observedIndex && playerTimeline.volume(observedIndex) > playerTimeline.volume(old))
//			controller.PlayFootStepAudio();
//	
//		if (old > observedIndex && playerTimeline.volume(observedIndex) < playerTimeline.volume(old))
//			controller.PlayFootStepAudio();

		p = playerTimeline.paradox (observedIndex);

        
    }

	void startOptimize(){
		if (!p) {
			if (sliderMax.value == 1){
				playerTimeline.erase (observedIndex);
				startRecord ();
			} else {
				optimizeStart = observedIndex; 
				playerState = State.optimizing;
				controller.on = true;
				controller.volume = playerTimeline.volume (observedIndex);
				controller.setLook (Camera.main.transform.parent.localRotation, transform.rotation);
				//controller.setLook (playerTimeline.rotation (optimizeStart), playerTimeline.rotation2 (optimizeStart));
				optimizeEnd = (int)(sliderMax.value * (float)playerTimeline.getLength ());
				//transform.rotation = playerTimeline.rotation2 (optimizeStart);
				templine.clear ();

				updateTimer = 0;
				foreach (Door door in doors) 
					door.startOpt();
			}
		}
	}

	void optimize(){
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		//observedTime = playerTimeline.duration ();
		//observedIndex = playerTimeline.getLength();
		updateTimer += Time.deltaTime;
		if (updateTimer >= 1f / hz) {
			templine.add (transform.position, m_Camera.transform.localPosition + Camera.main.transform.localPosition, m_Camera.transform.localRotation, p, templine.duration () + updateTimer, transform.rotation, controller.volume, controller.getVelocity ());
			if(!p && templine.getLength() > 0)
				p = checkGuards(templine.duration () + updateTimer + playerTimeline.time(optimizeStart), templine.position (templine.getLength ()-1), Vector3.Distance (transform.position-transform.position.y*Vector3.up, templine.position (templine.getLength()-1)-templine.position (templine.getLength()-1).y*Vector3.up));
			updateTimer = 0;
			if(p){
				templine.setParadox (templine.getLength ()-1, true);
				startScrub ();
			}
		}
		observedTime = playerTimeline.time (optimizeStart) + templine.duration ();

		if(Input.GetKeyDown(KeyCode.Mouse0)){
			RaycastHit hit;
			if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit)){
				print(hit.collider.name);
				if(hit.collider.tag == "Door" && hit.distance < 500)
					hit.collider.GetComponent<Door>().toggle(observedTime, true);
			}
		}

	}
	bool decide = false;

	void endOptimize(){
		templine.blur (1);
		if (templine.duration () > playerTimeline.time (optimizeEnd) - playerTimeline.time (optimizeStart)) {
			decide = true;
			templine.draw ();
		}else
			keepOptimize ();

		//observedIndex = optimizeEnd;
		startScrub ();
	}

	void keepOptimize(){
		decide = false;
		templine.erase();
		templine.timeShift (0, playerTimeline.time(optimizeStart));
		playerTimeline.replace (optimizeStart, optimizeEnd, templine);

		foreach (Door door in doors)
			door.merge(playerTimeline.time(optimizeStart), playerTimeline.time(optimizeEnd), playerTimeline.getShift(optimizeStart, optimizeEnd, templine));
		//observedIndex = optimizeStart + templine.getLength ();
		
		playerTimeline.smoothEdge (optimizeStart + templine.getLength ()-1);
		//playerTimeline.smoothEdge (optimizeStart, (int)(hz/2f));
		playerTimeline.draw ();
		timeSlider.value = (float)observedIndex / (float)playerTimeline.getLength ();

		if (playerTimeline.paradox(playerTimeline.getLength() - 1))
		    playerTimeline.erase (playerTimeline.getLength () -1);

		if (observedIndex != 0 && playerTimeline.paradox (observedIndex-1))
			createParadox (observedIndex);
		else {
			check (observedIndex);
		}
	}

	void check(int index){
		p = false;
		for(int i = index; i < playerTimeline.getLength (); i++){
			if(!p && checkGuards (playerTimeline.time (i), playerTimeline.position (i),playerTimeline.volume (i)))
				p = true;
			playerTimeline.setParadox (i, p);
		}
	}

	void clear(){
		if (playerTimeline.paradox (observedIndex))
			return;
		playerTimeline.erase (observedIndex+1);
		playerTimeline.draw ();
		timeSlider.value = 1;
		playerTimeline.complete = false;
	}


	void moveEnemies(){

		foreach (Guard enemy in enemies)
			enemy.setPosition(observedTime, deltaTime);

		foreach (Door door in doors)
            door.setPosition(observedTime);
	}

	void OnTriggerEnter(Collider col){
		if (col.gameObject == endMarker && playerState == State.optimizing) {
			endOptimize ();
		} else if (col.gameObject.tag == "levelEnd") {
			if(playerState == State.recording){ 
				endRecord ();
			}
			else if(playerState == State.optimizing){
				endOptimize ();
				playerTimeline.erase (observedIndex);
			}
			playerTimeline.complete = true;
			playerTimeline.exit = col.gameObject.GetComponent<exit>().number;
			if(Menu.GameControl.saves[Menu.GameControl.currentLevel+1].getLength () > 1){
				if (Menu.GameControl.saves[Menu.GameControl.currentLevel+1].entrance!= playerTimeline.exit){
					Menu.GameControl.saves[Menu.GameControl.currentLevel+1].setParadox (Menu.GameControl.saves[Menu.GameControl.currentLevel+1].getLength ()-1, true);
				} else if (!Menu.GameControl.saves[Menu.GameControl.currentLevel+1].paradox(Menu.GameControl.saves[Menu.GameControl.currentLevel+1].getLength ()-2)){
					Menu.GameControl.saves[Menu.GameControl.currentLevel+1].setParadox (Menu.GameControl.saves[Menu.GameControl.currentLevel+1].getLength ()-1, false);
				}
			}
		}
	}

	public void load(Timeline save){
		playerTimeline.copy (save);
	}

	void createParadox(int index){
		print ("paradoxing " + index);
		for (int i = index; i< playerTimeline.getLength (); i++) {
			if(playerTimeline.paradox (i))
				break;
			playerTimeline.setParadox (i, true);
		}
	}

//	void startView(){
//		Camera.main.transform.position += Vector3.up * 10;
//		Camera.main.transform.rotation = Quaternion.LookRotation (Vector3.down);
//	}
//	void endView(){
//		Camera.main.transform.position -= Vector3.up * 10;
//	}

	void OnGUI(){
		if (decide) {
			if(GUI.Button (new Rect(Camera.main.pixelWidth/2-100, Camera.main.pixelHeight/2-50, 200, 50), "Keep")){
				keepOptimize();
			}
			if(GUI.Button (new Rect(Camera.main.pixelWidth/2-100, Camera.main.pixelHeight/2+25, 200, 50), "Discard")){
				templine.erase ();
				templine.clear();
				decide = false;
			}
		}
	}

}