using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    public float speed = 3f;

    private Rigidbody2D rb;
    private Vector2 direction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        direction = Vector2.zero;
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.wKey.isPressed) direction.y += 1;
        if (keyboard.sKey.isPressed) direction.y -= 1;
        if (keyboard.dKey.isPressed) direction.x += 1;
        if (keyboard.aKey.isPressed) direction.x -= 1;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = direction.normalized * speed;
    }
}