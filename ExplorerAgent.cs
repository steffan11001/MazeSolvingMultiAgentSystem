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
        
        public override void Setup()
        {
            Console.WriteLine("setup Explorer " + Name);
            Console.WriteLine("Starting " + Name);

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
                GoBack(message.Sender, parameters);
            }
            if (action == "move")
            {
                Move(parameters);
            }
            if(action == "spawn")
            {
                _x = Convert.ToInt32(parameters[0]);
                _y = Convert.ToInt32(parameters[1]);
                MoveRandomly();
                Send("maze", Utils.Str("change", _x, _y));
            }

        }
        private void GoBack(string sender, List<string> parameters)
        {
            double weight = Convert.ToDouble(parameters[0]);
            Console.WriteLine(weight + " " + _my_weight);
            if (weight >= _my_weight)
            {
                Send(sender, Utils.Str("back", _my_weight));
                Send("maze", Utils.Str("change", _x, _y));
            }
            else
                Send("maze", Utils.Str("change", _previous_x, _previous_y));
        }

        public void Spawn()
        {
            _x = rand.Next(0, Utils.Size);
            _y = rand.Next(0, Utils.Size);
            Send("maze", Utils.Str("position", _x, _y));
        }
        private void MoveRandomly()
        {
            int d = Utils.RandNoGen.Next(4);
            switch (d)
            {
                case 0: if (_x > 0) _x--; break;
                case 1: if (_x < Utils.Size - 1) _x++; break;
                case 2: if (_y > 0) _y--; break;
                case 3: if (_y < Utils.Size - 1) _y++; break;
            }
        }
        private int max(double[] weights, int[] walls)
        {
            int index = 0;
            double max = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                if (max < weights[i] && walls[i] == 1)
                {
                    max = weights[i];
                    index = i;
                }
            }

            return index;
        }

        private bool blockPoint(double[] weightArray,int[] arrayWalls)
        {
            bool flag = false;
            int count = 0;

            for(int i=0; i < weightArray.Length; i++)
            {
                if (arrayWalls[i] == 0 || weightArray[i] == 0)
                    count = count + 1;
            }
            if (count == 3)
                flag = true;

            return flag;
        }
        private void Move(List<String> parameters)
        {
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

            if (_state == "ok")
            {

                double[] arrayWeight = { leftWeight, rightWeight, topWeight, bottomWeight };
                int[] arrayWalls = { left, right, top, bottom };
                int index = max(arrayWeight, arrayWalls);
                bool blockState = blockPoint(arrayWeight, arrayWalls);

                switch (index)
                {
                    case 0: if (_x > 0 && left == 1) _x--; _my_weight = leftWeight; break;
                    case 1: if (_x < Utils.Size - 1 && right == 1) _x++; _my_weight = rightWeight; break;
                    case 2: if (_y > 0 && top == 1) _y--; _my_weight = topWeight; break;
                    case 3: if (_y < Utils.Size - 1 && bottom == 1) _y++; _my_weight = bottomWeight; break;
                }
                Console.WriteLine("{0} {1}", _x, _y);

                if (blockState)
                {
                    Send("maze", Utils.Str("update_weight", _previous_x, _previous_y, "block"));
                    Send("maze", Utils.Str("change", _x, _y));
                }
                else
                {
                    Send("maze", Utils.Str("update_weight", _previous_x, _previous_y, "ok"));
                    Send("maze", Utils.Str("change", _x, _y));
                }
            }
            if(_state == "back")
            {
            
            }
        }


    }
}