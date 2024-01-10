using ActressMas;
using System;
using System.Collections.Generic;
namespace maze
{
    public class ExplorerAgent : Agent
    {
        private int _x, _y;
        private Random rand = new Random();
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
            if (action == "block")
            {
                MoveRandomly();
                Send("maze", Utils.Str("change", _x, _y));
            }
            if (action == "move")
            {
                MoveRandomly(parameters);
                Send("maze", Utils.Str("change", _x, _y));
            }
            if (action == "spawn")
            {
                Console.WriteLine(parameters);
                _x = Convert.ToInt32(parameters[0]);
                _y = Convert.ToInt32(parameters[1]);
                MoveRandomly();
                Send("maze", Utils.Str("change", _x, _y));
            }
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
        private void MoveRandomly(List<String> parameters)
        {
            int left = Convert.ToInt32(parameters[1]);
            int right = Convert.ToInt32(parameters[2]);
            int up = Convert.ToInt32(parameters[3]);
            int down = Convert.ToInt32(parameters[4]);

            //double up = cell.GetWeight(_x, _y, 0);
            //double down = cell.GetWeight(_x, _y, 1);
            //double left = cell.GetWeight(_x, _y, 2);
            //double right = cell.GetWeight(_x, _y, 3);
            //Console.WriteLine("Left {0} Right{1} Top{2} Bottom{3}", left, right, up, down);
            while (true)
            {
                bool flag = false;
                int d = Utils.RandNoGen.Next(4);
                switch (d)
                {
                    case 0: if (_x > 0 && left == 1)  _x--; flag = true; break;
                    case 1: if (_x < Utils.Size - 1 && right == 1) _x++; flag = true; break;
                    case 2: if (_y > 0 && up == 1) _y--; flag = true; break;
                    case 3: if (_y < Utils.Size - 1 && down == 1) _y++; flag = true; break;
                }
                if (flag) break;
            }
        }
    }
}