using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing;

public class ScreenManagement
{
    public Bitmap Screenshot()
    {
        Bitmap captureBitmap = new Bitmap(1920, 1080, PixelFormat.Format32bppArgb);
        Rectangle captureRectangle = Screen.AllScreens[0].Bounds;
        Graphics captureGraphics = Graphics.FromImage(captureBitmap);
        captureGraphics.CopyFromScreen(captureRectangle.Left,captureRectangle.Top,0,0,captureRectangle.Size);
        //captureBitmap.Save(@"C:\Users\gadea\Desktop\capture.png",ImageFormat.Jpeg);
        return captureBitmap;
    }

}