using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PointStar: MonoBehaviour
{

    public GameObject Effect;
    public int ScoresToAdd = 10;

    public void OnTriggerEnter2D(Collider2D other) {

        if (other.GetComponent<Player>() == null) // if the collider is not a player, exit
            return;

        GameController.AddScore(ScoresToAdd);
        Instantiate(Effect, transform.position, transform.rotation); // create the prefab in Effect at the specified location

        gameObject.SetActive(false); //do not destry the star, but set it not active; so if the player reborn at the checkpoint, it is re-activated

    }
}
