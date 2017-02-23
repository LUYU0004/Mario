using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/*GameController:
 * 1) Game floating display in the top bar
 * 2) Start and monitor eeg signal input
 * 3) 
     */
public class GameController : MonoBehaviour {

   // private enum GameModes : byte { pre = 1, relax, attentionLow, attentionHigh, gamePlaying};
    //private int gameMode = 0;
    
    private static int score;
    private float lifeRemaining = 5;
    private float startTime;
    private string textTime; //added this member variable here so we can access it through other scripts

    //EEGLogger logger;
    Thread EEGThread; //constant running thread to read eeg data
    Player player;

    void Awake() {
    }

    void OnGUI()
    {
        var guiTime = Time.time - startTime;


        int minutes= (int)guiTime / 60;
        int seconds = (int)guiTime % 60;
        int fraction = (int)(guiTime * 100) % 100;


        textTime = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, fraction);
        GUI.Label(new Rect(650, 25, 100, 30), "Time: "+textTime); //changed variable name to textTime -->text is not a good variable name since it has other use already
        GUI.Label(new Rect(450, 25, 100, 30), "Life: "+lifeRemaining.ToString());
        GUI.Label(new Rect(250, 25, 150, 30), "Speed: "+Player.CurrentSpeed.ToString("0.00")+" \\ "+Player.MaxSpeed);
        GUI.Label(new Rect( 50, 25, 100, 30), "Scores: "+score.ToString());

    }

    void Start() {

        score = 0;
        startTime = Time.time;

        //create a constantly working eeg signal thread
        EEGThread = new Thread(EEGLogger.OnRetrieveData);
        EEGThread.Name = "EEGThread";
        EEGThread.Start();
    }

    void UpdateScore() {
        //TODO: compute score        
    }

    public static void AddScore(int points) {
        score += points;
    }

    void OnApplicationQuit()
    {
        EEGThread.Abort();
        //EEGThread.Abort();
    }

    /*
     * private scoreValue = 10;
     * private GameController gameController; 
    //...start(){
    GameObject gameControllerObject = GameObject.FindingWithTag("GameController");
    if(gameControllerObject != null){
    gameController = gameControllerObject.GetComponent<GameController>();}
    
    if(gameController==null){
    Debug.Log("Can not find 'GmaeController' script");
    }
    onTriggerEnter(){...
    gameController.AddScore(scoreValue);
    */

}
