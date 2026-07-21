using Godot;
namespace FreeFall
{    
    public class Artloader : Loader
    {
        
        public static ImageTexture Image(
            byte[] databuffer,
            long dataOffSet,
            int width, int height,
            Palette palette,
            bool useAlphaChannel,
            bool useSingleRedChannel
            )
        {
            Godot.Image.Format imgformat;
            if (useSingleRedChannel)
            {
                imgformat = Godot.Image.Format.R8;
            }
            else
            {
                if (useAlphaChannel)
                {
                    imgformat = Godot.Image.Format.Rgba8;
                }
                else
                {
                    imgformat = Godot.Image.Format.Rgb8;
                }
            }

            var img = Godot.Image.CreateEmpty(width, height, false, imgformat);
            for (int iRow = 0; iRow < height; iRow++)
            {
                int iCol = 0;
                for (int j = iRow * width; j < (iRow * width) + width; j++)
                {
                    byte pixel = (byte)getAt(databuffer, dataOffSet + j, 8);
                    img.SetPixel(
                        x: iCol,
                        y: iRow,
                        color: palette.ColorAtIndex(
                            index: pixel,
                            useAlphaChannel: useAlphaChannel,
                            useSingleRedChannel: useSingleRedChannel));
                    iCol++;
                }
            }
            
            var tex = new ImageTexture();
            tex.SetImage(img);
            return tex;
        }
    }//end class
}//end namespace