using ActressMas;
using Message = ActressMas.Message;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace Reactive
{
    public class PlanetAgent : Agent
    {
        private PlanetForm _formGui;
        public Dictionary<string, string> ExplorerPositions { get; set; }
        public Dictionary<string, string> ResourcePositions { get; set; }
        public Dictionary<string, string> Loads { get; set; }
        private string _basePosition;

        public PlanetAgent()
        {
            ExplorerPositions = new Dictionary<string, string>();
            ResourcePositions = new Dictionary<string, string>();
            Loads = new Dictionary<string, string>();
            _basePosition = Utils.Str(Utils.Size / 2, Utils.Size / 2);

            Thread t = new Thread(new ThreadStart(GUIThread));
            t.Start();
        }

        private void GUIThread()
        {
            _formGui = new PlanetForm();
            _formGui.SetOwner(this);
            _formGui.ShowDialog();
            Application.Run();
        }

        public override void Setup()
        {
            Console.WriteLine("Starting " + Name);

            int offset = 2;
           
            List<string> resPos = new List<string>();
            string compPos = Utils.Str(Utils.Size / 2, Utils.Size / 2);
            resPos.Add(compPos); // the position of the base

            for (int i = 1; i <= Utils.NoResources; i++)
            {
                int x=Utils.RandNoGen.Next(Utils.Size);
                int y=Utils.RandNoGen.Next(Utils.Size);
                while ((resPos.Contains(compPos) || (int.Parse(resPos[i-1].Split(' ')[0].Split('{')[0]) - x > offset || int.Parse(resPos[i - 1].Split(' ')[1].Split('{')[0]) - y > offset)) && i!=1) // resources do not overlap
                {
                    Console.WriteLine(x +" " +y);

                    x = Utils.RandNoGen.Next(Utils.Size);
                    y = Utils.RandNoGen.Next(Utils.Size);
                    compPos = Utils.Str(x, y);
                }
                compPos = Utils.Str(x, y);


                ResourcePositions.Add("res" + i, compPos);
                resPos.Add(compPos);
            }
           
        }

        public override void Act(Message message)
        {
            Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

            string action; string parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);

            switch (action)
            {
                case "position":
                    HandlePosition(message.Sender, parameters);
                    break;

                case "change":
                    HandleChange(message.Sender, parameters);
                    break;

                case "pick-up":
                    HandlePickUp(message.Sender, parameters);
                    break;

                case "carry":
                    HandleCarry(message.Sender, parameters);
                    break;

                case "unload":
                    HandleUnload(message.Sender);
                    break;
                case "feromon":
                    //HandleFeromon(message.Sender, parameters);
                    break;
                default:
                    break;
            }
            _formGui.UpdatePlanetGUI();
        }

        private void HandlePosition(string sender, string position)
        {
            ExplorerPositions.Add(sender, position);
            Send(sender, "move");
        }

        private void HandleChange(string sender, string position)
        {
            ExplorerPositions[sender] = position;

            foreach (string k in ExplorerPositions.Keys)
            {
                if (k == sender)
                    continue;
                if (ExplorerPositions[k] == position)
                {
                    Send(sender, "block");
                    return;
                }
            }

            foreach (string k in ResourcePositions.Keys)
            {
                if (position != _basePosition && ResourcePositions[k] == position)
                {
                    Send(sender, "rock " + k);
                    return;
                }
            }

            Send(sender, "move");
        }

        private void HandlePickUp(string sender, string position)
        {
            Loads[sender] = position;
            Send(sender, "move");
        }

        private void HandleCarry(string sender, string position)
        {
            ExplorerPositions[sender] = position;
            string res = Loads[sender];
            ResourcePositions[res] = position;
            Send(sender, "move");
        }

        private void HandleUnload(string sender)
        {
            Loads.Remove(sender);
            Send(sender, "move");
        }

        private void HandleFeromon(string sender, string position)
        {
            Console.WriteLine("HandleFeromon");
        }
    }
}