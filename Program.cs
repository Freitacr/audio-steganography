using System;
using System.Runtime;
using System.Windows.Forms;
using System.Collections.Generic;
using AudioSteganography.Audio.Flac;
using AudioSteganography.Helper;

namespace AudioSteganography
{
    
    

    static class Program
    {

        static void ProcessMetadata(MetadataBlock blockIn)
        {
            Console.WriteLine(blockIn.MetadataType.ToString());
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "FLAC Files (*.flac)|*.flac";
            dlg.ShowDialog();
            if (dlg.FileName == "")
                return;
            FlacDecoderStream stream = new FlacDecoderStream();
            stream.InitializeFile(dlg.FileName);
            stream.MetadataCallback = ProcessMetadata;
            stream.ReadMetadata();
            
            //Application.Run(new MainForm());
        }
    }
}
