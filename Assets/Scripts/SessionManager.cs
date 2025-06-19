using System;
using UnityEngine;
using UnityUtils;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Multiplayer;
using Unity.Services.Lobbies.Models;

public class SessionManager : Singleton<SessionManager>
{
    ISession session;
    ISession activeSession
    {
        get => session;
        set
        {
            activeSession = value;
            Debug.Log($"SessionManager: Active session set to {activeSession}");
        }
    }
    const string playerNameProperty = "PlayerName";
    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("SessionManager: Unity Services initialized successfully. PlayerID: " + AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e)
        {
            Debug.LogError($"SessionManager: Failed to initialize Unity Services: {e.Message}");
            return;
        }
    }

    async UniTask<Dictionary<string, PlayerProperty>> GetPlayerPropertiesAsync()
    {
        var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
        var playerProperties = new PlayerProperty(playerName, VisibilityPropertyOptions.Member);
        return new Dictionary<string, PlayerProperty>
        {
            { playerNameProperty, playerProperties }
        };
    }
    async void StartSessionHost()
    {
        var playerProperties = await GetPlayerPropertiesAsync();
        var options = new SessionOptions
        {
            MaxPlayers = 4,
            IsLocked = false,
            IsPrivate = false,
            PlayerProperties = playerProperties,
        }.WithRelayNetwork();

        activeSession = await MultiplayerService.Instance.CreateSessionAsync(options);
        Debug.Log($"SessionManager: Session created with ID {activeSession.Id} and Session Code: {activeSession.Code}");
    }

    async UniTask JoinSessionByID(string session)
    {
        activeSession = await MultiplayerService.Instance.JoinSessionByIdAsync(session);
        Debug.Log($"SessionManager: Joined session with ID {activeSession.Id} and Session Code: {activeSession.Code}");
    }

    async UniTask JoinSessionByCode(string code)
    {
        activeSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);
        Debug.Log($"SessionManager: Joined session with ID {activeSession.Id} and Session Code: {activeSession.Code}");
    }

    async UniTask KickPlayerFromSessionAsync(string playerId)
    {
        if (!activeSession.IsHost) return;

        await activeSession.AsHost().RemovePlayerAsync(playerId);
    }

    async UniTask<IList<ISessionInfo>> QuerySession()
    {
        var sessionQueryOption = new QuerySessionsOptions();
        var result = await MultiplayerService.Instance.QuerySessionsAsync(sessionQueryOption);
        return result.Sessions;
    }

    async UniTask LeaveSession()
    {
        if (activeSession != null)
        {
            try
            {
                await activeSession.LeaveAsync();
                Debug.Log($"SessionManager: Left session with ID {activeSession.Id}");
            }
            catch (Exception e)
            {
                Debug.LogError($"SessionManager: Failed to leave session: {e.Message}");
            }
            finally
            {
                activeSession = null;
            }
        }
    }
}
