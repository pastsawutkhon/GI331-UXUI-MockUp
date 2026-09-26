using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine;

public static class PlayerProfileService
{
    private const string AvatarKey = "avatar";

    public static async Task SaveAvatarAsync(Texture2D source)
    {
        if (source == null) return;
        var target = RenderTexture.GetTemporary(128, 128, 0, RenderTextureFormat.ARGB32);
        var previous = RenderTexture.active;
        Texture2D readable = null;
        string encoded;
        try
        {
            Graphics.Blit(source, target);
            RenderTexture.active = target;
            readable = new Texture2D(128, 128, TextureFormat.RGB24, false);
            readable.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
            readable.Apply();
            encoded = Convert.ToBase64String(readable.EncodeToJPG(80));
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            if (readable != null) UnityEngine.Object.Destroy(readable);
        }
        await CloudSaveService.Instance.Data.Player.SaveAsync(
            new Dictionary<string, object> { { AvatarKey, encoded } },
            new Unity.Services.CloudSave.Models.Data.Player.SaveOptions(new PublicWriteAccessClassOptions()));
    }

    public static async Task<Texture2D> LoadAvatarAsync(string playerId)
    {
        Texture2D texture = null;
        try
        {
            var data = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { AvatarKey },
                new LoadOptions(new PublicReadAccessClassOptions(playerId)));
            if (!data.TryGetValue(AvatarKey, out var item)) return null;
            string encoded = item.Value.GetAs<string>();
            if (string.IsNullOrEmpty(encoded)) return null;
            texture = new Texture2D(2, 2);
            if (texture.LoadImage(Convert.FromBase64String(encoded))) return texture;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load profile picture for {playerId}: {exception.Message}");
        }
        if (texture != null) UnityEngine.Object.Destroy(texture);
        return null;
    }
}
