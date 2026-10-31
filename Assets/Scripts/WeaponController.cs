using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private Camera mainCamera;

    private bool isAttacking = false;
    public bool IsCurrentlyAttacking => isAttacking;
    private float attackTimer = 0f;
    private float attackDuration = 0.25f;
    private float startAngle = 0f;
    void Start()
    {
        mainCamera = Camera.main;
    }

    
    void Update()
    {

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCamera.transform.position.z;
        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);

        Vector2 direction = (Vector2)(worldMousePosition - transform.position);
        float angle = Mathf.Atan2(direction.y, direction.x)*Mathf.Rad2Deg;

        if (angle < 0)
        {
            angle += 360f;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            isAttacking = true;
            attackTimer = 0f;
            startAngle = angle;
        }

        if(isAttacking)
        {
            attackTimer += Time.deltaTime;
            float currentAngle = Mathf.Lerp(startAngle + 60, startAngle - 60, attackTimer/attackDuration);
            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

            if (attackTimer >= attackDuration)
            {
                isAttacking = false;
                Debug.Log("Attack Finished!");
            }
        }
    }

}
