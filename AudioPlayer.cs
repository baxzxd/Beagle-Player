using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//CSCore
using CSCore.CoreAudioAPI;
using CSCore.DSP;
using CSCore.Streams;
using CSCore.Utils;
using CSCore.Win32;
using CSCore.SoundOut;
using CSCore;
using CSCore.Codecs;
using CSCore.Streams.Effects;

namespace Music_Player_WPF
{
    public static class AudioPlayer
    {

        public static string CurrentTimestamp
        {
            get
            {
                long total_seconds = (long)(AudioPlayer.soundOut.WaveSource.Position / AudioPlayer.soundOut.WaveSource.WaveFormat.BytesPerSecond);
                int minutes = (int)(total_seconds / 60);
                int seconds = (int)(total_seconds - (minutes * 60));
                string time_string = minutes.ToString("00") + ":" + seconds.ToString("00");
                return time_string;
            }
        }

        public static event EventHandler<PlaybackStoppedEventArgs> PlaybackStopped;
        public static PlaybackState PlaybackState
        {
            get
            {
                if (soundOut != null)
                    return soundOut.PlaybackState;
                return PlaybackState.Stopped;
            }
        }

        private static Equalizer equalizer;
        public static ISoundOut soundOut;
        public static IWaveSource waveSource;

        public static string currentLength;

        public static void Play()
        {
            if (soundOut != null)
                soundOut.Play();

        }

        public static void Stop()
        {
            if (soundOut == null)
                return;
            
            soundOut.Stop();
        }

        public static void SetVolume(float v)
        {
            if (soundOut == null)
                return;

            soundOut.Volume = v;
        }

        public static void Open(string filename)
        {
            CleanupPlayback();

            MMDevice device = GetAudioDevice();

            waveSource =
                CodecFactory.Instance.GetCodec(filename)
                    .ToSampleSource()
                    .AppendSource(Equalizer.Create10BandEqualizer, out equalizer)
                    .ToWaveSource();
            
            soundOut = new WasapiOut() { Latency = 100, Device = device };
            soundOut.Initialize(waveSource);
            if (PlaybackStopped != null) soundOut.Stopped += PlaybackStopped;
        }

        private static MMDevice GetAudioDevice()
        {
            using (var mmdeviceEnumerator = new MMDeviceEnumerator())
            {
                using (var mmdeviceCollection = mmdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                {
                    // This uses the first device, but that isn't what you necessarily want
                    return mmdeviceCollection.First();
                }
            }
        }

        public static void CleanupPlayback()
        {
            if (soundOut != null)
            {
                soundOut.Dispose();
                soundOut = null;
            }
            if (waveSource != null)
            { 
                waveSource.Dispose();
                waveSource = null;
            }
        }

        public static string GetLengthString()
        {
            //Length and Position are the total bytes so divide by bitrate to get seconds
            long total_seconds = (long)(soundOut.WaveSource.Length / soundOut.WaveSource.WaveFormat.BytesPerSecond);

            //Get minutes and subtract from seconds
            int minutes = (int)(total_seconds / 60);
            int seconds = (int)(total_seconds - (minutes * 60));

            //String formatting
            string time_string = minutes.ToString("00") + ":" + seconds.ToString("00");
            return time_string;
        }

        public static string GetPositionString()
        {
            //Length and Position are the total bytes so divide by bitrate to get seconds
            long total_seconds = (long)(soundOut.WaveSource.Position / soundOut.WaveSource.WaveFormat.BytesPerSecond);

            //Get minutes and subtract from seconds
            int minutes = (int)(total_seconds / 60);
            int seconds = (int)(total_seconds - (minutes * 60));

            //String formatting
            string time_string = minutes.ToString("00") + ":" + seconds.ToString("00");
            return time_string;
        }
    }
}
