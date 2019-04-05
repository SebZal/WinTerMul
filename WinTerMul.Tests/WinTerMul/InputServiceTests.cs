using System;
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
            private const string PrefixKeyString = "C-k";
            private const byte PrefixKey = 11;
            private const char ClosePaneKey = 'x';

            [TestMethod]
            public void IsKeyEvent_InputDataIsWrittenToActiveTerminal()
            {
                // Arrange
                var inputRecord = new InputRecord();
                inputRecord.Event.KeyEvent.Char.AsciiChar = 99;
                inputRecord.Event.KeyEvent.Char.UnicodeChar = 'c';
                inputRecord.EventType = InputEventTypeFlag.KeyEvent;

                InputRecord? writtenInputRecord = null;
                var inputService = CreateInputService<InputData>(
                    inputRecord,
                    x => writtenInputRecord = x?.InputRecord);

                // Act
                inputService.HandleInputAsync().Wait();

                // Assert
                var expectedJson = JsonConvert.SerializeObject(inputRecord);
                var actualJson = JsonConvert.SerializeObject(writtenInputRecord);
                Assert.AreEqual(expectedJson, actualJson);
            }

            [TestMethod]
            public void IsNotKeyEvent_NoInputDataWritten()
            {
                // Arrange
                var inputRecord = new InputRecord
                {
                    EventType = InputEventTypeFlag.FocusEvent
                };

                InputRecord? writtenInputRecord = null;
                var inputService = CreateInputService<InputData>(
                    inputRecord,
                    x => writtenInputRecord = x?.InputRecord);

                // Act
                inputService.HandleInputAsync().Wait();

                // Assert
                Assert.IsNull(writtenInputRecord);
            }

            [TestMethod]
            public void PrefixKeyThenClosePaneKey_CloseCommandIsSent()
            {
                // Arrange
                var prefixKey = new InputRecord();
                prefixKey.Event.KeyEvent.Char.UnicodeChar = (char)PrefixKey;
                prefixKey.Event.KeyEvent.Char.AsciiChar = PrefixKey;
                prefixKey.EventType = InputEventTypeFlag.KeyEvent;

                var closePaneKey = new InputRecord();
                closePaneKey.Event.KeyEvent.Char.UnicodeChar = ClosePaneKey;
                closePaneKey.Event.KeyEvent.Char.AsciiChar = (byte)ClosePaneKey;
                closePaneKey.EventType = InputEventTypeFlag.KeyEvent;

                CloseCommand closeCommand = null;
                var inputService = CreateInputService<CloseCommand>(
                    prefixKey,
                    closePaneKey,
                    x => closeCommand = x);

                // Act
                inputService.HandleInputAsync().Wait();
                inputService.HandleInputAsync().Wait();

                // Assert
                Assert.IsNotNull(closeCommand);
            }

            private InputService CreateInputService<T>(InputRecord @in, Action<T> @out)
                where T : class
            {
                return CreateInputService(@in, default(InputRecord), @out);
            }

            private InputService CreateInputService<T>(InputRecord in1, InputRecord in2, Action<T> @out)
                where T : class
            {
                var inPipe = A.Fake<IPipe>();
                A.CallTo(() => inPipe.WriteAsync(A<ITransferable>._, A<bool>._, A<CancellationToken>._))
                    .Invokes(x => @out(x.Arguments[0] as T));

                var activeTerminal = A.Fake<ITerminal>();
                A.CallTo(() => activeTerminal.In).Returns(inPipe);

                var terminalContainer = A.Fake<ITerminalContainer>();
                A.CallTo(() => terminalContainer.ActiveTerminal).Returns(activeTerminal);

                var kernel32Api = A.Fake<IKernel32Api>();
                var counter = 0;
                A.CallTo(() => kernel32Api.ReadConsoleInput()).ReturnsLazily(x => counter++ == 0 ? in1 : in2);

                var configuration = A.Fake<IWinTerMulConfiguration>();
                A.CallTo(() => configuration.PrefixKey).Returns(PrefixKeyString);
                A.CallTo(() => configuration.ClosePaneKey).Returns(ClosePaneKey);

                return new InputService(
                    terminalContainer,
                    kernel32Api,
                    configuration,
                    A.Fake<ITerminalFactory>());
            }
        }
    }
}
