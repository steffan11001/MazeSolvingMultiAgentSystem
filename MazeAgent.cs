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
        private float DecreaseRate = 0.05f;
        public Dictionary<int,float> Weight;
        public MazeAgent()
        {
            cells = readMazeCells("D:\\SistemeMultiagent\\MazeSolvingMultiAgentSystem\\maze3.txt.txt");
            ExplorerPositions = new Dictionary<string, string>();
            Weight = new Dictionary<int, float>();
            int index = 0;
            for(int i=0; i< Utils.Size; i++)
                for(int j=0; j<Utils.Size; j++)
                {
                    index = i * Utils.Size + j;
                    Weight[index] = 1.0f;
                }

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
                case "update_weight":
                    UpdateWeight(message.Sender, parameters);
                    break;
                default:
                    break;
            }
            _formGui.UpdateMazeGUI();
        }

        private bool blockPoint(string cell)
        {
            bool flag = false;
            int count = 0;

            foreach(char c in cell)
            {
                if (c == '0')
                    count = count + 1;
            }

            if (count == 3)
                flag = true;

            return flag;
        }
        private void UpdateWeight(string sender, string parameters)
        {
            string[] param = parameters.Split(' ');
            int x = Convert.ToInt32(param[0]);
            int y = Convert.ToInt32(param[1]);
            string state = param[2];

            int cellPos = y * Utils.Size + x;

            if (state == "block")
                Weight[cellPos] = 0;
            else
                if(blockPoint(cells[cellPos]))
                    Weight[cellPos] = Weight[cellPos] - 2*DecreaseRate;
                else
                    Weight[cellPos] = Weight[cellPos] - DecreaseRate;

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
            float left=0, right=0, top=0, bottom=0;
            if (cellsPos % Utils.Size != 0)
                left = Weight[cellsPos-1];
            if (cellsPos % Utils.Size != Utils.Size - 1)
                right = Weight[cellsPos + 1];
            if (cellsPos >= Utils.Size)
                top = Weight[cellsPos - Utils.Size];
            if (cellsPos < Utils.Size * (Utils.Size - 1))
                bottom = Weight[cellsPos + Utils.Size];

            string weights = string.Format("{0} {1} {2} {3}", left, right, top, bottom);

            foreach (string k in ExplorerPositions.Keys)
            {
                if (k == sender)
                    continue;
                if (ExplorerPositions[k] == position)
                {
                    Send(k, Utils.Str("back", Weight[cellsPos]));
                    return;
                }
            }

            ExplorerPositions[sender] = position;

            Send(sender, Utils.StrForExplorer("move", cells[cellsPos], weights));
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
