using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class RelaxTraining : MonoBehaviour {

    public Texture BackgroundTexture;
    Thread EEGThread; //constant running thread to read eeg data

    void Start() {
        //create a constantly working eeg signal thread
        EEGThread = new Thread(EEGLogger.SetThresholdR);
        EEGThread.Start();
        Debug.Log("Start relax training@!");
    }

    void OnGUI()
    {
        //display our background texture
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), BackgroundTexture);

        //TODO: play music
        //TODO: thread to collect data and compute threshold relax 
    }

    void OnExit() {
        EEGThread.Abort();
        Application.LoadLevel("TrainMenu");//SceneManager.LoadScene("TrainMenu");
    }

    void OnApplicationQuit() {
        EEGThread.Abort();
    }
}
