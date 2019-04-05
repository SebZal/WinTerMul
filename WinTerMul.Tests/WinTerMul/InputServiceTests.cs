using System.Threading;

using FakeItEasy;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using WinTerMul.Common;
using WinTerMul.Common.Kernel32;

namespace WinTerMul.Tests.WinTerMul
{
    internal class InputServiceTests
    {
        [TestClass]
        public class HandleInputAsync
        {
            [TestMethod]
            public void IsKeyEvent_InputDataIsWrittenToActiveTerminal()
            {
                // Arrange
                InputData passedInputData = null;
                var expectedInputRecord = new InputRecord
                {
                    Event = new InputEventRecord
                    {
                        KeyEvent = new KeyEventRecord
                        {
                            Char = new CharInfoEncoding
                            {
                                AsciiChar = 99,
                                UnicodeChar = 'c'
                            }
                        }
                    },
                    EventType = InputEventTypeFlag.KeyEvent
                };

                var inPipe = A.Fake<IPipe>();
                A.CallTo(() => inPipe.WriteAsync(A<ITransferable>._, A<bool>._, A<CancellationToken>._))
                    .Invokes(x => passedInputData = x.Arguments[0] as InputData);

                var activeTerminal = A.Fake<ITerminal>();
                A.CallTo(() => activeTerminal.In).Returns(inPipe);

                var terminalContainer = A.Fake<ITerminalContainer>();
                A.CallTo(() => terminalContainer.ActiveTerminal).Returns(activeTerminal);

                var kernel32Api = A.Fake<IKernel32Api>();
                A.CallTo(() => kernel32Api.ReadConsoleInput()).Returns(expectedInputRecord);

                var inputService = CreateInputService(terminalContainer, kernel32Api);

                // Act
                inputService.HandleInputAsync().Wait();

                // Assert
                var expectedJson = JsonConvert.SerializeObject(expectedInputRecord);
                var actualJson = JsonConvert.SerializeObject(passedInputData?.InputRecord);
                Assert.AreEqual(expectedJson, actualJson);
            }

            // TODO test if is not key event
            // TODO test when prefix key is used

            private InputService CreateInputService(
                ITerminalContainer terminalContainer,
                IKernel32Api kernel32Api)
            {
                var configuration = A.Fake<IWinTerMulConfiguration>();
                A.CallTo(() => configuration.PrefixKey).Returns("C-k");

                return new InputService(
                    terminalContainer,
                    kernel32Api,
                    configuration,
                    A.Fake<ITerminalFactory>());
            }
        }
    }
}
