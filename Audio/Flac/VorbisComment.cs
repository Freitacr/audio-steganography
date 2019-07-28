using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AudioSteganography.Audio.Flac
{
    public class Comment
    {
        public string Field { get; private set; }
        public string Value { get; private set; }

        public Comment(string field, string value)
        {
            Field = field;
            Value = value;
        }
    }

    public class VorbisComment : MetadataBlock
    {
        private BinaryReader DataStream;
        private List<Comment> ContainedComments;
        public VorbisComment(MetadataBlock baseBlock)
        {
            Data = baseBlock.Data;
            Length = baseBlock.Length;
            IsLast = baseBlock.IsLast;
            MetadataType = BLOCK_TYPE.VORBIS_COMMENT;
            DataStream = new BinaryReader(new MemoryStream(Data));
            ContainedComments = new List<Comment>();
            ParseSelfFromData();
        }

        private void ParseSelfFromData()
        {
            ParseVendorInfo();
            ParseComments();
        }

        private void ParseVendorInfo()
        {
            uint vendorLength = DataStream.ReadUInt32();
            string vendorInfo = "";
            foreach (char x in DataStream.ReadChars((int)vendorLength))
                vendorInfo += x;
            Console.WriteLine(vendorInfo);
        }

        private void ParseComments()
        {
            uint commentListLength = DataStream.ReadUInt32();
            for (uint i = 0; i < commentListLength; i++)
            {
                uint commentLength = DataStream.ReadUInt32();
                string comment = "";
                foreach (char x in DataStream.ReadChars((int)commentLength))
                    comment += x;
                string[] commentSplit = comment.Split('=');
                ContainedComments.Add(new Comment(commentSplit[0], commentSplit[1]));
            }
        }

        public List<Comment>.Enumerator GetCommentEnumerator()
        {
            return ContainedComments.GetEnumerator();
        }
    }
}
