using ActressMas;
using System;
using System.Collections.Generic;

namespace maze
{
    public class ExplorerAgent : Agent
    {
        private int _x, _y;
        
        public override void Setup()
        {
            Console.WriteLine("setup Explorer " + Name);
            Console.WriteLine("Starting " + Name);

            _x = Utils.Size / 2;
            _y = Utils.Size / 2;
            Send("maze", Utils.Str("position", _x, _y));
        }
        
        public override void Act(Message message)
        {
            Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

            string action;
            List<string> parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);
            if (action == "block")
            {
                // R1. If you detect an obstacle, then change direction
                MoveRandomly();
                Send("maze", Utils.Str("change", _x, _y));
            }

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


    }
}