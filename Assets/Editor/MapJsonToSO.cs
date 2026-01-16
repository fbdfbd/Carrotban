using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

public static class MapJsonToSO
{
    private const string jsonPathDefault = "Assets/Maps.json";
    private const string outRootDefault = "Assets/SO/Maps";

    [MenuItem("Tools/Maps/MapJsonToSO")]
    public static void Run()
    {
        string jsonPath = jsonPathDefault;
        string outRoot = outRootDefault;

        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"JSON 파일이 없습니다: {jsonPath}");
            return;
        }

        string mapAssetDir = Path.Combine(outRoot, "MapAssets").Replace("\\", "/");
        string chapterAssetDir = Path.Combine(outRoot, "Chapters").Replace("\\", "/");
        Directory.CreateDirectory(mapAssetDir);
        Directory.CreateDirectory(chapterAssetDir);

        GameMaps data;
        try
        {
            string json = File.ReadAllText(jsonPath);
            data = JsonConvert.DeserializeObject<GameMaps>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON 파싱 실패: {e.Message}");
            return;
        }

        if (data?.maps == null || data.maps.Count == 0)
        {
            Debug.LogError("JSON에 maps가 없거나 비어있습니다.");
            return;
        }

        var bucket = new Dictionary<int, List<MapData>>();

        foreach (var m in data.maps)
        {
            int stageKey;
            if (!TryGetStageKey(m, out stageKey))
            {
                Debug.LogError($"map.name을 정수로 변환할 수 없습니다: {m.name}");
                continue;
            }

            if (m.width <= 0 || m.height <= 0)
            {
                Debug.LogError($"맵 크기가 유효하지 않습니다. stageKey:{stageKey}, width:{m.width}, height:{m.height}");
                continue;
            }

            if (m.tiles == null || m.tiles.Count != m.height)
            {
                Debug.LogError($"tiles 행 수가 height와 다릅니다. stageKey:{stageKey}");
                continue;
            }

            string mapAssetPath = Path.Combine(mapAssetDir, $"{stageKey}.asset").Replace("\\", "/");
            MapData mapSO = AssetDatabase.LoadAssetAtPath<MapData>(mapAssetPath);
            if (mapSO == null)
            {
                mapSO = ScriptableObject.CreateInstance<MapData>();
                AssetDatabase.CreateAsset(mapSO, mapAssetPath);
            }

            mapSO.mapName = stageKey;
            mapSO.width = m.width;
            mapSO.height = m.height;
            mapSO.tileSetFolder = m.tileSetFolder ?? string.Empty;

            var start = m.playerStartPos ?? m.playerStart ?? new Vec2I { x = 0, y = 0 };
            mapSO.playerStartPos = new Vector2Int(start.x, start.y);

            if (mapSO.tiles == null) mapSO.tiles = new List<TileRow>();
            mapSO.tiles.Clear();

            for (int y = 0; y < m.height; y++)
            {
                var rowData = m.tiles[y];
                var row = new TileRow { row = new List<int>(m.width) };

                if (rowData == null || rowData.Count != m.width)
                {
                    Debug.LogError($"stageKey:{stageKey} tiles[{y}] 폭이 width와 다릅니다. 부족분은 0으로 채웁니다.");
                    for (int x = 0; x < m.width; x++) row.row.Add((rowData != null && x < rowData.Count) ? rowData[x] : 0);
                }
                else
                {
                    row.row.AddRange(rowData);
                }

                mapSO.tiles.Add(row);
            }

            EditorUtility.SetDirty(mapSO);

            int chapter = Mathf.Max(1, stageKey / 100);
            if (!bucket.ContainsKey(chapter)) bucket[chapter] = new List<MapData>();
            bucket[chapter].Add(mapSO);
        }

        foreach (var kv in bucket)
        {
            int chapterId = kv.Key;
            var list = kv.Value.OrderBy(m => m.mapName).ToList();

            string chapterAssetPath = Path.Combine(chapterAssetDir, $"Chapter_{chapterId}.asset").Replace("\\", "/");
            ChapterMapsData ch = AssetDatabase.LoadAssetAtPath<ChapterMapsData>(chapterAssetPath);

            if (ch == null)
            {
                ch = ScriptableObject.CreateInstance<ChapterMapsData>();
                ch.chapterId = chapterId;
                ch.maps = new List<MapData>(list);
                AssetDatabase.CreateAsset(ch, chapterAssetPath);
            }
            else
            {
                ch.chapterId = chapterId;
                if (ch.maps == null) ch.maps = new List<MapData>();
                ch.maps.Clear();
                ch.maps.AddRange(list);
                EditorUtility.SetDirty(ch);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"맵 {bucket.Sum(b => b.Value.Count)}개, 챕터 {bucket.Count}개 업데이트");
    }

    [System.Serializable]
    private class GameMaps
    {
        public List<MapJson> maps;
    }

    [System.Serializable]
    private class MapJson
    {
        public string name;                 
        public int width;
        public int height;

        public Vec2I playerStart;         
        public Vec2I playerStartPos;       
        public string tileSetFolder;

        public List<List<int>> tiles;    
    }

    [System.Serializable]
    private class Vec2I { public int x; public int y; }

    private static bool TryGetStageKey(MapJson m, out int stageKey)
    {
        if (!string.IsNullOrEmpty(m.name) && int.TryParse(m.name, out stageKey)) return true;

        stageKey = 0;
        return false;
    }
}
