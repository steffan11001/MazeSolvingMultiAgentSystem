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
        private float DecreaseRate = 0.005f;
        public Dictionary<int,float> Weight;
        public int ExitPositionX, ExitPositionY;

        public MazeAgent()
        {
            cells = readMazeCells("D:\\SistemeMultiagent\\MazeSolvingMultiAgentSystem\\maze4.txt");
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
                case "to_exit":
                    HandleToExit(message.Sender, parameters);
                    break;
                default:
                    break;
            }
            _formGui.UpdateMazeGUI();
        }

        private void HandleToExit(string sender, string position)
        {
            string[] parameters = position.Split(' ');
            int x = Convert.ToInt32(parameters[0]);
            int y = Convert.ToInt32(parameters[1]);

            foreach (string k in ExplorerPositions.Keys)
            {
                if (k != sender)
                {
                    string[] agent_pos = ExplorerPositions[k].Split(' ');
                    int agent_x = Convert.ToInt32(agent_pos[0]);
                    int agent_y = Convert.ToInt32(agent_pos[1]);
                    if (x == agent_x && y == agent_y)
                    {
                        Console.WriteLine(sender + " " + k + " Position " + x + " " + y);
                        Send(sender, "wait");
                        return;
                    }
                }
            }

            ExplorerPositions[sender] = position;

            if (x == ExitPositionX && y == ExitPositionY)
            {
                Send(sender, "finish");
                ExplorerPositions.Remove(sender);
            }
            else
            {
                Send(sender, "move");
            }
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

            int cellPos = y * Utils.Size + x;

           
            if(blockPoint(cells[cellPos]))
                Weight[cellPos] = Weight[cellPos] - 2*DecreaseRate;
            else
                Weight[cellPos] = Weight[cellPos] - DecreaseRate;

        }
        private void HandleSpawn(string sender)
        {
            bool flag;
            
            while (true)
            {
                int x = Utils.RandNoGen.Next(Utils.Size);
                int y = Utils.RandNoGen.Next(Utils.Size);
                int cellsPos = y * Utils.Size + x;
                flag = false;

                foreach(string k in ExplorerPositions.Keys)
                {
                    string[] positions = ExplorerPositions[k].Split(' ');
                    int posX = Convert.ToInt32(positions[0]);
                    int posY = Convert.ToInt32(positions[1]);
               
                    if (x == posX && y == posY)
                        flag = true;
                }

                if (x == ExitPositionX && y == ExitPositionY)
                    flag = true;

                if (!flag)
                {
                    ExplorerPositions.Add(sender, string.Format("{0} {1}", x, y));
                    Send(sender, Utils.Str("spawn", x, y));

                    float left = 0, right = 0, top = 0, bottom = 0;
                    if (cellsPos % Utils.Size != 0 && cells[cellsPos][0] != '0')
                        left = Weight[cellsPos - 1];
                    if (cellsPos % Utils.Size != Utils.Size - 1 && cells[cellsPos][1] != '0')
                        right = Weight[cellsPos + 1];
                    if (cellsPos >= Utils.Size && cells[cellsPos][2] != '0')
                        top = Weight[cellsPos - Utils.Size];
                    if (cellsPos < Utils.Size * (Utils.Size - 1) && cells[cellsPos][3] != '0')
                        bottom = Weight[cellsPos + Utils.Size];

                    string weights = string.Format("{0} {1} {2} {3}", left, right, top, bottom);
                    Send(sender, Utils.StrForExplorer("move", cells[cellsPos], weights));

                    break;
                }
            }
        }
        private void HandleChangeBlock(string sender, int current_x, int current_y)
        {
            float left = 0, right = 0, top = 0, bottom = 0;
            int currentCellPos = current_y * Utils.Size + current_x;

            if (currentCellPos % Utils.Size != 0 && cells[currentCellPos][0] != '0')
                left = Weight[currentCellPos - 1];
            if (currentCellPos % Utils.Size != Utils.Size - 1 && cells[currentCellPos][1] != '0')
                right = Weight[currentCellPos + 1];
            if (currentCellPos >= Utils.Size && cells[currentCellPos][2] != '0')
                top = Weight[currentCellPos - Utils.Size];
            if (currentCellPos < Utils.Size * (Utils.Size - 1) && cells[currentCellPos][3] != '0')
                bottom = Weight[currentCellPos + Utils.Size];

            foreach (string k in ExplorerPositions.Keys)
            {
                string[] AgentPosition = ExplorerPositions[k].Split(' ');
                int agent_x = Convert.ToInt32(AgentPosition[0]);
                int agent_y = Convert.ToInt32(AgentPosition[1]);

                if (agent_x == current_x - 1 && agent_y == current_y)
                    left = 0;
                else if (agent_x == current_x + 1 && agent_y == current_y)
                    right = 0;
                else if (agent_x == current_x && agent_y == current_y - 1)
                    top = 0;
                else if (agent_x == current_x && agent_y == current_y + 1)
                    bottom = 0;

            }

            string weights = string.Format("{0} {1} {2} {3}", left, right, top, bottom);

            Send(sender, Utils.StrForExplorer("block", cells[currentCellPos], weights));
        }
        private void HandleChange(string sender, string position)
        {
            string[] parameters = position.Split(' ');
            string strPosition = parameters[0] + " " + parameters[1];
            int x = Convert.ToInt32(parameters[0]);
            int y = Convert.ToInt32(parameters[1]);
            int current_x = Convert.ToInt32(parameters[2]);
            int current_y = Convert.ToInt32(parameters[3]);

            int cellPos = y * Utils.Size + x;
           
            float left=0, right=0, top=0, bottom=0;
            if (cellPos % Utils.Size != 0 && cells[cellPos][0] != '0')
                left = Weight[cellPos-1];
            if (cellPos % Utils.Size != Utils.Size - 1 && cells[cellPos][1] != '0')
                right = Weight[cellPos + 1];
            if (cellPos >= Utils.Size && cells[cellPos][2] != '0')
                top = Weight[cellPos - Utils.Size];
            if (cellPos < Utils.Size * (Utils.Size - 1) && cells[cellPos][3] != '0')
                bottom = Weight[cellPos + Utils.Size];

            foreach(string k in ExplorerPositions.Keys)
            {
                string[] AgentPosition = ExplorerPositions[k].Split(' ');
                int agent_x = Convert.ToInt32(AgentPosition[0]);
                int agent_y = Convert.ToInt32(AgentPosition[1]);

                if (agent_x == x - 1 && agent_y == y)
                   left = 0;
                else if (agent_x == x + 1 && agent_y == y)
                   right = 0;
                else if (agent_x == x && agent_y == y - 1)
                   top = 0;
                else if (agent_x == x && agent_y == y + 1)
                   bottom = 0;

            }

            string weights = string.Format("{0} {1} {2} {3}", left, right, top, bottom);

            foreach (string k in ExplorerPositions.Keys)
            {
                if (k == sender)
                    continue;
                if (ExplorerPositions[k] == strPosition)
                {

                    HandleChangeBlock(sender, current_x, current_y);
                        return;
                }
            }

            ExplorerPositions[sender] = strPosition;
            if (x == ExitPositionX && y == ExitPositionY)
            {
                Send(sender, "finish");
                ExplorerPositions.Remove(sender);
            }
            else
                Send(sender, Utils.StrForExplorer("move", cells[cellPos], weights));
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
                    Utils.Size = Convert.ToInt32(reader.ReadLine());

                    int count = 0;
                    while (!reader.EndOfStream)
                    {
                        count++;
                        string line = reader.ReadLine();
                        result.Add(line);

                        if (count == Utils.Size * Utils.Size)
                            break;
                    }

                    string[] ExitPosition = reader.ReadLine().Split(' ');
                    ExitPositionX = Convert.ToInt32(ExitPosition[0]);
                    ExitPositionY = Convert.ToInt32(ExitPosition[1]);

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
