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

        }
        
        public override void Act(Message message)
        {
            Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);
            string action;
            List<string> parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);

        }

        
    }
}