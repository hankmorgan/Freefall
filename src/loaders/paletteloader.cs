using System.IO;

namespace FreeFall
{    
    //nearest palette matches. these match the first 300 or so bytes of the palette but the rest of the data does not match the palettes that were sourced from the gigamap extractor plnt0.pal. Where the palette data currently resides is a mystery.
    //I've ran a byte level search on every res file and unpacked contents and the 
    // my only theory is the palette is dynamically generated at runtime by combining data from various chunks. The logical location would be in the RESPLNTx.RES files likely candidate is chunk 50 in that res file since that data has a byte level match of some of the palette data.
    //Palette for planet0 is in Resgame/392 at offset 0x8D78D (in file), offset 0x7525 in subchunk
    //Palette for planet1 is in Resgame/351 at offset 0x18FA (in file), offset 0xE0 in subchunk
    //Palette for planet2 is in Resgame/392 at offset 0x8D78D (infile), offset 0x7525 in subchunk
    //Palette for planet3 is in Resgame/392 at offset 0x8D78D (infile), offset 0x7525 in subchunk
    //Palette for planet4 is in Resgame/392 at offset 0x8D78D (infile), offset 0x7525 in subchunk

    public class PaletteLoader : Loader
    {
        public static Palette[] palettes;
        public static void LoadPalette(string palettefile, int chunkid, int paletteno)
        {
            byte[] palette_res;
            if (ReadStreamFile(palettefile, out palette_res))
            {
                Resloader.Chunk pal_ark;
                if (Resloader.LoadChunk(palette_res, chunkid, out pal_ark))
                {
                    if (pal_ark.chunkCompressionType == 2)
                    {
                        // var NoOfEntries = getAt16(pal_ark.data, 0);
                        // palettes = new Palette[NoOfEntries];
                        // var addr_ptr = 2;
                        // for (int i = 0; i < NoOfEntries - 1; i++)
                        // {
                        //     var offset = getAt32(pal_ark.data, addr_ptr);
                        //     Debug.Print($"subblock {i} at {offset}");
                        //     var rawdata = new byte[256 * 3];
                        //     //create the palette.
                        //     palettes[i] = new Palette();
                        //     var cadd = 0;
                        //     for (int c = 0; c < 256; c++)
                        //     {
                        //         palettes[i].red[c] = pal_ark.data[offset + cadd + 0];
                        //         palettes[i].blue[c] = pal_ark.data[offset + cadd + 1];
                        //         palettes[i].green[c] = pal_ark.data[offset + cadd + 2];

                        //         rawdata[(c*3) + 0] = pal_ark.data[offset + cadd + 0];
                        //         rawdata[(c*3) + 1] = pal_ark.data[offset + cadd + 1];
                        //         rawdata[(c*3) + 2] = pal_ark.data[offset + cadd + 2];

                        //         palettes[i].alpha[c] = 255;
                        //         cadd += 3;
                        //     }
                        //     File.WriteAllBytes($"C:\\Temp\\tnova\\palettes\\palette{i.ToString("d2")}.dat", rawdata);
                        //     var img = palettes[i].toImage(1);
                        //     img.GetImage().SavePng($"C:\\Temp\\tnova\\palettes\\palette{i.ToString("d2")}.png");
                        //     addr_ptr += 4; //next palette
                        // }
                    }
                    //File.WriteAllBytes($"c:\\temp\\tnova\\palchunk_{chunkid}.dat", pal_ark.data);
                }
            }
        }        
    }//end class
}//end namespace