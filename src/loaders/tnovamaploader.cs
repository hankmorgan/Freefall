using System.IO;
using Godot;


namespace FreeFall
{

    public class TNovaMapLoader : Loader
    {
        //public static string path = "c:\\games\\tnova\\TNF108\\MAPS\\MAP1.RES";
        public static ImageTexture[] PlanetTextures = new ImageTexture[64];
        const int chunkToLoad = 86;
        public static int[,] height = new int[513, 513];
        public static int[,] texture = new int[513, 513];
        public static int[,] rotations = new int[513, 513];
        public static int[,] texturecounter = new int[63, 4]; //counts usages of each texture x rotation

        public static bool LoadTNovaMap(string sourcearkfile, string outputfilename = "")
        {

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
                //for (int x = 0; x <=height.GetUpperBound(0); x++)

                for (int y = 0; y <= height.GetUpperBound(1); y++)
                //for (int y = height.GetUpperBound(1); y >= 0; y--)
                {
                    //for (int x = height.GetUpperBound(0); x >=0; x--) 
                    for (int x = 0; x <= height.GetUpperBound(0); x++)
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

                        var rot = (byte1 >> 2) & 0x3;
                        rotations[x, y] = rot;
                        //var shade = byte1 & 0x3;
                        texturecounter[byte0, rot]++;

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
                if (outputfilename != "")
                {
                    var img = Godot.Image.CreateEmpty(513, 513, false, Image.Format.Rf);
                    //export as a height map
                    var normalisemaxheight = (float)(maxHeight - minHeight);
                    for (int x = 0; x <= height.GetUpperBound(0); x++)
                    {
                        for (int y = 0; y <= height.GetUpperBound(1); y++)
                        {
                            var normaliseheight = (float)(height[x, y] - minHeight);
                            var color = new Godot.Color(r: normaliseheight / normalisemaxheight, g: 0, b: 0);
                            color = new Color(r: (float)texture[x, y] * 8f / 255f, g: color.G, b: (float)rotations[x, y] * 32f / 255f);
                            img.SetPixel(x, y, color);
                        }
                    }
                    img.SetPixel(0, 0, new Color(0, 0, 0));
                    img.SavePng(outputfilename);
                }
                return true;

            }
            else
            {
                return false;
            }
        }

        public static bool LoadPlanetTextures(string planetresfile, string planetname, Palette overridepal = null, string palettename = "PLNT0.PAL")
        {
            var p = File.ReadAllBytes($"C:\\Games\\TNOVA\\{palettename}");
            //var p = ResourceLoader.Load($"res://resources/palettes/{palettename}"); //TODO figure out how to load the bundled .PAL (or ideally find the palette.)

            Palette GreyScaleIndexPalette;// = new Palette();
            if (overridepal == null)
            {
                GreyScaleIndexPalette = new Palette();
                for (int i = 0; i <= GreyScaleIndexPalette.blue.GetUpperBound(0); i++)
                {
                    GreyScaleIndexPalette.red[i] = p[(i * 3) + 0];// (byte)i;
                    GreyScaleIndexPalette.green[i] = p[(i * 3) + 1];// 0;
                    GreyScaleIndexPalette.blue[i] = p[(i * 3) + 2];// 0;                              
                }
            }
            else
            {
                GreyScaleIndexPalette = overridepal;
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
                var debugcolor = new Color(b: 255, r: 0, g: 0);
                for (int i = 0; i <= 63; i++)
                {
                    var img = Artloader.Image(databuffer: tex_ark.data, dataOffSet: addr_ptr, width: 64, height: 64, palette: GreyScaleIndexPalette, useAlphaChannel: false, useSingleRedChannel: false);
                    img.GetImage().SavePng($"c:\\temp\\tnova\\textures\\{planetname}_{i.ToString("d2")}.png");
                    addr_ptr += (64 * 64);
                    // var baseimg = img.GetImage();
                    // for (int x = 0; x < 64; x++)
                    // {
                    //     baseimg.SetPixel(x, 0, debugcolor);
                    // }
                    // baseimg.SetPixel(1, 1, debugcolor);
                    // for (int x = 0; x < 10; x++)
                    // {
                    //     baseimg.SetPixel(x, 2, debugcolor);
                    // }
                    // baseimg.SetPixel(2, 2, debugcolor);
                    // baseimg.SetPixel(3, 3, debugcolor);
                    // baseimg.SetPixel(4, 4, debugcolor);
                    // for (int y = 0; y < 32; y++)
                    // {
                    //     baseimg.SetPixel(0, y, debugcolor);
                    // }
                    // var tex = new ImageTexture();
                    // tex.SetImage(baseimg);
                    PlanetTextures[i] = img;
                }
            }
            return true;
        }
    }//end class
}//end 