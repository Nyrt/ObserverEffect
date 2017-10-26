using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Door : MonoBehaviour {

    [System.Serializable]
    public struct Opening
    {
        
        public float time;
        public float percent;
        public bool open;
    }

    [SerializeField] private List<Opening> openings;
    [SerializeField] private List<Opening> opening_tmp;
	public float openTime;
    [SerializeField] private int len;
    [SerializeField] private int currTime = 0;
    [SerializeField] private float percent_open;
    private int tmplen;

	private Vector3 closedPosition;
	private Vector3 openPosition;
	private Quaternion closedRotation;
	private Quaternion openRotation;

	public void toggle(float time, bool opt){
        setPosition(time);
        Opening tmp = new Opening();
        tmp.time = time;
        
        if(opt){
            tmp.open = !opening_tmp[getIndexOpt(time)].open;
            tmp.percent = percent_open;
            opening_tmp.Add(tmp);
        } else {
            tmp.open = !openings[getIndex(time)].open;
            tmp.percent = percent_open;
            openings.Add(tmp);
        }
	}
    
private int getIndex(float time){
    int i = 0;
    while(i < openings.Count && openings[i].time < time)
        i++;
    i = Mathf.Clamp(i, 0, openings.Count);
    return i - 1;
}

private int getIndexOpt(float time){
        int i = 0;
    while(i < opening_tmp.Count && opening_tmp[i].time < time)
        i++;
    i = Mathf.Clamp(i, 0, opening_tmp.Count);
    return i - 1;
}

    public void setPosition(float time){
        currTime = getIndex(time);

        percent_open = Mathf.Clamp(openings[currTime].percent + Mathf.Clamp((time - openings[currTime].time) / openTime, 0, 1) * ((openings[currTime].open?1:0) - (openings[currTime-1].open?1:0)), 0, 1);
        
        transform.position = Vector3.Lerp(openPosition, closedPosition, 1 - percent_open);
        transform.rotation = Quaternion.Lerp(openRotation, closedRotation, 1 - percent_open);
    }

    public void setPositionOpt(float time){
        currTime = getIndexOpt(time);
        percent_open = Mathf.Clamp(opening_tmp[currTime].percent + Mathf.Clamp((time - opening_tmp[currTime].time) / openTime, 0, 1) * ((opening_tmp[currTime].open?1:0) - (opening_tmp[currTime-1].open?1:0)), 0, 1);
        
        transform.position = Vector3.Lerp(openPosition, closedPosition, 1 - percent_open);
        transform.rotation = Quaternion.Lerp(openRotation, closedRotation, 1 - percent_open);
    }

	// Use this for initialization
	void Start () {
        len = 0;
		closedPosition = transform.position;
		closedRotation = transform.rotation;
        openPosition = transform.GetChild(0).position;
        openRotation= transform.GetChild(0).rotation;
        Opening tmp = new Opening();
        tmp.open = false;
        tmp.time = -1f;
        openings.Add(tmp);
        openings.Add(tmp);
    }

    public void startOpt(){
        opening_tmp.Clear();
        Opening tmp = new Opening();
        tmp.open = openings[currTime].open;
        tmp.time = -1f;
        opening_tmp.Add(tmp);
        opening_tmp.Add(tmp);
    }

    public void merge(float start, float end, float shift){
        erase(start, end);
        timeShift(end, shift);
        openings.InsertRange(getIndex(start) + 1, opening_tmp);
        openings.RemoveRange(0,2);
    }

    public void erase(float start, float end)
    {
        int s = getIndex(start);
        openings.RemoveRange(s + 1, getIndex(end) - s);
        while(openings.Count < 2){
            Opening tmp = new Opening();
            tmp.open = openings[currTime].open;
            tmp.time = -1f;
            openings.Add(tmp);
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void timeShift(float start, float diff)
    {
        currTime = 0;
        while (currTime < len && openings[currTime].time < start)
            currTime++;
        while (currTime < len)
        {
            Opening tmp = openings[currTime];
            tmp.time += diff;
            openings[currTime] = tmp;
            currTime++;
        }
    }
}
