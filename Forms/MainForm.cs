using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using FlaCdotNet;
using meta = FlaCdotNet.Metadata;
using AudioSteganography.Steganography;
using AudioSteganography.Audio;
using AudioSteganography.Helper;

namespace AudioSteganography
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private string DataFileName;
        private string AudioFileName;
        private FileInDecodingStream AudioFileInStream;
        private FileOutEncodingStream AudioFileOutStream;
        private Stream dataFileStream;
        private NLSBSteganographerDecoder Decoder;
        private NLSBSteganographerEncoder Encoder;
        private meta.Prototype[] PinnedMetaData;
        private Thread MainWorkerThread;
        private StreamWriter InfoFileWriter = new StreamWriter("Info.txt");

        private void WriteCallback(int[] toWrite)
        {
            AudioFileOutStream.ProcessInterleaved(toWrite, (uint)toWrite.Length / AudioFileInStream.GetChannels());
        }

        private void ProgressCallback(double progress)
        {
            MainProgressBar.Invoke((Action<double>)(unused => MainProgressBar.Value = (int)(progress*100)), progress);
        }

        private void AddDataFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files (*.*)|*.*";
            DialogResult res = dlg.ShowDialog();
            if (res != DialogResult.OK)
                return;
            DataFileName = dlg.FileName;
            DisplayDataFileBox.Text = DataFileName;
        }

        private void AddAudioFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "FLAC Files (*.flac)|*.flac";
            DialogResult res = dlg.ShowDialog();
            if (res != DialogResult.OK)
                return;
            AudioFileName = dlg.FileName;
            DisplayAudioFileBox.Text = AudioFileName;
        }

        private void SetupOutputStream(int dataFileSize)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.AddExtension = true;
            dlg.DefaultExt = ".flac";
            dlg.Filter = "FLAC files (*.flac)|*.flac";
            dlg.ShowDialog();
            AudioFileOutStream = new FileOutEncodingStream(dlg.FileName);
            PinnedMetaData = new meta.Prototype[1];
            var fileSizeComment = new meta.VorbisComment();
            fileSizeComment.AppendComment(new meta.VorbisComment.Entry("LEN", "" + dataFileSize));
            PinnedMetaData[0] = fileSizeComment;
            AudioFileOutStream.SetMetadata(PinnedMetaData);
            //AudioFileOutStream.SetCompressionLevel(8);
            AudioFileOutStream.InfoFileWriter = InfoFileWriter;
            AudioFileOutStream.InitializeStream(
                AudioFileInStream.GetChannels(),
                AudioFileInStream.GetBitsPerSample(),
                AudioFileInStream.GetSampleRate(),
                (uint)AudioFileInStream.GetTotalSamples(),
                false
            );
        }

        private void SetupAudioStreams(int dataFileSize)
        {
            AudioFileInStream = new FileInDecodingStream();
            //AudioFileInStream.InfoOutFile = InfoFileWriter;
            AudioFileInStream.Initialize(AudioFileName);
            AudioFileInStream.ProcessSingle();
            AudioFileInStream.ClearReadData();
            SetupOutputStream(dataFileSize);
            AudioFileInStream.Reset();
            //AudioFileInStream.MetadataCallback = NLSBSteganographerEncoder.MetadataCallback;
            AudioFileInStream.Initialize(AudioFileName);
            AudioFileInStream.ProgressCallback = ProgressCallback;
            MainWorkerThread = new Thread(new ParameterizedThreadStart(x => { Encoder.ProcessAudiofile(WriteCallback); CleanupEncoder(); }));
        }

        private void InsertDataButton_Click(object sender, EventArgs e)
        {
            if (MainWorkerThread != null && MainWorkerThread.IsAlive)
            {
                MessageBox.Show("Please wait for the previous operation to finish", "Please Wait");
                return;
            }
            int dataFileSize = (int)new FileInfo(DataFileName).Length;
            dataFileStream = new StreamReader(DataFileName).BaseStream;
            SetupAudioStreams(dataFileSize);
            Encoder = new NLSBSteganographerEncoder(
                AudioFileInStream, 
                dataFileStream, 
                KeyHelper.GenerateKeyFromUserInput(), 
                dataFileSize
            );
            
            //Encoder.ProcessFile(WriteCallback);
            MainWorkerThread.Start(null);
        }

        private void SetupDecoder(out Stream dataFileStream)
        {
            AudioFileInStream = new FileInDecodingStream();
            AudioFileInStream.InfoOutFile = InfoFileWriter;
            AudioFileInStream.SetMetadataRespond(MetadataType.VorbisComment);
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files (*.*)|*.*";
            dlg.ShowDialog();
            if (dlg.FileName == "")
            {
                dataFileStream = null;
                return;
            }
            dataFileStream = new StreamWriter(dlg.FileName).BaseStream;
            Decoder = new NLSBSteganographerDecoder(
                AudioFileInStream, 
                dataFileStream, 
                KeyHelper.GenerateKeyFromUserInput()
            );
            AudioFileInStream.Initialize(AudioFileName);
            AudioFileInStream.ProgressCallback = ProgressCallback;
            MainWorkerThread = new Thread(new ParameterizedThreadStart(x => { Decoder.ProcessAudioFile(); CleanupDecoder(); }));
        }

        private void CleanupDecoder()
        {
            dataFileStream.Close();
            AudioFileInStream.Close();
            AudioFileInStream = null;
            dataFileStream = null;
            AudioFileName = null;
            DataFileName = null;
            DisplayAudioFileBox.Invoke((Action<object>)(unused => DisplayAudioFileBox.Text = ""), 0);
            DisplayDataFileBox.Invoke((Action<object>)(unused => DisplayDataFileBox.Text = ""), 0);
            Decoder = null;
        }

        private void CleanupEncoder()
        {
            dataFileStream.Close();
            AudioFileInStream.Close();
            AudioFileOutStream.Close();
            AudioFileInStream = null;
            AudioFileOutStream = null;
            dataFileStream = null;
            AudioFileName = null;
            DataFileName = null;
            DisplayAudioFileBox.Invoke((Action<object>)(unused => DisplayAudioFileBox.Text = ""), 0);
            DisplayDataFileBox.Invoke((Action<object>)(unused => DisplayDataFileBox.Text = ""), 0);
            Encoder.Clear();
            Encoder = null;
            
        }

        private void ExtractDataButton_Click(object sender, EventArgs e)
        {
            if (MainWorkerThread != null && MainWorkerThread.IsAlive)
            {
                MessageBox.Show("Please wait for the previous operation to finish", "Please Wait");
                return;
            }
            SetupDecoder(out dataFileStream);
            if (dataFileStream == null)
                return;
            MainWorkerThread.Start(null);
        }

        protected override void OnClosed(EventArgs e)
        {
            InfoFileWriter.Close();
            base.OnClosed(e);
        }
    }
}
