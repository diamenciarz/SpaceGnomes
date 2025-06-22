using UnityEngine;

public class GunController : MonoBehaviour, IWeaponController
{
    [SerializeField] private string bulletPoolId = "PlasmaBullet"; // Pool ID for bullets
    [SerializeField] private Transform firePoint; // Where bullets spawn
    [SerializeField] private int maxAmmo = 10; // Maximum ammo capacity
    [SerializeField] private float replenishTimePerBullet = 0.5f; // Time to replenish one bullet
    [SerializeField] private bool waitForFullAmmo = false; // Wait for all ammo to replenish?
    [SerializeField] private float fireRate = 0.2f; // Time between shots

    private int currentAmmo;
    private float replenishTimer;
    private float fireTimer;
    private bool isShooting;

    private void Awake()
    {
        currentAmmo = maxAmmo;
        replenishTimer = 0f;
        fireTimer = 0f;
    }

    private void Update()
    {
        // Handle ammo replenishment
        if (currentAmmo < maxAmmo)
        {
            replenishTimer += Time.deltaTime;
            if (replenishTimer >= replenishTimePerBullet)
            {
                currentAmmo = Mathf.Min(currentAmmo + 1, maxAmmo);
                replenishTimer = 0f;
            }
        }

        // Handle shooting
        if (isShooting && CanShoot())
        {
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireRate)
            {
                Shoot();
                fireTimer = 0f;
            }
        }
    }

    public void SetShooting(bool isShooting)
    {
        this.isShooting = isShooting;
    }

    private bool CanShoot()
    {
        if (waitForFullAmmo)
        {
            return currentAmmo == maxAmmo;
        }
        return currentAmmo > 0;
    }

    private void Shoot()
    {
        if (firePoint == null)
        {
            Debug.LogWarning("Fire point not assigned!");
            return;
        }

        // Spawn bullet from pool
        ObjectPoolManager.Instance.Spawn(bulletPoolId, firePoint.position, firePoint.rotation);
        currentAmmo--;
    }
}