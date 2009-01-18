using System;
using System.IO;
using System.Text;
using Structorian.Engine.Fields;

namespace MADSPack
{
    public class MADSPackDecoder: BlobDecoder
    {
        public string Name
        {
            get { return "madspack"; }
        }

        public byte[] Decode(byte[] input, int decodedSize)
        {
            return new FABDecompressor(input).Result;
        }
    }

    class FABDecompressor
    {
        private readonly Stream _inputStream;
        private readonly MemoryStream _outputStream;
        private int _bitsLeft;
        private uint _bitBuffer;

        public FABDecompressor(byte[] input)
        {
            _inputStream = new MemoryStream(input);
            _outputStream = new MemoryStream();

            var signature = new byte[3];
            _inputStream.Read(signature, 0, 3);
            if (Encoding.ASCII.GetString(signature) != "FAB")
                throw new ArgumentException("Invalid compressed data");
            int shiftVal = _inputStream.ReadByte();
            if (shiftVal < 10 || shiftVal > 13)
                throw new ArgumentException("Invalid shift start");

            int copyOfsShift = 16 - shiftVal;
            int copyOfsMask = 0xFF << (shiftVal - 8);
	        int copyLenMask = (1 << copyOfsShift) - 1;

            // Initialise data fields
            _bitsLeft = 16;
            _bitBuffer = ReadWord();

            for (;;) {
                if (GetBit() == 0)
                {
                    uint copyOfs;
                    int copyLen;
                    if (GetBit() == 0)
                    {
                        copyLen = (int) (((GetBit() << 1) | GetBit()) + 2);
                        copyOfs = (uint) (_inputStream.ReadByte() | 0xFFFFFF00);
                    }
                    else
                    {
                        int b1 = _inputStream.ReadByte();
                        int b2 = _inputStream.ReadByte();
                        copyOfs = (uint) ((((b2 >> copyOfsShift) | copyOfsMask) << 8) | b1);
                        copyLen = b2 & copyLenMask;
                        if (copyLen == 0)
                        {
                            copyLen = _inputStream.ReadByte();
                            if (copyLen == 0)
                                break;
                            else if (copyLen == 1)
                                continue;
                            else
                                copyLen++;
                        }
                        else
                        {
                            copyLen += 2;
                        }
                        copyOfs |= 0xFFFF0000;
                    }

                    byte[] dataToCopy = new byte[copyLen];
                    long pos = _outputStream.Position;
                    _outputStream.Seek((int) copyOfs, SeekOrigin.Current);
                    _outputStream.Read(dataToCopy, 0, copyLen);
                    _outputStream.Position = pos;
                    _outputStream.Write(dataToCopy, 0, copyLen);
                }
                else {
                    int c = _inputStream.ReadByte();
                    _outputStream.WriteByte((byte) c);
		        }
	        }

            _outputStream.Capacity = (int) _outputStream.Position;
        }

        private uint ReadWord()
        {
            uint i = (uint) _inputStream.ReadByte();
            uint i2 = (uint) _inputStream.ReadByte();
            return (i2 << 8) + i;
        }

        private uint GetBit()
        {
            _bitsLeft--;
            if (_bitsLeft == 0)
            {
                _bitBuffer = (ReadWord() << 1) | (_bitBuffer & 1);
                _bitsLeft = 16;
            }

            uint bit = _bitBuffer & 1;
            _bitBuffer >>= 1;
            return bit;
        }

        public byte[] Result
        {
            get { return _outputStream.GetBuffer(); }
        }
    }
}
