using UnityEngine;

public class SkillController : MonoBehaviour
{
    public GameObject sword;
    public GameObject gun;

    public bool isGunMode = false;
    public bool IsGunMode => isGunMode;

    void Start()
    {
        isGunMode = false;
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E)&&!isGunMode)
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
    }
}
