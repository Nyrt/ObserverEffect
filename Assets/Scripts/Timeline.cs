using UnityEngine;
using System.Collections;

public class Timeline : MonoBehaviour {

	struct timeNode{
		public Vector3 position;
		public Vector3 position2;
		public Quaternion rotation;
		public bool paradox;
		public float time;
		public Quaternion rotation2;
		public float volume;
		public float yVelocity;
	}

	public bool complete = false;
	public int exit;
	public int entrance = -1;

	//public bool init = false;
	private timeNode[] timeline;
	[SerializeField] private int length = 0;
	private int INIT_CAP = 256;
	private int capacity = 256;

	// Use this for initialization
	void Start () {
		clear ();
	}

	//add a node to the end of the timeline
	public void add(Vector3 position, Vector3 position2, Quaternion rotation, bool paradox, float time, Quaternion rotation2, float volume, float yVelocity){
		if (length >= capacity) 
			expand ();
		timeNode new_node;
		new_node.position = position;
		new_node.position2 = position2;
		new_node.rotation = rotation;
		new_node.paradox = paradox;
		new_node.time = time;
		new_node.rotation2 = rotation2;
		new_node.volume = volume;
		new_node.yVelocity = yVelocity;
		timeline [length++] = new_node;
	}

	private void add(timeNode node){
		if (length >= capacity) 
			expand ();
		timeline [length++] = node;
	}

	void expand(){
		timeNode[] new_timeline = new timeNode[capacity * 2];
		for(int i = 0; i < length; i++)
			new_timeline[i] = timeline[i];
		capacity *= 2;
		timeline = new_timeline;
	}

	//change a node at a given index
	public void change(int index, Vector3 position, Vector3 position2, Quaternion rotation, bool paradox, float time, Quaternion rotation2, float volume, float yVelocity){
		if (index < length) {
			timeNode new_node;
			new_node.position = position;
			new_node.position2 = position2;
			new_node.rotation = rotation;
			new_node.paradox = paradox;
			new_node.time = time;
			new_node.rotation2 = rotation2;
			new_node.volume = volume;
			new_node.yVelocity = yVelocity;
			timeline [index] = new_node;
		} else {
			add (position, position2, rotation, paradox, time, rotation2, volume, yVelocity);
		}
	}

	public float getShift(int startIndex, int endIndex, Timeline insert){
		return (time (startIndex+1) + insert.duration ()-insert.time (0) - time (endIndex));
	}

	public void replace(int startIndex, int endIndex, Timeline insert){
		//print("Length: " + insert.getLength());
		//expands the size if the new timeline would be too large
		int newCap = capacity;
		int newLen = startIndex + insert.getLength () + length - endIndex;
		if (newCap < newLen)
			newCap *= 2;
		timeNode[] new_timeline = new timeNode[newCap];

		//finds and fixes the time slippage
		timeShift (endIndex, time (startIndex+1) + insert.duration ()-insert.time (0) - time (endIndex));

		//copy the first part of the original timeline
		for (int i = 0; i < startIndex; i++)
			new_timeline [i] = timeline [i];

		//copy the new section
		for (int i = 0; i < insert.getLength(); i++) {
			new_timeline [startIndex + i].position = insert.position(i);
			new_timeline [startIndex + i].position2 = insert.position2(i);
			new_timeline [startIndex + i].rotation = insert.rotation(i);
			new_timeline [startIndex + i].paradox = insert.paradox(i);
			new_timeline [startIndex + i].time = insert.time(i);
			new_timeline [startIndex + i].rotation2 = insert.rotation2(i);
			new_timeline [startIndex + i].volume = insert.volume(i);
			new_timeline [startIndex + i].yVelocity = insert.yVelocity(i);
		}

		//copy the remaining original timeline
		for(int i = endIndex; i < length; i++)
			new_timeline[startIndex-endIndex+insert.getLength ()+i] = timeline[i];

		length = newLen;
		capacity = newCap;
		timeline = new_timeline;
	}

	public void timeShift(int index, float amount){
		for (int i = index; i < length; i++)
			timeline [i].time += amount;
	}

	public void clear(){
		capacity = INIT_CAP;
		timeline = new timeNode[INIT_CAP];
		length = 0;
		complete = false;
	}

	//use to truncate the timeline in various ways
	private void removeLast(){length--;}

	public void erase(int index){
		while (length > index)
			removeLast ();
	}
	

	public void smoothEdge(int index){
		float dist = Vector3.Distance (timeline [index].position, timeline [index + 1].position);
		float speed = (Vector3.Distance (timeline [index - 1].position, timeline [index].position) / (timeline [index].time - timeline [index - 1].time));
		//print (dist);
		float dt = dist / speed;
		//timeShift ((index + 1), dt);
		//Debug.DrawLine (timeline [index].position, timeline [index + 1].position, Color.white, 100);
		//for(int t = 

		Timeline temp = gameObject.AddComponent <Timeline>();
		temp.clear ();
		for(float t = 0; t < dt; t += (timeline [index].time - timeline [index - 1].time)){
			timeNode node;
			node.position = Vector3.Lerp (timeline [index].position, timeline [index + 1].position, t/dt);
			node.position2 = Vector3.Lerp (timeline [index].position2, timeline [index + 1].position2, t/dt);
			node.rotation = Quaternion.Lerp (timeline [index].rotation, timeline [index + 1].rotation, t/dt);
			node.rotation2 = Quaternion.Lerp (timeline [index].rotation2, timeline [index + 1].rotation2, t/dt);
			node.paradox = timeline[index].paradox;
			node.time = timeline[index].time + t;
			node.volume = Mathf.Lerp (timeline [index].volume, timeline [index + 1].volume, t/dt);
			node.yVelocity = Mathf.Lerp (timeline [index].yVelocity, timeline [index + 1].yVelocity, t/dt);
			temp.add (node);
		}

		replace (index, index + 1, temp);

		Destroy (temp);
//		for(int i = index - buffer; i < index + buffer && i < length; i++){
//			if (i < 1)
//				i = 1;
//			timeline[i].rotation = Quaternion.Slerp (timeline[i].rotation, timeline[index].rotation, 1f-Mathf.Abs (((float)(i-index))/((float)buffer)));
//			timeline[i].rotation2 = Quaternion.Slerp (timeline[i].rotation2, timeline[index].rotation2, 1f-Mathf.Abs (((float)(i-index))/((float)buffer)));
//			//print (1f-Mathf.Abs (((float)(i-index))/((float)buffer)));
//		}
//
//		for (int i = 0; i < buffer; i++) {
//			if (i < 1)
//				i = 1;
//			if(index + i + 1 < length)
//				timeline [index+i].position = (timeline [index + i - 1].position + timeline [index + i + 1].position) / 2f;
//			if(index - i - 1 > 0)
//				timeline [index-i].position = (timeline [index - i - 1].position + timeline [index - i + 1].position) / 2f;
//		}
	}

	//use to find size of timeline
	public int getLength(){return length;}
	public float duration(){
		if (length > 0) {
			return timeline [length - 1].time;
		}
		return 0;
	}

	//use to get node values at a given index
	public Vector3 position(int index){return timeline [index].position;}
	public Vector3 position2(int index){return timeline [index].position2;}
	public Quaternion rotation(int index){return timeline [index].rotation;}
	public bool paradox(int index){return timeline [index].paradox;}
	public float time(int index){return timeline [index].time;}
	public Quaternion rotation2(int index){return timeline [index].rotation2;}
	public float yVelocity(int index){return timeline [index].yVelocity;}
	public float volume(int index){return timeline [index].volume;}

    public void setVolume(int index, float v) { timeline[index].volume = v; }

	public LineRenderer pathDrawer;

	public void draw(){
		pathDrawer.SetVertexCount (length);
		for (int i = 0; i < length; i++) {
			pathDrawer.SetPosition (i, position (i));
		}
	}

	public void erase(){
		pathDrawer.SetVertexCount (0);
	}
	

	public void setParadox(int index, bool newState){
		timeline [index].paradox = newState;
	}

	public void blur(int index){
		if (index <= 0)
			index = 1;
		timeNode[] temp = new timeNode[capacity];
		for (int i = 0; i < index; i++) {
			temp[i]= timeline [i];
		}
		for (int i = index; i < length-1; i++) {
			timeNode tmp;
			tmp.position = (timeline [i - 1].position + timeline [i].position + timeline [i + 1].position) / 3;
			tmp.position2 = timeline[i].position2;
			tmp.rotation = timeline[i].rotation; 
			tmp.rotation2 = timeline[i].rotation2;
			tmp.time = (timeline [i - 1].time + timeline [i].time + timeline [i + 1].time) / 3;
			tmp.paradox = timeline[i].paradox;
			tmp.volume = timeline[i].volume;
			tmp.yVelocity = timeline[i].yVelocity;
			temp [i] = tmp;
		}
		temp [length - 1] = timeline [length - 1];
		timeline = temp;
	}

	public void copy(Timeline source){
		clear ();
		for (int i = 0; i < source.getLength(); i++) {
			timeNode n = new timeNode();
			n.position = source.position(i);
			n.position2 = source.position2(i);
			n.rotation = source.rotation(i);
			n.rotation2 = source.rotation2(i);
			n.time = source.time(i);
			n.paradox = source.paradox(i);
			n.volume = source.volume(i);
			n.yVelocity = source.yVelocity(i);
			add (n);
		}
		exit = source.exit;
		entrance = source.entrance;
		if (source.complete) {
			complete = true;
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
