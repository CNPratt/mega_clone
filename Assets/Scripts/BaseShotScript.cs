using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseShotScript : MonoBehaviour
{
    public Rigidbody2D shot;
    public Vector2 direction;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        shot.velocity = direction;
    }
}
