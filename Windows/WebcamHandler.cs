using AForge.Video;
using AForge.Video.DirectShow;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing;

class WebcamHandler
{
    private Bitmap lastFrame  = new Bitmap(1920, 1080, PixelFormat.Format32bppArgb);
    public Bitmap getPhoto()
    {
        // Create a new video capture device
        FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);

        // Set the video resolution and frame rate
        videoSource.VideoResolution = videoSource.VideoCapabilities[0];

        // Start the video stream
        videoSource.Start();
        
        // Handles a new Frame
        videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
        
        // Waits for the picture to be saved
        System.Threading.Thread.Sleep(2000);

        // Stop source
        videoSource.SignalToStop();

        return lastFrame;
    }

    private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
    {
        // Get the current video frame
        Bitmap eventFrame = (Bitmap)eventArgs.Frame.Clone();

        // Save the video frame as the last frame
        lastFrame = eventFrame;

    }
}