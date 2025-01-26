using Mirror;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class RoomPlayer : NetworkRoomPlayer
{
    [SyncVar]
    public int ColorID = 0;
    [SyncVar]
    public string Name = "Player";

    [SyncVar]
    public bool showResults = false;
    [SyncVar]
    public string[] orderedPlayersNames;
    [SyncVar]
    public int[] orderedPlayersColorID;
    private InputAction fireAction;
    public GameObject lobbyCanvas;

    static bool localShowResults = false;

    private void Start()
    {
        base.Start();
        if (isLocalPlayer)
        {
            fireAction = InputSystem.actions.FindAction("Attack");
        }
    }

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
            if (showResults && fireAction.IsPressed())
                showResults = false;

            localShowResults = showResults;
            if(lobbyCanvas != null)
                lobbyCanvas.SetActive(!localShowResults);
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
        labelStyle.hover.textColor = Color.black;
        labelStyle.alignment = TextAnchor.MiddleCenter;

        float areaHeight = Screen.height * 0.13f;
        float areaWidth = Screen.width * 0.2f;
        float spacing = Screen.width * 0.05f;

        GUIStyle area = new GUIStyle();
        if (readyToBegin)
            area.normal.background = MakeTex(2, 2, new Color(0.439f, 1.0f, 0.518f));
        else
            area.normal.background = MakeTex(2, 2, new Color(1f, 0.416f, 0.416f));

        GUILayout.BeginArea(new Rect((index + 1) * spacing + (index * areaWidth), Screen.height * 0.1f, areaWidth, areaHeight));
        GUILayout.BeginVertical(area);
        if (isLocalPlayer)
        {
            string newName = GUILayout.TextField(Name, labelStyle, GUILayout.ExpandWidth(true));
            if (newName != Name)
                CmdChangeName(newName);
        }
        else
            GUILayout.Label(Name, labelStyle, GUILayout.ExpandWidth(true));
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

    [Command]
    public void CmdChangeName(string newName)
    {
        Name = newName;
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
        if (orderedPlayersNames.Length >= place)
            GUILayout.Label(orderedPlayersNames[place - 1], labelStyle);
        GUILayout.EndVertical();
        GUILayout.EndArea();

        GameNetworkManager gameManager = NetworkManager.singleton as GameNetworkManager;
        float texSize = podiumBlockRect.width * 0.8f;
        if (orderedPlayersColorID.Length >= place && gameManager.playerTextures[orderedPlayersColorID[place - 1]] != null)
            GUI.DrawTexture(new Rect(podiumBlockRect.x + podiumBlockRect.width * 0.1f, podiumBlockRect.y - texSize, texSize, texSize), gameManager.playerTextures[orderedPlayersColorID[place - 1]]);
    }
}
