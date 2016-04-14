﻿using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace MinedOut
{
    internal static class Program
    {
        static RenderWindow window;

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
                new VideoMode(800, 600), "Mined Out");
            window.SetFramerateLimit(60);
            window.Closed += (obj, e) => { window.Close(); };
            window.Size = new Vector2u(800, 600);
        }

        private static void UpdateDraw()
        {
            window.DispatchEvents();
            window.Clear();

            var circleShape = new CircleShape
            {
                Radius = 16f,
                FillColor = Color.Red
            };
            window.Draw(circleShape);

            window.Display();
        }
    }
}
