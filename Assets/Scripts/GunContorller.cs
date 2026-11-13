using Unity.VisualScripting;
using UnityEngine;

public class GunContorller : MonoBehaviour
{
    private Camera mainCamera;
    public Transform firePoint;
    public GameObject projectilePrefab;
    private SkillController skillController;
    public float ProjectileSpeed = 12f;

    public float angle;
    
    
    void Start()
    {
        mainCamera = Camera.main;
        skillController = GetComponentInParent<SkillController>();
    }


    void Update()
    {

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCamera.transform.position.z;
        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);

        Vector2 direction = (Vector2)(worldMousePosition - transform.position);
        angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle < 0)
        {
            angle += 360f;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if(Input.GetMouseButtonDown(0) && skillController.IsGunMode)
        {
            Shoot();
            skillController.ReturnToSword();
        }
    }

    void Shoot()
    {
        GameObject newProjectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        ProjectilContoller projectilContoller = newProjectile.GetComponent<ProjectilContoller>();

        projectilContoller.Launch(firePoint.right, ProjectileSpeed);
    }

}
