using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class WeaponUIController : MonoBehaviour
{
    public Sprite baseWeaponImage;
    public Sprite fastWeaponImage;
    public Sprite sniperWeaponImage;
    public Sprite aoeWeaponImage;

    private GameObject weaponIcon;
    private TMP_Text ammoAmount;

    private void Awake()
    {
        weaponIcon = GameObject.Find("WeaponIcon");
        ammoAmount = GameObject.Find("AmmoAmount").GetComponent<TMP_Text>();
    }


    public void UpdateWeaponType(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.DefaultWeapon:
                weaponIcon.GetComponent<Image>().sprite = baseWeaponImage;
                break;

            case WeaponType.FastWeapon:
                weaponIcon.GetComponent<Image>().sprite = fastWeaponImage;
                break;

            case WeaponType.SniperWeapon:
                weaponIcon.GetComponent<Image>().sprite = sniperWeaponImage;
                break;

            case WeaponType.AOEWeapon:
                weaponIcon.GetComponent<Image>().sprite = aoeWeaponImage;
                break;
        }
    }

    public void UpdateAmmo(BaseWeapon weapon)
    {
        if (weapon is WeaponWithLimitedAmmo w)
            ammoAmount.text = w.currentAmmo.ToString();
        else
            ammoAmount.text = "Inf.";
    }
}
