using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FakeItEasy;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using WinTerMul.Common;
using WinTerMul.Common.Kernel32;

namespace WinTerMul.Tests.WinTerMul
{
    internal class OutputServiceTests
    {
        [TestClass]
        public class HandleOutputAsync
        {
            [TestMethod]
            public void OutputDataIsReceived_ConsoleOutputIsWrittenToBuffer()
            {
                // Arrange
                var expectedData = new[]
                {
                    new CharInfo { Char = new CharInfoEncoding { UnicodeChar = 'a' } },
                    new CharInfo { Char = new CharInfoEncoding { UnicodeChar = 'b' } },
                    new CharInfo { Char = new CharInfoEncoding { UnicodeChar = 'c' } },
                    new CharInfo { Char = new CharInfoEncoding { UnicodeChar = 'd' } }
                };

                CharInfo[] writtenOutput = null;
                var outputService = CreateOutputService(expectedData, x => writtenOutput = x);

                // Act
                outputService.HandleOutputAsync().Wait();

                // Assert
                Assert.IsNotNull(writtenOutput);
                Assert.AreEqual(expectedData.Length, writtenOutput.Length);

                foreach (var expected in expectedData)
                {
                    Assert.IsTrue(writtenOutput.Any(x => x.Char.UnicodeChar == expected.Char.UnicodeChar));
                }
            }

            private OutputService CreateOutputService(CharInfo[] outputData, Action<CharInfo[]> writtenOutput)
            {
                var pipe = A.Fake<IPipe>();
                A.CallTo(() => pipe.ReadAsync(A<CancellationToken>._))
                    .Returns(Task.FromResult<ITransferable>(new OutputData { Buffer = outputData }));

                var terminal = A.Fake<ITerminal>();
                A.CallTo(() => terminal.Out).Returns(pipe);

                var terminalContainer = A.Fake<ITerminalContainer>();
                A.CallTo(() => terminalContainer.GetTerminals())
                    .Returns(new List<ITerminal>(new[] { terminal }));

                var kernel32Api = A.Fake<IKernel32Api>();
                A.CallTo(() => kernel32Api.WriteConsoleOutput(
                    A<CharInfo[]>._,
                    A<Coord>._,
                    A<Coord>._,
                    A<SmallRect>._))
                    .Invokes(x => writtenOutput(x.GetArgument<CharInfo[]>(0)));

                return new OutputService(
                    terminalContainer,
                    kernel32Api,
                    A.Fake<ILogger>());
            }
        }
    }
}
