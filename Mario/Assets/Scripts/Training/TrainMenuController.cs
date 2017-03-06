using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TrainMenuController : MonoBehaviour {

    public Texture BackgroundTexture;

    void OnGUI()
    {
        //display our background texture
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), BackgroundTexture);

        //float y_top = .2f;
        //float y_offset = .2f;

        //display buttons
        if (GUI.Button(new Rect(Screen.width * .4f, Screen.height * .3f, Screen.width * .2f, Screen.height * .1f), "Relax"))
        {
            print("Start Relaxing!");
            Application.LoadLevel("RelaxTraining");
            //RelaxTraining relaxTrain = new RelaxTraining();
            // In a function
            // SceneManager.LoadScene("Game");
        }

        if (GUI.Button(new Rect(Screen.width * .4f, Screen.height * .5f, Screen.width * .2f, Screen.height * .1f), "Focus"))
        {
            print("Start Focusing!");
            //SceneManager.LoadScene("Game");
        }

    }
}
