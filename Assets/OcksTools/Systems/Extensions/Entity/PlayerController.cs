using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rigid;
    public float move_speed = 2;
    public float decay = 0.8f;
    private Vector3 move = new Vector3(0, 0, 0);
    private void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        move *= decay;
        move += (Vector3)InputManager.GetWishDirection("Player");
        Vector3 bgalls = move * Time.deltaTime * move_speed * 20;
        rigid.linearVelocity += new Vector2(bgalls.x, bgalls.y);


    }
    private void Update()
    {
        if (MainCamera.Instance != null)
        {
            MainCamera.Instance.SetTargetPosition(transform.position);
        }
    }


}
