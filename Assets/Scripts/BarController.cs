using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class BarController : NetworkBehaviour
{
    private Slider slider;

    public void Start()
    {
        slider = GetComponentInChildren<Slider>();
    }


    public void SetValue(float value)
    {
        slider.value = value;
    }

}
