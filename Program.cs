using System;
using System.Runtime;
using System.Windows.Forms;
using System.Collections.Generic;
using AudioSteganography.Audio.Wave;
using AudioSteganography.Helper;

namespace AudioSteganography
{
    
    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            /*
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "WAV Files (*.wav)|*.wav";
            dlg.ShowDialog();
            if (dlg.FileName == "")
                return;
            WaveFileDecoder dec = new WaveFileDecoder(dlg.FileName);
            dec.DecodeFile(new Action<byte[]>(unused => { return; }));
            */
            Application.Run(new MainForm());
        }
    }
}
