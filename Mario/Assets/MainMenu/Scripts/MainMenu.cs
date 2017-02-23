//attached to main camera

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public Texture BackgroundTexture;

    void OnGUI() {
        //display our background texture
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), BackgroundTexture);

        //float y_top = .2f;
        //float y_offset = .2f;

        //display buttons
        if (GUI.Button(new Rect(Screen.width * .4f, Screen.height * .3f, Screen.width * .2f, Screen.height * .1f), "Play")) {
            // In a function
            if (EEGLogger.SetThresholds)
            {
                print("Start Gaming!");
                SceneManager.LoadScene("Game");
            }
            else {

                var message = "Please conduct training first!";
                var title = "Threshold Not Found";
                EditorUtility.DisplayDialog(title,message, "Get it");
            }
        }

        if (GUI.Button(new Rect(Screen.width * .4f, Screen.height * .5f, Screen.width * .2f, Screen.height * .1f), "Train"))
        {
            print("Start Training!");
            SceneManager.LoadScene("TrainMenu");

        }

    }
}
