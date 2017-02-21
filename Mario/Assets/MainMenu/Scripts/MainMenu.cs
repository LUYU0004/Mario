//attached to main camera

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public Texture BackgroundTexture;

    void OnGUI() {
        //display our background texture
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), BackgroundTexture);

        //display buttons
        if (GUI.Button(new Rect(Screen.width * .4f, Screen.height * .3f, Screen.width * .2f, Screen.height * .1f), "Play")){
            print("Let's Start!");
            // In a function
            SceneManager.LoadScene("Game");
        }

        if (GUI.Button(new Rect(Screen.width * .4f, Screen.height * .5f, Screen.width * .2f, Screen.height * .1f), "Train"))
        {
            print("Let's Start!");
        }

    }
}
