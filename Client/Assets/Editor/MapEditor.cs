using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using System.Collections.Generic;

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
            Tilemap tmBase = Util.FindChild<Tilemap>(go, "Tilemap_Base", true);
            Tilemap tmCol = Util.FindChild<Tilemap>(go, "Tilemap_Collision", true);

            // 콜리전 텍스트파일 맵 생성
            using (var writer = File.CreateText($"{pathPrefix}/{go.name}.txt"))
            {
                writer.WriteLine(tmBase.cellBounds.xMin);
                writer.WriteLine(tmBase.cellBounds.xMax - 1);
                writer.WriteLine(tmBase.cellBounds.yMin);
                writer.WriteLine(tmBase.cellBounds.yMax - 1);

                for (int y = tmBase.cellBounds.yMax - 1; y >= tmBase.cellBounds.yMin; y--)
                {
                    for (int x = tmBase.cellBounds.xMin; x <= tmBase.cellBounds.xMax - 1; x++)
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
                writer.WriteLine(tmBase.cellBounds.xMin);
                writer.WriteLine(tmBase.cellBounds.xMax - 1);
                writer.WriteLine(tmBase.cellBounds.yMin);
                writer.WriteLine(tmBase.cellBounds.yMax - 1);

                // 부쉬 타일 맵 오브젝트들을 모두 가져온다.
                List<Tilemap> bushMaps = new List<Tilemap>();

                foreach (Transform child in go.transform)
                {
                    if (child.name == "Tilemap_Bush")
                    {
                        bushMaps.Add(child.GetComponent<Tilemap>());
                    }
                }

                // 타일 맵을 순회
                for (int y = tmBase.cellBounds.yMax - 1; y >= tmBase.cellBounds.yMin; y--)
                {
                    for (int x = tmBase.cellBounds.xMin; x <= tmBase.cellBounds.xMax - 1; x++)
                    {
                        bool isThereBush = false;

                        for (int i = 1; i <= bushMaps.Count; i++)
                        {
                            Tilemap tileMap = bushMaps[i - 1];

                            TileBase tile = tileMap.GetTile(new Vector3Int(x, y, 0));
                            if (tile != null)
                            {
                                writer.Write(i);
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
                writer.WriteLine(tmBase.cellBounds.xMin);
                writer.WriteLine(tmBase.cellBounds.xMax - 1);
                writer.WriteLine(tmBase.cellBounds.yMin);
                writer.WriteLine(tmBase.cellBounds.yMax - 1);

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

                for (int y = tmBase.cellBounds.yMax - 1; y >= tmBase.cellBounds.yMin; y--)
                {
                    for (int x = tmBase.cellBounds.xMin; x <= tmBase.cellBounds.xMax - 1; x++)
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
