using ActressMas;
using System.Threading;

namespace maze
{
    public class Program
    {
        private static void Main(string[] args)
        {
            EnvironmentMas env = new EnvironmentMas(0, 100);

            var mazeAgent = new MazeAgent();
            env.Add(mazeAgent, "maze");

            for (int i = 1; i <= 2; i++)
            {
                var explorerAgent = new ExplorerAgent();
                env.Add(explorerAgent, "explorer" + i);
            }

            Thread.Sleep(500);

            env.Start();
        }
    }
}