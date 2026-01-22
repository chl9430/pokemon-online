using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine.UI;
using static UnityEngine.GridBrushBase;
using System.Text;





#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapEditor
{
#if UNITY_EDITOR
    [MenuItem("Tools/GenerateMap %fg")]
    private static void GenerateMap()
    {
        GenerateByPath("Assets/Resources/Map");
        GenerateByPath("../Common/MapData");
    }

    private static void GenerateByPath(string pathPrefix)
    {
        GameObject[] gameObjects = Resources.LoadAll<GameObject>("Prefabs/Map");

        foreach (GameObject go in gameObjects)
        {
            Grid grid = Util.FindChild<Grid>(go, "Grid", true);

            Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>();

            // 1. 모든 타일맵을 순회하며 전체 영역(Bounds) 계산
            BoundsInt totalBounds = new BoundsInt();
            bool firstBounds = true;

            foreach (var tm in tilemaps)
            {
                // 타일이 없는 빈 타일맵은 무시
                if (!tm.HasTile(tm.cellBounds.position))
                {
                    // CompressBounds를 호출하면 실제 타일이 있는 영역으로 크기가 줄어듭니다.
                    tm.CompressBounds();
                }

                if (firstBounds)
                {
                    totalBounds = tm.cellBounds;
                    firstBounds = false;
                }
                else
                {
                    // 기존 영역에 현재 타일맵의 영역을 합침
                    totalBounds.xMin = Mathf.Min(totalBounds.xMin, tm.cellBounds.xMin);
                    totalBounds.yMin = Mathf.Min(totalBounds.yMin, tm.cellBounds.yMin);
                    totalBounds.xMax = Mathf.Max(totalBounds.xMax, tm.cellBounds.xMax);
                    totalBounds.yMax = Mathf.Max(totalBounds.yMax, tm.cellBounds.yMax);
                }
            }

            int width = totalBounds.size.x;
            int height = totalBounds.size.y;
            string[,] mapData = new string[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    mapData[x, y] = "0";

            // 3. 타일 데이터 기록
            foreach (var tm in tilemaps)
            {
                string layerName = tm.name;
                string valueToStore = layerName;

                if (layerName.Contains("Collision")) valueToStore = "1";
                else if (layerName.Contains("Door"))
                {
                    string[] parts = layerName.Split('_');
                    valueToStore = parts.Length > 1 ? parts[1] : parts[0];
                }
                else if (layerName.Contains("Bush"))
                {
                    string[] parts = layerName.Split('_');
                    valueToStore = parts.Length > 1 ? parts[1] : parts[0];
                }
                else if (layerName.Contains("Base")) continue;
                else if (layerName.Contains("Env")) continue;

                foreach (var pos in totalBounds.allPositionsWithin)
                    {
                        if (tm.HasTile(pos))
                        {
                            int arrayX = pos.x - totalBounds.xMin;
                            int arrayY = pos.y - totalBounds.yMin;

                            mapData[arrayX, arrayY] = valueToStore;
                        }
                    }

                StringBuilder sb = new StringBuilder();

                sb.AppendLine(totalBounds.xMin.ToString());
                sb.AppendLine((totalBounds.xMax - 1).ToString());
                sb.AppendLine(totalBounds.yMin.ToString());
                sb.AppendLine((totalBounds.yMax - 1).ToString());

                for (int y = height - 1; y >= 0; y--)
                {
                    for (int x = 0; x < width; x++)
                    {
                        sb.Append(mapData[x, y]);
                        if (x < width - 1) sb.Append(", ");
                    }
                    sb.AppendLine();
                }

                string path = $"{pathPrefix}/{go.name}.txt";
                File.WriteAllText(path, sb.ToString());
            }

            // NPC 정보를 담는 파일 생성
            using (var writer = File.CreateText($"{pathPrefix}/{go.name}_NPCMap.txt"))
            {
                CreatureController[] npcs = go.GetComponentsInChildren<CreatureController>();
                writer.WriteLine(npcs.Length);

                foreach (CreatureController npc in npcs)
                {
                    ObjectContents content = npc.GetComponent<ObjectContents>();
                    SpriteRenderer sprite = npc.GetComponent<SpriteRenderer>();
                    Vector3 npcPos = npc.transform.position - new Vector3(0.5f, 0.5f);
                    Vector3Int cellPos = grid.WorldToCell(npcPos);
                    string contentType = "";
                    NPCType npcType = NPCType.NoneType;
                    string spriteName = npc.GetComponent<SpriteRenderer>().sprite.name;
                    MoveDir moveDir = MoveDir.Up;

                    string dirNum = spriteName.Substring(spriteName.LastIndexOf('_') + 1);

                    if (dirNum == "0")
                        moveDir = MoveDir.Down;
                    else if (dirNum == "1")
                        moveDir = MoveDir.Up;
                    else if (dirNum == "2")
                        moveDir = MoveDir.Left;
                    else if (dirNum == "3")
                        moveDir = MoveDir.Right;

                    if (content is TrainerContent)
                        contentType = "Trainer";
                    else
                        contentType = "NPC";

                    if (sprite.sprite.name.Contains(NPCType.SchoolBoy.ToString()))
                        npcType = NPCType.SchoolBoy;

                    writer.Write(npc.name + " " + cellPos.x + ", " + cellPos.y + ", " + cellPos.z + ", " + contentType + ", " + npcType + ", " + moveDir);
                }
            }
        }
    }
#endif
}
