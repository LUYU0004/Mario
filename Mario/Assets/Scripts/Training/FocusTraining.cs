//using UnityEngine.SceneManagement;
using UnityEngine;

public class FocusTraining : MonoBehaviour
{

    public Texture BackgroundTexture;

    void OnGUI()
    {
        //display our background texture
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), BackgroundTexture);

        //TODO: play IQ test
        //TODO: thread to collect data and compute threshold focus

    }
}
