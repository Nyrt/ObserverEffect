using UnityEngine;
using System.Collections;

public class Guard : MonoBehaviour {
	
	private Vector3[] waypoints;

	private Quaternion[] rotations;
	private float[] timeToNext;
	private float[] rotationTimes;
	public float speed;
	public float turnSpeed;
	public bool circle;
	public bool backtrack;
	public bool stationary;
	public bool swivel;
    public bool cam;

    private float forward;
    private float turn;

	public float swivelAngle;
	private Quaternion swivelAngleA;
	private Quaternion swivelAngleB;

	private Vector3 origin;

	public float fov;
	public float range;
	public float hearing;

	public float phase;

	[SerializeField] private float pathLength;
	private float pathTime;
	

	[SerializeField] private float t;
	[SerializeField] private int waypoint;

	[SerializeField] private float observedValue;

	public Light view;
	private float turnTime;
    private Animator anim;
	void Start(){
        if (!cam)
            anim = GetComponent<Animator>();
		if (view) {
			view.spotAngle = fov * 2;
			view.range = range;
			if(!swivel)
				view.transform.localPosition = new Vector3 (0, 1.7f, 0.2f);
			view.color = Color.cyan;
		}
		if (!stationary) {
			if (circle){
				origin = transform.position;
			}else{
				makePath ();
				setDistanceAndRotation ();
			}
		} else if (swivel) {
			swivelAngleA = Quaternion.AngleAxis (-swivelAngle/2, transform.up)*transform.rotation;
			swivelAngleB = Quaternion.AngleAxis (swivelAngle/2, transform.up)*transform.rotation;
	    	turnTime = turnSpeed;
		}

        
	}


	//sets internal variables in preparation for moving guard
	private void get(float time){

		t = 0; //t = time to reach previous waypoint
		waypoint = 0;
		if (!stationary) {
			while (waypoint < timeToNext.Length - 1 && t + timeToNext[waypoint] + rotationTimes[waypoint] < time) {
				t += timeToNext [waypoint];
				t += rotationTimes [waypoint];
				waypoint++;
			}
		}
	}

    //sets the guard's position and rotation to its position at the given time
    
	public void setPosition(float time, float deltaTime){
        transform.position = getPosition (time);
		transform.rotation = getRotation (time);
        if (!cam)
        {
            //Debug.Log(turn);
            anim.SetFloat("Forward", Mathf.Abs(forward)/1.5f);
            anim.SetFloat("Turn", -turn * 25);
            anim.SetFloat("TimeWarp", deltaTime);
        }
    }




	//returns the guard's position at the given time
	public Vector3 getPosition(float time){
		if (stationary) {
            forward = 0f;
            return transform.position; 
		} else if(circle) {
			float deg = (time/speed+phase)*2*Mathf.PI;
            forward = Mathf.PI*swivelAngle/speed/2;
            return origin + new Vector3(swivelAngle*Mathf.Cos (deg), 0, swivelAngle*Mathf.Sin (deg));
		}else{
			time += phase * pathTime;
			time %= pathTime;
			get (time);
            float progress = (time - t) / timeToNext[waypoint];
            if (progress > 1 || progress < 0)
                forward = 0f;
            else
                forward = pathLength/speed/3;
            return Vector3.Lerp(waypoints[waypoint], waypoints[waypoint + 1], progress);//(float)(System.Math.Tanh(((double)progress-0.5)*5)/2.0+0.5));
		}
	}

	// returns the guard's rotation at the given time
	public Quaternion getRotation(float time){
		time += phase * (swivel ? turnTime : pathTime);
		if (stationary) {
			if (!swivel) {
                turn = 0f;
				return transform.rotation;
			} else {
                turn = Mathf.Cos(time / turnTime * 2 * Mathf.PI);
                return Quaternion.Lerp (swivelAngleA, swivelAngleB, (Mathf.Sin (time / turnTime*2*Mathf.PI) + 1) / 2f);
			}
		} else {
			if (circle) {
                turn = 1f / speed;
				return Quaternion.AngleAxis (-(time/speed+phase)*360+Mathf.Sign (speed)*90-90, transform.up);
			} else {
				time %= pathTime;
				get (time);
                float progress = (time - t - timeToNext[(waypoint + waypoints.Length - 1) % timeToNext.Length]) / rotationTimes[waypoint % rotationTimes.Length];
                if (progress > 1 || progress < 0)
                    turn = 0f;
                else
                    turn = 4f / turnSpeed;
                return Quaternion.Lerp (rotations [waypoint], rotations [waypoint + 1], progress);
			}
		}
	}


    //constructs the guard's path
    private void makePath(){
		pathLength = 0;
		waypoints = new Vector3[transform.childCount-2];
		for (int i = 1; i < transform.childCount-1; i++) {
			waypoints[i-1] = transform.GetChild (i).position;
		}

		for (int i = 1; i < waypoints.Length; i++)
			pathLength += Vector3.Distance (waypoints [i - 1], waypoints [i]);
		
		if (backtrack) {
			pathLength *= 2;
			Vector3[] newPath = new Vector3[waypoints.Length * 2 - 1];
			for(int i = 0; i < waypoints.Length; i++){
				newPath[i] = waypoints[i];
				newPath[newPath.Length - 1 - i] = waypoints[i];
			}
			waypoints = newPath;
		}else{
			pathLength += Vector3.Distance (waypoints [0], waypoints [waypoints.Length - 1]);
			Vector3[] newPath = new Vector3[waypoints.Length + 1];
			for(int i = 0; i < waypoints.Length; i++){
				newPath[i] = waypoints[i];
			}
			newPath[waypoints.Length] = waypoints[0];
			waypoints = newPath;
		}
		
		pathTime = speed;
	}

	//constructs the time and distance arrays
	private void setDistanceAndRotation(){
		timeToNext = new float[waypoints.Length - 1];
		rotationTimes = new float[waypoints.Length -1];
		float totalRotationTime = 0;
		rotations = new Quaternion[waypoints.Length];
		
		for (int i = 0; i < timeToNext.Length; i++) {
			rotationTimes[i] = Vector3.Angle(waypoints[i + 1] - waypoints[i], waypoints[(i + 2) % waypoints.Length] - waypoints[i + 1] )/turnSpeed;
			totalRotationTime += rotationTimes[i];

//			pathTime+= rotationTimes[i];
			rotations[i] = Quaternion.LookRotation (waypoints[i + 1] - waypoints[i]);
		}


		//pathTime -= rotationTimes [rotationTimes.Length-1];
		totalRotationTime -= rotationTimes[rotationTimes.Length - 1];
		rotationTimes[rotationTimes.Length - 1] = Vector3.Angle(waypoints[0] - waypoints[rotationTimes.Length - 1], waypoints[(1) % waypoints.Length] - waypoints[0] )/turnSpeed;
		totalRotationTime += rotationTimes[rotationTimes.Length - 1];
		//pathTime += rotationTimes [rotationTimes.Length-1];
		rotations [rotations.Length - 1] = rotations [0];

		for (int i = 0; i < timeToNext.Length; i++) {
			timeToNext[i] = (pathTime - totalRotationTime)*Vector3.Distance (waypoints[i], waypoints[i+1])/pathLength;
		}
	}

	// returns true if the guard can see the given point at the given time
	public bool canSee(float time, Vector3 target, float volume){
		bool spotted = false;
		target += Vector3.up * 0.5f;
		Vector3 pos = getPosition (time)+Vector3.up*1.5f;
		Vector3 angleToTarget = (target - pos);
		float dist = Vector3.Distance (pos, target);
		if (dist > range)
			return false;
		RaycastHit hit;
		bool didHit;
		didHit = Physics.Raycast (pos, angleToTarget, out hit, Mathf.Min (dist, range));
		Debug.DrawLine (pos, pos + angleToTarget.normalized * Mathf.Min (dist, range));
		//print (!didHit);
		if(hearing > 0)
			//print (volume);
			spotted = ((Vector3.Angle (getRotation (time) * Vector3.forward, angleToTarget) < fov) && !didHit);
		if (!didHit && Vector3.Distance (pos, target) < ((Mathf.Log (volume)/9*hearing*hearing)))
			spotted = true;

		//print (Vector3.Angle (transform.forward, angleToPlayer));
		return spotted;
	}
	
	// Update is called once per frame
	void Update () {
        
    }
}
