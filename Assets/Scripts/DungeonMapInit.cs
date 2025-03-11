using Mirror;
using UnityEngine;

class DungeonMapInit : NetworkBehaviour
{
    public AudioClip music;
    private void Start()
    {
        GameObject light = GameObject.Find("Global Light 2D");
        if (light != null)
            light.SetActive(false);

        GameObject musicPlayer = GameObject.Find("Basic music player");
        if (musicPlayer != null)
        {
            var audio = musicPlayer.GetComponent<AudioSource>();
            audio.resource = music;
            audio.Play();
        }        
    }

}