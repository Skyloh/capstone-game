using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/LoadingZoneTile")]
public class LoadingZoneTile : TileBase
{
    public string sceneToLoad;
    public Sprite tileSprite;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = tileSprite;
        tileData.color = Color.white;
    }
}
