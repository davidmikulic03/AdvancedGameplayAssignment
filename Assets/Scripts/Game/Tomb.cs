using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace Game
{
    // a bit iffy level format found here: https://opentomb.github.io/TRosettaStone3/trosettastone.html
    // combine it with the info here: https://github.com/XProger/OpenLara/blob/master/src/platform/gba/packer/TR1_PC.h#L99
    // ... and perhaps you can make sense of it :)

    [ExecuteInEditMode]
    public class Tomb : MonoBehaviour
    {
        class TRTexture
        {
            public byte[] Tile;

            public TRTexture(BinaryReader br)
            {
                Tile = br.ReadBytes(256 * 256);
            }
        }

        struct TRRoomInfo
        {
            public int  x;             // X-offset of room (world coordinates)
            public int  z;             // Z-offset of room (world coordinates)
            public int  yBottom;
            public int  yTop;

            public TRRoomInfo(BinaryReader br)
            {
                x = br.ReadInt32();
                z = br.ReadInt32();
                yBottom = br.ReadInt32();
                yTop = br.ReadInt32();
            }
        };

        struct Vertex   // 6 bytes
        {
            public short    x;
            public short    y;
            public short    z;

            public Vertex(BinaryReader br)
            {
                x = br.ReadInt16();
                y = br.ReadInt16();
                z = br.ReadInt16();
            }
        };

        struct RoomVertex  // 8 bytes
        {
            public Vertex   Vertex;
            public short    Lighting;

            public RoomVertex(BinaryReader br)
            {
                Vertex = new Vertex(br);
                Lighting = br.ReadInt16();
            }
        };

        class TRFace3    // 8 bytes
        {
            public ushort[]     Vertices;
            public ushort       Texture;

            public TRFace3(BinaryReader br)
            {
                Vertices = new ushort[] { br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16() };
                Texture = br.ReadUInt16();
            }
        };

        class TRFace4    // 12 bytes
        {
            public ushort[]     Vertices;
            public ushort       Texture;

            public TRFace4(BinaryReader br)
            {
                Vertices = new ushort[] { br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16() };
                Texture = br.ReadUInt16();
            }

            public Vector3 CalculateNormal(TRRoomInfo info, RoomVertex[] vertices, out Vector3 vCenter)
            {
                Vector3[] corners = System.Array.ConvertAll(Vertices, i => TRRoomData.ToWorld(vertices[i], info));

                Plane p1 = new Plane(corners[0], corners[1], corners[2]);
                Plane p2 = new Plane(corners[0], corners[2], corners[3]);

                vCenter = (corners[0] + corners[1] + corners[2] + corners[3]) * 0.25f;

                return Vector3.Normalize(p1.normal + p2.normal);
            }
        };

        class TRRoomData
        {
            public short NumVertices;               // Number of vertices in the following list
            public RoomVertex[] Vertices;           // List of vertices (relative coordinates)

            public short NumRectangles;             // Number of textured rectangles
            public TRFace4[] Rectangles;            // List of textured rectangles

            public short NumTriangles;              // Number of textured triangles
            public TRFace3[] Triangles;             // List of textured triangles

            /*short NumSprites;                     // Number of sprites
            tr_room_sprite Sprites[NumSprites];     // List of sprites*/

            public TRRoomData(BinaryReader br)
            {
                NumVertices = br.ReadInt16();
                if (NumVertices > 0)
                {
                    Vertices = new RoomVertex[NumVertices];
                    for (int i = 0; i < NumVertices; i++)
                    {
                        Vertices[i] = new RoomVertex(br);
                    }
                }

                NumRectangles = br.ReadInt16();
                if (NumRectangles > 0)
                {
                    Rectangles = new TRFace4[NumRectangles];
                    for (int i = 0; i < NumRectangles; i++)
                    {
                        Rectangles[i] = new TRFace4(br);
                    }
                }

                NumTriangles = br.ReadInt16();
                if (NumTriangles > 0)
                {
                    Triangles = new TRFace3[NumTriangles];
                    for (int i = 0; i < NumTriangles; i++)
                    {
                        Triangles[i] = new TRFace3(br);
                    }
                }

                int iNumSprites = br.ReadInt16();
                if (iNumSprites > 0)
                {
                    br.ReadBytes(iNumSprites * 4);
                }
            }

            public static Vector3 ToWorld(RoomVertex v, TRRoomInfo info)
            {
                Vector3Int vv = new Vector3Int(info.x + v.Vertex.x, v.Vertex.y, info.z + v.Vertex.z);
                return new Vector3(vv.x / SCALE, vv.y / -SCALE, vv.z / SCALE);
            }

            public Vector3 AddToMesh(TRRoomInfo info, TRObjectTexture[] objTextures, List<Vector3> vertices, List<Vector2> uvs, List<int>[] triangles)
            {
                Vector3[] vertexPositions = System.Array.ConvertAll(Vertices, v =>
                {
                    Vector3Int vv = new Vector3Int(info.x + v.Vertex.x, v.Vertex.y, info.z + v.Vertex.z);
                    return new Vector3(vv.x / SCALE, vv.y / -SCALE, vv.z / SCALE);
                });

                // calculate room center
                Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3 vMax = new Vector3(-float.MaxValue, -float.MaxValue, -float.MaxValue);
                Vector3 vCenter = Vector3.zero;
                foreach (Vector3 v in vertexPositions)
                {
                    vCenter += v;
                }
                vCenter /= (vertexPositions.Length > 0 ? vertexPositions.Length : 1);

                if (Rectangles != null)
                {
                    foreach (TRFace4 quad in Rectangles)
                    {
                        // add quad vertices
                        int iStart = vertices.Count;
                        vertices.AddRange(System.Array.ConvertAll(quad.Vertices, i => vertexPositions[i]));

                        TRObjectTexture objTexture = objTextures[quad.Texture];
                        int iTexture = objTexture.Tile & 0x7fff;
                        objTexture.AddUVs(uvs, 4);

                        // add quad triangles
                        triangles[iTexture].AddRange(new int[]{
                            iStart + 0, iStart + 1, iStart + 2,
                            iStart + 0, iStart + 2, iStart + 3
                        });
                    }
                }

                if (Triangles != null)
                {
                    foreach (TRFace3 tri in Triangles)
                    {
                        // add triangle vertices
                        int iStart = vertices.Count;
                        vertices.AddRange(System.Array.ConvertAll(tri.Vertices, i => vertexPositions[i]));

                        TRObjectTexture objTexture = objTextures[tri.Texture];
                        int iTexture = objTexture.Tile & 0x7fff;
                        objTexture.AddUVs(uvs, 3);

                        // add triangle 
                        triangles[0].AddRange(new int[]{
                            iStart + 0, iStart + 1, iStart + 2,
                        });
                    }
                }

                return vCenter;
            }
        };

        class TRRoomLight                   // 18 bytes
        {
            public int x, y, z;            // Position of light, in world coordinates
            public ushort Intensity;      // Light intensity
            public uint Fade;             // Falloff value

            public TRRoomLight(BinaryReader br)
            {
                x = br.ReadInt32();
                y = br.ReadInt32();
                z = br.ReadInt32();
                Intensity = br.ReadUInt16();
                Fade = br.ReadUInt32();
            }

            public void CreateLight(Transform parent, Vector3 vOffset)
            {
                GameObject go = new GameObject("Light");
                go.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
                go.transform.parent = parent;
                go.transform.position = new Vector3(x / SCALE, -y / SCALE, z / SCALE) + vOffset;
                Light light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.shadows = LightShadows.Soft;
                light.intensity = (Intensity / (float)0x1FFF) * 2.0f;
                //light.range = (Fade / (float)0x7FFF) * SCALE;
                light.range = Fade / SCALE;
            }
        };

        class TRRoom
        {
            TRRoomInfo Info;               // Where the room exists, in world coordinates

            uint NumDataWords;       // Number of data words (uint16_t's)
            //ushort[]  Data;               // The raw data from which the rest of this is derived

            TRRoomData RoomData;           // The room mesh

            /*
            uint16_t NumPortals;                 // Number of visibility portals to other rooms
            tr_room_portal Portals[NumPortals];  // List of visibility portals

            uint16_t NumZsectors;                                  // ``Width'' of sector list
            uint16_t NumXsectors;                                  // ``Height'' of sector list
            tr_room_sector SectorList[NumXsectors * NumZsectors];  // List of sectors in this room
            */

            short AmbientIntensity;

            ushort NumLights;                   // Number of lights in this room
            TRRoomLight[] Lights;               // List of lights

            /*
            uint16_t NumStaticMeshes;                            // Number of static meshes
            tr2_room_staticmesh StaticMeshes[NumStaticMeshes];   // List of static meshes

            int16_t AlternateRoom;
            int16_t Flags;*/

            Vector3 m_vRoomCenter;

            #region Properties

            public Vector3 RoomCenter => m_vRoomCenter;

            #endregion

            public TRRoom(BinaryReader br)
            {
                // get room data
                Info = new TRRoomInfo(br);

                NumDataWords = br.ReadUInt32();
                RoomData = new TRRoomData(br);

                int iNumPortals = br.ReadUInt16();
                br.ReadBytes(iNumPortals * 32);

                int NumZsectors = br.ReadUInt16();
                int NumXsectors = br.ReadUInt16();
                br.ReadBytes(NumZsectors * NumXsectors * 8);

                int iAmbientLight = br.ReadInt16();
                NumLights = br.ReadUInt16();
                Lights = new TRRoomLight[NumLights];
                for (int i = 0; i < NumLights; i++)
                {
                    Lights[i] = new TRRoomLight(br);
                }

                int iNumStaticMeshes = br.ReadInt16();
                br.ReadBytes(iNumStaticMeshes * 18);

                int iAlternateRoom = br.ReadInt16();
                int iFlags = br.ReadInt16();
            }

            public void AddToMesh(TRObjectTexture[] objTextures, List<Vector3> vertices, List<Vector2> uvs, List<int>[] triangles)
            {
                m_vRoomCenter = RoomData.AddToMesh(Info, objTextures, vertices, uvs, triangles);
            }

            public void CreateLights(Transform parent, Vector3 vOffset)
            {
                foreach (TRRoomLight trl in Lights)
                {
                    trl.CreateLight(parent, vOffset);
                }
            }

            public void CreateLedges(Vector3 vOffset)
            {
                // build vertex to quad lookup
                Dictionary<ushort, List<TRFace4>> vertexToQuads = new Dictionary<ushort, List<TRFace4>>();
                foreach (TRFace4 quad in RoomData.Rectangles)
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        ushort v = quad.Vertices[i];
                        if (!vertexToQuads.ContainsKey(v))
                        {
                            vertexToQuads[v] = new List<TRFace4>();
                        }
                        vertexToQuads[v].Add(quad);
                    }
                }

                // search through all quads
                foreach (TRFace4 quad in RoomData.Rectangles)
                {
                    Vector3 vCenter1;
                    Vector3 vNormal1 = quad.CalculateNormal(Info, RoomData.Vertices, out vCenter1);
                    if (Vector3.Dot(Vector3.up, vNormal1) > 0.2f)
                    {
                        continue;
                    }

                    // loop over edges
                    for (int i = 0; i < 4; ++i)
                    {
                        ushort v1 = quad.Vertices[i];
                        ushort v2 = quad.Vertices[(i + 1) % 4];

                        TRFace4 neighbor = vertexToQuads[v1].Find(q =>
                        {
                            return q != quad &&
                                   System.Array.IndexOf(q.Vertices, v1) >= 0 &&
                                   System.Array.IndexOf(q.Vertices, v2) >= 0;
                        });

                        // got neighbor?
                        if (neighbor != null)
                        {
                            Vector3 vCenter2;
                            Vector3 vNormal2 = neighbor.CalculateNormal(Info, RoomData.Vertices, out vCenter2);
                            if (Vector3.Dot(Vector3.up, vNormal2) > 0.85f &&
                                vCenter2.y > vCenter1.y)
                            {
                                Vector3 vA = TRRoomData.ToWorld(RoomData.Vertices[v1], Info) + vOffset;
                                Vector3 vB = TRRoomData.ToWorld(RoomData.Vertices[v2], Info) + vOffset;

                                // vA & vB are the 2 points of the Ledge Segment
                                Ledge newLedge = new Ledge(vA, vB, vNormal1);
                                InteractionManager.Instance.AddInteraction(newLedge);
                            }
                        }
                    }
                }
            }
        };

        class TRObjectTexture
        {
            public ushort Attribute;
            public ushort Tile;
            public Vector2[] UVs;

            public TRObjectTexture(BinaryReader br)
            {
                Attribute = br.ReadUInt16();
                Tile = br.ReadUInt16();

                uint[] values = new uint[] { br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32() };
                UVs = System.Array.ConvertAll(values, v =>
                {
                    byte xh = (byte)(v & 0xFF);          // Byte 0 (LSB)
                    byte xl = (byte)((v >> 8) & 0xFF);   // Byte 1
                    byte yh = (byte)((v >> 16) & 0xFF);  // Byte 2
                    byte yl = (byte)((v >> 24) & 0xFF);  // Byte 3 (MSB)

                    float fX = xl / 255.0f;
                    float fY = yl / 255.0f;

                    return new Vector2(fX, fY);
                });
            }

            public void AddUVs(List<Vector2> uvs, int iCount)
            {
                for (int i = 0; i < iCount; ++i)
                {
                    uvs.Add(UVs[i]);
                }
            }
        }

        private const float SCALE = 512.0f;

        [SerializeField]
        public TextAsset    m_levelFile;

        private Bounds      m_bounds;

        #region Properties

        public Bounds Bounds => m_bounds;

        #endregion

        private void OnEnable()
        {
            ImportLevel();
        }

        public void ImportLevel()
        {
            if (m_levelFile == null)
            {
                Debug.LogError("No Level file specified");
                return;
            }

            MemoryStream ms = new MemoryStream(m_levelFile.bytes);
            BinaryReader br = new BinaryReader(ms);

            uint iVersion = br.ReadUInt32();

            // read textures
            uint iNumTextures = br.ReadUInt32();
            TRTexture[] textures = new TRTexture[iNumTextures];
            for (int i = 0; i < iNumTextures; i++)
            {
                textures[i] = new TRTexture(br);
            }

            uint iUnused = br.ReadUInt32();

            // read rooms
            short iNumRooms = br.ReadInt16();
            TRRoom[] rooms = new TRRoom[iNumRooms];
            for (int i = 0; i < iNumRooms; i++)
            {
                rooms[i] = new TRRoom(br);
            }

            // floors
            int iFloorCount = br.ReadInt32();
            br.ReadBytes(iFloorCount * 2);

            // meshdata
            int iMeshDataCount = br.ReadInt32();
            br.ReadBytes(iMeshDataCount * 2);
            int iMeshOffsetCount = br.ReadInt32();
            br.ReadBytes(iMeshOffsetCount * 4);

            // animation
            int iAnimationCount = br.ReadInt32();
            br.ReadBytes(iAnimationCount * 32);
            int iAnimationStateCount = br.ReadInt32();
            br.ReadBytes(iAnimationStateCount * 6);
            int iAnimationRangeCount = br.ReadInt32();
            br.ReadBytes(iAnimationRangeCount * 8);

            // commands
            int iCommandCount = br.ReadInt32();
            br.ReadBytes(iCommandCount * 2);
            int iNodeDataSize = br.ReadInt32();
            br.ReadBytes(iNodeDataSize * 4);
            int iFrameDataSize = br.ReadInt32();
            br.ReadBytes(iFrameDataSize * 2);

            // models
            int iModelCount = br.ReadInt32();
            br.ReadBytes(iModelCount * 18);

            // static meshes
            int iStaticMeshCount = br.ReadInt32();
            br.ReadBytes(iStaticMeshCount * 32);

            // object textures
            int iObjTxtCount = br.ReadInt32();
            TRObjectTexture[] objectTextures = new TRObjectTexture[iObjTxtCount];
            for (int i = 0; i < iObjTxtCount; i++)
            {
                objectTextures[i] = new TRObjectTexture(br);
            }

            // sprite textures
            int iSpriteTxtCount = br.ReadInt32();
            br.ReadBytes(iSpriteTxtCount * 16);
            int iSpriteSequenceCount = br.ReadInt32();
            br.ReadBytes(iSpriteSequenceCount * 8);

            // cameras
            int iCameraCount = br.ReadInt32();
            br.ReadBytes(iCameraCount * 16);

            // sound sources
            int iSoundSourceCount = br.ReadInt32();
            br.ReadBytes(iSoundSourceCount * 16);

            // boxes?
            int iBoxesCount = br.ReadInt32();
            br.ReadBytes(iBoxesCount * 20);
            int iOverlapCount = br.ReadInt32();
            br.ReadBytes(iOverlapCount * 2);

            // zones x2
            br.ReadBytes(6 * iBoxesCount * 2);

            // animated textures
            int iAnimatedTexturesCount = br.ReadInt32();
            br.ReadBytes(iAnimatedTexturesCount * 2);

            // items
            int iItemCount = br.ReadInt32();
            br.ReadBytes(iItemCount * 22);

            // lightmap
            br.ReadBytes(32 * 256);

            // palette!
            byte[] paletteData = br.ReadBytes(256 * 3);
            Color[] palette = new Color[256];
            for (int i = 0; i < 256; ++i)
            {
                palette[i] = new Color32((byte)(paletteData[i * 3 + 0] * 4),
                                         (byte)(paletteData[i * 3 + 1] * 4),
                                         (byte)(paletteData[i * 3 + 2] * 4), 
                                         255);
            }

            // cleanup
            br.Close();
            ms.Close();

            // create textures and materials
            Material[] materials = new Material[iNumTextures];
            Material tombMaterial = Resources.Load<Material>("Materials/TombMaterial");
            for (int i = 0; i < iNumTextures; ++i)
            {
                Texture2D tex = new Texture2D(256, 256);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Point;
                for (int y = 0; y < 256; ++y)
                {
                    for (int x = 0; x < 256; ++x)
                    {
                        tex.SetPixel(x, y, palette[textures[i].Tile[y * 256 + x]]);
                    }
                }
                tex.Apply();
                Material m = new Material(tombMaterial);
                m.mainTexture = tex;
                materials[i] = m;
            }

            // get mesh data from rooms
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int>[] triangles = System.Array.ConvertAll(textures, t => new List<int>());
            foreach(TRRoom room in rooms)
            {
                room.AddToMesh(objectTextures, vertices, uvs, triangles);
            }

            // place room #0 in origo
            vertices = vertices.ConvertAll(v => v - rooms[0].RoomCenter);

            // create mesh
            Mesh mesh = new Mesh();
            mesh.name = "Tomb";
            mesh.subMeshCount = (int)iNumTextures;
            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();
            for (int i = 0; i < iNumTextures; i++)
            {
                mesh.SetTriangles(triangles[i].ToArray(), i);
            }
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            // calculate smooth normals
            Vector3[] normals = mesh.normals;
            Dictionary<Vector3, Vector3> normalLookup = new Dictionary<Vector3, Vector3>();
            for (int i = 0; i < vertices.Count; ++i)
            {
                Vector3 vNormal;
                if (!normalLookup.TryGetValue(vertices[i], out vNormal))
                {
                    vNormal = Vector3.zero;
                }
                vNormal += normals[i];
                normalLookup[vertices[i]] = vNormal;
            }
            mesh.normals = vertices.ConvertAll(v => normalLookup[v].normalized).ToArray();

            // destroy old level?
            Transform oldLevel = transform.Find("Level");
            if (oldLevel != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(oldLevel.gameObject);
                }
                else
                {
                    DestroyImmediate(oldLevel.gameObject);
                }
            }

            // create level gameobject
            GameObject go = new GameObject("Level");
            go.transform.parent = transform;
            go.transform.localScale = Vector3.one;
            go.layer = gameObject.layer;
            go.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials = materials;
            go.AddComponent<MeshCollider>().sharedMesh = mesh;
            m_bounds = go.GetComponent<MeshRenderer>().bounds;

            // create lights
            foreach (TRRoom room in rooms)
            {
                room.CreateLights(go.transform, -rooms[0].RoomCenter);

                if (Application.isPlaying)
                {
                    room.CreateLedges(-rooms[0].RoomCenter);
                }
            }

            // create minimap
            CreateMinimapRepresentation(mesh);
        }

        private void CreateMinimapRepresentation(Mesh mesh)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            Dictionary<int, int> vertexLookup = new Dictionary<int, int>();

            Vector3[] oldVertices = mesh.vertices;
            int[] oldTriangles = mesh.triangles;
            for (int i = 0; i < oldTriangles.Length; i += 3)
            {
                Vector3 v1 = oldVertices[oldTriangles[i + 0]];
                Vector3 v2 = oldVertices[oldTriangles[i + 1]];
                Vector3 v3 = oldVertices[oldTriangles[i + 2]];

                // keep the triangle?
                Plane p = new Plane(v1, v2, v3);
                if (Vector3.Dot(p.normal, Vector3.up) > 0.7f)
                {
                    for (int j = 0; j < 3; ++j)
                    {
                        int iOldIndex = oldTriangles[i + j];
                        int iIndex;
                        if (!vertexLookup.TryGetValue(iOldIndex, out iIndex))
                        {
                            iIndex = vertices.Count;
                            vertices.Add(oldVertices[iOldIndex]);
                            vertexLookup[iOldIndex] = iIndex;
                        }

                        triangles.Add(iIndex);
                    }
                }
            }

            // create mesh
            Mesh minimapMesh = new Mesh();
            minimapMesh.name = "Minimap Mesh";
            minimapMesh.hideFlags = HideFlags.DontSave;
            minimapMesh.vertices = vertices.ToArray();
            minimapMesh.triangles = triangles.ToArray();
            minimapMesh.RecalculateBounds();
            minimapMesh.RecalculateNormals();

            // create minimap renderer game object
            GameObject go = new GameObject("Minimap");
            go.transform.parent = transform;
            go.transform.localScale = Vector3.one;
            go.layer = LayerMask.NameToLayer("Minimap");
            go.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            go.AddComponent<MeshFilter>().sharedMesh = minimapMesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("Materials/Minimap");
        }
    }
}