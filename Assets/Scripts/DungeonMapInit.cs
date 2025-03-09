using Mirror;
using UnityEngine;

class DungeonMapInit : NetworkBehaviour
{
    private void Start()
    {
        GameObject light = GameObject.Find("Global Light 2D");
        if (light != null)
            light.SetActive(false);
    }

}