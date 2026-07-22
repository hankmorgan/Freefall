using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
namespace FreeFall
{
    public class TilemapRender
    {

        static bool[,] hasRendered = new bool[513, 513];
        const int FacesPerBatch = 256;
        static Shader tileshader;
        public static void RenderTileMap(int[,] tileheights, int[,] textures, int[] texturecounts)
        {
            Node3D the_tiles = main.instance.GetNode<Node3D>("/root/Freefall/Planet");
            //var a_mesh = new ArrayMesh();
            tileshader = (Shader)ResourceLoader.Load("res://resources/shaders/tnovashader.gdshader");
            int surfIdx = 0;
            //for each tile.
            for (int tex = 0; tex <= texturecounts.GetUpperBound(0); tex++)
            {
                if (texturecounts[tex] > 0)
                {
                    // if (tex == 3)
                    // {
                        Debug.Print($"Drawing texture {tex}");
                        RenderTerrainTile(
                            the_tiles: the_tiles,
                            tileheights: tileheights,
                            textures: textures,
                            textureToDraw: tex,
                            numberOfFaces: texturecounts[tex],
                            surfIdx: surfIdx);
                    //}
                }
            }

            // for (int x = 0; x <= hasRendered.GetUpperBound(0); x++)
            // {
            //     for (int y = 0; y <= hasRendered.GetUpperBound(0); y++)
            //     {
            //         if ((hasRendered[x, y] == false) && (textures[x, y])== 3)
            //         {
            //             Debug.Print($"tile {x},{y} with texture {textures[x, y]} of count {texturecounts[textures[x, y]]} was not rendered");
            //         }
            //     }
            // }
        }


        static void RenderTerrainTile(Node3D the_tiles, int[,] tileheights, int[,] textures, int textureToDraw, int numberOfFaces, int surfIdx)
        {
            //int debugcount = 0;
            //var a_mesh = new ArrayMesh();
            float brushSize = 1.2f;

            int NoOfLoops = numberOfFaces / FacesPerBatch;
            int remainder = numberOfFaces % FacesPerBatch;


            int startx = 0; int starty = 0;
            //int resumex = 0; int resumey = 0;
            while (NoOfLoops >= 0)
            {
                int currNoOfFaces;
                if (NoOfLoops > 0)
                {
                    currNoOfFaces = FacesPerBatch;
                }
                else
                {
                    currNoOfFaces = remainder;
                }

                if (currNoOfFaces == 0)
                {
                    return;
                }

                Vector3[] verts = new Vector3[4 * currNoOfFaces];
                Vector2[] uvs = new Vector2[4 * currNoOfFaces];
                int[] indices = new int[6 * currNoOfFaces];


                // var material = textures[x, y];

                float[] heights = new float[4];
                int FaceCounter = 0;
                bool DrawMesh = false;


                for (int x = startx; x <= 512; x++)
                {
                    //resumex = x;// + 1;
                    for (int y = starty; y <= 512; y++)
                    {
                        // if ((x == 452) && (y==40))
                        // {
                        //     Debug.Print("here");
                        // }
                        // if (x==280)
                        // {
                        //     Debug.Print($"{debugcount++} drawing at {x}, {y}");  
                        // }
                                               

                        //resumey = y;
                        if (textures[x, y] == textureToDraw)
                        {
                            DrawMesh = true;
                            if (hasRendered[x, y] == false)
                            {
                                heights[0] = (float)+tileheights[x, y] / 10f;
                                if (y == 512)
                                {
                                    heights[1] = (float)+tileheights[x, y] / 10f;
                                }
                                else
                                {
                                    heights[1] = (float)+tileheights[x, y + 1] / 10f;
                                }

                                if ((x == 512) || (y == 512))
                                {
                                    heights[2] = (float)+tileheights[x, y] / 10f;
                                }
                                else
                                {
                                    heights[2] = (float)+tileheights[x + 1, y + 1] / 10f;
                                }

                                if (x == 512)
                                {
                                    heights[3] = (float)+tileheights[x, y] / 10f;
                                }
                                else
                                {
                                    heights[3] = (float)+tileheights[x + 1, y] / 10f;
                                }

                                float cornerX = (float)x * brushSize;
                                float cornerY = (float)y * brushSize;

                                verts[0 + (FaceCounter * 4)] = new Vector3(cornerX + 0.0f, heights[0], cornerY + 0.0f); //0,0
                                verts[1 + (FaceCounter * 4)] = new Vector3(cornerX + 0.0f, heights[1], cornerY + brushSize); // 0, 1
                                verts[2 + (FaceCounter * 4)] = new Vector3(cornerX + brushSize, heights[2], cornerY + brushSize); // 1, 1
                                verts[3 + (FaceCounter * 4)] = new Vector3(cornerX + brushSize, heights[3], cornerY + 0.0f); // 1, 0


                                //These will be rotated later in the shader
                                uvs[0 + (FaceCounter * 4)] = new Vector2(1.0f, 0.0f);
                                uvs[1 + (FaceCounter * 4)] = new Vector2(1.0f, 1.0f);
                                uvs[2 + (FaceCounter * 4)] = new Vector2(0.0f, 1.0f);
                                uvs[3 + (FaceCounter * 4)] = new Vector2(0.0f, 0.0f);

                                indices[0 + (FaceCounter * 6)] = 1 + (FaceCounter * 4);
                                indices[1 + (FaceCounter * 6)] = 0 + (FaceCounter * 4);
                                indices[2 + (FaceCounter * 6)] = 2 + (FaceCounter * 4);
                                indices[3 + (FaceCounter * 6)] = 3 + (FaceCounter * 4);
                                indices[4 + (FaceCounter * 6)] = 2 + (FaceCounter * 4);
                                indices[5 + (FaceCounter * 6)] = 0 + (FaceCounter * 4);

                                hasRendered[x, y] = true;

                                FaceCounter++;
                                if (FaceCounter >= currNoOfFaces)
                                {
                                   
                                    goto DoDrawMesh;
                                }
                            }
                        }
                    }
                }

            //resumex = 0; resumey = 0;

            DoDrawMesh:
                if (DrawMesh)
                {
                    DrawMesh = false;
                    var a_mesh = new ArrayMesh();
                    //Debug.Print($"drawing mesh {textureToDraw} {NoOfLoops}");
                    TilemapRender.DrawMesh(
                        the_tiles: the_tiles,
                        textureToDraw: textureToDraw,
                        surfIdx: surfIdx,
                        a_mesh: a_mesh,
                        NoOfLoops: NoOfLoops,
                        verts: verts,
                        uvs: uvs,
                        indices: indices);
                    FaceCounter = 0;
                    NoOfLoops--;
                    // startx = resumex;
                    // starty = resumey;
                }
                else
                {
                    return;
                }

            }//loop
        }

        private static void DrawMesh(Node3D the_tiles, int textureToDraw, int surfIdx, ArrayMesh a_mesh, int NoOfLoops, Vector3[] verts, Vector2[] uvs, int[] indices)
        {
            var normals = new List<Vector3>();
            foreach (var vert in verts)
            {
                normals.Add(vert.Normalized());
            }

            AddSurfaceToMesh(
                verts: verts,
                uvs: uvs,
                textureindex: textureToDraw,
                a_mesh: a_mesh,
                normals: normals,
                indices: indices,
                surfIdx: surfIdx);
            CreateMeshInstance(parent: the_tiles, TileName: $"planettiles_{textureToDraw}_{NoOfLoops}", a_mesh: a_mesh);
        }


        /// <summary>
        /// Adds a surface built from the various uv, vertices and materials arrays to a mesh
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="uvs"></param>
        /// <param name="textureindex"></param>
        /// <param name="FaceCounter"></param>
        /// <param name="a_mesh"></param>
        /// <param name="normals"></param>
        /// <param name="indices"></param>
        private static void AddSurfaceToMesh(Vector3[] verts, Vector2[] uvs, int textureindex, ArrayMesh a_mesh, List<Vector3> normals, int[] indices, int surfIdx)
        {
            var surfaceArray = new Godot.Collections.Array();
            surfaceArray.Resize((int)Mesh.ArrayType.Max);

            surfaceArray[(int)Mesh.ArrayType.Vertex] = verts;
            surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs;
            surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
            surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

            var mat = new ShaderMaterial();
            mat.Shader = tileshader;
            //add the texture          
            mat.SetShaderParameter("texture_albedo", (Texture)TNovaMapLoader.PlanetTextures[textureindex]);
            mat.SetShaderParameter("albedo", new Color(1, 1, 1, 1));
            mat.SetShaderParameter("uv1_scale", new Vector3(1, 1, 1));
            mat.SetShaderParameter("uv2_scale", new Vector3(1, 1, 1));
            mat.SetShaderParameter("UseAlpha", false);


            //TODO add params for tile rotation, shade etc.
            // mat.SetShaderParameter("tileflags", tileflags & 0xFF); //tileflags
            // mat.SetShaderParameter("objectindex_lowerbytes", tilex & 0xFF); //tilex
            // mat.SetShaderParameter("objectindex_upperbytes", tiley & 0xFF); // tiley
            //Add the new surface to the mesh
            a_mesh.AddSurfaceFromArrays(primitive: Mesh.PrimitiveType.Triangles, arrays: surfaceArray);
            a_mesh.SurfaceSetMaterial(
                surfIdx: surfIdx,
                material: mat
                );
        }


        private static Node3D CreateMeshInstance(Node3D parent, string TileName, ArrayMesh a_mesh)
        {
            var final_mesh = new MeshInstance3D();
            parent.AddChild(final_mesh);
            final_mesh.Position = Vector3.Zero;//new Vector3(x * 1.2f, 0.0f, y * 1.2f);
            final_mesh.Name = TileName;
            final_mesh.Mesh = a_mesh;

            //Debug.Print($"Meshname is {final_mesh.Name}");
            return final_mesh;
        }

    }//end class
}//end namesace