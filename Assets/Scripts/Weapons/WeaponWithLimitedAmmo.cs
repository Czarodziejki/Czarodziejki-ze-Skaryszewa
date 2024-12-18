using UnityEngine;

public class WeaponWithLimitedAmmo: BaseWeapon
{
    public int actualAmmo { get; private set; }
    public int maxAmmo;


    protected new void Start()
    {
        actualAmmo = maxAmmo;

        base.Start();
    }


    public override bool TryToUseWeapon()
    {
        if (actualAmmo > 0 && base.TryToUseWeapon())
        {
            --actualAmmo;

            return true;
        }

        return false;
    }
}
