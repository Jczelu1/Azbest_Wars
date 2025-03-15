using UnityEngine;

public class CameraMover : MonoBehaviour
{
    // Time interval between moves in seconds
    public float moveInterval = 0.25f;
    private float timer = 0f;
    private int horizontal = 0;
    private int vertical = 0;
    private bool moving = false;

    void Update()
    {
        timer += Time.deltaTime;
        int newHorizontal = (int)Input.GetAxisRaw("Horizontal");
        int newVertical = (int)Input.GetAxisRaw("Vertical");
        if (!moving)
        {
            if (newHorizontal != 0)
            {
                horizontal = newHorizontal;
            }
            if (newVertical != 0)
            {
                vertical = newVertical;
            }
        }
        if (timer >= moveInterval)
        {
            if (moving)
            {
                horizontal = newHorizontal;
                vertical = newVertical;
            }
            moving = false;
            if (horizontal != 0 || vertical != 0)
            {
                moving = true;

                Vector3 move = new Vector3(horizontal, vertical, 0);

                if (move.magnitude > 1)
                    move.Normalize();

                transform.Translate(move, Space.World);
                horizontal = 0;
                vertical = 0;
            }
            timer = 0f;
        }
    }
}
