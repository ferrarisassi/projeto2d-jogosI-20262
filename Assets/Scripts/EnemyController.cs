using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform position1;
    public Transform position2;

    private Vector3 targetPos;

    public float velocity;

    private bool seguindoPos1 = true;
    private Rigidbody2D rb;

    [SerializeField]
    private float minDist = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        targetPos = position1.position;
    }

    // Update is called once per frame
    void Update()
    {
        
        float dist = Vector3.Distance(this.transform.position, targetPos);
        if (dist < minDist)
        {
            seguindoPos1 = !seguindoPos1;
            if (seguindoPos1)
            {
                targetPos = position1.position;
            }
            else
            {
                targetPos = position2.position;
            }
        }

        if(this.gameObject.transform.position.x>targetPos.x){
            rb.linearVelocity = new Vector2(-velocity,
                                            rb.linearVelocity.y);
        }else{
            rb.linearVelocity = new Vector2(velocity,
                                            rb.linearVelocity.y);
        }
    }
}
