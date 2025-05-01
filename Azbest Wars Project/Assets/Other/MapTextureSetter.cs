using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class MapTextureSetter : MonoBehaviour
{
    RawImage rawImage;
    void Update()
    {
        rawImage = GetComponent<RawImage>();

        Texture2D texture = MapTextureSystem.mapTexture;
        if (texture == null) return;
        rawImage.texture = MapTextureSystem.mapTexture;

    }
}
