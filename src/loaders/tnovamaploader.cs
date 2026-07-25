using System.IO;
using Godot;


namespace FreeFall
{
    public class TNovaMap
    {
        /// <summary>
        /// The height of the corner of a tile.
        /// </summary>
        public int[,] height;// = new int[513, 513];

        /// <summary>
        /// The texture as mapped in RESLNTx.RES
        /// </summary>
        public int[,] texture;// = new int[513, 513];

        /// <summary>
        /// The cardinal direction of a tile.
        /// </summary>
        public int[,] rotations;// = new int[513, 513];

        /// <summary>
        /// counts usages of each texture and rotation
        /// </summary>
        public int[,] texturecounter = new int[63, 4];

        public int maxHeight = 0;
        public int minHeight = 0;

        /// <summary>
        /// Size in meters of a tile
        /// </summary>
        public float UnitSize = 6;

        /// <summary>
        /// The upper array index that the height map will render.
        /// </summary>
        public int MapUpperBound;//= 512;



        /// <summary>
        /// For HiRes use size = 513
        /// </summary>
        /// <param name="size"></param>
        public TNovaMap(bool HiRes)
        {
            var size = 513;   
       
            if (!HiRes)
            {
                size = 257; 
                UnitSize = 24f;
            }
            height = new int[size, size];
            texture = new int[size, size];
            rotations = new int[size, size];
            MapUpperBound = size - 1;            
        }
    }

    public class TNovaMapLoader : Loader
    {
        //public static string path = "c:\\games\\tnova\\TNF108\\MAPS\\MAP1.RES";
        public static ImageTexture[] PlanetTextures = new ImageTexture[64];
        public static ImageTexture[] SkyTexture = new ImageTexture[1];
        //public static int SkyHeight;

        const int HiResMapChunk = 86;
        const int LoResMapChunk = 85;


        public static TNovaMap LoadTNovaMap(string sourcearkfile, bool HiRes = true, string outputfilename = "")
        {
            var map = new TNovaMap(HiRes);
            var chunktoLoad = HiResMapChunk;
            if (!HiRes)
            {
                chunktoLoad = LoResMapChunk;
            }
            //float brushSize = 12f;
            byte[] archive_ark;
            if (ReadStreamFile(sourcearkfile, out archive_ark))
            {
                long address_pointer = 0;
                Resloader.Chunk lev_ark;
                if (!Resloader.LoadChunk(archive_ark: archive_ark, chunkNo: chunktoLoad, data_ark: out lev_ark))
                {
                    return null;
                }

                address_pointer = 0;
                int meshcount = 1;

                for (int y = 0; y <= map.height.GetUpperBound(1); y++)
                {
                    for (int x = 0; x <= map.height.GetUpperBound(0); x++)
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

                        map.texture[x, y] = byte0;

                        var rot = (byte1 >> 2) & 0x3;
                        map.rotations[x, y] = rot;
                        //var shade = byte1 & 0x3;
                        map.texturecounter[byte0, rot]++;

                        byte1 = byte1 & 0xF0;         //AND with 11110000b: remove shadow+rotation in lower half of byte
                        map.height[x, y] = (byte2 << 4) | (byte1 >> 4);
                        if (byte2 > 0x7F)            //negative height
                        { map.height[x, y] = map.height[x, y] - 4096; }
                        //height[x,y] =height[x,y] + 2048;
                        if ((x == 0) && (y == 0))
                        {
                            map.maxHeight = map.height[x, y];
                            map.minHeight = map.height[x, y];
                        }
                        if (map.height[x, y] > map.maxHeight)
                        {
                            map.maxHeight = map.height[x, y];
                        }
                        if (map.height[x, y] < map.minHeight)
                        {
                            map.minHeight = map.height[x, y];
                        }
                    }
                }
                //SkyHeight = map.maxHeight + 256;//temp
                if (outputfilename != "")
                {
                    //export as a height map
                    var img = Godot.Image.CreateEmpty(map.MapUpperBound + 1, map.MapUpperBound + 1, false, Image.Format.Rf);                    
                    var normalisemaxheight = (float)(map.maxHeight - map.minHeight);
                    for (int x = 0; x <= map.height.GetUpperBound(0); x++)
                    {
                        for (int y = 0; y <= map.height.GetUpperBound(1); y++)
                        {
                            var normaliseheight = (float)(map.height[x, y] - map.minHeight);
                            var color = new Godot.Color(r: normaliseheight / normalisemaxheight, g: 0, b: 0);
                            //color = new Color(r: (float)map.texture[x, y] * 8f / 255f, g: color.G, b: (float)map.rotations[x, y] * 32f / 255f);
                            img.SetPixel(x, y, color);
                        }
                    }
                    img.SetPixel(0, 0, new Color(0, 0, 0));
                    img.SavePng(outputfilename);
                }
                return map;

            }
            else
            {
                return null;
            }
        }


        public static bool LoadSky(string skyresfile, string skyname, Palette overridepal = null, string palettename = "PLNT0.PAL")
        {
            Palette TexturePalette = PickPalette(overridepal, palettename);

            byte[] archive_ark;

            if (ReadStreamFile(skyresfile, out archive_ark))
            {
                Resloader.Chunk tex_ark;
                if (!Resloader.LoadChunk(archive_ark, 152, out tex_ark))
                {
                    return false;
                }

                File.WriteAllBytes($"c:\\temp\\tnova\\textures\\skyname_152.dat", tex_ark.data);
                int addr_ptr = 0;
                if (tex_ark.chunkCompressionType == 2)
                {
                    //the data is in a uncompressed subdir. I'm not sure currently of the header format of subdirs. but I do not the sky is a 256*256 image at offset 0x52 within the subchunk
                    addr_ptr += 0x52;
                }
                //var debugcolor = new Color(b: 255, r: 0, g: 0);
                var img = Artloader.Image(databuffer: tex_ark.data, dataOffSet: addr_ptr, width: 256, height: 256, palette: TexturePalette, useAlphaChannel: false, useSingleRedChannel: false);
                img.GetImage().SavePng($"c:\\temp\\tnova\\textures\\{skyname}.png");
                SkyTexture[0] = img;
            }
            return true;
        }

        public static bool LoadPlanetTextures(string planetresfile, string planetname, Palette overridepal = null, string palettename = "PLNT0.PAL")
        {
            Palette TexturePalette = PickPalette(overridepal, palettename);

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
                    var img = Artloader.Image(databuffer: tex_ark.data, dataOffSet: addr_ptr, width: 64, height: 64, palette: TexturePalette, useAlphaChannel: false, useSingleRedChannel: false);
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

        private static Palette PickPalette(Palette overridepal, string palettename)
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

            return GreyScaleIndexPalette;
        }
    }//end class
}//end 