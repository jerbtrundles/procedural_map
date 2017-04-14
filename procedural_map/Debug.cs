﻿using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Diagnostics;
using Windows.UI;

namespace procedural_map {
    static class Debug {
        public enum DRAW_MODE {
            BACKGROUND_COLOR,
            ELEVATION
        }

        public static DRAW_MODE DrawMode = DRAW_MODE.BACKGROUND_COLOR;

        public static object TimedStringsLock = new object();
        public static List<long> ChunkLoadTimes = new List<long>();
        public static List<string> Strings = new List<string>();
        public static long LastDrawTime { get; set; }
        public static long LastUpdateTime { get; set; }
        public static long LastDebugUpdateTime { get; set; }
        public static long LastFullLoopTime { get; set; }
        public static long MaxFullLoopTime { get; set; }
        public static int TotalChunkCount { get { return Map.DebugChunkCount; } }
        public static int OnScreenChunkCount = 0;
        public static int SlowFrames = 0;
        public static int TotalFrames = 0;

        public static string ChunkSizeMB { get; set; }    

        public static List<TimedString> TimedStrings = new List<TimedString>();

        public static void Draw(CanvasAnimatedDrawEventArgs args) {
            Strings.Clear();
            Strings.Add("Mouse: " + Mouse.CoordinatesString);
            Strings.Add("Mouse left: " + (Mouse.LeftButtonDown ? "DOWN" : "UP"));
            Strings.Add("Mouse right: " + (Mouse.RightButtonDown ? "DOWN" : "UP"));
            // Strings.Add("Mouse pressed: " + Mouse.PressedString);
            Strings.Add("Draw: " + LastDrawTime.ToString() + "ms");
            Strings.Add("Update: " + LastUpdateTime.ToString() + "ms");
            Strings.Add("Debug update: " + LastDebugUpdateTime.ToString() + "ms");
            Strings.Add("Full loop: " + LastFullLoopTime.ToString() + "ms");
            Strings.Add("Full loop (max): " + MaxFullLoopTime.ToString() + "ms");
            Strings.Add("Total frames: " + TotalFrames.ToString());
            Strings.Add("Slow frames: " + SlowFrames.ToString());
            Strings.Add("Camera offset: " + Camera.CoordinatesString());
            Strings.Add("Camera offset (chunk): " + Camera.ChunkPositionString());
            Strings.Add("Camera offset (tile): " + Camera.ChunkTilePositionString());
            Strings.Add("Draw mode: " + Debug.DrawMode.ToString());
            if(Map.DebugChunkCount > 0) {
                Strings.Add("Chunk count: " + TotalChunkCount.ToString());
                Strings.Add("Chunks on screen: " + OnScreenChunkCount.ToString());
                Strings.Add("Average chunk load time: " + ChunkLoadTimes.Average().ToString("F") + "ms");
                Strings.Add("Last chunk load time: " + ChunkLoadTimes.Last().ToString() + "ms");
            }

            ProcessMemoryUsageReport report = ProcessDiagnosticInfo.GetForCurrentProcess().MemoryUsage.GetReport();
            Strings.Add("Working set: " + (report.WorkingSetSizeInBytes / 1000000).ToString() + "MB");
            Strings.Add(ChunkSizeMB);

            int x = 1500;
            int y = 20;
            int width = 410;
            int height = (Strings.Count + 1) * 20;
            Color backgroundColor = Colors.CornflowerBlue;
            Color borderColor = Colors.White;
            args.DrawingSession.FillRectangle(new Windows.Foundation.Rect(x - 5, y - 5, width, height), backgroundColor);
            args.DrawingSession.DrawRoundedRectangle(new Windows.Foundation.Rect(x - 5, y - 5, width, height), 3, 3, borderColor);
            foreach (string str in Strings) {
                args.DrawingSession.DrawText(str, new Vector2(x, y), Colors.White);
                y += 20;
            }

            if (TimedStrings.Count > 0) {
                y += 50;
                height = (TimedStrings.Count + 1) * 20;
                args.DrawingSession.FillRectangle(new Windows.Foundation.Rect(x - 5, y - 5, width, height), backgroundColor);
                args.DrawingSession.DrawRoundedRectangle(new Windows.Foundation.Rect(x - 5, y - 5, width, height), 3, 3, borderColor);
                lock (Debug.TimedStringsLock) {
                    foreach (TimedString str in TimedStrings) {
                        str.Draw(args, new Vector2(x, y));
                        y += 20;
                    }
                }
            }
        }

        public static void Update(CanvasAnimatedUpdateEventArgs args) {
            OnScreenChunkCount = 0;
            lock (Debug.TimedStringsLock) {
                for (int i = TimedStrings.Count - 1; i >= 0; i--) {
                    if (TimedStrings[i].Dead) { TimedStrings.RemoveAt(i); }
                    else { TimedStrings[i].Update(args); }
                }
            }
        }

        public static void AddTimedString(string str, Color color) {
            lock (Debug.TimedStringsLock) {
                TimedStrings.Add(new TimedString(str, color));
            }
        }

        public static void Reset() {
            MaxFullLoopTime = 0;
            SlowFrames = 0;
            TotalFrames = 0;
        }
    }
}
