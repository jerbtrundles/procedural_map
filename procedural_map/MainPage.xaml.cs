﻿using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace procedural_map {
    public sealed partial class MainPage : Page {
        Stopwatch fullLoopTimer;

        public MainPage() {
            this.InitializeComponent();

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
            //Application.Current.DebugSettings.EnableFrameRateCounter = false;

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
        }

        private void CoreWindow_KeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args) { }
        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args) {
            switch (args.VirtualKey) {
                case Windows.System.VirtualKey.Right:
                case Windows.System.VirtualKey.Left:
                case Windows.System.VirtualKey.Up:
                case Windows.System.VirtualKey.Down:
                    Camera.KeyDown(args.VirtualKey);
                    break;
                //case Windows.System.VirtualKey.A:
                //    Debug.TimedStrings.Add(new TimedString("Test string"));
                //    break;
                case Windows.System.VirtualKey.R:
                    Debug.Reset();
                    break;
                case Windows.System.VirtualKey.D:
                    Debug.DrawMode = (Debug.DrawMode == Debug.DRAW_MODE.BACKGROUND_COLOR) ? Debug.DRAW_MODE.ELEVATION : Debug.DRAW_MODE.BACKGROUND_COLOR;
                    break;
            }
        }

        private void canvasMain_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args) {
            if (fullLoopTimer != null) {
                fullLoopTimer.Stop();
                Debug.LastFullLoopTime = fullLoopTimer.ElapsedMilliseconds;
                if (Debug.LastFullLoopTime > Debug.MaxFullLoopTime) { Debug.MaxFullLoopTime = Debug.LastFullLoopTime; }
                if (fullLoopTimer.ElapsedMilliseconds > canvasMain.TargetElapsedTime.TotalMilliseconds + 1) { Debug.SlowFrames++; }
            }

            fullLoopTimer = Stopwatch.StartNew();

            Stopwatch s = Stopwatch.StartNew();
            Map.Draw(args);
            Debug.Draw(args);
            s.Stop();

            Debug.LastDrawTime = s.ElapsedMilliseconds;
        }

        private async void canvasMain_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args) {
            Stopwatch s = Stopwatch.StartNew();
            await Map.Update(args);
            s.Stop();

            Debug.LastUpdateTime = s.ElapsedMilliseconds;
            Debug.TotalFrames++;

            Stopwatch d = Stopwatch.StartNew();
            Debug.Update(args);
            d.Stop();

            Debug.LastDebugUpdateTime = d.ElapsedMilliseconds;

            Camera.Update(args);
        }

        private void canvasMain_CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args) {
            Statics.Initialize(canvasMain);
            Map.Initialize(sender.Device);
        }

        private void canvasMain_PointerMoved(object sender, PointerRoutedEventArgs e) {
            PointerPoint p = e.GetCurrentPoint(canvasMain);
            Mouse.X = p.Position.X;
            Mouse.Y = p.Position.Y;
            if (Mouse.LeftButtonDown) {
                Camera.PositionX -= Mouse.DeltaX;
                Camera.PositionY -= Mouse.DeltaY;
            }
        }

        private void canvasMain_PointerPressed(object sender, PointerRoutedEventArgs e) {
            PointerPoint p = e.GetCurrentPoint(canvasMain);
            Mouse.LeftButtonDown = p.Properties.IsLeftButtonPressed;
            Mouse.RightButtonDown = p.Properties.IsRightButtonPressed;
        }

        private void canvasMain_PointerReleased(object sender, PointerRoutedEventArgs e) {
            PointerPoint p = e.GetCurrentPoint(canvasMain);
            Mouse.LeftButtonDown = p.Properties.IsLeftButtonPressed;
            Mouse.RightButtonDown = p.Properties.IsRightButtonPressed;
        }
    }
}
