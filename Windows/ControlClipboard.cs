using System.Windows.Forms;
using System.Drawing;

public class ControlClipboard
{
    public int verifyClipboard(){
        int verif = 0;
        if (Clipboard.ContainsText(TextDataFormat.Text)){
                verif = 1;
        }
        if (Clipboard.ContainsImage()){
                verif = 2;
        }
        return verif;
    }

    public String ReadClipboardText()
    {
        String returnHtmlText = "";
        if (Clipboard.ContainsText(TextDataFormat.Text))
        {
            returnHtmlText = Clipboard.GetText(TextDataFormat.Text);
        }
        
        return returnHtmlText;
    }

    public void SwapClipboardText(String replacementText)
    {
        if (Clipboard.ContainsText(TextDataFormat.Text))
        {
            Clipboard.SetText(replacementText, TextDataFormat.Text);
        }     
    }

    public Image ReadClipboardImage(){
        Image returnImage = null;
        if (Clipboard.ContainsImage())
        {
            returnImage = Clipboard.GetImage();
        }
        return returnImage;
    }

    public void SwapClipboardImage(Image replacementImage)
    { 
        if (Clipboard.ContainsImage())
        {
            Clipboard.SetImage(replacementImage);
        }
    }

}