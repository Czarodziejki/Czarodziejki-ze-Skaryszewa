using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;


public class RoomPlayer : NetworkRoomPlayer
{
    public enum DisplayType
    {
        DisplayPlayerSelection,
        DisplayMapSelection,
        DisplayGameResults,
    }

    [SyncVar]
    public int VariantID = 0;

    [SyncVar]
    bool variantAvaliable;

    [SyncVar]
    public DisplayType displayType;
    static private DisplayType localDisplayType;

    [SyncVar]
    public int[] orderedPlayersVariants;
    public GameObject lobbyCanvas;

    [SyncVar]
    public bool characterSelected = false;

    public override void OnClientEnterRoom()
    {
        if (isLocalPlayer)
        {
            lobbyCanvas = GameObject.Find("LobbyCanvas");
        }
    }

    public void Update()
    {
        if (isLocalPlayer)
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame && displayType == DisplayType.DisplayGameResults)
                displayType = DisplayType.DisplayPlayerSelection;

            localDisplayType = displayType;
            if(lobbyCanvas != null)
                lobbyCanvas.SetActive(localDisplayType == DisplayType.DisplayPlayerSelection);
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

            switch (localDisplayType)
            {
                case DisplayType.DisplayPlayerSelection:
                    if(isLocalPlayer)
                        CmdCheckVariantAvaliable();
                    DrawPlayerState();
                    if (NetworkClient.active && isLocalPlayer)
                    {
                        DrawPlayerSelection();
                        DrawPlayerReadyButton();
                    }
                    break;

                case DisplayType.DisplayMapSelection:
                    DrawMapSelection();
                    break;

                case DisplayType.DisplayGameResults:
                    DrawResults();
                    break;
            }
        }
    }

    void DrawPlayerState()
    {
        GameNetworkManager manager = NetworkManager.singleton as GameNetworkManager;
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 70;
        labelStyle.font = Resources.Load<Font>("Fonts/VT323-Regular");
        labelStyle.normal.textColor = Color.black;
        labelStyle.hover.textColor = Color.black;
        labelStyle.alignment = TextAnchor.MiddleCenter;

        float areaHeight = Screen.height * 0.13f;
        float areaWidth = Screen.width * 0.2f;
        float side_spacing = Screen.width * 0.05f;
        float inner_spacing = (Screen.width - 2.0f * side_spacing - 4.0f * areaWidth) / 3.0f;

        GUIStyle area = new GUIStyle();
        if (characterSelected)
            area.normal.background = MakeTex(2, 2, new Color(0.439f, 1.0f, 0.518f));
        else
            area.normal.background = MakeTex(2, 2, new Color(1f, 0.416f, 0.416f));

        GUILayout.BeginArea(new Rect(side_spacing + index * (inner_spacing + areaWidth), Screen.height * 0.1f, areaWidth, areaHeight));
        GUILayout.BeginVertical(area);
        GUILayout.Label(manager.playerNames[VariantID], labelStyle, GUILayout.ExpandWidth(true));
        GUILayout.Space(10);
        if (((isServer && index > 0) || isServerOnly) && GUILayout.Button("REMOVE", new GUIStyle(GUI.skin.button)
        {
            fontSize = 50,
            font = Resources.Load<Font>("Fonts/VT323-Regular"),
        }, GUILayout.ExpandWidth(true), GUILayout.Height(areaHeight * 0.3f)))
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
            if (characterSelected)
            {
                if (GUILayout.Button("CANCEL", buttonStyle, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(100)))
                {
                    CmdFreeVariant(GetComponent<NetworkIdentity>());
                }
            }
            else
            {
                GUI.enabled = variantAvaliable;
                if (GUILayout.Button("READY", buttonStyle, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(100)))
                {
                    CmdReserveVariant(GetComponent<NetworkIdentity>());
                }
                GUI.enabled = true;
            }

            GUILayout.EndArea();
        }
    }

    void DrawPlayerSelection()
    {
        GameNetworkManager manager = NetworkManager.singleton as GameNetworkManager;

        float size = Screen.height * 0.4f;
        float arrowsHeight = Screen.height * 0.05f;
        float spacing = 20.0f;

        GUILayout.BeginArea(new Rect(Screen.width * 0.5f - size / 2.0f, Screen.height * 0.5f - size / 2.0f, size, size + spacing + arrowsHeight));
        if (manager.playerTextures[VariantID] != null)
        {
            GUI.DrawTexture(new Rect(0, 0, size, size), manager.playerTextures[VariantID], ScaleMode.StretchToFill);
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
            int newVariantID = (VariantID - 1 + manager.playerTextures.Length) % manager.playerTextures.Length;
            CmdSetVariantID(newVariantID);
        }
        if (GUILayout.Button(">", buttonStyle, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(arrowsHeight)))
        {
            int newVariantID = (VariantID + 1) % manager.playerTextures.Length;
            CmdSetVariantID(newVariantID);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
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
    public void CmdSetVariantID(int newVariantID)
    {
        VariantID = newVariantID;
    }

    private void DrawResults()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float podiumWidth = screenWidth * 0.75f;
        float podiumHeight = screenHeight * 0.5f;
        float podiumX = screenWidth * 0.5f - podiumWidth * 0.5f;
        float podiumY = screenHeight * 0.7f;

        DrawPodiumBlock(podiumX, podiumY, podiumWidth * 0.3f, podiumHeight * 0.5f, "2nd", 2); // Second place
        DrawPodiumBlock(podiumX + podiumWidth * 0.3f, podiumY - podiumHeight * 0.25f, podiumWidth * 0.4f, podiumHeight * 0.75f, "1st", 1); // First place
        DrawPodiumBlock(podiumX + podiumWidth * 0.7f, podiumY, podiumWidth * 0.3f, podiumHeight * 0.5f, "3rd", 3); // Third place
    }

    private void DrawPodiumBlock(float x, float y, float width, float height, string label, int place)
    {
        GameNetworkManager manager = NetworkManager.singleton as GameNetworkManager;

        // Draw the block
        Rect podiumBlockRect = new Rect(x, y, width, height);
        GUIStyle backgroundStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = Texture2D.whiteTexture }
        };
        GUI.Box(podiumBlockRect, GUIContent.none, backgroundStyle);

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 70;
        labelStyle.font = Resources.Load<Font>("Fonts/VT323-Regular");
        labelStyle.normal.textColor = Color.black;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.BeginArea(podiumBlockRect);
        GUILayout.BeginVertical();
        GUILayout.Label(label, labelStyle);
        if (orderedPlayersVariants.Length >= place)
            GUILayout.Label(manager.playerNames[orderedPlayersVariants[place - 1]], labelStyle);
        GUILayout.EndVertical();
        GUILayout.EndArea();

        GameNetworkManager gameManager = NetworkManager.singleton as GameNetworkManager;
        float texSize = podiumBlockRect.width * 0.8f;
        if (orderedPlayersVariants.Length >= place && gameManager.playerTextures[orderedPlayersVariants[place - 1]] != null)
            GUI.DrawTexture(new Rect(podiumBlockRect.x + podiumBlockRect.width * 0.1f, podiumBlockRect.y - texSize, texSize, texSize), gameManager.playerTextures[orderedPlayersVariants[place - 1]]);
    }

    private void DrawMapSelection()
    {
        if (isServer || isServerOnly)
        {
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 80;
            labelStyle.font = Resources.Load<Font>("Fonts/VT323-Regular");
            labelStyle.normal.textColor = Color.black;
            labelStyle.hover.textColor = Color.black;
            labelStyle.alignment = TextAnchor.MiddleCenter;            

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 72;
            buttonStyle.font = Resources.Load<Font>("Fonts/VT323-Regular");
            buttonStyle.normal.textColor = Color.black;
            buttonStyle.hover.textColor = Color.black;
            buttonStyle.active.textColor = Color.black;
            buttonStyle.normal.background = MakeTex(2, 2, new Color(0.9607844f, 0.7294118f, 0.9215687f));
            buttonStyle.hover.background = MakeTex(2, 2, new Color(1f, 0.8820755f, 0.980345f));
            buttonStyle.active.background = MakeTex(2, 2, new Color(0.9811321f, 0.513706f, 0.9019072f));
            buttonStyle.alignment = TextAnchor.MiddleCenter;

            GameNetworkManager gameManager = NetworkManager.singleton as GameNetworkManager;

            GUILayout.BeginArea(new Rect(Screen.width * 0.25f, Screen.height * 0.3f, Screen.width * 0.5f, Screen.height * 0.5f));
            GUILayout.Label("Select map", labelStyle, GUILayout.ExpandWidth(true));

            GUILayout.Space(20);

            foreach (var map in gameManager.avaiableMaps)
            {
                if (GUILayout.Button(map.GetComponent<MapName>().Value, buttonStyle, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(100)))
                {
                    gameManager.SelectMap(map);
                    gameManager.StartGame();
                }
                GUILayout.Space(10);
            }

            GUILayout.EndArea();
        }
        else if (isLocalPlayer)
        {
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 70;
            labelStyle.font = Resources.Load<Font>("Fonts/VT323-Regular");
            labelStyle.normal.textColor = Color.black;
            labelStyle.hover.textColor = Color.black;
            labelStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.BeginArea(new Rect(Screen.width * 0.25f, Screen.height * 0.3f, Screen.width * 0.5f, Screen.height * 0.2f));
            GUILayout.Label("Wait for host to select a map", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.EndArea();
        }
    }

    [Command]
    public void CmdCheckVariantAvaliable()
    {
        GameNetworkManager gameManager = NetworkManager.singleton as GameNetworkManager;
        variantAvaliable = gameManager.IsVariantAvaliable(VariantID);
    }

    [Command]
    public void CmdReserveVariant(NetworkIdentity identity)
    {
        GameNetworkManager gameManager = NetworkManager.singleton as GameNetworkManager;
        gameManager.ReserveVariant(identity.connectionToClient, VariantID);

        identity.gameObject.GetComponent<RoomPlayer>().characterSelected = true;
    }

    [Command]
    public void CmdFreeVariant(NetworkIdentity identity)
    {
        GameNetworkManager gameManager = NetworkManager.singleton as GameNetworkManager;
        gameManager.FreeVariant(identity.connectionToClient, VariantID);

        identity.gameObject.GetComponent<RoomPlayer>().characterSelected = false;
    }
}
