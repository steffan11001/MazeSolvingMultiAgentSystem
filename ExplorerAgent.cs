using ActressMas;
using System;
using System.Collections.Generic;

namespace maze
{
    public class ExplorerAgent : Agent
    {
        private int _x, _y;
        private Random rand = new Random();
        private string _state = "ok";
        private int _previous_x, _previous_y;
        private double _my_weight = 0;
        private double _previous_weight = 0;
        private int step = 0;
        private Dictionary<int, string> path;
        private byte LastDirection;
        private byte[] OppositeDirection = { 1, 0, 3, 2};
        private double[] arrayWeight = new double[4];
        private int[] arrayWalls = new int[4];
        private bool[,] maze;

        public override void Setup()
        {
            Console.WriteLine("setup Explorer " + Name);
            Console.WriteLine("Starting " + Name);

            path = new Dictionary<int, string>();

            maze = new bool[Utils.Size, Utils.Size];

            for (int i = 0; i < Utils.Size; i++)
                for (int j = 0; j < Utils.Size; j++)
                    maze[i,j] = false;

            Send("maze", Utils.Str("spawn", 0, 0));
        }
        public override void Act(Message message)
        {
            Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

            string action;
            List<string> parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);
            if (action == "back")
            {
                GoBack();
            }
            if (action == "move")
            {
                Move(parameters);
            }
            if(action == "spawn")
            {
                _x = Convert.ToInt32(parameters[0]);
                _y = Convert.ToInt32(parameters[1]);
            }

        }
        private void GoBack()
        {
            if (step == 0 || step == 1)
            {
                Send("maze", Utils.Str("change", _x, _y));
            }
            else
            {
                string[] position = path[step - 2].Split(' ');
                step = step - 1;
                _previous_x = _x;
                _previous_y = _y;
                _x = Convert.ToInt32(position[0]);
                _y = Convert.ToInt32(position[1]);
                Send("maze", Utils.Str("change", _x, _y));
            }
        }

        public void Spawn()
        {
            _x = rand.Next(0, Utils.Size);
            _y = rand.Next(0, Utils.Size);
            Send("maze", Utils.Str("position", _x, _y));
        }
        private List<string> getNeightbours()
        {
            List<string> Neighbours=new List<string>();
            for (int i = 0; i < 4; i++)
            {
                Neighbours.Add("");
            }

            if (arrayWalls[0] == 1)
                Neighbours[0] = Convert.ToString((_x - 1) + " " + _y);
            if (arrayWalls[1] == 1)
                Neighbours[1] = Convert.ToString((_x + 1) + " " + _y);
            if (arrayWalls[2] == 1)
                Neighbours[2] = Convert.ToString(_x + " " + (_y-1));
            if (arrayWalls[3] == 1)
                Neighbours[3] = Convert.ToString(_x + " " + (_y+1));

            return Neighbours;
        }
        private bool visitedAllNeighbours(List<string> Neighbours)
        {
            bool flag = true;
            int x, y;

            for(int i = 0; i < Neighbours.Count; i++)
            {
                if (Neighbours[i] != "")
                {
                    string[] position = Neighbours[i].Split(' ');
                    x = Convert.ToInt32(position[0]);
                    y = Convert.ToInt32(position[1]);
                    if (maze[y, x] == false)
                        flag = false;
                }
            }

            return flag;
        }

        private List<string> getUnvisitedNeighbours()
        {
            List<string> Neighbours = new List<string>();
            for (int i = 0; i < 4; i++){
                Neighbours.Add("");
            }

            if (arrayWalls[0] == 1 && maze[_y, _x - 1] == false)
                Neighbours[0] = Convert.ToString((_x - 1) + " " + _y);
            if (arrayWalls[1] == 1 && maze[_y, _x + 1] == false)
                Neighbours[1] = Convert.ToString((_x + 1) + " " + _y);
            if (arrayWalls[2] == 1 && maze[_y - 1, _x] == false)
                Neighbours[2] = Convert.ToString(_x + " " + (_y - 1));
            if (arrayWalls[3] == 1 && maze[_y + 1, _x] == false)
                Neighbours[3] = Convert.ToString(_x + " " + (_y + 1));

            return Neighbours;
        }
        private int max(List<string> Neighbours)
        {
            int index = 0;
            double max = 0;

            if (getNeighboursCount(Neighbours) == 1)
            {
                for (int i = 0; i < Neighbours.Count; i++)
                    if (Neighbours[i] != "")
                        index = i;
            }
            else
            {
                for (int i = 0; i < Neighbours.Count; i++)
                {
                    if (max < arrayWeight[i] && arrayWalls[i] == 1 && Neighbours[i] != "" && i != OppositeDirection[LastDirection])
                    {
                        max = arrayWeight[i];
                        index = i;
                    }
                }
            }

            return index;
        }
        private int getNeighboursCount(List<string> Neighbours)
        {
            int count = 0;
            for( int i=0; i<Neighbours.Count; i++)
                if (Neighbours[i] != "")
                    count = count + 1;

            return count;
        }
        private void Move(List<String> parameters)
        {
            if (step == path.Count)
                path.Add(step, Convert.ToString(_x + " " + _y));
            else
                path[step] = Convert.ToString(_x + " " + _y);

            step++;

            _previous_x = _x;
            _previous_y = _y;
            _previous_weight = _my_weight;
            int left = Convert.ToInt32(parameters[0]);
            int right = Convert.ToInt32(parameters[1]);
            int top = Convert.ToInt32(parameters[2]);
            int bottom = Convert.ToInt32(parameters[3]);
            double leftWeight = Convert.ToDouble(parameters[4]);
            double rightWeight = Convert.ToDouble(parameters[5]);
            double topWeight = Convert.ToDouble(parameters[6]);
            double bottomWeight = Convert.ToDouble(parameters[7]);
            int index;

            if (_state == "ok")
            {

                arrayWeight[0] = leftWeight; arrayWeight[1] = rightWeight; arrayWeight[2] = topWeight; arrayWeight[3] = bottomWeight;
                arrayWalls[0] = left; arrayWalls[1] = right; arrayWalls[2] = top; arrayWalls[3] = bottom;
               
                List<string> Neighbours = getNeightbours();
                List<string> UnvisitedNeighbours;
               
                if (!visitedAllNeighbours(Neighbours))
                {
                    UnvisitedNeighbours = getUnvisitedNeighbours();
                    index = max(UnvisitedNeighbours);
                }
                else
                {
                    index = max(Neighbours);
                }

                switch (index)
                {
                    case 0: if (_x > 0 && left == 1) _x--; _my_weight = leftWeight; LastDirection = 0; break;
                    case 1: if (_x < Utils.Size - 1 && right == 1) _x++; _my_weight = rightWeight; LastDirection = 1; break;
                    case 2: if (_y > 0 && top == 1) _y--; _my_weight = topWeight; LastDirection = 2; break;
                    case 3: if (_y < Utils.Size - 1 && bottom == 1) _y++; _my_weight = bottomWeight; LastDirection = 3; break;
                }

                Console.WriteLine("{0} {1}", _x, _y);

                Send("maze", Utils.Str("update_weight", _previous_x, _previous_y));
                Send("maze", Utils.Str("change", _x, _y));
                maze[_y, _x] = true;
            }
        }

    }
}