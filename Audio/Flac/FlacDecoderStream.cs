using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AudioSteganography.Helper;

namespace AudioSteganography.Audio.Flac
{
    public class FlacDecoderStream
    {
        private Stream FileStream;
        private List<MetadataBlock> MetadataBlocks;
        public StreamInfo StreamInfoBlock { get; private set; }
        public Action<MetadataBlock> MetadataCallback { private get; set; }
        public Action<int[]> ReadCallback { private get; set; }

        public FlacDecoderStream()
        {
            MetadataBlocks = new List<MetadataBlock>();
        }

        private void ReadFlacDeclaration(Stream readerIn)
        {
            int flacDec = ByteHelper.ReadInt32(readerIn);
            if (flacDec != 0x664C6143) //flacDec != "fLaC"
                throw new ImproperFormatException();
        }

        /**
         * <summary>Initializes a stream to read from the file specified by <paramref name="filePath"/></summary>
         * 
         */
        public void InitializeFile(string filePath)
        {
            FileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            ReadFlacDeclaration(FileStream);
        }

        /**
         * <summary>Reads all metadata blocks stored in the initialized stream</summary>
         * <remarks>If <code>this.MetadataCallback</code> is set, it is called on all metadata blocks that are read</remarks>
         * <exception cref="ImproperFormatException"/>
         */
        public void ReadMetadata()
        {
            MetadataBlock currentBlock = MetadataBlock.ReadMetadataBlock(FileStream);
            if (currentBlock.MetadataType != MetadataBlock.BLOCK_TYPE.STREAMINFO)
                throw new ImproperFormatException("Flac file did not start its metadata with a STREAMINFO block");
            StreamInfoBlock = currentBlock as StreamInfo;
            MetadataBlocks.Add(currentBlock);
            MetadataCallback?.Invoke(currentBlock);
            while (!currentBlock.IsLast)
            {
                currentBlock = MetadataBlock.ReadMetadataBlock(FileStream);
                MetadataBlocks.Add(currentBlock);
                MetadataCallback?.Invoke(currentBlock);
            }
        }

        public void ProcessStream(Action<int[]> readCallback = null)
        {
            ReadCallback = readCallback ?? ReadCallback;
            if (ReadCallback == null)
                throw new ArgumentNullException("readCallback", "ReadCallback was not set before calling, and was not supplied to this function");
            //Start reading audio frames
            while (true)
            {
                FlacAudioFrame currFrame = new FlacAudioFrame(this);
                try
                {
                    currFrame.ParseAudioFrame(FileStream);
                } catch (EndOfStreamException)
                {
                    break;
                }
                byte[] streamOracle = new byte[32];
                FileStream.Read(streamOracle, 0, 32);
                FileStream.Seek(-32, SeekOrigin.Current);
            }
            
            throw new NotImplementedException();
        }

        public List<MetadataBlock>.Enumerator GetMetadataEnumerator()
        {
            return MetadataBlocks.GetEnumerator();
        }
    }
}
