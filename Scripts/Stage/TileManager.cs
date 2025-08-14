using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    [SerializeField] private StageManager stageManager;
    [SerializeField] private List<ChapterMapsData> chapterMapsData;
    [SerializeField] private MapData mapData;
    [SerializeField] private Player player;
    [SerializeField] private List<Tilemap> tilemaps = new List<Tilemap>();

    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private GameObject TreePrefab;
    [SerializeField] private GameObject farmlandPrefab;

    private Dictionary<TileType, TileData> tileDictionary = new Dictionary<TileType, TileData>();
    public MapData MapData => mapData;

    [Header("Background Auto Fill")]
    [Tooltip("카메라 화면 밖으로 몇 칸 더 깔아둘지(버퍼).")]
    [SerializeField] private int backgroundBufferCells = 4;

    private Vector3Int bgMin, bgMax;        
    private Vector2Int _lastScreen;         
    private float _lastOrtho; 

    public void TileInit(int stageKey, StageManager stageM)
    {
        stageManager = stageM;

        mapData = chapterMapsData[(stageKey / 100) - 1].maps.Find(m => m.mapName == stageKey);
        if (mapData == null)
        {
            Debug.LogError($"StageKey {stageKey} 에 해당하는 맵을 찾을 수 없습니다.");
            return;
        }

        LoadTileData(mapData);


        GenerateBackground(); 
        GenerateFloor(mapData);
        GenerateWalkable(mapData);
        GenerateFence(mapData);
        GenerateOthers(mapData);
        GenerateObjects(mapData);
        PositionPlayer(mapData);

        var cam = Camera.main;
        _lastScreen = new Vector2Int(Screen.width, Screen.height);
        _lastOrtho = cam ? cam.orthographicSize : 0f;
    }

    private void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        if (_lastScreen.x != Screen.width || _lastScreen.y != Screen.height ||
            Mathf.Abs(cam.orthographicSize - _lastOrtho) > 0.001f)
        {
            _lastScreen = new Vector2Int(Screen.width, Screen.height);
            _lastOrtho = cam.orthographicSize;
            GenerateBackground();
        }
    }

    private void LoadTileData(MapData mapData)
    {
        string tilePath = $"Tiles/{mapData.tileSetFolder}/";

        foreach (TileType tileType in System.Enum.GetValues(typeof(TileType)))
        {
            string path = tilePath + tileType.ToString();
            TileData tileData = Resources.Load<TileData>(path);

            if (tileData != null)
            {
                tileDictionary[tileType] = tileData;
            }
            else
            {
                Debug.LogWarning($"타일을 찾을 수 없음: {path}");
            }
        }
    }

    private void GenerateBackground()
    {
        if (!tileDictionary.TryGetValue(TileType.Background, out TileData backgroundTile))
        {
            Debug.LogError("Background 타일없음");
            return;
        }

        if (tilemaps == null || tilemaps.Count == 0 || tilemaps[0] == null)
        {
            Debug.LogError("Background Tilemap (index 0) not found!");
            return;
        }

        Tilemap backgroundLayer = tilemaps[0];
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 bl = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        Vector3Int min = backgroundLayer.WorldToCell(bl) + new Vector3Int(-backgroundBufferCells, -backgroundBufferCells, 0);
        Vector3Int max = backgroundLayer.WorldToCell(tr) + new Vector3Int(backgroundBufferCells, backgroundBufferCells, 0);

        backgroundLayer.ClearAllTiles();

        for (int y = min.y; y <= max.y; y++)
        {
            for (int x = min.x; x <= max.x; x++)
            {
                backgroundLayer.SetTile(new Vector3Int(x, y, 0), backgroundTile.tileBase);
            }
        }

        bgMin = min; bgMax = max; 
        backgroundLayer.RefreshAllTiles();
    }

    private void GenerateFloor(MapData mapData)
    {
        if (!tileDictionary.TryGetValue(TileType.Floor, out TileData floorTile))
        {
            Debug.LogError("Floor 타일없음");
            return;
        }

        Tilemap groundLayer = tilemaps[1];
        groundLayer.ClearAllTiles();

        int startX = 0;
        int startY = mapData.height - 1;

        for (int y = 0; y < mapData.height; y++)
        {
            for (int x = 0; x < mapData.width; x++)
            {
                groundLayer.SetTile(new Vector3Int(startX + x, startY - y, 0), floorTile.tileBase);
            }
        }
        groundLayer.RefreshAllTiles();
    }

    private void GenerateFence(MapData mapData)
    {
        if (!tileDictionary.TryGetValue(TileType.Fence, out TileData fenceTile))
        {
            Debug.LogError("Fence 타일없음");
            return;
        }

        Tilemap fenceLayer = tilemaps[4];
        fenceLayer.ClearAllTiles();

        int startX = 0;
        int startY = mapData.height - 1;

        for (int y = 0; y < mapData.height; y++)
        {
            for (int x = 0; x < mapData.width; x++)
            {
                if (mapData.tiles[y].row[x] == (int)TileType.Fence)
                {
                    fenceLayer.SetTile(new Vector3Int(startX + x, startY - y, 0), fenceTile.tileBase);
                }
            }
        }
    }

    private void GenerateWalkable(MapData mapData)
    {
        if (!tileDictionary.TryGetValue(TileType.Walkable, out TileData walkableTile))
        {
            Debug.LogError("Walkable 타일없음");
            return;
        }

        Tilemap walkableLayer = tilemaps[2];
        walkableLayer.ClearAllTiles();

        int startX = 0;
        int startY = mapData.height - 1;

        for (int y = 0; y < mapData.height; y++)
        {
            for (int x = 0; x < mapData.width; x++)
            {
                int tileTypeValue = mapData.tiles[y].row[x];

                if (y == mapData.height - 1 && tileTypeValue == (int)TileType.Floor)
                    continue;

                if (tileTypeValue == (int)TileType.Block || tileTypeValue == (int)TileType.Floor)
                {
                    walkableLayer.SetTile(new Vector3Int(startX + x, startY - y, 0), walkableTile.tileBase);
                }
            }
        }
    }

    private void GenerateOthers(MapData mapData)
    {
        Tilemap otherTilesLayer = tilemaps[3];
        if (otherTilesLayer == null) { Debug.LogError("OtherTiles Tilemap (index 3) not found!"); return; }
        otherTilesLayer.ClearAllTiles();

        TileData wallTile = tileDictionary.ContainsKey(TileType.Wall) ? tileDictionary[TileType.Wall] : null;
        TileData noneTile = tileDictionary.ContainsKey(TileType.None) ? tileDictionary[TileType.None] : null;

        int startX = 0;
        int startY = mapData.height - 1;

        for (int y = 0; y < mapData.height; y++)
        {
            for (int x = 0; x < mapData.width; x++)
            {
                Vector3Int tilePosInt = new Vector3Int(startX + x, startY - y, 0);
                int tileTypeValue = mapData.tiles[y].row[x];
                TileBase selectedTile = null;

                if (tileTypeValue == (int)TileType.Wall && wallTile != null)
                    selectedTile = wallTile.tileBase;
                else if (tileTypeValue == (int)TileType.None && noneTile != null)
                    selectedTile = noneTile.tileBase;

                if (selectedTile != null)
                {
                    otherTilesLayer.SetTile(tilePosInt, selectedTile);
                }
            }
        }
        otherTilesLayer.RefreshAllTiles();
    }

    private void GenerateObjects(MapData mapData)
    {
        Transform objectHolder = new GameObject("GeneratedObjects").transform;
        objectHolder.SetParent(this.transform);

        int startX = 0;
        int startY = mapData.height - 1;

        Tilemap referenceTilemap = tilemaps.Count > 0 ? tilemaps[0] : null;
        if (referenceTilemap == null) { Debug.LogError("Cannot calculate spawn positions without a reference tilemap!"); return; }

        for (int y = 0; y < mapData.height; y++)
        {
            for (int x = 0; x < mapData.width; x++)
            {
                Vector3Int tilePosInt = new Vector3Int(startX + x, startY - y, 0);
                Vector3 spawnPos = referenceTilemap.GetCellCenterWorld(tilePosInt);
                int tileTypeValue = mapData.tiles[y].row[x];

                GameObject createdObject = null;

                if (tileTypeValue == (int)TileType.Block && blockPrefab != null)
                    createdObject = Instantiate(blockPrefab, spawnPos, Quaternion.identity);
                else if (tileTypeValue == (int)TileType.Tree && TreePrefab != null)
                    createdObject = Instantiate(TreePrefab, spawnPos, Quaternion.identity);
                else if (tileTypeValue == (int)TileType.Farmland && farmlandPrefab != null)
                {
                    createdObject = Instantiate(farmlandPrefab, spawnPos, Quaternion.identity);
                    Farmland farmlandInstance = createdObject.GetComponent<Farmland>();
                    if (farmlandInstance != null && stageManager != null)
                        farmlandInstance.OnWateringComplete.AddListener(() => HandleGameClearAsync());
                }

                if (createdObject != null)
                    createdObject.transform.SetParent(objectHolder);
            }
        }
    }

    private async void HandleGameClearAsync()
    {
        if (stageManager != null)
            await stageManager.GameClear();
    }

    private void PositionPlayer(MapData mapData)
    {
        if (player != null)
        {
            int startX = 0;
            int startY = mapData.height - 1;
            Vector3Int playerStartTile = new Vector3Int(startX + mapData.playerStartPos.x, startY - mapData.playerStartPos.y, 0);
            Tilemap referenceTilemap = tilemaps.Count > 0 ? tilemaps[0] : null;
            if (referenceTilemap != null)
            {
                Vector3 playerWorldPos = referenceTilemap.GetCellCenterWorld(playerStartTile);
                player.transform.position = playerWorldPos;
            }
            else { Debug.LogError("Cannot position player without a reference tilemap!"); }
        }
        else Debug.LogError("Player object is not assigned in TileManager!");
    }
}
