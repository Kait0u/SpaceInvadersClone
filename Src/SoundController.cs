using NAudio.Vorbis;
using NAudio.Wave;


namespace SpaceInvadersClone
{
    class LoopStream : WaveStream
    {
        // Heavily inspired by: https://mark-dot-net.blogspot.com/2009/10/looped-playback-in-net-with-naudio.html

        public LoopStream(WaveStream source)
        {
            this.sourceStream = source;
            this.EnableLooping = true;
        }

        public bool EnableLooping { get; set; }

        public override WaveFormat WaveFormat
        {
            get => sourceStream.WaveFormat;
        }

        public override long Length
        {
            get => sourceStream.Length;
        }

        public override long Position
        {
            get => sourceStream.Position; 
            set => sourceStream.Position = value; 
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (sourceStream.Position == 0 || !EnableLooping)
                    {
                        // something wrong with the source stream
                        break;
                    }
                    // loop
                    sourceStream.Position = 0;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }

        WaveStream sourceStream;
    }

    class SoundController
    {
        public SoundController()
        {
            soundList = new List<DirectSoundOut>();
            garbageList = new List<DirectSoundOut>();
            soundQueue = new Queue<DirectSoundOut>();
        }

        public void Update()
        {
            // Remove items marked for removal
            foreach (DirectSoundOut waveOut in garbageList)
            {
                waveOut.Stop();
                waveOut.Dispose();
                soundList.Remove(waveOut);
            }

            // Clear the list of items marked for removal
            garbageList.Clear();

            if (soundList.Count <= 0 && soundQueue.Count <= 0) return;
            // Stop if there's nothing to do

            lock (soundQueue) // Prevent others from accessing the queue
            {
                while (soundQueue.Count > 0)
                {
                    var temp = soundQueue.Dequeue();
                    soundList.Add(temp);
                    temp.Play();
                }
            }

            foreach (DirectSoundOut waveOut in soundList)
            {
                if (waveOut != null && waveOut.PlaybackState == PlaybackState.Stopped) garbageList.Add(waveOut);
                // Mark the DirectSoundOut object for removal if it's done playing
            }
        }

        public DirectSoundOut Play(WaveStream stream)
        {
            DirectSoundOut waveOut = new DirectSoundOut();
            waveOut.Init(stream);
            lock(soundQueue) soundQueue.Enqueue(waveOut);
            // It will play when dequeued

            return waveOut;
        }

        public void Stop(DirectSoundOut waveOut)
        {
            waveOut.Stop();
        }

        public static AudioFileReader LoadFromPath(string path) => new AudioFileReader(path);
        public static VorbisWaveReader LoadOggFromPath(string path) => new VorbisWaveReader(path);

        List<DirectSoundOut> soundList, garbageList;
        Queue<DirectSoundOut> soundQueue;

    }
}
