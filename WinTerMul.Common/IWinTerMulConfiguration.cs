using Microsoft.Extensions.Logging;

namespace WinTerMul.Common
{
    public interface IWinTerMulConfiguration
    {
        LogLevel LogLevel { get; }
        string LogPath { get; }
        string PrefixKey { get; }
        char SetNextTerminalActiveKey { get; }
        char SetPreviousTerminalActive { get; }
        char VerticalSplitKey { get; }
        char ClosePaneKey { get; }
    }
}
