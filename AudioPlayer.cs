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

namespace Music_Player_WPF
{
    public static class AudioPlayer
    {
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
                    .ToWaveSource();
            
            soundOut = new WasapiOut() { Latency = 100, Device = device };
            soundOut.Initialize(waveSource);
            if (PlaybackStopped != null) soundOut.Stopped += PlaybackStopped;
            SetVolume(.1f);
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
    }
}
