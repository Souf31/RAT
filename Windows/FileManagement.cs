using System.Windows.Forms;
using System.Text;  
using System.IO; 


public class FileManagement 
{  
    public string ReadData(string path)  
    {  
        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);  
        StreamReader sr = new StreamReader(fs);  
        Console.WriteLine("Program to show content of test file");  
        sr.BaseStream.Seek(0, SeekOrigin.Begin);
        string text = "";
        string str = sr.ReadLine(); 
        text = text + str;
        while (str != null)  
        {  
            // Console.WriteLine(str);  
            str = sr.ReadLine(); 
            text = text + str; 
        }  
        sr.Close();  
        fs.Close(); 
        return text; 
    }  

    public void WriteData(string path, string text)  
    {  
        FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write);  
        StreamWriter sw = new StreamWriter(fs);  
        sw.Write(text);  
        sw.Flush();  
        sw.Close();  
        fs.Close();  
    }

    public void CreateFile(string path){
        // string path = "h:/Desktop/test1.txt";
        FileInfo fl = new FileInfo(path);
        File.Create(path); {
            Console.WriteLine("File has been created");
        }
    }

    public void DeleteFile(string path){
        FileInfo fi = new FileInfo(path);
        fi.Delete();
        Console.WriteLine("File has been deleted");
    }

    public void CopyFile(string path, string path2){
        FileInfo fi1 = new FileInfo(path);
        FileInfo fi2 = new FileInfo(path2);
        fi1.CopyTo(path2);
        Console.WriteLine("{0} was copied to {1}.", path, path2);
    }

    public void MoveFile(string path, string path2){
        FileInfo fi1 = new FileInfo(path);
        FileInfo fi2 = new FileInfo(path2);
        fi1.MoveTo(path2);
        Console.WriteLine("{0} was copied to {1}.", path, path2);
    }
}