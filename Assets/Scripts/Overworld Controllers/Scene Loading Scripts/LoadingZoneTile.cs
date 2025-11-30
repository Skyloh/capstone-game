using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/LoadingZoneTile")]
public class LoadingZoneTile : TileBase
{
    public string sceneToLoad;
    // ID for spawn point to support multiple spawn points in a scene
    public string targetSpawnPointID;
    public Sprite tileSprite;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = tileSprite;
        tileData.color = Color.white;
    }
}
