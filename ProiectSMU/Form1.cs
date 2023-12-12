using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProiectSMU
{
    public partial class Form1 : Form
    {
        private Cell[,] _matrix;
        private Bitmap _doubleBufferImage;
        private PictureBox pictureBox=new PictureBox();

        public Form1()
        {
            InitializeComponent();
            InitializeMatrixFromTxt("F:\\laborator SMu\\ProiectSMU\\ProiectSMU\\maze.txt"); // Provide the path to your text file
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void InitializeMatrixFromTxt(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    // Read matrix size from the first line
                    //[] sizeTokens = reader.ReadLine().Split('=');
                    int rows = 5;

                    //sizeTokens = reader.ReadLine().Split('=');
                    int cols = 5;

                    _matrix = new Cell[rows, cols];

                    // Read matrix elements from the file
                    for (int i = 0; i < rows; i++)
                    {
                        
                        for (int j = 0; j < cols; j++)
                        {
                            string line = reader.ReadLine();
                            // Assuming each element is represented by a binary string
                            _matrix[i, j] = new Cell { Wall = Convert.ToByte(line.ToString(), 2) };
                            Console.WriteLine(_matrix[i,j]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading the file: " + ex.Message);
            }
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e) => DrawMaze(GetPictureBox());

        private void pictureBox_Resize(object sender, EventArgs e) => DrawMaze(GetPictureBox());

        private PictureBox GetPictureBox()
        {
            return pictureBox;
        }

        private void DrawMaze(PictureBox pictureBox)
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
            int cellSize = (minXY - 40) / _matrix.GetLength(0); // Assuming the matrix is square

            for (int i = 0; i < _matrix.GetLength(0); i++)
            {
                for (int j = 0; j < _matrix.GetLength(1); j++)
                {
                    int x = 20 + j * cellSize;
                    int y = 20 + i * cellSize;

                    // Draw walls based on the 'wall' property
                    DrawWalls(g, _matrix[i, j], x, y, cellSize);
                }
            }

            Graphics pbg = pictureBox.CreateGraphics();
            pbg.DrawImage(_doubleBufferImage, 0, 0);
        }

        private void DrawWalls(Graphics g, Cell cell, int x, int y, int cellSize)
        {
            // Check each direction and draw walls accordingly
            if ((cell.Wall & 8) != 0) // Binary: 1000 (up)
                g.DrawLine(Pens.DarkGray, x, y, x + cellSize, y);

            if ((cell.Wall & 4) != 0) // Binary: 0100 (down)
                g.DrawLine(Pens.DarkGray, x, y + cellSize, x + cellSize, y + cellSize);

            if ((cell.Wall & 2) != 0) // Binary: 0010 (left)
                g.DrawLine(Pens.DarkGray, x, y, x, y + cellSize);

            if ((cell.Wall & 1) != 0) // Binary: 0001 (right)
                g.DrawLine(Pens.DarkGray, x + cellSize, y, x + cellSize, y + cellSize);
        }
    }

    public class Cell
    {
        public byte Wall { get; set; }
    }

  
    }
