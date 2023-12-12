using ActressMas;
using Message = ActressMas.Message;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace maze
{
    public class MazeAgent : Agent
    {
        private MazeForm _formGui;
        public Dictionary<string, string> ExplorerPositions { get; set; }
        public MazeAgent()
        {
            ExplorerPositions = new Dictionary<string, string>();
            Thread t = new Thread(new ThreadStart(GUIThread));
            t.Start();
        }
        private void GUIThread()
        {
            _formGui = new MazeForm();
            _formGui.SetOwner(this);
            _formGui.ShowDialog();
            Application.Run();
        }

        public override void Setup()
        {
            Console.WriteLine("Starting " + Name);

            string compPos = Utils.Str(Utils.Size / 2, Utils.Size / 2);
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
                default:
                    break;
            }
            _formGui.UpdateMazeGUI();
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

            Send(sender, "move");
        }

    }
}
