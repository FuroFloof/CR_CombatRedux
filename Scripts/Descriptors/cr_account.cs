using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class cr_account : cr_MonoBehavior
{
    private static cr_account _instance;
    public static cr_account Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<cr_account>();
            }
            return _instance;
        }
    }


    public static string localToken = "";
    public static string sessionToken = "";
    public static string publicToken = "";

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        // int accountID = Random.Range(0, 999999);
        // string randomNum = Random.Range(0, 99).ToString();
        // var parser = cr_game_file_parser.Instance;
        // var tarPath = Path.Combine(cr_game.CustomGameConfigPath, "localAccount.cfg");
        // var fileName = parser.GetFileVar<string>(tarPath, "displayName", "Guest");
        // bool isGuest = parser.GetFileVar<bool>(tarPath, "isGuest", true);
        // string clientColor = parser.GetFileVar<string>(tarPath, "clientColor", cr_game_defaults_colors.GetColorHexFromColorCode("8"));


        //int maxChars = 16;
        
        // fileName = fileName.Substring(0, fileName.Length > maxChars - 1 ? maxChars - 1 : fileName.Length); // cap username at 16 chars
        
        // if(isGuest)
        // {
        //     fileName += $" ({randomNum})";
        // }

        //localToken = $"{accountID}~{fileName}~{clientColor}";
    }
    
    
    public async Task<bool> GetAndSetPublicToken()
    {
        var parser = cr_game_file_parser.Instance;
        var playerPath = Path.Combine(cr_game.CustomGameConfigPath, "auth", "player.cfg");
        sessionToken = parser.GetFileVar<string>(playerPath, "cr_player_session", "");
        
    
        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            Debug.LogError("[cr_account] sessionToken is empty. Skipping public token fetch.");
            return false;
        }

        Debug.Log("[cr_account] Fetching Public Token...");
        var publicTokenWebResult = await GetAccountPublicTokenFromSession(sessionToken);

        if (publicTokenWebResult == null)
        {
            Debug.LogError("[cr_account] Public Token Web Request returned null object.");
            return false;
        }

        if (!publicTokenWebResult.success || string.IsNullOrEmpty(publicTokenWebResult.public_token))
        {
            Debug.LogError("[cr_account] Public Token request failed or token missing.");
            return false;
        }

        publicToken = publicTokenWebResult.public_token;
        Debug.Log("[cr_account] Public Token: " + publicToken);
        return true;
    }


    public void Login(string username, string password)
    {
        
    }
    
    public void RegisterGuest(string guestUsername)
    {
        
    }
    
    public static string GetLocalTokenForSelf()
    {
        return publicToken;
    }
    
    public static async Task<cr_account_instance> GetPlayerAccountInstanceByPublicToken(string token)
    {
        cr_account_web_response_account_data accountData = await GetAccountDataFromPublicToken(token);
        

        // string[] splToken = token.Split('~');

        // int accountID = int.Parse(splToken[0]);
        // string displayName = splToken[1];
        // string clientColor = splToken[2];
        
        
        return new cr_account_instance()
        {
            displayName = accountData.displayname,
            accountId = accountData.account_id,
            isDeveloper = false,
            isAdmin = false,
            isModerator = false,
            isGuest = false,
            clientColor = cr_game_defaults_colors.GetColorHexFromColorCode("7")
        };
    }
    
    public static async Task<cr_account_web_response_account_data> GetAccountDataFromPublicToken(string publicToken)
    {
        string callURL = $"{cr_game.APIURL}/account/data/{publicToken}";
        using (var req = UnityWebRequest.Get(callURL))
        {
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = 10;

            await SendAsync(req);

            if (HasRequestError(req))
            {
                Debug.LogWarning($"[cr_account] GetAccountDataFromPublicToken network error: {req.error} | url={callURL}");
                return new cr_account_web_response_account_data { success = false };
            }

            try
            {
                var json = req.downloadHandler.text;
                var data = JsonUtility.FromJson<cr_account_web_response_account_data>(json);
                return data ?? new cr_account_web_response_account_data { success = false };
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[cr_account] JSON parse error for account/data: {ex.Message}");
                return new cr_account_web_response_account_data { success = false };
            }
        }
    }

    public static async Task<cr_account_web_response_public_token> GetAccountPublicTokenFromSession(string sessionToken)
    {
        string callURL = $"{cr_game.APIURL}/account/public_token";

        var payload = new cr_account_web_request_public_token { session_token = sessionToken };
        string json = JsonUtility.ToJson(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);

        var req = new UnityWebRequest(callURL, UnityWebRequest.kHttpVerbGET);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.timeout = 10;

        using (req)
        {
            await SendAsync(req);

            if (HasRequestError(req))
            {
                Debug.LogWarning($"[cr_account] GetAccountPublicTokenFromSession network error: {req.error} | url={callURL}");
                return new cr_account_web_response_public_token { success = false, public_token = null };
            }

            try
            {
                var respJson = req.downloadHandler.text;
                var data = JsonUtility.FromJson<cr_account_web_response_public_token>(respJson);
                return data ?? new cr_account_web_response_public_token { success = false, public_token = null };
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[cr_account] JSON parse error for account/public_token: {ex.Message}");
                return new cr_account_web_response_public_token { success = false, public_token = null };
            }
        }
    }

    // --- Helpers ---

    private static async Task SendAsync(UnityWebRequest req)
    {
        var op = req.SendWebRequest();

        var tcs = new TaskCompletionSource<bool>();
        op.completed += _ => tcs.TrySetResult(true);
        await tcs.Task;
    }

    private static bool HasRequestError(UnityWebRequest req)
    {
        return req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.DataProcessingError;
    }
}

[System.Serializable]
public class cr_account_instance
{
    public string displayName;
    public int accountId;
    public bool isDeveloper;
    public bool isAdmin;
    public bool isModerator;
    public bool isGuest;
    public string clientColor;
}

[System.Serializable]
public class cr_account_web_response_account_data
{
    public bool success;
    public int account_id;
    public string displayname;
    public string username;
    public int account_created_at;
    public string origin;
}

[System.Serializable]
public class cr_account_web_response_public_token
{
    public bool success;
    public string public_token;
}

[System.Serializable]
public class cr_account_web_request_public_token
{
    public string session_token;
}

[System.Serializable]
public class cr_account_web_response_login
{
    public bool success;
    public int account_id;
    public string session_token;
}

[System.Serializable]
public class cr_account_web_request_register
{
    public string origin;
    public string username;
    public string password;
}

[System.Serializable]
public class cr_account_web_response_register
{
    public bool success;
    public int account_id;
    public string username;
    public string session_token;
}