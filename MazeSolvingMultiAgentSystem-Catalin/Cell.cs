using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace maze
{
    public class Cell
    {

        double[,,] weights = new double[20, 20, 4];
        public List<string> cells;
        


        /**
        0 - sus
        1 - dreapta
        2 - jos 
        3 - stanga
        **/

        public void initWeights()
        {
            cells = ReadMazeCells("E:\\PROIECT SMU\\MazeSolvingMultiAgentSystem-Catalin\\maze2.txt");
            int index = 0;
            for (int i = 0; i < 20; ++i)
            {
                for (int j = 0; j < 20; ++j)
                {
                    index = i * Utils.Size + j;
                    for (int direction = 0; direction < 4; ++direction)
                    {
                        weights[i, j, direction] = cells[index][direction];
                    }
                }
            }
        }
        public double GetWeight(int x, int y, int d)
        {
            double weight = weights[x, y, d];
            return weight;
        }
        public void updateWeight(int line, int coloumn, int direction)
        {
            weights[line, coloumn, direction] -= 0.001;
        }

        

        private List<string> ReadMazeCells(string filePath)
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
