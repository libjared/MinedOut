﻿using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace MinedOut
{
    internal interface IControls
    {
        bool MoveUp { get; }
        bool MoveDn { get; }
        bool MoveLf { get; }
        bool MoveRt { get; }
        bool FlagUp { get; }
        bool FlagDn { get; }
        bool FlagLf { get; }
        bool FlagRt { get; }

        void Update();
    }

    internal class HumanControls : IControls
    {
        public bool MoveUp => currMoveUp && !lastMoveUp && !currFlag;
        public bool MoveDn => currMoveDn && !lastMoveDn && !currFlag;
        public bool MoveLf => currMoveLf && !lastMoveLf && !currFlag;
        public bool MoveRt => currMoveRt && !lastMoveRt && !currFlag;
        public bool FlagUp => currMoveUp && !lastMoveUp && currFlag;
        public bool FlagDn => currMoveDn && !lastMoveDn && currFlag;
        public bool FlagLf => currMoveLf && !lastMoveLf && currFlag;
        public bool FlagRt => currMoveRt && !lastMoveRt && currFlag;

        private bool lastMoveUp;
        private bool lastMoveDn;
        private bool lastMoveLf;
        private bool lastMoveRt;
        private bool currMoveUp;
        private bool currMoveDn;
        private bool currMoveLf;
        private bool currMoveRt;
        private bool currFlag;

        public void Update()
        {
            lastMoveUp = currMoveUp;
            lastMoveDn = currMoveDn;
            lastMoveLf = currMoveLf;
            lastMoveRt = currMoveRt;

            currMoveUp = Keyboard.IsKeyPressed(Keyboard.Key.Up);
            currMoveDn = Keyboard.IsKeyPressed(Keyboard.Key.Down);
            currMoveLf = Keyboard.IsKeyPressed(Keyboard.Key.Left);
            currMoveRt = Keyboard.IsKeyPressed(Keyboard.Key.Right);
            currFlag = Keyboard.IsKeyPressed(Keyboard.Key.LShift) || Keyboard.IsKeyPressed(Keyboard.Key.RShift);
        }
    }

    internal class AiControls : IControls
    {
        private readonly AiPlayer plr;
        private readonly Minefield field;
        private HashSet<Tile> explorePls;
        public bool MoveUp { get; private set; }
        public bool MoveDn { get; private set; }
        public bool MoveLf { get; private set; }
        public bool MoveRt { get; private set; }
        public bool FlagUp { get; private set; }
        public bool FlagDn { get; private set; }
        public bool FlagLf { get; private set; }
        public bool FlagRt { get; private set; }

        public AiControls(AiPlayer plr, Minefield field)
        {
            this.plr = plr;
            this.field = field;
            explorePls = new HashSet<Tile>();
        }

        private bool lastAdvance;
        private bool currAdvance;
        private bool oddFrame;
        public void Update()
        {
            //read something from plr and set the bools
            //ai phase 1: travel where mine counter is 0

            lastAdvance = currAdvance;
            currAdvance = Keyboard.IsKeyPressed(Keyboard.Key.A);

            ResetControls();
            if (currAdvance && !lastAdvance)
            {
                oddFrame = !oddFrame;
                if (oddFrame)
                    UpdatePriorities();
                else
                    UpdateControls();
                //exploreThese.Count should be 2 and then something else idk
            }
        }

        public void Draw(DrawCommandCollection drawCmds)
        {
            //draw the lists for debug purposes
            foreach (var expl in explorePls)
            {
                drawCmds.Add(new DrawCommand(expl.X, expl.Y, '#', Color.Yellow, Color.Transparent));
            }
        }

        private void ResetControls()
        {
            MoveUp = false;
            MoveDn = false;
            MoveLf = false;
            MoveRt = false;
            FlagUp = false;
            FlagDn = false;
            FlagLf = false;
            FlagRt = false;
        }

        private void UpdateControls()
        {
            //get explore request, sorted by distance ascending
            var exploreThese = explorePls.ToList().OrderBy(tile => tile.DistanceTo(plr.X, plr.Y)).ToList();
            var hasDuggedPath = exploreThese.Where(HasDuggedPath);
            var exploreHere = hasDuggedPath.First();

            var nextStep = NextStepInMovingTowards(exploreHere);

            if (MagnitudeFromPlayerIsOne(exploreHere))
            {
                explorePls.Remove(exploreHere);
                exploreThese.Remove(exploreHere);
            }
            else
            {
                FlagUp = true;
                FlagDn = true;
                FlagLf = true;
                FlagRt = true;
            }

            var diffX = nextStep.X - plr.X;
            var diffY = nextStep.Y - plr.Y;
            if (diffX == 1)  MoveRt = true;
            if (diffX == -1) MoveLf = true;
            if (diffY == 1)  MoveDn = true;
            if (diffY == -1) MoveUp = true;
        }

        private Vector2i NextStepInMovingTowards(Tile exploreHere)
        {
            //cardinal ONLY
            if (MagnitudeFromPlayerIsOne(exploreHere))
            {
                return new Vector2i(exploreHere.X, exploreHere.Y);
            }
            return new Vector2i(0, 0);
        }

        private bool HasDuggedPath(Tile targetTile)
        {
            var surrounding = field.GetCardinalAdjacent(targetTile.X, targetTile.Y);
            return surrounding.Any(t => t.Dug);
        }

        private bool MagnitudeFromPlayerIsOne(Tile tile)
        {
            return tile.DistanceTo(plr.X, plr.Y) == 1;
        }

        private void UpdatePriorities()
        {
            //save current count to MCM
            var mcmWhereX = plr.X;
            var mcmWhereY = plr.Y;
            int mcmHere = GetAdjMineCount(mcmWhereX, mcmWhereY);

            //if the counter is 0, explore all unexplored neighbors
            if (mcmHere == 0)
            {
                var adjUnexplored = GetAdjUnexplored(mcmWhereX, mcmWhereY);
                PleaseExplore(adjUnexplored);
            }
        }

        private void PleaseExplore(IEnumerable<Tile> where)
        {
            explorePls.UnionWith(where);
        }

        private IEnumerable<Tile> GetAdjUnexplored(int x, int y)
        {
            var found = field.GetAdjacent(x, y).Where(t => t.Dug == false);
            return found;
        }

        private int GetAdjMineCount(int x, int y)
        {
            return field.GetAdjacentMineCount(x, y);
        }
    }
}