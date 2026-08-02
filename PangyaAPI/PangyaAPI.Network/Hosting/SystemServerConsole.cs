using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PangyaAPI.Network.Resources;

namespace PangyaAPI.Network.Hosting
{
    public sealed class SystemServerConsole : IServerConsole
    {
        private const int InputPollMilliseconds = 25;
        private readonly object _sync = new object();
        private readonly TextReader _input;
        private readonly TextWriter _rawOutput;
        private readonly bool _interactive;
        private IReadOnlyList<string> _activeFrame = Array.Empty<string>();
        private readonly StringBuilder _inputBuffer = new StringBuilder();
        private int _caretIndex;
        private int _inputWindowStart;
        private int _frameStartTop;
        private int _frameRows;
        private bool _frameVisible;

        public SystemServerConsole()
            : this(Console.In, Console.Out, !Console.IsInputRedirected && !Console.IsOutputRedirected, true)
        {
        }

        internal SystemServerConsole(TextReader input, TextWriter output, bool interactive, bool installOutput)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _rawOutput = output ?? throw new ArgumentNullException(nameof(output));
            _interactive = interactive;
            OutputWriter = new CoordinatedConsoleWriter(this);

            if (installOutput)
                Console.SetOut(OutputWriter);
        }

        public TextWriter OutputWriter { get; }

        public async Task<string> ReadLineAsync(
            IReadOnlyList<string> frameLines,
            CancellationToken cancellationToken)
        {
            if (frameLines == null)
                throw new ArgumentNullException(nameof(frameLines));

            if (!_interactive)
            {
                foreach (var line in frameLines)
                    OutputWriter.WriteLine(line);
                OutputWriter.Flush();
                return await _input.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            }

            lock (_sync)
            {
                EraseFrameLocked();
                _activeFrame = frameLines;
                _inputBuffer.Clear();
                _caretIndex = 0;
                _inputWindowStart = 0;
                RenderFrameLocked();
            }

            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!Console.KeyAvailable)
                    {
                        await Task.Delay(InputPollMilliseconds, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    var key = Console.ReadKey(intercept: true);
                    lock (_sync)
                    {
                        if (key.Key == ConsoleKey.Enter)
                        {
                            var result = _inputBuffer.ToString();
                            EraseFrameLocked();
                            ResetFrameLocked();
                            return result;
                        }

                        if (key.Key == ConsoleKey.Z && key.Modifiers.HasFlag(ConsoleModifiers.Control))
                        {
                            EraseFrameLocked();
                            ResetFrameLocked();
                            return null;
                        }

                        if (ApplyEditingKeyLocked(key))
                            RenderInputLocked();
                    }
                }
            }
            catch
            {
                lock (_sync)
                {
                    EraseFrameLocked();
                    ResetFrameLocked();
                }
                throw;
            }
        }

        public void WriteLine(string value = "")
        {
            OutputWriter.WriteLine(value ?? string.Empty);
            OutputWriter.Flush();
        }

        public void Clear()
        {
            if (!_interactive)
                throw new IOException(NetworkMessages.Get("RedirectedConsoleCannotClear"));

            lock (_sync)
            {
                Console.Clear();
                _frameVisible = false;
                if (_activeFrame.Count > 0)
                    RenderFrameLocked();
            }
        }

        private bool ApplyEditingKeyLocked(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Backspace:
                    if (_caretIndex == 0)
                        return false;
                    _inputBuffer.Remove(--_caretIndex, 1);
                    return true;

                case ConsoleKey.Delete:
                    if (_caretIndex >= _inputBuffer.Length)
                        return false;
                    _inputBuffer.Remove(_caretIndex, 1);
                    return true;

                case ConsoleKey.LeftArrow:
                    if (_caretIndex == 0)
                        return false;
                    _caretIndex--;
                    return true;

                case ConsoleKey.RightArrow:
                    if (_caretIndex >= _inputBuffer.Length)
                        return false;
                    _caretIndex++;
                    return true;

                case ConsoleKey.Home:
                    if (_caretIndex == 0)
                        return false;
                    _caretIndex = 0;
                    return true;

                case ConsoleKey.End:
                    if (_caretIndex == _inputBuffer.Length)
                        return false;
                    _caretIndex = _inputBuffer.Length;
                    return true;
            }

            if (char.IsControl(key.KeyChar))
                return false;

            _inputBuffer.Insert(_caretIndex++, key.KeyChar);
            return true;
        }

        private void WriteCompletedLine(IReadOnlyList<ColoredSegment> segments)
        {
            lock (_sync)
            {
                var previousColor = GetForegroundColor();
                EraseFrameLocked();

                foreach (var segment in segments)
                {
                    SetForegroundColor(segment.Color);
                    _rawOutput.Write(segment.Text);
                }

                _rawOutput.WriteLine();
                _rawOutput.Flush();
                SetForegroundColor(previousColor);

                if (_activeFrame.Count > 0)
                    RenderFrameLocked();
            }
        }

        private void RenderFrameLocked()
        {
            if (!_interactive || _activeFrame.Count == 0)
                return;

            var width = GetUsableWidth();
            foreach (var line in _activeFrame)
                _rawOutput.WriteLine(Clip(line ?? string.Empty, width));

            _frameRows = _activeFrame.Count + 1;
            _frameStartTop = Math.Max(0, Console.CursorTop - _activeFrame.Count);
            _frameVisible = true;
            RenderInputLocked();
            _rawOutput.Flush();
        }

        private void RenderInputLocked()
        {
            if (!_frameVisible)
                return;

            var width = GetUsableWidth();
            if (_caretIndex < _inputWindowStart)
                _inputWindowStart = _caretIndex;
            if (_caretIndex >= _inputWindowStart + width)
                _inputWindowStart = _caretIndex - width + 1;
            if (_inputBuffer.Length < _inputWindowStart)
                _inputWindowStart = _inputBuffer.Length;

            var available = Math.Min(width, _inputBuffer.Length - _inputWindowStart);
            var visibleInput = available > 0
                ? _inputBuffer.ToString(_inputWindowStart, available)
                : string.Empty;

            var inputRow = Math.Min(Console.BufferHeight - 1, _frameStartTop + _frameRows - 1);
            Console.SetCursorPosition(0, inputRow);
            _rawOutput.Write(new string(' ', width));
            Console.SetCursorPosition(0, inputRow);
            _rawOutput.Write(visibleInput);
            Console.SetCursorPosition(Math.Min(width - 1, _caretIndex - _inputWindowStart), inputRow);
            _rawOutput.Flush();
        }

        private void EraseFrameLocked()
        {
            if (!_interactive || !_frameVisible)
                return;

            var width = GetUsableWidth();
            for (var row = 0; row < _frameRows; row++)
            {
                var targetRow = _frameStartTop + row;
                if (targetRow < 0 || targetRow >= Console.BufferHeight)
                    continue;
                Console.SetCursorPosition(0, targetRow);
                _rawOutput.Write(new string(' ', width));
            }

            Console.SetCursorPosition(0, Math.Min(_frameStartTop, Console.BufferHeight - 1));
            _rawOutput.Flush();
            _frameVisible = false;
        }

        private void ResetFrameLocked()
        {
            _activeFrame = Array.Empty<string>();
            _inputBuffer.Clear();
            _caretIndex = 0;
            _inputWindowStart = 0;
            _frameRows = 0;
            _frameStartTop = 0;
        }

        private static int GetUsableWidth()
        {
            try
            {
                return Math.Max(1, Console.BufferWidth - 1);
            }
            catch
            {
                return 79;
            }
        }

        private static string Clip(string value, int width)
        {
            return value.Length <= width ? value : value.Substring(0, width);
        }

        private static ConsoleColor GetForegroundColor()
        {
            try
            {
                return Console.ForegroundColor;
            }
            catch
            {
                return ConsoleColor.Gray;
            }
        }

        private static void SetForegroundColor(ConsoleColor color)
        {
            try
            {
                Console.ForegroundColor = color;
            }
            catch
            {
                // Color is optional when the host does not expose a full console.
            }
        }

        private readonly struct ColoredSegment
        {
            public ColoredSegment(string text, ConsoleColor color)
            {
                Text = text;
                Color = color;
            }

            public string Text { get; }
            public ConsoleColor Color { get; }
        }

        private sealed class CoordinatedConsoleWriter : TextWriter
        {
            private readonly SystemServerConsole _owner;
            private readonly ThreadLocal<LineBuffer> _buffers = new ThreadLocal<LineBuffer>(() => new LineBuffer());

            public CoordinatedConsoleWriter(SystemServerConsole owner)
            {
                _owner = owner;
            }

            public override Encoding Encoding => _owner._rawOutput.Encoding;

            public override void Write(char value)
            {
                Append(value.ToString());
            }

            public override void Write(string value)
            {
                Append(value);
            }

            public override void WriteLine()
            {
                CompleteLine();
            }

            public override void WriteLine(string value)
            {
                Append(value);
                CompleteLine();
            }

            public override void Flush()
            {
                _owner._rawOutput.Flush();
            }

            private void Append(string value)
            {
                if (string.IsNullOrEmpty(value))
                    return;

                var start = 0;
                for (var index = 0; index < value.Length; index++)
                {
                    if (value[index] != '\r' && value[index] != '\n')
                        continue;

                    if (index > start)
                        AddSegment(value.Substring(start, index - start));

                    if (value[index] == '\n')
                        CompleteLine();

                    start = index + 1;
                }

                if (start < value.Length)
                    AddSegment(value.Substring(start));
            }

            private void AddSegment(string value)
            {
                var buffer = _buffers.Value;
                var color = GetForegroundColor();
                if (buffer.Segments.Count > 0 && buffer.Segments[buffer.Segments.Count - 1].Color == color)
                {
                    var previous = buffer.Segments[buffer.Segments.Count - 1];
                    buffer.Segments[buffer.Segments.Count - 1] = new ColoredSegment(previous.Text + value, color);
                }
                else
                {
                    buffer.Segments.Add(new ColoredSegment(value, color));
                }
            }

            private void CompleteLine()
            {
                var buffer = _buffers.Value;
                _owner.WriteCompletedLine(buffer.Segments.ToArray());
                buffer.Segments.Clear();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    _buffers.Dispose();
                base.Dispose(disposing);
            }

            private sealed class LineBuffer
            {
                public List<ColoredSegment> Segments { get; } = new List<ColoredSegment>();
            }
        }
    }
}
