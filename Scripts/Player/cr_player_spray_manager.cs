using System.Collections;
using System.Collections.Generic;
using System.IO;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class cr_player_spray_manager : cr_NetworkBehavior
{
    public cr_player_api playerAPI;
    public static string LocalSprayFolder => Path.Combine(cr_game.CustomGameConfigPath, "textures", "sprays");
    public static string CacheSprayFolder => Path.Combine(cr_game.GameEnginePath, "cache", "sprays");
    string CDNURL => $"{cr_game.CDNURL}/cdn/spray/" + "{0}";

    public Image imageRenderer;
    public Canvas canvas;

    static readonly Dictionary<string, Sprite> _memCache = new Dictionary<string, Sprite>();




    [ContextMenu("DEV_PlaceSpray")]
    public void DEV_PlaceSprayAtCenterWithClientOffset()
    {
        Vector3 pos = Vector3.zero + (Vector3.up * playerAPI.playerId);
        Vector3 rot = Vector3.zero;

        PlaceDecalAt(pos, rot);
    }




    void EnsureCacheDir()
    {
        if (!Directory.Exists(CacheSprayFolder))
            Directory.CreateDirectory(CacheSprayFolder);
    }

    string CachePathFor(string imageHash) => Path.Combine(CacheSprayFolder, imageHash + ".png");

    public bool IsSprayInCache(string imageHash)
    {
        if (string.IsNullOrWhiteSpace(imageHash)) return false;
        EnsureCacheDir();
        return File.Exists(CachePathFor(imageHash));
    }

    public void PlaceDecalAt(Vector3 position, Vector3 rotation)
    {
        var parser = cr_game_file_parser.Instance;
        string filePath = Path.Combine(cr_game.CustomGameConfigPath, "auth", "player.cfg");
        string localImageHash = parser.GetFileVar<string>(filePath, "cr_player_spray_hash", "");

        if (string.IsNullOrEmpty(localImageHash)) return;

        RPC_PlaceDecalAt(position, rotation, localImageHash);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_PlaceDecalAt(Vector3 position, Vector3 rotation, string imageHash)
    {
        // Fetch then spawn on each client
        StartCoroutine(CFetchAndSpawn(position, rotation, imageHash));
    }

    IEnumerator CFetchAndSpawn(Vector3 position, Vector3 rotation, string imageHash)
    {
        Sprite s = null;

        // Fast path in-memory
        if (_memCache.TryGetValue(imageHash, out s) && s != null)
        {
            ApplyAndSpawn(s, position, rotation);
            yield break;
        }

        // Disk cache
        if (IsSprayInCache(imageHash))
        {
            s = GetImageFromCache(imageHash);
            if (s != null)
            {
                _memCache[imageHash] = s;
                ApplyAndSpawn(s, position, rotation);
                yield break;
            }
        }

        // Download and cache
        yield return StartCoroutine(DownloadAndCacheFromCDN(imageHash, sprite =>
        {
            s = sprite;
            if (s != null) _memCache[imageHash] = s;
        }));

        if (s == null)
        {
            Debug.LogError("[Spray] Could not fetch sprite from CDN");
            yield break;
        }

        ApplyAndSpawn(s, position, rotation);
    }

    void ApplyAndSpawn(Sprite s, Vector3 position, Vector3 rotation)
    {
        imageRenderer.sprite = s;
        SpawnSprayAt(position, rotation);
    }

    public void SpawnSprayAt(Vector3 position, Vector3 rotation)
    {
        canvas.transform.position = position;
        canvas.transform.eulerAngles = rotation;
        canvas.gameObject.SetActive(true);
    }

    IEnumerator DownloadAndCacheFromCDN(string imageHash, Action<Sprite> onDone)
    {
        onDone ??= _ => { };
        if (string.IsNullOrWhiteSpace(imageHash))
        {
            onDone(null);
            yield break;
        }

        string url = string.Format(CDNURL, imageHash);

        Debug.Log(url);
        
        using (var req = UnityWebRequestTexture.GetTexture(url))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Spray] Download failed {req.responseCode}: {req.error}");
                onDone(null);
                yield break;
            }

            Texture2D tex = DownloadHandlerTexture.GetContent(req);
            if (tex == null)
            {
                onDone(null);
                yield break;
            }

            // Save as PNG to cache
            try
            {
                EnsureCacheDir();
                string cachePath = CachePathFor(imageHash);
                byte[] png = tex.EncodeToPNG();
                File.WriteAllBytes(cachePath, png);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Spray] Failed to write cache file: {e.Message}");
            }

            var sprite = SpriteFromTexture(tex);
            onDone(sprite);
        }
    }

    public Sprite GetImageFromCache(string imageHash)
    {
        try
        {
            string path = CachePathFor(imageHash);
            if (!File.Exists(path)) return null;

            byte[] data = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(data, markNonReadable: false))
                return null;

            return SpriteFromTexture(tex);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Spray] Cache load failed: {e.Message}");
            return null;
        }
    }

    Sprite SpriteFromTexture(Texture2D tex)
    {
        var rect = new Rect(0, 0, tex.width, tex.height);
        var pivot = new Vector2(0.5f, 0.5f);
        // Use 100 pixels per unit by default. Adjust if your UI world-space scale differs.
        return Sprite.Create(tex, rect, pivot, 100f);
    }
}
