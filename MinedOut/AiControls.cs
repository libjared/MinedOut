using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;
using SFML.Window;

namespace MinedOut
{
    internal class AiControls : IControls
    {
        private readonly AiPlayer plr;
        private readonly Minefield field;
        private readonly HashSet<DrawableTile> explorePls;
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
            explorePls = new HashSet<DrawableTile>();
            pathfinder = new Pathfinder(plr, field);
        }

        private bool lastAdvance;
        private bool currAdvance;
        private bool oddFrame;
        private readonly Pathfinder pathfinder;

        public void Update()
        {
            //ai phase 1: travel where mine counter is 0

            //lastAdvance = currAdvance;
            currAdvance = Keyboard.IsKeyPressed(Keyboard.Key.A);

            ResetControls();
            if (currAdvance && !lastAdvance)
            {
                oddFrame = !oddFrame;
                if (oddFrame)
                    UpdatePriorities();
                else
                    UpdateControls();
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

        private void UpdatePriorities()
        {
            //if we're on an explore candidate, remove it from the todolist
            explorePls.RemoveWhere(t =>
                t.X == plr.X &&
                t.Y == plr.Y
            );

            //save current count to MCM
            var mcmWhereX = plr.X;
            var mcmWhereY = plr.Y;
            var mcmHere = field.GetAdjacentMineCount(mcmWhereX, mcmWhereY);

            //if the counter is 0, explore all unexplored neighbors
            if (mcmHere == 0)
            {
                var adjUnexplored =
                    field.GetAdjacent(mcmWhereX, mcmWhereY)
                        .Where(t => t.Dug == false);
                explorePls.UnionWith(adjUnexplored);
            }
        }

        private DrawableTile GetAnExploreTarget()
        {
            //get explore request, sorted by distance ascending
            var hasADuggedPathToIt = explorePls.Where(HasADuggedPathToIt).ToList();
            var sortedExplore = hasADuggedPathToIt.OrderBy(tile => tile.DistanceTo(plr.X, plr.Y)).ToList();
            var topPriority = sortedExplore.FirstOrDefault();
            return topPriority;
        }

        private void MoveTowardTarget(DrawableTile exploreTarget)
        {
            var nextStep = pathfinder.NextStepInMovingTowards(exploreTarget);

            //can reach our target in one step? remove it from todolist, then move to it
            var tileTmp = new GroundTile(nextStep.X, nextStep.Y);
            if (tileTmp.DistanceTo(plr.X, plr.Y) == 1)
            {
                var diffX = nextStep.X - plr.X;
                var diffY = nextStep.Y - plr.Y;
                if (diffX == 1) MoveRt = true;
                if (diffX == -1) MoveLf = true;
                if (diffY == 1) MoveDn = true;
                if (diffY == -1) MoveUp = true;
            }
            else
            {
                //meaning: the distance is not 1 so it's a pathfinding error probably
                FlagDn = true;
            }
        }
        
        private void UpdateControls()
        {
            var exploreTarget = GetAnExploreTarget();
            if (exploreTarget == null)
            {
                //meaning: no more tiles to check.
                FlagRt = true;
                return;
            }

            MoveTowardTarget(exploreTarget);
        }

        private bool HasADuggedPathToIt(DrawableTile targetTile)
        {
            var surrounding = field.GetCardinalAdjacent(targetTile.X, targetTile.Y);
            return surrounding.Any(t => t.Dug);
        }

        public void Draw(DrawCommandCollection drawCmds)
        {
            //draw the lists for debug purposes
            foreach (var expl in explorePls)
            {
                drawCmds.Add(new DrawCommand(expl.X, expl.Y, '#', Color.Yellow, Color.Transparent));
            }

            pathfinder.Draw(drawCmds);
        }
    }
}