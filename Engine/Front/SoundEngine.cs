using OpenTK;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FSEngine.Audio
{
    public static class WAVLoader
    {
        public static byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                int sample_rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();

                string data_signature = new string(reader.ReadChars(4));
                if (data_signature != "data")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int data_chunk_size = reader.ReadInt32();

                channels = num_channels;
                bits = bits_per_sample;
                rate = sample_rate;

                return reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }

        public static int GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? (int)ALFormat.Mono8 : (int)ALFormat.Mono16;
                case 2: return bits == 8 ? (int)ALFormat.Stereo8 : (int)ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }

    }
    public unsafe class Sound : IDisposable
    {
        private uint source;
        private uint buffer;

        public bool Loop
        {
            set
            {
                AL.Source(source, ALSourceb.Looping, value);
            }
        }

        public static Sound FromFile(string path)
        {
            Sound s = new Sound();
            Stream wav = File.OpenRead(path);

            byte[] buf = WAVLoader.LoadWave(wav, out int channels, out int bits, out int rate);

            wav.Close();
            wav.Dispose();

            AL.GenBuffers(1, out s.buffer);
            AL.GenSources(1, out s.source);

            fixed(byte* ptr = &buf[0])
                AL.BufferData(s.buffer, (ALFormat)WAVLoader.GetSoundFormat(channels, bits), (IntPtr)ptr, buf.Length, rate);

            AL.Source(s.source, ALSourcei.Buffer, (int)s.buffer);

            return s;
        }

        public Task PlayAsync()
        {
            return Task.Run(() =>
            {
                AL.SourcePlay(source);
                do
                {
                    Thread.Sleep(50);
                }
                while (Playing());
            });
        }


        public bool Playing()
        {
            ALSourceState state = AL.GetSourceState(source);
            return state == ALSourceState.Playing;
        }
        public void Play()
        {
            Console.WriteLine("EEE");
            AL.SourcePlay(source);
        }
        public void Volume(float volume)
        {
            AL.Source(source, ALSourcef.Gain, volume);
        }

        float target_gain = 0;
        public float gain = 0;
        public void InterpolateVolume(float volume, float add = 0, float resolution = 20)
        {
            this.gain += (target_gain - this.gain) / resolution;
            if(Math.Abs(target_gain - gain) <= 0.01f)
                target_gain = volume;
            AL.Source(source, ALSourcef.Gain, gain + add);
        }
        int delay;
        long delay_start;
        public bool PlayDelay(int delay)
        {
            if(Time.ElapsedMilliseconds >= delay_start + delay)
            {
                delay = 0;
            }
            if(delay == 0)
            {
                this.delay = delay;
                delay_start = Time.ElapsedMilliseconds;
                if (Playing())
                    Stop();
                Play();
                return true;
            }
            return false;

        }
        public void Stop()
        {
            AL.SourceStop(source);
        }
        public void Pause()
        {
            AL.SourcePause(source);
        }
        public void Dispose()
        {
            AL.SourceStop(source);
            AL.DeleteSource(ref source);
            AL.DeleteBuffer(ref buffer);
        }
    }

    public static class SoundManager
    {
        private static IntPtr device;
        private static ContextHandle context;
        public static unsafe bool InitializeOpenAL()
        {
            device = Alc.OpenDevice(null);

            if (device == IntPtr.Zero)
                return false;

            context = Alc.CreateContext(device, (int*)0);

            if (context == ContextHandle.Zero)
                return false;

            Alc.MakeContextCurrent(context);

            return true;
        }
        public static void CloseOpenAL()
        {
            ///Dispose
            if (context != ContextHandle.Zero)
            {
                Alc.MakeContextCurrent(ContextHandle.Zero);
                Alc.DestroyContext(context);
            }
            context = ContextHandle.Zero;

            if (device != IntPtr.Zero)
            {
                Alc.CloseDevice(device);
            }
            device = IntPtr.Zero;
        }
    }
}
