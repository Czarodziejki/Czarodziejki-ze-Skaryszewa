using UnityEngine;


public class SpectatorController : BasePlayerController
{
    public float movementSpeedCoef = 0.3f;

    private bool activated = false;


    void Update()
    {
        if (!isLocalPlayer || !activated)
            return;

        Vector2 movement = moveAction.ReadValue<Vector2>();
        var transform = GetComponent<Transform>();

        transform.position += new Vector3(movement.x, movement.y) * movementSpeedCoef;
    }

    public void OnGUI()
    {
        if (!isLocalPlayer)
            return;

        if (activated)
        {
            GUILayout.BeginArea(new Rect(10, 10f, 140f, 40f));
            GUI.Box(new Rect(10, 10, 100, 30), "Spectator mode");
            GUILayout.EndArea();
        }
        else
        {
            float messageWidth = 200;
            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize = 40
            };

            GUI.Box(new Rect((Screen.width-messageWidth)/2.0f, 20, messageWidth, 50), "You died", style);

            float buttonWidth = 120;
            if (GUI.Button(new Rect((Screen.width-buttonWidth)/2.0f, 85, 100, 30), "Spectator mode"))
                activated = true;
        }
    }
}