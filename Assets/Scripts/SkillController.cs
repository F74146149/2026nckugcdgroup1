using UnityEngine;

public class SkillController : MonoBehaviour
{
    public GameObject sword;
    public GameObject gun;

    public bool isGunMode = false;
    public bool IsGunMode => isGunMode;

    [SerializeField]private float gunCooldown = 3f;
    private float cooldownTimer = 0;
    
    void Start()
    {
        isGunMode = false;
    }
    void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if(Input.GetKeyDown(KeyCode.E) && !isGunMode && cooldownTimer <= 0f)
        {
            SwitchToGun();
        }
    }

    void SwitchToGun()
    {
        isGunMode = true;

        sword.SetActive(false);
        gun.SetActive(true);
    }

    public void ReturnToSword()
    {
        isGunMode = false;

        sword.SetActive(true);
        gun.SetActive(false);

        cooldownTimer = gunCooldown;
    }
}
