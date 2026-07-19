
using System.IO;
using Godot;


namespace FreeFall
{

    public class TNovaMapLoader : Loader
    {
        //public static string path = "c:\\games\\tnova\\TNF108\\MAPS\\MAP1.RES";

        const int chunkToLoad = 86;

        public static bool BuildTNovaMap(string sourcearkfile, string outputfilename)
        {
            int[,] height = new int[513, 513];
            int[,] texture = new int[513, 513];
            //float brushSize = 12f;
            byte[] archive_ark;
            if (ReadStreamFile(sourcearkfile, out archive_ark))
            {
                long address_pointer = 0;
                Resloader.Chunk lev_ark;
                if (!Resloader.LoadChunk(archive_ark, chunkToLoad, out lev_ark))
                {
                    return false;
                }

                address_pointer = 0;
                int meshcount = 1;
                long maxHeight = 0; long minHeight = 0;
                for (int x = 0; x <= height.GetUpperBound(0); x++)
                {
                    for (int y = 0; y <= height.GetUpperBound(1); y++)
                    {
                        meshcount++;
                        int byte0 = (int)getAt(lev_ark.data, address_pointer++, 8);//Texture
                        int byte1 = (int)getAt(lev_ark.data, address_pointer++, 8);//Rotation and part of height
                        int byte2 = (int)getAt(lev_ark.data, address_pointer++, 8);//Object object list index?

                        //this is old code. sbyte will probably work better here.
                        if (byte0 > 191)
                            byte0 = byte0 - 64;
                        if (byte0 > 127)
                            byte0 = byte0 - 128;
                        if (byte0 > 63)
                            byte0 = byte0 - 64;

                        texture[x, y] = byte0;

                        byte1 = byte1 & 0xF0;         //AND with 11110000b: remove shadow+rotation in lower half of byte
                        height[x, y] = (byte2 << 4) | (byte1 >> 4);
                        if (byte2 > 0x7F)            //negative height
                        { height[x, y] = height[x, y] - 4096; }
                        //height[x,y] =height[x,y] + 2048;
                        if ((x == 0) && (y == 0))
                        {
                            maxHeight = height[x, y];
                            minHeight = height[x, y];
                        }
                        if (height[x, y] > maxHeight)
                        {
                            maxHeight = height[x, y];
                        }
                        if (height[x, y] < minHeight)
                        {
                            minHeight = height[x, y];
                        }
                    }
                }
                var img = Godot.Image.CreateEmpty(513, 513, false, Image.Format.Rf);
                //export as a height map
                var normalisemaxheight = (float)(maxHeight - minHeight);
                for (int x = 0; x <= height.GetUpperBound(0); x++)
                {
                    for (int y = 0; y <= height.GetUpperBound(1); y++)
                    {
                        var normaliseheight = (float)(height[x,y] - minHeight);
                        var color = new Godot.Color(r: normaliseheight / normalisemaxheight, g: 0, b: 0);
                        img.SetPixel(x,y,color);
                    }
                }

                img.SavePng(outputfilename);



                // GameObject Tile = new GameObject("TNOVAMAP_" + sectionX + "_" + sectionY);
                // Tile.transform.parent = this.gameObject.transform;
                // Tile.transform.position = Vector3.zero;
                // Tile.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

                // MeshFilter mf = Tile.AddComponent<MeshFilter>();
                //MeshRenderer mr = Tile.AddComponent<MeshRenderer>();
                //  MeshCollider mc = Tile.AddComponent<MeshCollider>();
                // mc.sharedMesh=null;
                //Mesh mesh = new Mesh();
                // meshcount = 64 * 64;
                // mesh.subMeshCount=64;//meshcount;//Should be no of visible faces
                //mesh.subMeshCount = textureUsageCounter;
                //Vector3[] verts = new Vector3[meshcount * 4];
                //Vector2[] uvs = new Vector2[meshcount * 4];

                //  int FaceCounter = 0;

                // textureCount[texture[x,y]]++;//Tracks how many of each texture there is
                // float[] heights = new float[4];
                // heights[0] = (float)-height[x, y];
                // heights[1] = (float)-height[x, y + 1];
                // heights[2] = (float)-height[x + 1, y + 1];
                // heights[3] = (float)-height[x + 1, y];

                //Allocate enough verticea and UVs for the faces

                // float cornerX = (float)x * brushSize;
                // float cornerY = (float)y * brushSize;
                // verts[0 + (4 * FaceCounter)] = new Vector3(cornerX + 0.0f, cornerY + 0.0f, heights[0]);
                // verts[1 + (4 * FaceCounter)] = new Vector3(cornerX + 0.0f, cornerY + brushSize, heights[1]);
                // verts[2 + (4 * FaceCounter)] = new Vector3(cornerX + brushSize, cornerY + brushSize, heights[2]);
                // verts[3 + (4 * FaceCounter)] = new Vector3(cornerX + brushSize, cornerY + 0.0f, heights[3]);

                // //Allocate UVs
                // uvs[0 + (4 * FaceCounter)] = new Vector2(0.0f, 0.0f);
                // uvs[1 + (4 * FaceCounter)] = new Vector2(0.0f, 1.0f);
                // uvs[2 + (4 * FaceCounter)] = new Vector2(1.0f, 1.0f);
                // uvs[3 + (4 * FaceCounter)] = new Vector2(1.0f, 0.0f);

                // FaceCounter++;
                // mesh.vertices = verts;
                // mesh.uv = uvs;
                return true;

            }
            else
            {
                return false;
            }
        }

        public static bool DumpPlanet(string planetresfile, string planetname)
        {

            var GreyScaleIndexPalette = new Palette();
            for (int i = 0; i <= GreyScaleIndexPalette.blue.GetUpperBound(0); i++)
            {
                GreyScaleIndexPalette.red[i] = (byte)i;
                GreyScaleIndexPalette.blue[i] = 0;
                GreyScaleIndexPalette.green[i] = 0;              
            }

            byte[] archive_ark;
            if (ReadStreamFile(planetresfile, out archive_ark))
            {
                Resloader.Chunk tex_ark;
                if (!Resloader.LoadChunk(archive_ark, 48, out tex_ark))
                {
                    return false;
                }
                int addr_ptr = 0;
                for (int i = 0; i<=63;i++)
                {
                    var img = Artloader.Image(databuffer: tex_ark.data, dataOffSet: addr_ptr, width: 64, height: 64, palette: GreyScaleIndexPalette, useAlphaChannel: false, useSingleRedChannel: true );
                    img.GetImage().SavePng($"c:\\temp\\tnova\\textures\\{planetname}_{i.ToString("d2")}.png");
                    addr_ptr += (64*64);
                }


                //File.WriteAllBytes("c:\\temp\\tnova\\test.dat", tex_ark.data);
            }
            return true;
        }
    }//end class
}//end 