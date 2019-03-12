using System;
using System.IO.MemoryMappedFiles;

namespace WinTerMul.Common
{
    public static class MemoryMappedFileUtility
    {
        public static MemoryMappedFile CreateMemoryMappedFile(out string mapName)
        {
            mapName = Guid.NewGuid().ToString();
            return MemoryMappedFile.CreateNew(mapName, 65536);
        }

        public static MemoryMappedFile OpenMemoryMappedFile(string mapName)
        {
            return MemoryMappedFile.OpenExisting(mapName);
        }
    }
}
