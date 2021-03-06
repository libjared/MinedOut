﻿using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace MinedOut
{
    internal static class Program
    {
        private const int WindowW = 80*9;
        private const int WindowH = 25*16;
        private static RenderWindow window;
        private static GameScene gameScene;
        private static bool currEsc;
        private static bool lastEsc;

        private static void Main(string[] args)
        {
            PreRun();
            LoadContentInitialize();

            while (window.IsOpen)
            {
                UpdateDraw();
            }
        }

        private static void PreRun()
        {
            if (!Shader.IsAvailable)
            {
                Console.WriteLine("No shader available. Please update your graphics drivers.");
                Environment.Exit(1);
            }
        }

        private static void LoadContentInitialize()
        {
            window = new RenderWindow(
                new VideoMode(WindowW, WindowH), "Mined Out");
            window.SetFramerateLimit(60);
            window.Closed += (obj, e) => { window.Close(); };
            window.Size = new Vector2u(WindowW, WindowH);
            
            gameScene = new GameScene();
        }

        private static void UpdateDraw()
        {
            window.DispatchEvents();

            lastEsc = currEsc;
            currEsc = Keyboard.IsKeyPressed(Keyboard.Key.Escape);
            if (currEsc && !lastEsc)
            {
                gameScene = new GameScene();
            }

            gameScene.Update();
            window.Clear();
            window.Draw(gameScene);
            window.Display();
        }
    }
}
