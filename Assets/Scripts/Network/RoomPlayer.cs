using Mirror;
using UnityEngine;

public class RoomPlayer : NetworkRoomPlayer
{
    [SyncVar]
    public int ColorID;

    public override void OnGUI()
    {
        if (!showRoomGUI)
            return;

        NetworkRoomManager room = NetworkManager.singleton as NetworkRoomManager;
        if (room)
        {
            if (!room.showRoomGUI)
                return;

            if (!Utils.IsSceneActive(room.RoomScene))
                return;

            DrawPlayerState();
            if (NetworkClient.active && isLocalPlayer)
            {
                DrawPlayerSelection();
                DrawPlayerReadyButton();
            }
        }
    }

    void DrawPlayerState()
    {
        float areaHeight = Screen.height * 0.2f;
        float areaWidth = Screen.width * 0.2f;
        float spacing = Screen.width * 0.05f;
        
        GUILayout.BeginArea(new Rect((index + 1) * spacing + (index * areaWidth), Screen.height * 0.1f, areaWidth, areaHeight));
        GUILayout.Label($"Player [{index + 1}]", new GUIStyle(GUI.skin.label)
        {
            fontSize = 36,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
        }, GUILayout.ExpandWidth(true));
        GUIStyle readyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };
        if (readyToBegin)
            GUILayout.Label("Ready", readyStyle, GUILayout.ExpandWidth(true));
        else
            GUILayout.Label("Not Ready", readyStyle, GUILayout.ExpandWidth(true));

        if (((isServer && index > 0) || isServerOnly) && GUILayout.Button("REMOVE", GUILayout.ExpandWidth(true)))
        {
            //GUILayout.Space(100);
            GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
        }
        GUILayout.EndArea();
    }

    void DrawPlayerReadyButton()
    {
        if (NetworkClient.active && isLocalPlayer)
        {
            GUILayout.BeginArea(new Rect(Screen.width * 0.25f, Screen.height * 0.8f, Screen.width * 0.5f, Screen.height * 0.2f));
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 48,
                alignment = TextAnchor.MiddleCenter
            };
            if (readyToBegin)
            {
                if (GUILayout.Button("Cancel", buttonStyle, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(100)))
                    CmdChangeReadyState(false);
            }
            else
            {
                if (GUILayout.Button("Ready", buttonStyle, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(100)))
                    CmdChangeReadyState(true);
            }

            GUILayout.EndArea();
        }
    }

    void DrawPlayerSelection()
    {
        GameNetworkManager gameManager = NetworkManager.singleton as GameNetworkManager;
        
        float size = Screen.height * 0.4f;

        GUILayout.BeginArea(new Rect(Screen.width * 0.5f - size / 2.0f, Screen.height * 0.5f - size / 2.0f, size, size));
        if (gameManager.playerTextures[ColorID] != null)
        {
            // Stretch the texture to fill the whole box (200x200)
            GUI.DrawTexture(new Rect(0, 0, size, size), gameManager.playerTextures[ColorID], ScaleMode.StretchToFill);
        }
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(Screen.width * 0.5f - size / 2.0f, Screen.height * 0.5f + size / 2.0f, size, Screen.height * 0.05f));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(50)))
        {
            int colorID = (ColorID - 1 + gameManager.playerTextures.Length) % gameManager.playerTextures.Length;
            CmdChangeColorID(colorID);
        }
        if (GUILayout.Button(">", GUILayout.ExpandWidth(true), GUILayout.MaxHeight(50)))
        {
            int colorID = (ColorID + 1) % gameManager.playerTextures.Length;
            CmdChangeColorID(colorID);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    [Command]
    public void CmdChangeColorID(int newColorID)
    {
        ColorID = newColorID;
    }
}
