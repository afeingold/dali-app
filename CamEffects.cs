using UnityEngine;
using System.Collections;

/// <summary>
/// This class (a singleton) holds the functionality for music and for camera positioning. 
/// It fades music out and in at the beginning and end of levels, respectively,
/// and its "SetEffect" function is called each time the player reaches a new peg. 
/// </summary>
public class CamEffects : MonoBehaviour {

	[HideInInspector] public bool horLock = false;//whether the camera is currently locked in the x direction
	[HideInInspector] public bool verLock = false;//whether the camera is currently locked in the y direction
	private float lockX = 0;//x coordinate of horizontal lock relative to world
	private float lockY = 0;//y coordinate of horizontal lock relative to world

	private bool zoomedOut = false;//whether the camera is currently in wide mode
	private AudioSource audio;//reference to audio player component
	private GameObject player;//reference to player object
	private GameObject lockPeg;//peg relative to which camera is locked

	private static CamEffects instance = null;//camera should carry over between scenes; we keep and use instance variable to ensure uniqueness
 	public static CamEffects Instance {
    	get { 
     		return instance; 
     	}
 	}

 	/// <summary>
 	///	Awake is called each time level is loaded, and prepares the camera for the level.
 	/// </summary>
	void Awake()
	{
		if (instance != null && instance != this) {//if a second camera is created
         	Destroy(this.gameObject);//destroy it
         	return;
     	} else {
         	instance = this;//otherwise make this the active instance
     	}
     	DontDestroyOnLoad(this.gameObject);//ensure longevity of camera between scenes


		audio = GetComponent<AudioSource>();//establish reference
		audio.Play();//begin music playing
		FadeOutMusic();//start fading music out
	}

	/// <summary>
	/// Called each time level is reloaded. Ensures camera level setup is correct in case of strange transitions.
	/// </summary>
	void OnLevelWasLoaded(int level)
	{
		audio = GetComponent<AudioSource>();//establish reference
		player = GameObject.Find("Player");	//establish reference
		if(level == Application.loadedLevel)
			FadeOutMusic();
	}

	/// <summary>
	///	Start is called once. It establishes the inter-object connections post-setup.
	/// <summary>
	void Start() {
		player = GameObject.Find("Player");	
	}
	
	/// <summary>
	/// Update is called once per frame. Here, it maintains the camera's position if locked.
	/// </summary>
	void Update () {
		if(horLock)
			transform.position = new Vector3(lockX, transform.position.y, transform.position.z);//keep latest position but with locked x coordinate
		if(verLock)
			transform.position = new Vector3(transform.position.x, lockY, transform.position.z);//keep latest position but with locked y coordinate
	}

	/// <summary>
	/// SetEffect is called when the player reaches a peg, and sets the lock and zoom effects of the camera
	/// </summary>
	/// <param name="peg">peg from which method is called</param>
	/// <param name="zoomEffect">type of zoom effect established at this peg</param>
	/// <param name="lockEffect">type of lock effect established at this peg</param>
	/// <param name="x">x coordinate of horizontal lock relative to <paramref name="peg"/>'s position</param>
	/// <param name="y">y coordinate of horizontal lock relative to <paramref name="peg"/>'s position</param>
	public void SetEffect(GameObject peg, ZoomEffect zoomEffect, LockEffect lockEffect, float x = 0, float y = 0)
	{

		//check for the zoom effect and zoom out or in if necessary
		switch(zoomEffect)
		{
		case ZoomEffect.DoNotChange://if zoom effect is DNC
			break;//do nothing
		case ZoomEffect.Standard:
			if(zoomedOut)
				ZoomIn();
			break;
		case ZoomEffect.Wide:
			if(!zoomedOut)
				ZoomOut();
			break;
		default:
			break;
		}

		//check for the lock effect and set the appropriate variables if necessary
		switch(lockEffect)
		{
		case LockEffect.DoNotChange:
			break;
		case LockEffect.NoLock:
			verLock = false;
			horLock = false;
			lockPeg = null;
			break;
		case LockEffect.HLock:
			verLock = false;
			horLock = true;
			if(peg != lockPeg)//avoid relocking
			{
				lockX = peg.transform.position.x + x;//get absolute locked x coordinate
				lockPeg = peg;
			}
			break;
		case LockEffect.VLock:
			verLock = true;
			horLock = false;
			if(peg != lockPeg)//avoid relocking
			{
				lockY = peg.transform.position.y + y;//get absolute locked y coordinate
				lockPeg = peg;
			}
			break;
		case LockEffect.FullLock:
			verLock = true;
			horLock = true;
			if(peg != lockPeg)
			{
				lockX = peg.transform.position.x + x;
				lockY = peg.transform.position.y + y;
				lockPeg = peg;
			}
			break;
		default:
			break;
		}
	}

	/// <summary>
	///	Calls coroutine to zoom in over a period of time.
	/// </summary>
	void ZoomIn()
	{
		StartCoroutine("ZoomI");
		zoomedOut = false;
	}
	IEnumerator ZoomI()
	{
		for(float z = -15.0f; z <= -10.0f; z += 0.1f)//iterate over camera's z coordinate
		{
			transform.position = new Vector3(transform.position.x, transform.position.y, z);//set new z coordinate
			yield return new WaitForSeconds(0.01f);//wait 10 ms
		}

	} 

	/// <summary>
	///	Analogous to ZoomIn
	/// </summary>
	void ZoomOut()
	{
		StartCoroutine("ZoomO");
		zoomedOut = true;
	}
	IEnumerator ZoomO()
	{
		print("Zooming out.");

		for(float z = -10.0f; z >= -15.0f; z -= 0.1f)
		{
			transform.position = new Vector3(transform.position.x, transform.position.y, z);
			yield return new WaitForSeconds(0.01f);
		}
	}

	/// <summary>
	///	Calls coroutine to fade music out over a period of time.
	/// </summary>
	public void FadeOutMusic()
	{
		StartCoroutine("FadeOut");
	}
	IEnumerator FadeOut()
	{
		float volMod = audio.volume;//store max volume
		yield return new WaitForSeconds(1.0f);//wait 1s
		for(float v = 1.0f; v > 0.3f; v-=0.02f)//from full down to 30% volume
		{
			audio.volume = volMod * v;//change volume relative to max
			yield return new WaitForSeconds(0.1f);//wait 100ms
		}

	}

	/// <summary>
	///	Calls coroutine to fade music in over a period of time.
	/// </summary>
	public void FadeInMusic()
	{
		StartCoroutine("FadeIn");
	}
	IEnumerator FadeIn()
	{
		float volMod = audio.volume / 0.3f;//calculate max volume based on FadeOut constants
		for(float v = 0.3f; v < 1.0f; v+=0.02f)//from 30% up to full max volume
		{
			audio.volume = volMod * v;//set volume relative to max
			yield return new WaitForSeconds(0.1f);//wait 100ms
		}

	}
}
