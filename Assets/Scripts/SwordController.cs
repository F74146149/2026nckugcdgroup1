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
    [SerializeField] private WeaponData weaponData;
   
    private AttackData currentAttack;

    public int CurrentDamage { get; private set; }

    private float currentAttackDuration;

    public float angle;
    private float startAngle = 0f;

    private int MaxComboStep =>
    weaponData != null && weaponData.ComboAttacks != null
    ? weaponData.ComboAttacks.Length
    : 0;

    //[SerializeField]private float comboResetDuration = 0.5f;
    private int comboStep = 0;
    private float comboResetTimer = 0f;
    private bool comboQueued = false;
    //[SerializeField]private float comboInputWindow = 0.15f;

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

        if (Input.GetMouseButtonDown(0) && !skillController.IsGunMode)
        {
            if(!isAttacking)
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

                
            } else if (comboStep < MaxComboStep && 
                attackTimer >= currentAttackDuration - weaponData.ComboInputWindow)
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

        float progress = Mathf.Clamp01(attackTimer / currentAttackDuration);

        float currentAngle = Mathf.Lerp
            (startAngle + currentAttack.startAngle, startAngle + currentAttack.endAngle, progress);

        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

        if (attackTimer >= currentAttackDuration)
        {
            isAttacking = false;

            if (comboStep >= MaxComboStep)
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
                comboResetTimer = weaponData.ComboResetDuration;
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

        if (weaponData == null ||
        weaponData.ComboAttacks == null ||
        weaponData.ComboAttacks.Length == 0)
        {
            Debug.LogError("尚未設定有效的 WeaponData！");
            return;
        }

        if (comboStep < 1 || comboStep > MaxComboStep)
        {
            Debug.LogError("Combo Step 超出 AttackData 範圍！");
            return;
        }

        currentAttack = weaponData.ComboAttacks[comboStep - 1];
        currentAttackDuration = currentAttack.duration;
        CurrentDamage = currentAttack.damage;

    }

    public void SetWeaponData(WeaponData newWeaponData)
    {
        if (newWeaponData == null)
        {
            Debug.LogError("傳入的 WeaponData 為空！");
            return;
        }

        weaponData = newWeaponData;

        isAttacking = false;
        attackTimer = 0f;
        comboStep = 0;
        comboResetTimer = 0f;
        comboQueued = false;

        Debug.Log("目前裝備武器：" + weaponData.WeaponName);
    }
}


