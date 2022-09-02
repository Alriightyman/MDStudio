using GenesisPlusGXInterface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MDStudioPlus.Targets
{
    class GenesisPlusGxThread
    {
        private static GenesisPlusGX genPlusGx = null;
        private static GenesisPlusGxThread Instance;

        private Task task = null;
        CancellationTokenSource cts = null;

        public void Init(int windowWidth, int windowHeight, IntPtr parent, bool pal, char region, bool useGamepad = true)
        {
            Instance = this;

            genPlusGx = new GenesisPlusGX();
            genPlusGx.Init(windowWidth, windowHeight, parent, pal, (sbyte)region, useGamepad);

            cts = new CancellationTokenSource();

            if (Settings.Default.DGenWindowLocation != null)
            {
                genPlusGx.SetWindowPosition(Settings.Default.DGenWindowLocation.X, Settings.Default.DGenWindowLocation.Y);
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

            genPlusGx.LoadRom(path);
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

            if (genPlusGx != null)
            {
                Settings.Default.DGenWindowLocation = new System.Drawing.Point(genPlusGx.GetWindowXPosition(), genPlusGx.GetWindowYPosition());
                genPlusGx.Reset();
            }

        }

        public void BringToFront()
        {
            if (genPlusGx != null)
            {
                genPlusGx.BringToFront();
            }
        }

        public void Destroy()
        {
            if (genPlusGx != null)
            {
                genPlusGx.Dispose();
                genPlusGx = null;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        static public GenesisPlusGX GetGenPlusGX()
        {
            return genPlusGx;
        }

        private static void ThreadLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // TODO: update function that gathers all possible needed data?
                genPlusGx.UpdateDebug();
                genPlusGx.Update();
            }
        }
    }
}
