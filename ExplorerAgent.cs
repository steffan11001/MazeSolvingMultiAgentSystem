using ActressMas;
using System;
using System.Collections.Generic;

namespace maze
{
    public class ExplorerAgent : Agent
    {
        private int _x, _y;
        private int _previous_x, _previous_y;
        private double _my_weight = 0;
        private int step = 0;
        private Dictionary<int, string> Path;
        private List<List<string>> ExitPath;
        private List<string> Exit;
        private byte _last_direction, _direction;
        private byte[] OppositeDirection = { 1, 0, 3, 2};
        private double[] arrayWeight = new double[4];
        private int[] arrayWalls = new int[4];
        private bool[,] maze;
        private bool SearchingState = true;

        public override void Setup()
        {
            Console.WriteLine("setup Explorer " + Name);
            Console.WriteLine("Starting " + Name);

            Path = new Dictionary<int, string>();
            ExitPath = new List<List<string>>();

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
                if (SearchingState)
                {
                    Send("maze", Utils.Str("update_weight", _previous_x, _previous_y));
                    maze[_y, _x] = true;
                    Move(parameters);
                }
                else
                {
                    int position_in_path = inPath(_x+"_"+_y);
                    AddInPath(_x + "_" +_y, position_in_path);
                    Send("maze", Utils.Str("update_weight", _previous_x, _previous_y));
                    maze[_y, _x] = true;
                    ToExitPoint();
                }
            }
            if (action == "wait")
            {
                _x = _previous_x;
                _y = _previous_y;
                ToExitPoint();
            }
            if (action == "spawn")
            {
                _x = Convert.ToInt32(parameters[0]);
                _y = Convert.ToInt32(parameters[1]);
            }
            if(action == "finish")
            {
                int position_in_path = inPath(_x + "_" + _y);
                AddInPath(_x + "_" + _y, position_in_path);
                Console.WriteLine(Name + ":");
                string line = "";
                for(int i = 0; i < step; i++)
                {
                    if (i == 0)
                        line = Path[i];
                    else
                        line = line + " " + Path[i];
                }
                Broadcast(Utils.Str("path", line));
            }
            if(action == "path")
            {
                List<string> ReceivedPath = new List<string>();
                for (int i = 0; i < parameters.Count; i++)
                {
                    ReceivedPath.Add(parameters[i]);
                }
                ExitPath.Add(ReceivedPath);

                Console.Write(Name + ": ");
                for (int i = 0; i < step; i++)
                {
                    Console.Write(Path[i]+" ");
                }
                Console.WriteLine();
            }

        }
        private void ToExitPoint()
        {
  
            int direction = -1;
            string position = _x + "_" + _y;
            _previous_x = _x;
            _previous_y = _y;

            Console.WriteLine(Name + " Position " + _x + " " + _y);
            Console.Write(Name + ": ");
            for(int i = 0; i < Exit.Count; i++)
            {
                Console.Write(Exit[i] + " ");
            }
            Console.WriteLine();



            for (int i=0; i<Exit.Count-1; i++)
                if(Exit[i] == position)
                {
                    string[] pos = Exit[i + 1].Split('_');
                    int x = Convert.ToInt32(pos[0]);
                    int y = Convert.ToInt32(pos[1]);
                    Console.WriteLine(Name + " Position " + _x + " " + _y + " "+x+" "+y);
                    if (_x == x + 1)
                        direction = 0;
                    if (_x == x - 1)
                        direction = 1;
                    if (_y == y + 1)
                        direction = 2;
                    if (_y == y - 1)
                        direction = 3;
                    Console.WriteLine(Name + " " + direction);
                    break;
                }

            switch (direction)
            {
                case 0: Send("maze", Utils.Str("to_exit", _x - 1, _y)); _x = _x - 1; break;
                case 1: Send("maze", Utils.Str("to_exit", _x + 1, _y)); _x = _x + 1; break;
                case 2: Send("maze", Utils.Str("to_exit", _x, _y - 1)); _y = _y - 1; break;
                case 3: Send("maze", Utils.Str("to_exit", _x, _y + 1)); _y = _y + 1; break;
            }


        }
        private void GoBack()
        {
            if (step == 0 || step == 1)
            {
                List<string> Neighbours = getNeightbours();
                for(int i = 0; i < Neighbours.Count; i++)
                {
                    if (Neighbours[i] != "") {
                        string[] position = Neighbours[i].Split(' ');
                        int x = Convert.ToInt32(position[0]);
                        int y = Convert.ToInt32(position[1]);
                        if(x != _x && y != _y)
                        {
                            _x = x;
                            _y = y;
                            break;
                        }
                    }
                }
                Send("maze", Utils.Str("change", _x, _y));

            }
            else
            {
                string[] position = Path[step - 2].Split('_');
                step = step - 2;
                _previous_x = _x;
                _previous_y = _y;
                _direction = OppositeDirection[_last_direction];
                _x = Convert.ToInt32(position[0]);
                _y = Convert.ToInt32(position[1]);
                Send("maze", Utils.Str("change", _x, _y));
            }
        }
        private List<string> getNeightbours()
        {
            List<string> Neighbours=new List<string>();
            for (int i = 0; i < 4; i++)
            {
                Neighbours.Add("");
            }

            if (arrayWalls[0] == 1 && arrayWeight[0] != 0)
                Neighbours[0] = Convert.ToString((_x - 1) + " " + _y);
            if (arrayWalls[1] == 1 && arrayWeight[1] != 0)
                Neighbours[1] = Convert.ToString((_x + 1) + " " + _y);
            if (arrayWalls[2] == 1 && arrayWeight[2] != 0)
                Neighbours[2] = Convert.ToString(_x + " " + (_y-1));
            if (arrayWalls[3] == 1 && arrayWeight[3] != 0)
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
                    if (maze[y, x] == false && arrayWeight[i] != 0)
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

            if (arrayWalls[0] == 1 && maze[_y, _x - 1] == false && arrayWeight[0] != 0)
                Neighbours[0] = Convert.ToString((_x - 1) + " " + _y);
            if (arrayWalls[1] == 1 && maze[_y, _x + 1] == false && arrayWeight[1] != 0)
                Neighbours[1] = Convert.ToString((_x + 1) + " " + _y);
            if (arrayWalls[2] == 1 && maze[_y - 1, _x] == false && arrayWeight[2] != 0)
                Neighbours[2] = Convert.ToString(_x + " " + (_y - 1));
            if (arrayWalls[3] == 1 && maze[_y + 1, _x] == false && arrayWeight[3] != 0)
                Neighbours[3] = Convert.ToString(_x + " " + (_y + 1));

            return Neighbours;
        }
        private int getNeighboursCount(List<string> Neighbours)
        {
            int count = 0;
            for (int i = 0; i < Neighbours.Count; i++)
                if (Neighbours[i] != "")
                    count = count + 1;

            return count;
        }
        private int inPath(string position)
        {
            int index = -1;

            foreach (int i in Path.Keys)
                if (Path[i] == position)
                    index = i;

            return index;
        }
        private bool searchInExitPath()
        {
            bool flag = false;

            for(int k = step-1; k >= 0; k--)
                for(int i=0;i<ExitPath.Count; i++)
                    for(int j=0; j<ExitPath[i].Count; j++)
                        if(ExitPath[i][j] == Path[k]){  flag = true; break;  }
            
            return flag;
        }
        private List<string> buildingPathToExit()
        {
            List<string> ToExit = new List<string>();
            int temp_i = -1, temp_j = -1, temp_k = -1;
            for (int k = 0; k < step; k++)
                for (int i = 0; i < ExitPath.Count; i++)
                    for (int j = 0; j < ExitPath[i].Count; j++)
                        if (ExitPath[i][j] == Path[k])
                        {
                            temp_i = i; temp_j = j; temp_k = k; break;
                        }

            for (int k = step - 1; k >= temp_k; k--)
                ToExit.Add(Path[k]);

            for (int j = temp_j + 1; j < ExitPath[temp_i].Count; j++)
            {
                ToExit.Add(ExitPath[temp_i][j]);
            }
            int index = 0;
            while (index<ToExit.Count-1) {
                if (ToExit[index] == ToExit[index + 1])
                    ToExit.RemoveAt(index);
                else
                    index++;
            }

            return ToExit;
        }
        private int max(List<string> Neighbours)
        {
            int index = -1;
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
                    if (max < arrayWeight[i] && Neighbours[i] != "" && i != OppositeDirection[_last_direction])
                    {
                        max = arrayWeight[i];
                        index = i;
                    }
                }
            }

            return index;
        }
        private void AddInPath(string position, int position_in_path)
        {
            if (position_in_path != -1)
            {
                step = position_in_path;
                Path[step] = position;
            }
            else if (step < Path.Count)
                Path[step] = position;
            else
                Path.Add(step, position);

            step++;
        }
        private void Move(List<String> parameters)
        {
            string position = _x + "_" + _y;
            int position_in_path = inPath(position);
            AddInPath(position, position_in_path);
            SearchingState = searchInExitPath() ? false : true;

            if (SearchingState)
            {

                _previous_x = _x;
                _previous_y = _y;
                _last_direction = _direction;

                int left = Convert.ToInt32(parameters[0]);
                int right = Convert.ToInt32(parameters[1]);
                int top = Convert.ToInt32(parameters[2]);
                int bottom = Convert.ToInt32(parameters[3]);
                double leftWeight = Convert.ToDouble(parameters[4]);
                double rightWeight = Convert.ToDouble(parameters[5]);
                double topWeight = Convert.ToDouble(parameters[6]);
                double bottomWeight = Convert.ToDouble(parameters[7]);
                int index;

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
                    case 0: if (_x > 0 && left == 1) _x--; _my_weight = leftWeight; _direction = 0; break;
                    case 1: if (_x < Utils.Size - 1 && right == 1) _x++; _my_weight = rightWeight; _direction = 1; break;
                    case 2: if (_y > 0 && top == 1) _y--; _my_weight = topWeight; _direction = 2; break;
                    case 3: if (_y < Utils.Size - 1 && bottom == 1) _y++; _my_weight = bottomWeight; _direction = 3; break;
                }

                Send("maze", Utils.Str("change", _x, _y));
            }
            else
            {
                Exit = buildingPathToExit();

                ToExitPoint();
                //Console.Write(Name + ": ");
                //for(int i = 0; i < Exit.Count; i++)
                //{
                //    Console.Write(Exit[i] + " ");
                //}
                //Console.WriteLine();
            }
        }

    }
}