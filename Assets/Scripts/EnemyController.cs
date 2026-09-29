using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform position1;
    public Transform position2;

    public float velocity;

    private bool seguindoPos1 = true;
    private Rigidbody2D rb;

    public float minimumDistance;
    private float targetX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        targetX = position1.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        float personagemX = this.gameObject.transform.position.x;
        float distance = Mathf.Abs(personagemX-targetX);

        if (distance < minimumDistance)
        {
            if (seguindoPos1)
            {
                targetX = position2.position.x;
            }
            else
            {
                targetX = position1.position.x;
            }

            seguindoPos1 = !seguindoPos1;
        }

        if(personagemX>targetX){
            rb.linearVelocity = new Vector2(-velocity,
                                            rb.linearVelocity.y);
        }else{
            rb.linearVelocity = new Vector2(velocity,
                                            rb.linearVelocity.y);
        }
    }
}
