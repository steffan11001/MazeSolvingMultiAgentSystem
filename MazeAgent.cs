using ActressMas;
using Message = ActressMas.Message;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace maze
{
    public class MazeAgent : Agent
    {
        private MazeForm _formGui;
        public Dictionary<string, string> ExplorerPositions { get; set; }
        private List<string> cells;
        public MazeAgent()
        {
            cells = readMazeCells("D:\\SistemeMultiagent\\MazeSolvingMultiAgentSystem\\maze2.txt");
            ExplorerPositions = new Dictionary<string, string>();
            
            Thread t = new Thread(new ThreadStart(GUIThread));
            t.Start();
        }
        private void GUIThread()
        {
            _formGui = new MazeForm(cells);
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
                case "spawn":
                    HandleSpawn(message.Sender);
                    break;
                case "change":
                    HandleChange(message.Sender, parameters);
                    break;
                default:
                    break;
            }
            _formGui.UpdateMazeGUI();
        }
        private void HandleSpawn(string sender)
        {
            bool flag = false;
            
            while (true)
            {
                int x = Utils.RandNoGen.Next(Utils.Size);
                int y = Utils.RandNoGen.Next(Utils.Size);

                foreach(string k in ExplorerPositions.Keys)
                {
                    string[] positions = ExplorerPositions[k].Split(' ');
                    int posX = Convert.ToInt32(positions[0]);
                    int posY = Convert.ToInt32(positions[1]);
                    if (x == posX && y == posY)
                        flag = true;
                    Console.WriteLine(posX + " " + posY);
                }

                if (!flag)
                {
                    ExplorerPositions.Add(sender, string.Format("{0} {1}", x, y));
                    Send(sender, Utils.Str("spawn", x, y));
                    break;
                }
            }
        }
        private void HandleChange(string sender, string position)
        {
            string[] parameters = position.Split(' ');
            int x = Convert.ToInt32(parameters[0]);
            int y = Convert.ToInt32(parameters[1]);
            int cellsPos = y * Utils.Size + x;
            Console.WriteLine(cellsPos);

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

            for (int i=0; i<cells.Count; i++)
            {
                Console.WriteLine(i+" "+cells[i]);
            }

            Console.WriteLine(cells[cellsPos]);
            Send(sender, Utils.Str("move", 1, cells[cellsPos][0], cells[cellsPos][1], cells[cellsPos][2], cells[cellsPos][3]));
        }

        private List<string> readMazeCells(string filePath)
        {
            List<string> result = new List<string>();
            if (File.Exists(filePath))
            {
                // Open the file with a StreamReader
                using (StreamReader reader = new StreamReader(filePath))
                {
                    // Read each line until the end of the file
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        result.Add(line);
                        Console.WriteLine(line);
                        // You can store or process each string as needed
                    }
                }
            }
            else
            {
                Console.WriteLine("File does not exist: " + filePath);
            }
            return result;
        }

    }
}
