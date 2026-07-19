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
    }//end class
}//end namespace