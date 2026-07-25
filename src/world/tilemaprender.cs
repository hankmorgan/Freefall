using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
namespace FreeFall
{
    public class TilemapRender
    {        
        const int FacesPerBatch = 256;
        //const float brushSize = 1.2f;
        const float HeightScale = 12f;
        static Shader tileshader;
        public static void RenderTileMap(TNovaMap map, string Root, bool renderSky = false, float PositionAdjustment = 0f)
        {
            Node3D the_tiles = main.instance.GetNode<Node3D>(Root);//"/root/Freefall/Planet");
            tileshader = (Shader)ResourceLoader.Load("res://resources/shaders/tnovashader.gdshader");
            //for each tile.
            for (int tex = 0; tex <= map.texturecounter.GetUpperBound(0); tex++)
            {
                for (int rot = 0; rot <=  map.texturecounter.GetUpperBound(1); rot++)
                {
                    if ( map.texturecounter[tex, rot] > 0)
                    {
                        RenderTerrainSurface(
                            the_tiles: the_tiles,
                            map: map,
                            textureToDraw: tex,
                            rotation: rot, 
                            PositionAdjustment: PositionAdjustment );
                    }
                }
            }
            if (renderSky)
            {
                RenderSky(the_tiles, map.maxHeight + 256);    
            }
            
        }

        static void RenderSky(Node3D the_tiles, int SkyHeight)
        {           

            //Allocate enough verticea and UVs for the faces
            Vector3[] verts = new Vector3[4];
            Vector2[] uvs = new Vector2[4];
            float baseHeight = (float)(SkyHeight / HeightScale);

            //Now allocate the visible faces to triangles.
            int FaceCounter = 0;//Tracks which number face we are now on.



            verts[0 + (4 * FaceCounter)] = new Vector3(0f - 1024f, baseHeight,512f + (1.2f * 512f));
            verts[1 + (4 * FaceCounter)] = new Vector3(0f - 1024f, baseHeight, 0f - 1024f);
            verts[2 + (4 * FaceCounter)] = new Vector3(1024f + (1.2f * 512f), baseHeight, 0f - 1024f);
            verts[3 + (4 * FaceCounter)] = new Vector3(1024f + (1.2f * 512f), baseHeight, 1024f + (1.2f * 512f));
            //Change default UVs
            uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, 0.0f);
            uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, 1.0f * 4);
            uvs[2 + (4 * FaceCounter)] = new Vector2(4, 1.0f * 4);
            uvs[3 + (4 * FaceCounter)] = new Vector2(4, 0.0f);

            var normals = new List<Vector3>();
            foreach (var vert in verts)
            {
                normals.Add(vert.Normalized());
            }

            //Apply the uvs and create my tris

            FaceCounter = 0;
            int[] indices = new int[6];
            indices[0] = 2 + (4 * FaceCounter);
            indices[1] = 1 + (4 * FaceCounter);
            indices[2] = 0 + (4 * FaceCounter);
            indices[3] = 3 + (4 * FaceCounter);
            indices[4] = 2 + (4 * FaceCounter);
            indices[5] = 0 + (4 * FaceCounter);

            
            var a_mesh = new ArrayMesh();
            TilemapRender.DrawMesh(
                the_tiles: the_tiles,
                textureToDraw:0,
                textures: TNovaMapLoader.SkyTexture,
                surfIdx: 0,
                a_mesh: a_mesh,
                NoOfLoops: 0,
                verts: verts,
                uvs: uvs,
                indices: indices);
        }


        static void RenderTerrainSurface(Node3D the_tiles, TNovaMap map, int textureToDraw, int rotation, float PositionAdjustment)
        {
            float brushSize = map.UnitSize;
            int numberOfFaces = map.texturecounter[textureToDraw, rotation];
            int NoOfLoops = numberOfFaces / FacesPerBatch;
            int remainder = numberOfFaces % FacesPerBatch;

            int startx = 0; int starty = 0;

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


                float[] heights = new float[4];
                int FaceCounter = 0;
                bool DrawMesh = false;


                for (int x = startx; x <= map.MapUpperBound; x++)
                {
                    for (int y = starty; y <=  map.MapUpperBound; y++)
                    {
                        if ((map.texture[x, y] == textureToDraw) && (map.rotations[x,y] == rotation))
                        {
                            DrawMesh = true;
                            if (map.hasRendered[x, y] == false)
                            {
                                heights[0] = (float)+map.height[x, y] / HeightScale;
                                if (y ==  map.MapUpperBound)
                                {
                                    heights[1] = (float)+map.height[x, y] / HeightScale;
                                }
                                else
                                {
                                    heights[1] = (float)+map.height[x, y + 1] / HeightScale;
                                }

                                if ((x == map.MapUpperBound) || (y == map.MapUpperBound))
                                {
                                    heights[2] = (float)+map.height[x, y] / HeightScale;
                                }
                                else
                                {
                                    heights[2] = (float)+map.height[x + 1, y + 1] / HeightScale;
                                }

                                if (x == map.MapUpperBound)
                                {
                                    heights[3] = (float)+map.height[x, y] / HeightScale;
                                }
                                else
                                {
                                    heights[3] = (float)+map.height[x + 1, y] / HeightScale;
                                }

                                float cornerX = PositionAdjustment + (float)x * brushSize;
                                float cornerY = PositionAdjustment + (float)y * brushSize;

                                verts[0 + (FaceCounter * 4)] = new Vector3(cornerX + 0.0f, heights[0], cornerY + 0.0f); //0,0
                                verts[1 + (FaceCounter * 4)] = new Vector3(cornerX + 0.0f, heights[1], cornerY + brushSize); // 0, 1
                                verts[2 + (FaceCounter * 4)] = new Vector3(cornerX + brushSize, heights[2], cornerY + brushSize); // 1, 1
                                verts[3 + (FaceCounter * 4)] = new Vector3(cornerX + brushSize, heights[3], cornerY + 0.0f); // 1, 0


                                switch (rotation)
                                {
                                    case 3:
                                        uvs[2 + (FaceCounter * 4)] = new Vector2(0.0f, 0.0f);
                                        uvs[3 + (FaceCounter * 4)] = new Vector2(+1.0f, 0.0f);
                                        uvs[0 + (FaceCounter * 4)] = new Vector2(+1.0f, -1.0f);
                                        uvs[1 + (FaceCounter * 4)] = new Vector2(0.0f, -1.0f);
                                        break;
                                    case 2:
                                        uvs[3 + (FaceCounter * 4)] = new Vector2(0.0f, 0.0f);
                                        uvs[0 + (FaceCounter * 4)] = new Vector2(+1.0f, 0.0f);
                                        uvs[1 + (FaceCounter * 4)] = new Vector2(+1.0f, -1.0f);
                                        uvs[2 + (FaceCounter * 4)] = new Vector2(0.0f, -1.0f);
                                        break;
                                    case 1:
                                        uvs[0 + (FaceCounter * 4)] = new Vector2(0.0f, 0.0f);
                                        uvs[1 + (FaceCounter * 4)] = new Vector2(+1.0f, 0.0f);
                                        uvs[2 + (FaceCounter * 4)] = new Vector2(+1.0f, -1.0f);
                                        uvs[3 + (FaceCounter * 4)] = new Vector2(0.0f, -1.0f);
                                        break;
                                    case 0:
                                    default:
                                        uvs[1 + (FaceCounter * 4)] = new Vector2(0.0f, 0.0f);
                                        uvs[2 + (FaceCounter * 4)] = new Vector2(+1.0f, 0.0f);
                                        uvs[3 + (FaceCounter * 4)] = new Vector2(+1.0f, -1.0f);
                                        uvs[0 + (FaceCounter * 4)] = new Vector2(0.0f, -1.0f);
                                        break;
                                }


                                indices[0 + (FaceCounter * 6)] = 1 + (FaceCounter * 4);
                                indices[1 + (FaceCounter * 6)] = 0 + (FaceCounter * 4);
                                indices[2 + (FaceCounter * 6)] = 2 + (FaceCounter * 4);
                                indices[3 + (FaceCounter * 6)] = 3 + (FaceCounter * 4);
                                indices[4 + (FaceCounter * 6)] = 2 + (FaceCounter * 4);
                                indices[5 + (FaceCounter * 6)] = 0 + (FaceCounter * 4);

                                map.hasRendered[x, y] = true;

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
                        textures: TNovaMapLoader.PlanetTextures,
                        surfIdx: 0,
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

        private static void DrawMesh(Node3D the_tiles, int textureToDraw, ImageTexture[] textures, int surfIdx, ArrayMesh a_mesh, int NoOfLoops, Vector3[] verts, Vector2[] uvs, int[] indices)
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
                textures: textures,
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
        private static void AddSurfaceToMesh(Vector3[] verts, Vector2[] uvs, ImageTexture[] textures, int textureindex, ArrayMesh a_mesh, List<Vector3> normals, int[] indices, int surfIdx )
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
            mat.SetShaderParameter("texture_albedo", (Texture)textures[textureindex]);
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