using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace maze
{
    public partial class MazeForm: Form
    {
        private MazeAgent _ownerAgent;
        private Bitmap _doubleBufferImage;
        private List<string> cells;
        public MazeForm()
        {
            cells = readMazeCells("E:\\Sisteme_multiagent\\proiect\\MazeSolvingMultiAgentSystem\\maze.txt");
            InitializeComponent();
        }
       
        public void SetOwner(MazeAgent a)
        {
            _ownerAgent = a;
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            DrawPlanet();
        }

        public void UpdateMazeGUI()
        {
            DrawPlanet();
        }

        private void pictureBox_Resize(object sender, EventArgs e)
        {
            DrawPlanet();
        }


        private void DrawPlanet()
        {

            int w = pictureBox.Width;
            int h = pictureBox.Height;

            if (_doubleBufferImage != null)
            {
                _doubleBufferImage.Dispose();
                GC.Collect(); // prevents memory leaks
            }

            _doubleBufferImage = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(_doubleBufferImage);
            g.Clear(Color.White);

            int minXY = Math.Min(w, h);
            int cellSize = (minXY - 40) / Utils.Size;

            for (int i = 0; i <= Utils.Size; i++)
            {
                g.DrawLine(new Pen(Color.Black, 10.0f), 20, 20 + i * cellSize, 20 + Utils.Size * cellSize, 20 + i * cellSize);
                g.DrawLine(new Pen(new SolidBrush(Color.Black)), 20 + i * cellSize, 20, 20 + i * cellSize, 20 + Utils.Size * cellSize);
            }

            g.FillEllipse(Brushes.Red, 20 + Utils.Size / 2 * cellSize + 4, 20 + Utils.Size / 2 * cellSize + 4, cellSize - 8, cellSize - 8); // the base

            if (_ownerAgent != null)
            {
                foreach (string v in _ownerAgent.ExplorerPositions.Values)
                {
                    string[] t = v.Split();
                    int x = Convert.ToInt32(t[0]);
                    int y = Convert.ToInt32(t[1]);

                    g.FillEllipse(Brushes.Blue, 20 + x * cellSize + 6, 20 + y * cellSize + 6, cellSize - 12, cellSize - 12);
                }

               
            }

            Graphics pbg = pictureBox.CreateGraphics();
            pbg.DrawImage(_doubleBufferImage, 0, 0);
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
