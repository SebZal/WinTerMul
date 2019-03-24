using System.Threading;

using WinTerMul.Common.Kernel32;

namespace WinTerMul
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var inputHandle = PInvoke.Kernel32.GetStdHandle(PInvoke.Kernel32.StdHandle.STD_INPUT_HANDLE);
            var outputHandle = PInvoke.Kernel32.GetStdHandle(PInvoke.Kernel32.StdHandle.STD_OUTPUT_HANDLE);

            // TODO Use IoC container and make the container dispose these classes
            var kernel32Api = new Kernel32Api();
            var terminalContainer = new TerminalContainer(Terminal.Create());
            var resizeHandler = new ResizeHandler(terminalContainer, outputHandle);
            var inputHandler = new InputHandler(terminalContainer, kernel32Api);

            new Renderer(terminalContainer, kernel32Api).StartRendererThread();

            while (true)
            {
                Thread.Sleep(10);

                var terminals = terminalContainer.GetTerminals();
                if (terminals.Count == 0)
                {
                    break;
                }

                resizeHandler.HandleResize();
                inputHandler.HandleInput();
            }
        }
    }
}
