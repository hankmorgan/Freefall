using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
namespace FreeFall
{
    public class TilemapRender
    {

        static Shader tileshader;
        public static void RenderTileMap(int[,] tileheights, int[,] textures)
        {
            Node3D the_tiles = main.instance.GetNode<Node3D>("/root/Freefall/Planet");
            tileshader = (Shader)ResourceLoader.Load("res://resources/shaders/tnovashader.gdshader");
            //for each tile.
            for (int x = 0; x < tileheights.GetUpperBound(0); x++)
            {
                for (int y = 0; y < tileheights.GetUpperBound(0); y++)
                {
                    if ((x >= 64) && (x <= 256) && (y >= 64) && (y <= 256))
                    {
                        // Debug.Print($"{x},{y}");
                        RenderTerrainTile(parent: the_tiles, x: x, y: y, tileheights: tileheights, textures: textures);
                    }
                }
            }
        }


        static Node3D RenderTerrainTile(Node3D parent, int x, int y, int[,] tileheights, int[,] textures)
        {
            float brushSize = 1.2f;
            Vector3[] verts = new Vector3[4];
            Vector2[] uvs = new Vector2[4];
            int[] indices = new int[6];

            var a_mesh = new ArrayMesh();
           // var material = textures[x, y];

            float[] heights = new float[4];
            heights[0] = (float)+tileheights[x, y] /10f;
            heights[1] = (float)+tileheights[x, y + 1]/10f;
            heights[2] = (float)+tileheights[x + 1, y + 1]/10f;
            heights[3] = (float)+tileheights[x + 1, y]/10f;

            float cornerX = (float)x * brushSize;
            float cornerY = (float)y * brushSize;

            verts[0] = new Vector3(cornerX + 0.0f, heights[0], cornerY + 0.0f); //0,0
            verts[1] = new Vector3(cornerX + 0.0f, heights[1], cornerY + brushSize); // 0, 1
            verts[2] = new Vector3(cornerX + brushSize, heights[2], cornerY + brushSize); // 1, 1
            verts[3] = new Vector3(cornerX + brushSize, heights[3], cornerY + 0.0f); // 1, 0

            //These will be rotated later in the shader
            uvs[2] = new Vector2(0.0f, 1.0f);
            uvs[1] = new Vector2(1.0f, 1.0f);
            uvs[0] = new Vector2(1.0f, 0.0f);
            uvs[3] = new Vector2(0.0f, 0.0f);

            indices[0] = 1;
            indices[1] = 0;
            indices[2] = 2;
            indices[3] = 3;
            indices[4] = 2;
            indices[5] = 0;

            var normals = new List<Vector3>();
            foreach (var vert in verts)
            {
                normals.Add(vert.Normalized());
            }

            AddSurfaceToMesh(
                verts: verts,
                uvs: uvs,
                textureindex: textures[x, y],
                a_mesh: a_mesh,
                normals: normals,
                indices: indices);

            return CreateMeshInstance(parent, x, y, $"tile{x.ToString("d3")}_{y.ToString("d3")}", a_mesh);

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
        private static void AddSurfaceToMesh(Vector3[] verts, Vector2[] uvs, int textureindex, ArrayMesh a_mesh, List<Vector3> normals, int[] indices)
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
            a_mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
            a_mesh.SurfaceSetMaterial(
                surfIdx: 0,
                material: mat
                );
        }


        private static Node3D CreateMeshInstance(Node3D parent, int x, int y, string TileName, ArrayMesh a_mesh)
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