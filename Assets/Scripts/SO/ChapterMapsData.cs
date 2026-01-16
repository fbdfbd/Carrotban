using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/ChapterMapsData")]
public class ChapterMapsData : ScriptableObject
{
    public int chapterId;
    public List<MapData> maps;
}
