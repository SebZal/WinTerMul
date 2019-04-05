using System;
using System.Threading;
using System.Threading.Tasks;

namespace WinTerMul.Common
{
    public interface IPipe : IDisposable
    {
        string Id { get; }

        Task<bool> WriteAsync(
            ITransferable @object,
            bool writeOnlyIfDataHasChanged = false,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<ITransferable> ReadAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
