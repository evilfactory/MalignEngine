using Silk.NET.OpenAL;
using System.Buffers.Binary;
using System.Text;

namespace MalignEngine;

public interface IALBuffer
{
    uint GetALBuffer();
}

public interface ISoundResource { }

public class SoundResource : ISoundResource, IALBuffer, IDisposable
{
    private AL _al;
    private uint _buffer;

    public SoundResource(AL al, ISoundResourceDescriptor descriptor)
    {
        _al = al;

        ReadOnlySpan<byte> file = descriptor.SoundData;

        int index = 0;
        if (file[index++] != 'R' || file[index++] != 'I' || file[index++] != 'F' || file[index++] != 'F')
        {
            throw new ArgumentException("Given file is not in RIFF format");
        }

        var chunkSize = BinaryPrimitives.ReadInt32LittleEndian(file.Slice(index, 4));
        index += 4;

        if (file[index++] != 'W' || file[index++] != 'A' || file[index++] != 'V' || file[index++] != 'E')
        {
            throw new ArgumentException("Given file is not in WAVE format");
        }

        short numChannels = -1;
        int sampleRate = -1;
        int byteRate = -1;
        short blockAlign = -1;
        short bitsPerSample = -1;
        BufferFormat format = 0;

        _buffer = _al.GenBuffer();

        while (index + 4 < file.Length)
        {
            var identifier = "" + (char)file[index++] + (char)file[index++] + (char)file[index++] + (char)file[index++];
            var size = BinaryPrimitives.ReadInt32LittleEndian(file.Slice(index, 4));
            index += 4;
            if (identifier == "fmt ")
            {
                if (size != 16)
                {
                    Console.WriteLine($"Unknown Audio Format with subchunk1 size {size}");
                }
                else
                {
                    var audioFormat = BinaryPrimitives.ReadInt16LittleEndian(file.Slice(index, 2));
                    index += 2;
                    if (audioFormat != 1)
                    {
                        Console.WriteLine($"Unknown Audio Format with ID {audioFormat}");
                    }
                    else
                    {
                        numChannels = BinaryPrimitives.ReadInt16LittleEndian(file.Slice(index, 2));
                        index += 2;
                        sampleRate = BinaryPrimitives.ReadInt32LittleEndian(file.Slice(index, 4));
                        index += 4;
                        byteRate = BinaryPrimitives.ReadInt32LittleEndian(file.Slice(index, 4));
                        index += 4;
                        blockAlign = BinaryPrimitives.ReadInt16LittleEndian(file.Slice(index, 2));
                        index += 2;
                        bitsPerSample = BinaryPrimitives.ReadInt16LittleEndian(file.Slice(index, 2));
                        index += 2;

                        if (numChannels == 1)
                        {
                            if (bitsPerSample == 8)
                                format = BufferFormat.Mono8;
                            else if (bitsPerSample == 16)
                                format = BufferFormat.Mono16;
                            else
                            {
                                throw new ArgumentException($"Can't Play mono {bitsPerSample} sound.");
                            }
                        }
                        else if (numChannels == 2)
                        {
                            if (bitsPerSample == 8)
                                format = BufferFormat.Stereo8;
                            else if (bitsPerSample == 16)
                                format = BufferFormat.Stereo16;
                            else
                            {
                                throw new ArgumentException($"Can't Play stereo {bitsPerSample} sound.");
                            }
                        }
                        else
                        {
                            throw new ArgumentException($"Can't play audio with {numChannels} sound");
                        }
                    }
                }
            }
            else if (identifier == "data")
            {
                var data = file.Slice(44, size);
                index += size;

                unsafe
                {
                    fixed (byte* pData = data)
                        _al.BufferData(_buffer, format, pData, size, sampleRate);
                }
            }
            else if (identifier == "JUNK")
            {
                index += size;
            }
            else if (identifier == "iXML")
            {
                var v = file.Slice(index, size);
                var str = Encoding.ASCII.GetString(v);
                index += size;
            }
            else
            {
                index += size;
            }
        }
    }

    public void Dispose()
    {
        _al.DeleteBuffer(_buffer);
    }

    public uint GetALBuffer() => _buffer;
}
