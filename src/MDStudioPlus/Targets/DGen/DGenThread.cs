using DGenInterface;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MDStudioPlus.Targets
{

    /*class DGenThread
    {
        private static DGen dGen = null;
        private static DGenThread Instance;

        private Thread myThread = null;

        public void Init(int windowWidth, int windowHeight, IntPtr parent, bool pal, char region)
        {
            Instance = this;

            dGen = new DGen();
            dGen.Init(windowWidth, windowHeight, parent, pal, (sbyte)region);

            if (Settings.Default.DGenWindowLocation != null)
            {
                dGen.SetWindowPosition(Settings.Default.DGenWindowLocation.X, Settings.Default.DGenWindowLocation.Y);
            }
        }

        public void LoadRom(string path)
        {
            if (myThread != null)
            {
                myThread.Abort();
                while (myThread.IsAlive)
                {
                    Thread.Sleep(1);
                }
            }

            dGen.LoadRom(path);
        }

        public void Start()
        {
            myThread = new Thread(new ThreadStart(ThreadLoop));

            // These are needed when running from a WPF application
            myThread.SetApartmentState(ApartmentState.STA);
            myThread.IsBackground = true;
            myThread.Start();
            System.Windows.Threading.Dispatcher.Run();
        }

        public void Stop()
        {
            if (myThread != null)
            {
                myThread.Abort();
                while (myThread.IsAlive)
                {
                    Thread.Sleep(1);
                }
            }

            myThread = null;

            if (dGen != null)
            {
                Settings.Default.DGenWindowLocation = new System.Drawing.Point(dGen.GetWindowXPosition(), dGen.GetWindowYPosition());
                dGen.Reset();
            }
        }

        public void BringToFront()
        {
            if (dGen != null)
            {
                dGen.BringToFront();
            }
        }
        public void Destroy()
        {
            if (dGen != null)
            {
                dGen.Dispose();
                dGen = null;
            }
        }

        static public DGen GetDGen()
        {
            return dGen;
        }

        private static void ThreadLoop()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                dGen.Update();
                Thread.Sleep(1);
            }
        }
    }*/
    class DGenThread
    {
        private static DGen dGen = null;
        private static DGenThread Instance;

        private Task task = null;
        CancellationTokenSource cts = null;

        public void Init(int windowWidth, int windowHeight, IntPtr parent, bool pal, char region, bool useGamepad = true)
        {
            Instance = this;

            dGen = new DGen();
            dGen.Init(windowWidth, windowHeight, parent, pal, (sbyte)region, useGamepad);

            cts = new CancellationTokenSource();

            if (Settings.Default.DGenWindowLocation != null)
            {
                dGen.SetWindowPosition(Settings.Default.DGenWindowLocation.X, Settings.Default.DGenWindowLocation.Y);
            }
        }

        public void LoadRom(string path)
        {
            if (task != null)
            {
                cts.Cancel();
                while (!task.IsCompleted)
                {
                    Thread.Sleep(1);
                }
            }

            dGen.LoadRom(path);
        }

        public void Start()
        {
            task = new Task(() => ThreadLoop(cts.Token), cts.Token);
            task.Start();

            // Process thread otherwise the emulator window will lock up
            System.Windows.Threading.Dispatcher.Run();
        }

        public void Stop()
        {
            if (task != null)
            {
                cts.Cancel();
                while (!task.IsCompleted)
                {
                    Thread.Sleep(1);
                }
            }

            task.Dispose();
            task = null;
            cts.Dispose();
            cts = null;

            if (dGen != null)
            {
                Settings.Default.DGenWindowLocation = new System.Drawing.Point(dGen.GetWindowXPosition(), dGen.GetWindowYPosition());
                dGen.Reset();
            }

        }

        public void BringToFront()
        {
            if (dGen != null)
            {
                dGen.BringToFront();
            }
        }

        public void Destroy()
        {
            if (dGen != null)
            {
                dGen.Dispose();
                dGen = null;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        static public DGen GetDGen()
        {
            return dGen;
        }

        private static void ThreadLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var sw = Stopwatch.StartNew();
                    dGen.Update();
                    //Thread.Sleep(1);
                    //Debug.WriteLine($"Frame Time: {sw.ElapsedMilliseconds}");
                    sw.Stop();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
