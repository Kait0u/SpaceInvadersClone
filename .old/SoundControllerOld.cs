using SFML.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceInvadersCloneOld
{
    internal class SoundController
    {
        public SoundController()
        {
            soundList = new List<Sound>();
            garbageList = new List<Sound>();
            soundQueue = new Queue<Sound>();
        }

        public void Update()
        {
            if (soundList.Count <= 0 && soundQueue.Count <= 0) return;

            foreach (Sound sound in garbageList)
            {
                soundList.Remove(sound);
            }

            garbageList.Clear();

            lock (soundQueue)
            {
                while (soundQueue.Count > 0) soundList.Add(soundQueue.Dequeue());
            }

            foreach (Sound sound in soundList)
            {
                if (sound != null && sound.Status == SoundStatus.Stopped)
                {
                    garbageList.Add(sound);
                }
            }



        }

        public void RegisterSound(Sound sound)
        {
            soundQueue.Enqueue(sound);
        }

        public void RegisterCopySound(Sound sound)
        {
            Sound s = new Sound(sound.SoundBuffer);
            RegisterSound(s);
        }

        public void RegisterPlaySound(Sound sound)
        {
            if (soundList.Count < 255) // Adhere to SFML recommendations
            {
                Sound s = new Sound(sound.SoundBuffer);
                s.Play();
                RegisterSound(s);
            }
        }


        Queue<Sound> soundQueue;
        List<Sound> soundList, garbageList;

    }
}
