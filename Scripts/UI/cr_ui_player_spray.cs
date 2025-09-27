using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class cr_ui_player_spray : MonoBehaviour
{
    public Image CurrentSprayPreview;
    public GameObject SprayModal;
    public GameObject UploadingSprayModal;
    public cr_ui_player_spray_button[] buttons;
    public Button UploadButton;

    /// 
    /// IMPLEMENT ACCOUNT INTEGRATION
    /// 



    void OnEnable()
    {
        SprayModal.SetActive(false);
        UploadingSprayModal.SetActive(false);
    }

    public void OnPickNewSprayButton()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(false);
        }
        StartCoroutine(LoadModalAndSprayFiles());
    }

    public IEnumerator LoadModalAndSprayFiles()
    {
        SprayModal.SetActive(false);
        
        string folderPath = cr_player_spray_manager.LocalSprayFolder;

        string[] imagePaths = Directory.GetFiles(folderPath, "*.*");
        var validSprites = new List<Sprite>();
        var validImagePaths = new List<string>();

        foreach (string path in imagePaths)
        {
            string ext = Path.GetExtension(path).ToLower();
            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg") continue;

            byte[] data = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(data);

            if (tex.width == 512 && tex.height == 512)
            {
                Sprite sprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f)
                );
                validSprites.Add(sprite);
                validImagePaths.Add(path);
            }
        }

        // Wait one frame so Unity can breathe
        yield return null;

        // Apply found sprites to buttons
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].sprayID = i;
            if (i < validSprites.Count)
            {
                buttons[i].gameObject.SetActive(true);
                buttons[i].button.image.sprite = validSprites[i];
                buttons[i].sprayFilePath = validImagePaths[i];
            }
            else
            {
                buttons[i].gameObject.SetActive(false);
            }
        }
        
        SprayModal.SetActive(true);
    }
    
    public void OnSprayPicked(cr_ui_player_spray_button spray)
    {
        StartCoroutine(COnSprayPicked(spray));
    }
    public IEnumerator COnSprayPicked(cr_ui_player_spray_button spray)
    {
        UploadingSprayModal.SetActive(true);

        string filePath = spray.sprayFilePath;
        string returnedImageHash = null;
        bool success = false;
        string accountSessionToken = cr_account.sessionToken;

        // Basic client side checks
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Debug.LogError("[CDN Upload] File path invalid or does not exist");
            UploadingSprayModal.SetActive(false);
            yield break;
        }
        if (string.IsNullOrEmpty(accountSessionToken))
        {
            Debug.LogError("[CDN Upload] Missing session token");
            UploadingSprayModal.SetActive(false);
            yield break;
        }

        // Optional size precheck to avoid round trips
        // You can remove this block if you want to rely only on server validation
        byte[] rawBytes = File.ReadAllBytes(filePath);
        var texCheck = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (texCheck.LoadImage(rawBytes, markNonReadable: true))
        {
            if (texCheck.width > 512 || texCheck.height > 512)
            {
                Debug.LogError("[CDN Upload] Image must be at most 512x512");
                UploadingSprayModal.SetActive(false);
                yield break;
            }
        }
        // Build multipart form
        WWWForm form = new WWWForm();
        string fileName = Path.GetFileName(filePath);
        // Try to guess content type based on extension. Sharp will transcode to PNG server side anyway
        string contentType = "application/octet-stream";
        string ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext == ".png") contentType = "image/png";
        else if (ext == ".jpg" || ext == ".jpeg") contentType = "image/jpeg";
        else if (ext == ".webp") contentType = "image/webp";
        else if (ext == ".bmp") contentType = "image/bmp";

        form.AddBinaryData("image", rawBytes, fileName, contentType);
        form.AddField("session_token", accountSessionToken);

        string uploadUrl = $"{cr_game.CDNURL}/upload/spray/";

        using (UnityWebRequest req = UnityWebRequest.Post(uploadUrl, form))
        {
            yield return req.SendWebRequest();
            
            bool netError = req.result != UnityWebRequest.Result.Success;

            if (netError)
            {
                Debug.LogError($"[CDN Upload] HTTP error {req.responseCode}: {req.error}\n{req.downloadHandler.text}");
            }
            else
            {
                string json = req.downloadHandler.text;
                CdnUploadResponse resp = null;
                try { resp = JsonUtility.FromJson<CdnUploadResponse>(json); }
                catch { Debug.LogError("[CDN Upload] Failed parsing server response"); }

                if (resp != null && resp.success && !string.IsNullOrEmpty(resp.file_hash))
                {
                    returnedImageHash = resp.file_hash;
                    success = true;
                }
                else
                {
                    var msg = resp != null ? resp.error : "Unknown server response";
                    Debug.LogError($"[CDN Upload] Server refused upload: {msg}");
                }
            }
        }

        if (success)
        {
            CurrentSprayPreview.sprite = spray.button.image.sprite;

            var parser = cr_game_file_parser.Instance;
            string configFilePath = Path.Combine(cr_game.CustomGameConfigPath, "auth", "player.cfg");
            parser.WriteFileVar(configFilePath, "cr_player_spray_hash", returnedImageHash);
        }

        UploadingSprayModal.SetActive(false);
        SprayModal.SetActive(false);
    }

    
    public void OnCancelPressed()
    {
        SprayModal.SetActive(false);
    }
}


[System.Serializable]
public class CdnUploadResponse
{
    public bool success;
    public string file_hash;
    public string error;
}