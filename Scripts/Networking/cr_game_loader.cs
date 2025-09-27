using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class cr_game_loader : MonoBehaviour
{
    private static cr_game_loader _instance;
    public static cr_game_loader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<cr_game_loader>();
            }
            return _instance;
        }
    }
    
    
    public Task SwapViewIfPlayerExists()
    {
        var locP = cr_networking.localPlayer;
        if (!locP) return Task.CompletedTask;

        var tcs = new TaskCompletionSource<bool>();

        void OnFinish()
        {
            try { SwapViewDone(); }
            finally { tcs.TrySetResult(true); }
        }

        Action swapViewsAction = locP.loadingManager.SwapViews;
        
        locP.loadingManager.FadeAndDoAction(0f, 10f, 1f, 1f, swapViewsAction, OnFinish);
        cr_player_audio_listener_manager.Instance.SwapListenerTarget(0, 1);
        return tcs.Task;
    }
    public void SwapViewDone()
    {
        Debug.Log("Swapping view completed");
    }


    public GameObject NetworkRunnerPrefab; //has a netwokr runner, event handler, and scene manager attached to it
    private NetworkSceneManagerDefault nsm;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    public async void StartLocalSingle(string sceneName)
    {
        await SwapViewIfPlayerExists();
    
        var runner = CreateRunner();
        
        runner.ProvideInput = true; // still needed so your input callback fires

        nsm = runner.GetComponent<NetworkSceneManagerDefault>();
        cr_networking.Instance.Bind(runner);

        int buildIndex = FindBuildIndexByName(sceneName);

        var result = await runner.StartGame(new StartGameArgs {
            GameMode     = GameMode.Single,                 // offline, no Photon connection
            SceneManager = nsm,
            Scene        = SceneRef.FromIndex(buildIndex)   // load the scene as a networked scene
            // No SessionName, no CustomPhotonAppSettings
        });

        if (!result.Ok)
            Debug.LogError($"Single start failed: {result.ShutdownReason}");
    }

    
    public async void Host(string roomName, string sceneName)
    {   
        await SwapViewIfPlayerExists();
    
        var app = PhotonAppSettings.Global.AppSettings.GetCopy();
        
        app.FixedRegion = cr_networking.Instance.NetworkingRegion.ToLowerInvariant();
    
    
        var runner = CreateRunner();
        nsm = runner.gameObject.GetComponent<NetworkSceneManagerDefault>();

        cr_networking.Instance.Bind(runner);
        
        await runner.StartGame(new StartGameArgs {
            GameMode = GameMode.Host,
            SceneManager = nsm,
            SessionName = roomName,
            CustomPhotonAppSettings = app
        });
        
        int buildIndex = FindBuildIndexByName(sceneName);
        await runner.LoadScene(SceneRef.FromIndex(buildIndex));
    }
    
    public async void AutoHostOrJoin(string roomName, string sceneName)
    {
        await SwapViewIfPlayerExists();
    
        var app = PhotonAppSettings.Global.AppSettings.GetCopy();
        
        app.FixedRegion = cr_networking.Instance.NetworkingRegion.ToLowerInvariant();
        
    
        var runner = CreateRunner();
        nsm = runner.gameObject.GetComponent<NetworkSceneManagerDefault>();

        cr_networking.Instance.Bind(runner);

        var result = await runner.StartGame(new StartGameArgs {
            GameMode     = GameMode.AutoHostOrClient, // join if exists, otherwise host
            SceneManager = nsm,
            SessionName  = roomName,
            CustomPhotonAppSettings = app
        });

        if (!result.Ok)
        {
            Debug.LogError($"AutoHostOrClient failed: {result.ShutdownReason}");
            return;
        }

        // Only the side that became host should load the scene
        if (runner.IsServer)
        {
            int buildIndex = FindBuildIndexByName(sceneName);
            await runner.LoadScene(SceneRef.FromIndex(buildIndex));
        }
        // Clients do not call LoadScene. They will follow the host automatically.
    }

    
    public async void Join(string roomName)
    {
        var app = PhotonAppSettings.Global.AppSettings.GetCopy();
        
        app.FixedRegion = cr_networking.Instance.NetworkingRegion.ToLowerInvariant();
    
    
        var runner = CreateRunner();
        nsm = runner.gameObject.GetComponent<NetworkSceneManagerDefault>();
        
        cr_networking.Instance.Bind(runner);
        
        await runner.StartGame(new StartGameArgs {
            GameMode = GameMode.Client,
            SceneManager = nsm,
            SessionName = roomName,
            CustomPhotonAppSettings = app
        });
    }
    
    public NetworkRunner CreateRunner()
    {
        
    
        if(cr_networking.fusionRunner != null)
        {
            cr_networking.fusionRunner.Shutdown();
            Destroy(cr_networking.fusionRunner.gameObject);
        }
    
        var go = Instantiate(NetworkRunnerPrefab);

        DontDestroyOnLoad(go);
        
        var runner = go.GetComponent<NetworkRunner>();
        
        return runner;
    }
    
    public static int FindBuildIndexByName(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string file = System.IO.Path.GetFileNameWithoutExtension(path);
            if (string.Equals(file, sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return -1;
    }
}