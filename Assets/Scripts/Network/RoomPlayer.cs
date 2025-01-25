using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RoomPlayer : NetworkRoomPlayer
{
    [SyncVar]
    public int ColorID;

    [SyncVar]
    public bool showResults = false;
    [SyncVar]
    public int place = -1;
    private InputAction fireAction;

    static bool localShowResults = false;

    private void Start()
    {
        base.Start();
        if (isLocalPlayer)
        {
            fireAction = InputSystem.actions.FindAction("Attack");
        }
    }

    public void Update()
    {
        if (isLocalPlayer)
        {
            if(showResults && fireAction.IsPressed())
                showResults = false;

            localShowResults = showResults;
        }
    }

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

            if (localShowResults)
            {
                DrawResults();
            }
            else
            {
                DrawPlayerState();
                if (NetworkClient.active && isLocalPlayer)
                {
                    DrawPlayerSelection();
                    DrawPlayerReadyButton();
                }
            }
        }
    }

    void DrawPlayerState()
    {
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 70;
        labelStyle.font = Resources.Load<Font>("Fonts/VT323-Regular");
        labelStyle.normal.textColor = Color.black;
        labelStyle.alignment = TextAnchor.MiddleCenter;

        float areaHeight = Screen.height * 0.2f;
        float areaWidth = Screen.width * 0.2f;
        float spacing = Screen.width * 0.05f;

        GUIStyle area = new GUIStyle();
        if (readyToBegin)
            area.normal.background = MakeTex(2, 2, new Color(0.439f, 1.0f, 0.518f));
        else
            area.normal.background = MakeTex(2, 2, new Color(0.831f, 0.071f, 0.071f));

        GUILayout.BeginArea(new Rect((index + 1) * spacing + (index * areaWidth), Screen.height * 0.1f, areaWidth, areaHeight));
        GUILayout.BeginVertical(area);
        GUILayout.Label($"Player {index + 1}", labelStyle, GUILayout.ExpandWidth(true));

        if (((isServer && index > 0) || isServerOnly) && GUILayout.Button("REMOVE", new GUIStyle(GUI.skin.button)
        {
            fontSize = 50,
            font = Resources.Load<Font>("Fonts/VT323-Regular"),
        }, GUILayout.ExpandWidth(true), GUILayout.Height(areaHeight * 0.2f)))
        {
            GUILayout.Space(50);
            GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
        }
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    void DrawPlayerReadyButton()
    {
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 100;
        buttonStyle.font = Resources.Load<Font>("Fonts/VT323-Regular");
        buttonStyle.normal.textColor = Color.black;
        buttonStyle.hover.textColor = Color.black;
        buttonStyle.active.textColor = Color.black;
        buttonStyle.normal.background = MakeTex(2, 2, new Color(0.9607844f, 0.7294118f, 0.9215687f));
        buttonStyle.hover.background = MakeTex(2, 2, new Color(1f, 0.8820755f, 0.980345f));
        buttonStyle.active.background = MakeTex(2, 2, new Color(0.9811321f, 0.513706f, 0.9019072f));
        buttonStyle.alignment = TextAnchor.MiddleCenter;

        if (NetworkClient.active && isLocalPlayer)
        {
            GUILayout.BeginArea(new Rect(Screen.width * 0.25f, Screen.height * 0.8f, Screen.width * 0.5f, Screen.height * 0.2f));
            if (readyToBegin)
            {
                if (GUILayout.Button("CANCEL", buttonStyle, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(100)))
                    CmdChangeReadyState(false);
            }
            else
            {
                if (GUILayout.Button("READY", buttonStyle, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(100)))
                    CmdChangeReadyState(true);
            }

            GUILayout.EndArea();
        }
    }

    void DrawPlayerSelection()
    {
        GameNetworkManager gameManager = NetworkManager.singleton as GameNetworkManager;

        float size = Screen.height * 0.4f;
        float arrowsHeight = Screen.height * 0.05f;
        float spacing = 20.0f;

        GUILayout.BeginArea(new Rect(Screen.width * 0.5f - size / 2.0f, Screen.height * 0.5f - size / 2.0f, size, size + spacing + arrowsHeight));
        if (gameManager.playerTextures[ColorID] != null)
        {
            // Stretch the texture to fill the whole box (200x200)
            GUI.DrawTexture(new Rect(0, 0, size, size), gameManager.playerTextures[ColorID], ScaleMode.StretchToFill);
        }
        GUILayout.EndArea();

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 36;
        buttonStyle.font = Resources.Load<Font>("Fonts/VT323-Regular");
        buttonStyle.normal.textColor = Color.black;
        buttonStyle.hover.textColor = Color.black;
        buttonStyle.active.textColor = Color.black;
        buttonStyle.normal.background = MakeTex(2, 2, new Color(0.9607844f, 0.7294118f, 0.9215687f));
        buttonStyle.hover.background = MakeTex(2, 2, new Color(1f, 0.8820755f, 0.980345f));
        buttonStyle.active.background = MakeTex(2, 2, new Color(0.9811321f, 0.513706f, 0.9019072f));
        buttonStyle.alignment = TextAnchor.MiddleCenter;

        GUILayout.BeginArea(new Rect(Screen.width * 0.5f - size / 2.0f, Screen.height * 0.5f - size / 2.0f + size + spacing, size, arrowsHeight));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<", buttonStyle, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(arrowsHeight)))
        {
            int colorID = (ColorID - 1 + gameManager.playerTextures.Length) % gameManager.playerTextures.Length;
            CmdChangeColorID(colorID);
        }
        if (GUILayout.Button(">", buttonStyle, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(arrowsHeight)))
        {
            int colorID = (ColorID + 1) % gameManager.playerTextures.Length;
            CmdChangeColorID(colorID);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    public void ChangeColor(int direction)
    {
        GameNetworkManager gameManager = NetworkManager.singleton as GameNetworkManager;
        if (!gameManager) return;
        int colorID = (ColorID + direction + gameManager.playerTextures.Length) % gameManager.playerTextures.Length;
        CmdChangeColorID(colorID);
    }

    public void MainMenuButton()
    {
        GameNetworkManager gameManager = NetworkManager.singleton as GameNetworkManager;
        gameManager.ReturnToMainMenu();
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    [Command]
    public void CmdChangeColorID(int newColorID)
    {
        ColorID = newColorID;
    }

    private void DrawResults()
    {
        if (!isLocalPlayer) return;

        string resultText = "";
        switch (place)
        {
            case 1:
                resultText = "You won!!!";
                break;
            case 2:
                resultText = "2nd place";
                break;
            case 3:
                resultText = "3rd place";
                break;
            case 4:
                resultText = "4th place";
                break;
        }

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 70;
        labelStyle.font = Resources.Load<Font>("Fonts/VT323-Regular");
        labelStyle.normal.textColor = Color.black;
        labelStyle.alignment = TextAnchor.MiddleCenter;

        GUIStyle area = new GUIStyle();
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.Label(resultText, labelStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUILayout.EndArea();
    }
}
