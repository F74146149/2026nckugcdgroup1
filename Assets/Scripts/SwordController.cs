using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    private Camera mainCamera;
    private SkillController skillController;
    private WeaponHitBox weaponHitBox;
    //近戰攻擊系統
    private bool isAttacking = false;
    public bool IsCurrentlyAttacking => isAttacking;
    private float attackTimer = 0f;
    //三連段攻擊調整
    [SerializeField]private int firstAttackDamage = 20;
    [SerializeField] private int secondAttackDamage = 25;
    [SerializeField] private int thirdAttackDamage = 40;
    public int CurrentDamage { get; private set; }

    [SerializeField] private float firstAttackDuration = 0.20f;
    [SerializeField] private float secondAttackDuration = 0.30f;
    [SerializeField] private float thirdAttackDuration = 0.50f;
    private float currentAttackDuration;

    public float angle;
    private float startAngle = 0f;

    [SerializeField]private int maxComboStep = 3;
    [SerializeField]private float comboResetDuration = 0.5f;
    private int comboStep = 0;
    private float comboResetTimer = 0f;
    private bool comboQueued = false;
    [SerializeField]private float comboInputWindow = 0.15f;

    void Start()
    {
        mainCamera = Camera.main;
        skillController = GetComponentInParent<SkillController>();
        weaponHitBox = GetComponentInChildren<WeaponHitBox>();
    }

    
    void Update()
    {

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCamera.transform.position.z;
        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);

        Vector2 direction = (Vector2)(worldMousePosition - transform.position);
        angle = Mathf.Atan2(direction.y, direction.x)*Mathf.Rad2Deg;

        if (angle < 0)
        {
            angle += 360f;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (Input.GetMouseButtonDown(0))
        {
            if(!isAttacking && !skillController.isGunMode)
            {
                if (comboStep == 0)
                {
                    comboStep = 1;
                }
                else
                {
                    comboStep++;
                }
                
                StartAttack();

                
            } else if (comboStep < maxComboStep && attackTimer >= currentAttackDuration - comboInputWindow)
            {
                comboQueued = true;

                Debug.Log("Next Attack Queued!");
            }
            
        }

        if(isAttacking)
        {
            AttackRotation();
        }

        if (!isAttacking && comboResetTimer > 0f)
        {
            comboResetTimer -= Time.deltaTime;
            
            if(comboResetTimer <= 0)
            {
                comboStep = 0;
                Debug.Log("combo reset!");
            }
            
        }
    }

    void AttackRotation()
    {
        attackTimer += Time.deltaTime;

        float swingStartOffset = 60f;
        float swingEndOffset = -60f;

        switch(comboStep)
        {
            case 1:
                swingStartOffset = 60f;
                swingEndOffset = -60f;
                break;

            case 2:
                swingStartOffset = -60f;
                swingEndOffset = 60f;
                break;

            case 3:
                swingStartOffset = 90f;
                swingEndOffset = -90f;
                break;
        }

        float progress = Mathf.Clamp01(attackTimer / currentAttackDuration);

        float currentAngle = Mathf.Lerp(startAngle + swingStartOffset, startAngle + swingEndOffset, progress);

        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

        if (attackTimer >= currentAttackDuration)
        {
            isAttacking = false;

            if (comboStep >= maxComboStep)
            {
                comboStep = 0;
                comboQueued = false;
                comboResetTimer = 0f;

                Debug.Log("Combo finished!");
            }
            else if(comboQueued)
            {
                comboStep++;

                comboQueued = false;

                Debug.Log("Combo Step: " + comboStep);

                StartAttack();
            } 
            else
            {
                comboResetTimer = comboResetDuration;
            }

        }
    }

    void StartAttack()
    {
        isAttacking = true;
        attackTimer = 0f;
        startAngle = angle;

        comboResetTimer = 0f;

        weaponHitBox.ResetHitEnemies();


        switch (comboStep)
        {
            case 1:
                currentAttackDuration = firstAttackDuration;
                CurrentDamage = firstAttackDamage;
                break;

            case 2:
                currentAttackDuration = secondAttackDuration;
                CurrentDamage = secondAttackDamage;
                break;

            case 3:
                currentAttackDuration = thirdAttackDuration;
                CurrentDamage = thirdAttackDamage;
                break;
        }
    }
}
