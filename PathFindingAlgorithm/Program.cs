using System;
using System.Collections.Generic;
using System.Threading;

namespace PathFindingAlgorithm
{
    class Program
    {

        public static FinishNode finish = new FinishNode(5, 0);
        public static MovableNode character = new MovableNode(3, 3);
        public static Dictionary<(int x, int y), BarrierNode> barrierNodes = new Dictionary<(int x, int y), BarrierNode>();
        public static Dictionary<(int x, int y), Road> roadNodes = new Dictionary<(int x, int y), Road>();
        public static Dictionary<int, List<Road>> getNextCellByWeight = new Dictionary<int, List<Road>>();
        public static string[,] maze = {
                {"#","#","#","#","#", "O", "#", "#" },
                {"#","O","O","O","O", "O", "O", "#" },
                {"#","O","#","#","#", "#", "O", "#" },
                {"#","O","#","O","#", "O", "O", "#" },
                {"#","O","#","O","#", "#", "O", "#" },
                {"#","O","#","O","#", "O", "O", "#" },
                {"#","O","O","O","O", "O", "O", "#" },
                {"#","#","#","#","#", "#", "#", "#" }};

        static void Main(string[] args)
        {
            //Setup
            
            RenderStatic(); // creating static objects
            Draw(); //drawing static and dynamic elements to the console
            RecalculateWeights(); //Initial path weight calculation
            Init();
        }

        private static void Init()
        {
            Console.WriteLine("Path finding algorithm");
            Console.WriteLine("===");
            Console.WriteLine("Menu:");
            Console.WriteLine("1 - change start position");
            Console.WriteLine("2 - change finish position");
            Console.WriteLine("3 - animate movement start to finish");
            Console.WriteLine("===");

            Console.WriteLine();
            Console.Write("Proceed with a command: ");
            string input = Console.ReadLine();
            Console.WriteLine();

            switch (input)
            {
                case "1":
                    ChangeNodeCoords("start");
                    break;
                case "2":
                    ChangeNodeCoords("finish");
                    break;
                case "3":
                    MoveWithThreadSleep();
                    break;
                default:
                    break;
            }
        }

        private static void RecalculateWeights()
        {

            // recalculating weights
            List<Road> nextSquaresToTraverse = new List<Road>();

            ClearWeights();

            nextSquaresToTraverse.Add(roadNodes[(finish.X, finish.Y)]);
            int iteration = 0;
            var currentAdjacents = FindAdjacent(nextSquaresToTraverse, iteration);
            //========

            //check if last adjacent is character node, cause if so then no need to calculate new adjacent cells
            while (currentAdjacents.Count > 0)
            {
                iteration++;
                nextSquaresToTraverse = new List<Road>();
                nextSquaresToTraverse.AddRange(currentAdjacents);
                currentAdjacents = FindAdjacent(nextSquaresToTraverse, iteration);
            }
        }

        private static void ClearWeights()
        {
            getNextCellByWeight = new Dictionary<int, List<Road>>();
            

            foreach (var element in roadNodes)
            {
                element.Value.Weight = -1;
            }
        }

        private static void ChangeNodeCoords(string type)
        {
            Console.WriteLine("===\n" +
                $"You will be prompted to change coordinates for {type}\n" +
                $"(note: Y axis is top to bottom)\n");

            try
            {
                Console.Write("X: ");
                string xInput = Console.ReadLine();
                var xParsed = int.Parse(xInput);
                Console.Write("Y: ");
                string yInput = Console.ReadLine();
                var yParsed = int.Parse(xInput);
                if (type == "start")
                {
                    character.X = xParsed;
                    character.Y = yParsed;
                }
                else if (type == "finish")
                {
                    finish.X = xParsed;
                    finish.Y = yParsed;
                }
                if (barrierNodes.ContainsKey((xParsed, yParsed)))
                {
                    throw new ArgumentException("Can't place nodes onto walls");
                }
                RecalculateWeights();

            }
            catch (ArgumentException)
            {
                Console.WriteLine($"Incorrect arguments type try again (must be 0 to {maze.GetLength(1)})");
            }
            Draw();
            Init();
        }

        private static void MoveWithThreadSleep()
        {

            int prevPos = roadNodes[(character.X, character.Y)].Weight;
            int currentPos = prevPos;

            while (true)
            {
                //Console.Clear();

                if (character.X != finish.X || character.Y != finish.Y)
                {
                    prevPos = currentPos;
                    currentPos = prevPos - 1;
                    var nextPosition = FindClosestSingleAdjacent(currentPos);
                    if (nextPosition != null)
                    {
                        character.X = nextPosition.X;
                        character.Y = nextPosition.Y;
                    }
                    else
                    {
                        break;
                    }
                    Console.WriteLine("X=" + character.X + ":" + "Y=" + character.Y);
                    Draw();

                }
                else
                {
                    break;
                }

                Thread.Sleep(900);

            }

            Console.WriteLine("Final result");
            Draw(); //draw image which wont be wiped after loop break
            Init();
        }

        private static Road FindClosestSingleAdjacent(int currentPos)
        {
            if (currentPos < 0)
            {
                return new Road(finish.X, finish.Y);
            }
            for (int i = 0; i < getNextCellByWeight[currentPos].Count; i++)
            {
                if ((getNextCellByWeight[currentPos][i].X == character.X || getNextCellByWeight[currentPos][i].Y == character.Y)
                    && (Math.Abs(getNextCellByWeight[currentPos][i].X - character.X) <= 1 && Math.Abs(getNextCellByWeight[currentPos][i].Y - character.Y) <= 1))
                {
                    Console.WriteLine(getNextCellByWeight[currentPos][i].X + ":" + getNextCellByWeight[currentPos][i].Y);
                    return getNextCellByWeight[currentPos][i];
                }
            }
            return null;
        }

        private static List<Road> FindAdjacent(List<Road> adjacentToSearchFor, int iteration)
        {
            List<Road> newAdjacentElements = new List<Road>();

            for (int i = 0; i < adjacentToSearchFor.Count; i++)
            {
                for (int y = adjacentToSearchFor[i].Y - 1; y <= adjacentToSearchFor[i].Y + 1; y++)
                {
                    for (int x = adjacentToSearchFor[i].X - 1; x <= adjacentToSearchFor[i].X + 1; x++)
                    {
                        if (roadNodes.ContainsKey((x, y)) && (x == adjacentToSearchFor[i].X || y == adjacentToSearchFor[i].Y))
                        {
                            if (roadNodes[(x, y)].Weight == -1) //can set weight only to nodes that dont have it (initialized with -1)
                            {

                                var foundRoadNode = roadNodes[(x, y)];
                                foundRoadNode.Weight = adjacentToSearchFor[i].Weight + 1; //k + 1 weight

                                newAdjacentElements.Add(foundRoadNode);
                                if (getNextCellByWeight.ContainsKey(iteration))
                                {
                                    getNextCellByWeight[iteration].Add(foundRoadNode);
                                }
                                else
                                {
                                    getNextCellByWeight.Add(iteration, new List<Road>());
                                    getNextCellByWeight[iteration].Add(foundRoadNode);
                                }
                            }
                        }
                    }
                }
            }
            return newAdjacentElements;
        }

        private static void RenderStatic()
        {
            for (int i = 0; i < maze.GetLength(0); i++)
            {
                for (int j = 0; j < maze.GetLength(1); j++)
                {
                    if (maze[j, i].Equals("#"))
                    {
                        barrierNodes.Add((i, j), new BarrierNode(i, j));
                    }
                    else if (maze[j, i].Equals("O"))
                    {
                        roadNodes.Add((i, j), new Road(i, j));
                    }
                }
            }
        }

        static void Draw()
        {
            string[,] outputRender = (string[,])maze.Clone(); // copy with new ref not to mess up original maze

            outputRender[finish.Y, finish.X] = "F";
            outputRender[character.Y, character.X] = "A";

            for (int i = 0; i < outputRender.GetLength(0); i++)
            {
                for (int j = 0; j < outputRender.GetLength(1); j++)
                {
                    Console.Write(outputRender[i, j] + " ");
                }
                Console.WriteLine("");
            }

            Console.WriteLine();
        }

        static void SetFinish(int newX, int newY)
        {
            if (!barrierNodes.ContainsKey((newX, newY)))
            {
                finish.X = newX;
                finish.Y = newY;
            }
        }
    }

    abstract class PathNode
    {
        public PathNode(int x, int y)
        {
            X = x;
            Y = y;
        }
        public int X { get; set; }
        public int Y { get; set; }
    }

    sealed class FinishNode : PathNode
    {
        public FinishNode(int x, int y) : base(x, y) { }
    }

    sealed class MovableNode : PathNode
    {

        public MovableNode(int x, int y) : base(x, y) { }
    }

    sealed class BarrierNode : PathNode
    {

        public BarrierNode(int x, int y) : base(x, y) { }
    }

    sealed class Road : PathNode
    {
        public Road(int x, int y) : base(x, y) { }
        public int Weight { get; set; } = -1;
    }
}
