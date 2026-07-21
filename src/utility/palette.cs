using Godot;

namespace FreeFall
{
    public class Palette
    {
        public byte[] red = new byte[256];
        public byte[] blue = new byte[256];
        public byte[] green = new byte[256];
        public byte[] alpha = new byte[256];

        public Color ColorAtIndex(byte index, bool useAlphaChannel, bool useSingleRedChannel)
        {
            byte alphabyte;
            if (useAlphaChannel == true)
            {
                if (index != 0) //Alpha
                {
                    alphabyte = alpha[index]; //255
                }
                else
                {
                    alphabyte = 0;  //transparent
                }
            }
            else
            {
                alphabyte = 255; //no alpha
            }


            if (useSingleRedChannel)
            { //This means the shader will contain the colour information in a palette parameter
                return Color.Color8(
                    g8: 0,
                    r8: index,
                    b8: 0,
                    a8: 0
                );
            }
            else
            {
                return Color.Color8(
                        g8: green[index],
                        r8: red[index],
                        b8: blue[index],
                        a8: alphabyte
                    );
            }
        }



        public virtual ImageTexture toImage(int ColourBandSize = 1)
        {
            int ImageHeight, NoOfColours;
            byte[] imgData;
            BuildPaletteImgData(ColourBandSize, out ImageHeight, out NoOfColours, out imgData);
            var output = FreeFall.Artloader.Image(
                    databuffer: imgData,
                    dataOffSet: 0,
                    width: NoOfColours * ColourBandSize,
                    height: ImageHeight,
                    palette: this,
                    useAlphaChannel: true,
                    useSingleRedChannel: false
                    );
            return output;
        }

        protected static void BuildPaletteImgData(int ColourBandSize, out int ImageHeight, out int NoOfColours, out byte[] imgData)
        {
            ImageHeight = 16;
            NoOfColours = 256;
            imgData = new byte[ImageHeight * NoOfColours * ColourBandSize];
            int x = 0;
            for (int h = 0; h < ImageHeight; h++)
            {
                int i = 0;
                for (int w = 0; w < NoOfColours; w++)
                {
                    for (int b = 0; b < ColourBandSize; b++)
                    {
                        imgData[x++] = (byte)i;
                    }
                    i++;
                }
            }
        }


    }//end class
}//end namespace