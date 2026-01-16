using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine.UI;



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
            Tilemap tmBase = Util.FindChild<Tilemap>(go, "Tilemap_Base", true);
            Tilemap tmCol = Util.FindChild<Tilemap>(go, "Tilemap_Collision", true);

            int xMin = tmBase.cellBounds.xMin;
            int xMax = tmBase.cellBounds.xMax - 1;
            int yMin = tmBase.cellBounds.yMin;
            int yMax = tmBase.cellBounds.yMax - 1;

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

            // 콜리전 텍스트파일 맵 생성
            using (var writer = File.CreateText($"{pathPrefix}/{go.name}.txt"))
            {
                writer.WriteLine(xMin);
                writer.WriteLine(xMax);
                writer.WriteLine(yMin);
                writer.WriteLine(yMax);

                for (int y = yMax; y >= yMin; y--)
                {
                    for (int x = xMin; x <= xMax; x++)
                    {
                        
                        TileBase tile = tmCol.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
                            writer.Write("1");
                        else
                            writer.Write("0");
                    }
                    writer.WriteLine();
                }
            }

            // 부쉬 타일의 정보를 담는 파일을 생성
            using (var writer = File.CreateText($"{pathPrefix}/{go.name}_BushMap.txt"))
            {
                writer.WriteLine(xMin);
                writer.WriteLine(xMax);
                writer.WriteLine(yMin);
                writer.WriteLine(yMax);

                // 부쉬 타일 맵 오브젝트들을 모두 가져온다.
                List<Tilemap> bushMaps = new List<Tilemap>();

                int bushId = 1;
                while (true)
                {
                    Tilemap tileMap = Util.FindChild<Tilemap>(go, $"Tilemap_Bush_{bushId}", true);

                    if (tileMap != null)
                    {
                        bushMaps.Add(tileMap);
                        bushId++;
                    }
                    else
                        break;
                }

                // 타일 맵을 순회
                for (int y = yMax; y >= yMin; y--)
                {
                    for (int x = xMin; x <= xMax; x++)
                    {
                        bool isThereBush = false;

                        for (int i = 0; i < bushMaps.Count; i++)
                        {
                            Tilemap bushMap = bushMaps[i];

                            TileBase bushTile = bushMap.GetTile(new Vector3Int(x, y, 0));
                            if (bushTile != null)
                            {
                                string name = bushMap.gameObject.name;

                                int bushNum = int.Parse(name.Substring(name.Length - 1));
                                writer.Write(bushNum);
                                isThereBush = true;
                                break;
                            }
                        }

                        if (!isThereBush)
                            writer.Write(0);
                    }
                    writer.WriteLine();
                }
            }

            // 문 타일의 정보를 담는 파일을 생성
            using (var writer = File.CreateText($"{pathPrefix}/{go.name}_DoorMap.txt"))
            {
                writer.WriteLine(xMin);
                writer.WriteLine(xMax);
                writer.WriteLine(yMin);
                writer.WriteLine(yMax);

                List<Tilemap> doorMaps = new List<Tilemap>();

                int doorId = 1;
                while (true)
                {
                    Tilemap tileMap = Util.FindChild<Tilemap>(go, $"Tilemap_Door_{doorId}", true);

                    if (tileMap != null)
                    {
                        doorMaps.Add(tileMap);
                        doorId++;
                    }
                    else
                        break;
                }

                for (int y = yMax; y >= yMin; y--)
                {
                    for (int x = xMin; x <= xMax; x++)
                    {
                        bool isThereDoor = false;

                        for (int i = 0; i < doorMaps.Count; i++)
                        {
                            Tilemap doorMap = doorMaps[i];

                            TileBase doorTile = doorMap.GetTile(new Vector3Int(x, y, 0));
                            if (doorTile != null)
                            {
                                string name = doorMap.gameObject.name;

                                int doorNum = int.Parse(name.Substring(name.Length - 1));
                                writer.Write(doorNum);
                                isThereDoor = true;
                                break;
                            }
                        }

                        if (!isThereDoor)
                            writer.Write(0);
                    }
                    writer.WriteLine();
                }
            }
        }
    }
#endif
}
