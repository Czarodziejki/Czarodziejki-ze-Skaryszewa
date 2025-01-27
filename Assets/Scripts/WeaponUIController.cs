using UnityEngine;
using UnityEngine.UI;


public class WeaponUIController : MonoBehaviour
{
    public Sprite baseWeaponImage;
    public Sprite fastWeaponImage;
    public Sprite sniperWeaponImage;

    private GameObject weaponIcon;

    private void Awake()
    {
        weaponIcon = GameObject.Find("WeaponIcon");
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
        }

        
    }
}
