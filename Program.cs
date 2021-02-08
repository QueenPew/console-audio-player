using System;
using System.IO;
using System.Threading;
using LibVLCSharp.Shared;
using System.Threading.Tasks;
namespace musique
{

    class Program
    {
        public static string folderCreator(string path)
        {
        Directory.CreateDirectory(path);


            return path;
        }
        public static long forwardFile(long position, long maxLength)
        {
            if (position + 5000 > maxLength)
            {
                return maxLength;
            }

            return position + 5000;

        }
        public static long rewindFile(long position)
        {
            if (position - 5000 < 0)
            {
                return 0;
            }

            return position - 5000;

        }
        public static System.Int32 prevFile(int index)
        {
            if (index > 0)
            {
                return index - 1;
            }

            return fileEntries.Length - 1;

        }
        public static System.Int32 nextFile(int index)
        {
            if (index + 1 > fileEntries.Length - 1)
            {
                return 0;
            }

            return index + 1;

        }

        public static string path = folderCreator(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "/console-app");
        public static int onEndReachedIndex;
        public static CancellationTokenSource stopToken = new CancellationTokenSource();
        public static CancellationToken token;
        public static string[] fileEntries = Directory.GetFiles(path);

        static void Main(string[] args)
        {
            Boolean loop = true;
            Core.Initialize();
            Console.WriteLine("Put only audio files in " + path + " so the software can read it.");
            using (var libvlc = new LibVLC())
            {
                MediaPlayer mediaPlayer = new MediaPlayer(libvlc);
                int musicIndex = 0;
                do
                {
                    token = new CancellationToken();
                    stopToken = new CancellationTokenSource();
                    token = stopToken.Token;
                    try
                    {
                        var media = new Media(libvlc, fileEntries[musicIndex]);
                        mediaPlayer.Play(media);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Console.WriteLine("There are no audio files in " + path + " !");
                        Environment.Exit(1);
                    }

                    inputHandler handleTask = new inputHandler();
                    try
                    {
                        handleTask.Start(musicIndex, mediaPlayer);
                        handleTask.task.Wait(token);
                        musicIndex = handleTask.task.Result;
                    }
                    catch (Exception)
                    {
                        musicIndex = onEndReachedIndex;
                    }
                }
                while (loop);
            }
        }
    }
    public class inputHandler
    {
        public Task<int> task;
        public Task onEndReached;
        public void Start(int musicIndex, MediaPlayer mediaPlayer)
        {
            task = Task<int>.Factory.StartNew(() => loop(musicIndex, mediaPlayer), Program.token);
            onEndReached = Task.Factory.StartNew(() => mediaPlayer.EndReached += (sender, args) => { onEndReachedEventHandler(mediaPlayer, musicIndex, sender, args); });
        }
        private void onEndReachedEventHandler(MediaPlayer mediaPlayer, int musicIndex, object sender, System.EventArgs e)
        {
            stopLoop(musicIndex);
        }
        private System.Int32 loop(int musicIndex, MediaPlayer mediaPlayer)
        {
            int indexAtStart = musicIndex;
            ConsoleKeyInfo keyPressed;
            while (musicIndex == indexAtStart)
            {

                keyPressed = Console.ReadKey();
                if (keyPressed.Key == ConsoleKey.DownArrow)
                {
                    musicIndex = Program.prevFile(musicIndex);
                }
                else if (keyPressed.Key == ConsoleKey.UpArrow)
                {
                    musicIndex = Program.nextFile(musicIndex);
                }
                else if (keyPressed.Key == ConsoleKey.Spacebar)
                {
                    mediaPlayer.Pause();
                }
                else if (keyPressed.Key == ConsoleKey.LeftArrow)
                {
                    mediaPlayer.Time = Program.rewindFile(mediaPlayer.Time);
                }
                else if (keyPressed.Key == ConsoleKey.RightArrow)
                {
                    mediaPlayer.Time = Program.forwardFile(mediaPlayer.Time, mediaPlayer.Length);
                }
                else if (keyPressed.Key == ConsoleKey.W)
                {

                    mediaPlayer.Volume = mediaPlayer.Volume + 1;
                }
                else if (keyPressed.Key == ConsoleKey.S)
                {
                    mediaPlayer.Volume = mediaPlayer.Volume - 1;
                }
            }
            return musicIndex;
        }
        private void stopLoop(int musicIndex)
        {
            Program.onEndReachedIndex = Program.nextFile(musicIndex);
            Program.stopToken.Cancel();
        }
    }
}

