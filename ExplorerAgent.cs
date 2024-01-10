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
        private byte _last_direction=255, _direction=255;
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
            if (action == "block")
            {
                _direction = _last_direction;
                _x = _previous_x;
                _y = _previous_y;
                Move(parameters);
            }
            if (action == "move")
            {
                if (SearchingState)
                {
                    if (_previous_x != _x || _previous_y != _y)
                    {
                        Send("maze", Utils.Str("update_weight", _previous_x, _previous_y));
                        AddInPath(_previous_x + "_" + _previous_y);
                    }
                    maze[_previous_y, _previous_x] = true;
                    Move(parameters);
                }
                else
                {
                    if (_previous_x != _x || _previous_y != _y)
                    {
                        AddInPath(_previous_x + "_" + _previous_y);
                        Send("maze", Utils.Str("update_weight", _previous_x, _previous_y));
                    }
                    maze[_previous_y, _previous_x] = true;
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
                _previous_x = _x;
                _previous_y = _y;
            }
            
            if(action == "path")
            {
                bool intersection = false;
                List<string> ReceivedPath = new List<string>();
                for (int i = 0; i < parameters.Count; i++)
                {
                    ReceivedPath.Add(parameters[i]);
                }
                for(int i=0; i<ExitPath.Count; i++)
                {
                    List<string> TempPath = ExitPath[i];
                    for (int j = 0; j < TempPath.Count - 1; j++)
                        for (int k = ReceivedPath.Count - 1; k > 0; k--)
                            if (TempPath[j] == ReceivedPath[k] && TempPath[j + 1] == ReceivedPath[k - 1])
                                intersection = true;
                }

                if(intersection == false)
                    ExitPath.Add(ReceivedPath);

                //Console.WriteLine(Name + ":" + intersection);

            }

            if(action == "finish")
            {
                AddInPath(_previous_x + "_" + _previous_y);
                AddInPath(_x + "_" + _y);
                string line = "";
                for (int i = 0; i < step; i++)
                {
                    if (i == 0)
                        line = Path[i];
                    else
                        line = line + " " + Path[i];
                }
                Broadcast(Utils.Str("path", line));
              
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
            for (int i = 0; i < Exit.Count; i++)
            {
                Console.Write(Exit[i] + " ");
            }
            Console.WriteLine();



            for (int i = 0; i < Exit.Count - 1; i++)
                if (Exit[i] == position)
                {
                    string[] pos = Exit[i + 1].Split('_');
                    int x = Convert.ToInt32(pos[0]);
                    int y = Convert.ToInt32(pos[1]);
                    Console.WriteLine(Name + " Position " + _x + " " + _y + " " + x + " " + y);
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

        private List<string> getUnexploredNeighbours(double [] weights)
        {
            List<string> unexplored = new List<string>();
            if (_x - 1 >= 0 && maze[_y, _x-1] == false && weights[0] > 0)
                unexplored.Add((_x - 1) + "_" + _y);
            else
                unexplored.Add("");

            if (_x + 1 < Utils.Size && maze[_y, _x+1] == false && weights[1] > 0)
                unexplored.Add((_x + 1) + "_" + _y);
            else
                unexplored.Add("");

            if (_y - 1 >= 0 && maze[_y - 1,_x] == false && weights[2] > 0)
                unexplored.Add(_x + "_" + (_y - 1));
            else
                unexplored.Add("");

            if (_y + 1 < Utils.Size && maze[_y + 1,_x] == false && weights[3] > 0)
                unexplored.Add(_x + "_" + (_y + 1));
            else
                unexplored.Add("");

            return unexplored;
            
        }
        private void AddInPath(string position)
        {
            for(int i=0; i < Path.Count; i++)
            {
                if (Path[i] == position)
                {
                    step = i+1;
                    return;
                }
            }

            if (step == Path.Count)
                Path.Add(step, position);
            else
                Path[step] = position;
            
            step++;

        }
       
        private int getDirection(double [] weights)
        {
            List<string> unexplored = getUnexploredNeighbours(weights);
            int direction = -1;

            for (int i = 0; i < 4; i++)
                if (unexplored[i] != "")
                {
                    direction = i;
                    return direction;
                }

            double max = 0;

            for(int i=0; i < weights.Length; i++)
            {
                if (weights[i] > max && i != OppositeDirection[_last_direction])
                {
                    max = weights[i];
                    direction = i;
                }
            }

            if (direction == -1 && _last_direction != 255 && weights[OppositeDirection[_last_direction]] > 0)
                direction = OppositeDirection[_last_direction];

            return direction;

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
            while (index < ToExit.Count - 1)
            {
                if (ToExit[index] == ToExit[index + 1])
                    ToExit.RemoveAt(index);
                else
                    index++;
            }

            return ToExit;
        }

        private bool IsExit()
        {

            for (int k = 0; k < step; k++)
                for (int i = 0; i < ExitPath.Count; i++)
                    for (int j = 0; j < ExitPath[i].Count; j++)
                        if (ExitPath[i][j] == Path[k])
                        {
                            return false;
                        }

            return true;
        }
       
       
        private void Move(List<String> parameters)
        {
            string position = _x + "_" + _y;
            
            _previous_x = _x;
            _previous_y = _y;
            _last_direction = _direction;

            SearchingState = IsExit();

            if (SearchingState)
            {
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

                index = getDirection(arrayWeight);

                switch (index)
                {
                    case 0: if (_x > 0 && left == 1) _x--; _direction = 0; break;
                    case 1: if (_x < Utils.Size - 1 && right == 1) _x++; _direction = 1; break;
                    case 2: if (_y > 0 && top == 1) _y--; _direction = 2; break;
                    case 3: if (_y < Utils.Size - 1 && bottom == 1) _y++; _direction = 3; break;
                }

                Send("maze", Utils.Str("change", _x, _y, _previous_x, _previous_y));
            }
            else
            {

                Exit = buildingPathToExit();
                Console.Write(Name);
                for(int i = 0; i<Exit.Count; i++)
                {
                    Console.Write(" " + Exit[i]);
                }
                
                Console.WriteLine();

                bool fl = false;
                for(int i=0; i < Exit.Count; i++)
                {
                    if (Exit[i] == (_x + "_" + _y))
                        fl = true;
                }
                if(fl == false)
                    Exit.Insert(0, _x + "_" + _y);

                ToExitPoint();
            }
            
        }

    }
}