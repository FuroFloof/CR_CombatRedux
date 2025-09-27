using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;
using UnityEngine.Networking;
using System;
using System.Text;

public class cr_ui_login_menu_manager : MonoBehaviour
{
    public GameObject HomeModal;
    public GameObject AccountModal;
    public GameObject GuestModal;
    public GameObject AccountRegisterModal;

    public GameObject AccountLoginSuccessModal;
    public GameObject AccountLoginFailureModal;
    public GameObject AccountRegisterSuccessModal;
    public GameObject AccountRegisterFailureModal;
    


    void Awake()
    {
        
    }

    void Start()
    {
        CloseAllModals();
        
        CheckLoginState();
    }
    
    public async void CheckLoginState()
    {
        bool sessionIsValid = await cr_account.Instance.GetAndSetPublicToken();
    
        var parser = cr_game_file_parser.Instance;
        string filePath = Path.Combine(cr_game.CustomGameConfigPath, "auth", "player.cfg");

        string session = parser.GetFileVar<string>(filePath, "cr_player_session", "");
        if (session.IsNullOrEmpty() || !sessionIsValid)
        {
            parser.WriteFileVar(filePath, "cr_player_session", "");
            OpenHomeModal();
        }else 
        {
            LoadIntoBootScene();
        }
    }

    public void CloseAllModals()
    {
        HomeModal.SetActive(false);
        AccountModal.SetActive(false);
        GuestModal.SetActive(false);
        AccountLoginSuccessModal.SetActive(false);
        AccountLoginFailureModal.SetActive(false);
        AccountRegisterModal.SetActive(false);
        AccountRegisterSuccessModal.SetActive(false);
        AccountRegisterFailureModal.SetActive(false);
    }


    public void OpenHomeModal()
    {
        CloseAllModals();
        HomeModal.SetActive(true);
    }

    public void OpenAccountModal()
    {
        CloseAllModals();
        AccountModal.SetActive(true);
    }
    
    public void OpenGuestModal()
    {
        CloseAllModals();
        GuestModal.SetActive(true);
    }
    
    public void OpenAccountRegisterModal()
    {
        CloseAllModals();
        AccountRegisterModal.SetActive(true);
    }
    
    
    public void OpenAccountLoginSuccessModal()
    {
        CloseAllModals();
        AccountLoginSuccessModal.SetActive(true);
    }
    public void OpenAccountLoginFailureModal()
    {
        CloseAllModals();
        AccountLoginFailureModal.SetActive(true);
    }
    
    public void OpenAccountRegisterSuccessModal()
    {
        CloseAllModals();
        AccountRegisterSuccessModal.SetActive(true);
    }
    public void OpenAccountRegisterFailureModal()
    {
        CloseAllModals();
        AccountRegisterFailureModal.SetActive(true);
    }
    
    
    public TMP_InputField AccountRegisterUsernameInput;
    public TMP_InputField AccountRegisterPasswordInput;
    public TMP_InputField AccountRegisterPasswordRepeatInput;
    public async void FireAccountRegister()
    {
        string username = AccountRegisterUsernameInput.text?.Trim() ?? "";
        string password = AccountRegisterPasswordInput.text ?? "";
        string repeated = AccountRegisterPasswordRepeatInput.text ?? "";

        // quick client validation
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            Debug.LogWarning("[Register] Username or password empty.");
            AccountRegisterFailure();
            return;
        }
        if (!string.Equals(password, repeated))
        {
            Debug.LogWarning("[Register] Passwords do not match.");
            AccountRegisterFailure();
            return;
        }

        string url = $"{cr_game.APIURL}/register/account";

        var payload = new cr_account_web_request_register
        {
            origin = "STEAM",        // set this from UI if needed
            username = username,
            password = password
        };

        string json = JsonUtility.ToJson(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);

        var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.timeout = 10;

        cr_account_web_response_register resp = null;

        using (req)
        {
            await AwaitRequest(req);

            if (RequestHasError(req))
            {
                Debug.LogWarning($"[Register] HTTP error: {req.error} code={req.responseCode} url={url} body={req.downloadHandler?.text}");
                AccountRegisterFailure();
                return;
            }

            try
            {
                string respJson = req.downloadHandler.text;
                resp = JsonUtility.FromJson<cr_account_web_response_register>(respJson);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Register] JSON parse error: {ex.Message}");
                AccountRegisterFailure();
                return;
            }
        }

        if (resp != null && resp.success && !string.IsNullOrEmpty(resp.session_token))
        {
            AccountRegisterSuccess(resp.session_token);
        }
        else
        {
            Debug.LogWarning($"[Register] Server reported failure or missing token. code={req.responseCode} body={req.downloadHandler?.text}");
            AccountRegisterFailure();
        }
    }

    
    public void AccountRegisterSuccess(string sessionToken)
    {
        var parser = cr_game_file_parser.Instance;
        string filePath = Path.Combine(cr_game.CustomGameConfigPath, "auth", "player.cfg");

        parser.WriteFileVar(filePath, "cr_player_session", sessionToken); // write session token

        OpenAccountRegisterSuccessModal();
        LoadIntoBootScene();
    }
    
    public void AccountRegisterFailure()
    {
        OpenAccountRegisterFailureModal();

        Invoke(nameof(OpenHomeModal), 2);
    }
    
    
    

    public TMP_InputField AccountUsernameInput;
    public TMP_InputField AccountPasswordInput;
    
    public async void FireAccountLogin()
    {
        string username = AccountUsernameInput.text?.Trim() ?? "";
        string password = AccountPasswordInput.text ?? "";

        string safeUser = UnityWebRequest.EscapeURL(username);
        string safePass = UnityWebRequest.EscapeURL(password);

        string loginURL = $"{cr_game.APIURL}/login/account/{safeUser}/{safePass}";

        var loginResponse = new cr_account_web_response_login
        {
            success = false,
            account_id = -1,
            session_token = ""
        };

        var req = new UnityWebRequest(loginURL, UnityWebRequest.kHttpVerbPOST);
        req.uploadHandler = new UploadHandlerRaw(Array.Empty<byte>()); // empty body
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.timeout = 10;

        using (req)
        {
            await AwaitRequest(req);

            if (RequestHasError(req))
            {
                Debug.LogWarning($"[Login] Network or HTTP error: {req.error} | url={loginURL}");
            }
            else
            {
                try
                {
                    string json = req.downloadHandler.text;
                    var parsed = JsonUtility.FromJson<cr_account_web_response_login>(json);
                    if (parsed != null) loginResponse = parsed;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Login] JSON parse error: {ex.Message}");
                }
            }
        }

        if (loginResponse.success) AccountLoginSuccess(loginResponse.session_token);
        if (!loginResponse.success) AccountLoginFailed();
    }

    private static async Task AwaitRequest(UnityWebRequest req)
    {
        var op = req.SendWebRequest();
        var tcs = new TaskCompletionSource<bool>();
        op.completed += _ => tcs.TrySetResult(true);
        await tcs.Task;
    }

    private static bool RequestHasError(UnityWebRequest req)
    {
        return req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.DataProcessingError;
    }
    
    
    public void AccountLoginFailed()
    {
        OpenAccountLoginFailureModal();

        Invoke(nameof(OpenHomeModal), 2);
    }
    
    public void AccountLoginSuccess(string sessionToken)
    {
        var parser = cr_game_file_parser.Instance;
        string filePath = Path.Combine(cr_game.CustomGameConfigPath, "auth", "player.cfg");

        parser.WriteFileVar(filePath, "cr_player_session", sessionToken); // write session token

        OpenAccountLoginSuccessModal();
        LoadIntoBootScene();
    }
    public void LoadIntoBootScene()
    {
        StartCoroutine(CLoadIntoBootScene());
    }

    public MeshRenderer MR;
    public IEnumerator CLoadIntoBootScene()
    {
        float fadeInTime = 2;
    
        MeshRenderer mr = MR;
        Material mat = mr.material;

        float elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInTime);

            Color c = mat.color;
            c.a = Mathf.Lerp(0f, 1f, t);
            mat.color = c;

            yield return null;
        }

        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("00_boot");
    }
    
    
    
    
    public TMP_InputField GuestUsernameInput;
    public void FireGuestLogin()
    {
        
    }
}