using System;

namespace WinTerMul.Terminal
{
    internal class InputArguments
    {
        public InputArguments(string[] args)
        {
            if (args?.Length != 3)
            {
                var error = "Must provide identifiers for output pipe, input pipe, and parent process.";
                throw new ArgumentException(error);
            }

            if (!int.TryParse(args[2], out var parentProcessId))
            {
                throw new ArgumentException("Invalid parent process identifier.");
            }

            OutputPipeId = args[0];
            InputPipeId = args[1];
            ParentProcessId = parentProcessId;
        }

        public string OutputPipeId { get; }
        public string InputPipeId { get; }
        public int ParentProcessId { get; }
    }
}
