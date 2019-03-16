using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WinTerMul.Common
{
    public static class Serializers
    {
        public static readonly ReadOnlyCollection<ISerializer> All = new ReadOnlyCollection<ISerializer>(
            new List<ISerializer>(new[]
            {
                new ConsoleOutputSerializer()
            }));
    }
}
