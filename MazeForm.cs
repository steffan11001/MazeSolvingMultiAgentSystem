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
        public MazeForm(List<string> maze)
        {
            cells = maze;
            Console.WriteLine(cells);
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
            int index = 0;

            for (int i = 0; i < Utils.Size; i++)
                for (int j = 0; j < Utils.Size; j++) {
                    index = i * Utils.Size + j;
                    if (cells[index][0] == '0')
                        g.DrawLine(new Pen(Color.Black, 2.0f), 20 + j * cellSize, 20 + i * cellSize, 20 + j * cellSize, 20 + (i+1) * cellSize);
                    if (cells[index][1] == '0')
                        g.DrawLine(new Pen(Color.Black, 2.0f), 20 + (j+1) * cellSize, 20 + i * cellSize, 20 + (j+1) * cellSize, 20 + (i+1) * cellSize);
                    if (cells[index][2] == '0')
                        g.DrawLine(new Pen(Color.Black, 2.0f), 20 + j * cellSize, 20 + i * cellSize, 20 + (j+1) * cellSize, 20 + i * cellSize);
                    if (cells[index][3] == '0')
                        g.DrawLine(new Pen(Color.Black, 2.0f), 20 + j * cellSize, 20 + (i+1) * cellSize, 20 + (j+1) * cellSize, 20 + (i+1) * cellSize);

                }

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

    }
}
