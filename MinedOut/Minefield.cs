﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

namespace MinedOut
{
    internal class Minefield : IGameDrawable
    {
        private const int SizeX = 54;
        private const int SizeY = 19;

        private readonly Tile[,] tiles;

        public Minefield()
        {
            tiles = new Tile[SizeX, SizeY];

            for (var y = 0; y < SizeY; y++)
            {
                for (var x = 0; x < SizeX; x++)
                {
                    tiles[x, y] = new GroundTile(x, y);
                }
            }

            //now place mines
            for (var i = 0; i < 200; i++)
            {
                PlaceMine();
            }
        }

        private void PlaceMine()
        {
            var r = new Random();

            //get random coord
            int x;
            int y;

            while (true)
            {
                x = r.Next(SizeX);
                y = r.Next(SizeY);

                //places not to put a mine: start, end, somewhere where a mine already exists
                var badEnter = x == 0 && y == 0;
                var badExit = x == SizeX - 1 && y == SizeY - 1;
                var badAlready = tiles[x, y] is MineTile;
                if (!badEnter && !badExit && !badAlready)
                    break;
            }

            //place mine
            tiles[x, y] = new MineTile(x, y);
        }

        public void Draw(DrawCommandCollection drawCmds)
        {
            for (var y = 0; y < SizeY; y++)
            {
                for (var x = 0; x < SizeX; x++)
                {
                    var tile = tiles[x, y];
                    tile.Draw(drawCmds);
                }
            }
        }
    }
}
