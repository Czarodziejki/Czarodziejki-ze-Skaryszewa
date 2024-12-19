using UnityEngine;

public class WeaponWithLimitedAmmo: BaseWeapon
{
    public int maxAmmo;
	private int currentAmmo;

	protected new void Start()
    {
		currentAmmo = maxAmmo;

        base.Start();
    }


    public override bool TryToUseWeapon()
    {
        if (currentAmmo == 0)
        {
            playerTransform.GetComponent<ShootingController>().EquipWeapon<DefaultWeapon>();
			return false;
        }

        if (base.TryToUseWeapon())
        {
            --currentAmmo;

            return true;
        }

        return false;
    }
}
