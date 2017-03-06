using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.SceneManagement;
using System.Threading;

public class WaitingForPlugin : MonoBehaviour {


    public Texture BackgroundTexture;
    Thread WaitForUser;
    EEGLogger logger;

    void Awake()
    {
        EEGLogger logger = new EEGLogger();
        Debug.Log("WAITING!");
        //WaitForUser = new Thread(EEGLogger.WaitingForUser);
        //WaitForUser.Start();
        int counter = 100;

        for (; counter > 0; counter--) {
            Debug.Log("Main");
            Thread.Sleep(1000);
        }
        //WaitForUser.Abort();
        Debug.Log("Abort wait!");

    }


    void OnGUI()
    {
        //display our background texture
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), BackgroundTexture);

        GUIStyle fontStyle = new GUIStyle();
        fontStyle.fontSize = 25;
        //var centeredStyle = GUI.skin.GetStyle("Label");
        fontStyle.alignment = TextAnchor.UpperCenter;

        GUIStyle fontStyle2 = new GUIStyle();
        fontStyle2.fontSize = 20;
        //var centeredStyle = GUI.skin.GetStyle("Label");
        fontStyle2.alignment = TextAnchor.UpperCenter;

        GUI.Label(new Rect(Screen.width * .3f, Screen.height * .1f, Screen.width * .4f, Screen.height * .2f), "Welcome To RunYourMind!", fontStyle);
        GUI.Label(new Rect(Screen.width * .3f, Screen.height * .3f, Screen.width * .4f, Screen.height * .2f), "Please plug in the headset!", fontStyle2);
    }



}
