using UnityEngine;

public class SpriteHolder : MonoBehaviour
{
    public static SpriteHolder Instance;

    [SerializeField]
    public Sprite[] healthbarSprites;
    [SerializeField]
    public Sprite selectedSprite;
    [SerializeField]
    public Sprite areaMarkerSprite;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
