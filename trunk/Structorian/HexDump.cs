using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Structorian
{
    class HexDump: ScrollableControl
    {
        private Stream _stream;
        private long _streamSize;
        private int _charHeight;
        private long _topLine;
        private long _lineCount;
        private int _visibleLines;
        private int _visibleWholeLines;
        private long _selectionStart = 0;
        private long _selectionEnd = 1;
        private long _selectionAnchor;
        private long _selectionLead;
        private int _lineSize = 16;
        private const int BUFFER_SIZE = 4096;
        private byte[] _buffer = new byte[BUFFER_SIZE];
        private long _bufferStart;

        internal class Highlighter
        {
            private long _startOffset;
            private long _endOffset;
            private readonly HexDump _hexDump;
            private readonly Color? _textColor;
            private readonly Color? _backgroundColor;

            public Highlighter(HexDump hexDump, Color? foreground, Color? background)
            {
                _startOffset = -1;
                _endOffset = -1;
                _hexDump = hexDump;
                _textColor = foreground;
                _backgroundColor = background;
            }

            internal long StartOffset { get { return _startOffset; } }
            internal long EndOffset { get { return _endOffset; } }
            internal Color? TextColor { get { return _textColor; } }
            internal Color? BackgroundColor { get { return _backgroundColor; } }

            public void SetRange(long startOffset, long endOffset)
            {
                _startOffset = startOffset;
                _endOffset = endOffset;
                _hexDump.Invalidate();
            }

            public bool Intersects(long startOffset, long endOffset)
            {
                return _endOffset > startOffset && _startOffset < endOffset;
            }
        }

        private readonly List<Highlighter> _highlighters = new List<Highlighter>();
        private readonly Highlighter _selectionHighlighter;

        public HexDump()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
            _selectionHighlighter = AddHighlighter(SystemColors.HighlightText, SystemColors.Highlight);
        }

        public Stream Stream
        {
            get { return _stream; }
            set
            {
                if (_stream != value)
                {
                    _stream = value;
                    _bufferStart = -1;
                    _streamSize = _stream != null ? _stream.Length : 0;
                    _lineCount = (_streamSize/16 + 1);
                    if (_topLine > _lineCount)
                        _topLine = _lineCount - 1;
                    AdjustScrollbars();
                    Invalidate();
                }
            }
        }

        public void SelectBytes(long startOffset, int count)
        {
            SetSelection(startOffset, count);
            _selectionAnchor = startOffset;
            _selectionLead = startOffset + count - 1;
        }
        
        public void SetSelection(long startOffset, int count)
        {
            _selectionStart = Math.Max(0, Math.Min(startOffset, _streamSize - 1));
            _selectionEnd = Math.Max(1, Math.Min(startOffset+count, _streamSize));
            _selectionHighlighter.SetRange(_selectionStart, _selectionEnd);
            ScrollInView();
            Invalidate();
            UpdateStatusText();
        }

        private void ScrollInView()
        {
            long selStartLine = _selectionStart/16;
            long selEndLine = _selectionEnd/16;
            if (selStartLine < _topLine)
                SetTopLine(selStartLine);
            else if (selEndLine >= _topLine + _visibleWholeLines)
            {
                if (selEndLine - selStartLine >= _visibleWholeLines)
                    SetTopLine(selStartLine);
                else
                    SetTopLine(selEndLine - (_visibleWholeLines-1));
            }
        }

        private void AdjustScrollbars()
        {
            VerticalScroll.Maximum = (int) _lineCount;
            VerticalScroll.LargeChange = _visibleLines;
            VerticalScroll.Visible = true;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (_charHeight > 0)
            {
                _visibleLines = Height / _charHeight;
                _visibleWholeLines = _visibleLines;
                if ((Height / _charHeight) * _charHeight < Height)
                    _visibleLines++;
                AdjustScrollbars();
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            Size s = TextRenderer.MeasureText("A", Font, new Size(100, 100), TextFormatFlags.NoPadding);
            _charHeight = s.Height;
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            long newTopLine = _topLine;
            switch(se.Type)
            {
                case ScrollEventType.SmallDecrement:
                    newTopLine = _topLine - 1;
                    break;
                    
                case ScrollEventType.SmallIncrement:
                    newTopLine = _topLine + 1;
                    break;
                    
                case ScrollEventType.LargeDecrement:
                    newTopLine = _topLine - _visibleLines + 1;
                    break;
                    
                case ScrollEventType.LargeIncrement:
                    newTopLine = _topLine + _visibleLines - 1;
                    break;
                    
                case ScrollEventType.ThumbPosition:
                    newTopLine = se.NewValue;
                    break;

                case ScrollEventType.ThumbTrack:
                    newTopLine = se.NewValue;
                    break;
            }
            SetTopLine(newTopLine);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            SetTopLine(_topLine - e.Delta/60);
        }
        
        private void SetTopLine(long newTopLine)
        {
            if (newTopLine < 0)
                newTopLine = 0;
            if (newTopLine >= _lineCount - _visibleLines)
            {
                if (_visibleLines > _lineCount)
                    newTopLine = 0;
                else
                    newTopLine = _lineCount - _visibleLines;
            }
            if (newTopLine != _topLine)
            {
                _topLine = newTopLine;
                VerticalScroll.Value = (int) _topLine;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                Focus();
                long clickOffset = GetOffsetAt(e.X, e.Y);
                if (clickOffset != -1 && (clickOffset < _selectionStart || clickOffset >= _selectionEnd))
                {
                    if ((ModifierKeys & Keys.Shift) != 0)
                    {
                        ExtendSelectionTo(clickOffset);
                        _selectionLead = clickOffset;
                    }
                    else
                    {
                        SelectBytes(clickOffset, 1);
                    }
                }
            }
        }

        private void ExtendSelectionTo(long clickOffset)
        {
            SetSelection(Math.Min(_selectionAnchor, clickOffset), (int) Math.Abs(_selectionAnchor - clickOffset) + 1);
        }

        private long GetOffsetAt(int x, int y)
        {
            int charsInLine = 10 + 16*4 + 2;
            Size size = TextRenderer.MeasureText(new string('A', charsInLine), Font);
            int col = x*charsInLine/size.Width;
            int row = y/size.Height;
            int byteCol;
            if (col >= 10 && col < 10 + 3 * 16)
                byteCol = (col - 10) / 3;
            else if (col >= 10 + 3 * 16 + 2)
                byteCol = col - (10 + 3 * 16 + 2);
            else
                return -1;

            long result = _topLine*16 + row*16 + byteCol;
            if (result >= _streamSize) return -1;
            return result;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (IsArrowKey(e.KeyData))
            {
                long newOffset = _selectionLead;
                if (e.KeyCode == Keys.Up)
                    newOffset -= _lineSize;
                else if (e.KeyCode == Keys.Down)
                    newOffset += _lineSize;
                else if (e.KeyCode == Keys.Left)
                    newOffset--;
                else if (e.KeyCode == Keys.Right)
                    newOffset++;

                if (newOffset < 0)
                    newOffset = 0;
                else if (newOffset >= _streamSize)
                    newOffset = _streamSize - 1;

                if (e.Shift)
                {
                    ExtendSelectionTo(newOffset);
                    _selectionLead = newOffset;
                }
                else
                {
                    SelectBytes(newOffset, 1);
                }
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (IsArrowKey(keyData))
                return true;
            return base.IsInputKey(keyData);
        }

        private static bool IsArrowKey(Keys keyData)
        {
            keyData = keyData & ~Keys.Shift;
            return keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Left || keyData == Keys.Right;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_stream == null) return;
            long offset = _topLine * 16;

            EnsureBuffered(offset, _visibleLines*_lineSize);

            int y = 0;
            for (int i = 0; i < _visibleLines; i++)
            {
                DrawHexLine(e.Graphics, y, offset);
                offset += _lineSize;
                y += _charHeight;
                if (offset >= _streamSize)
                    break;
            }
        }

        private void EnsureBuffered(long offset, int size)
        {
            if (_bufferStart == -1 || offset < _bufferStart || offset + size >= _bufferStart + BUFFER_SIZE)
            {
                _stream.Position = offset;
                _stream.Read(_buffer, 0, Math.Min((int) (_streamSize - offset), BUFFER_SIZE));
                _bufferStart = offset;
            }
        }

        private void DrawHexLine(Graphics g, int top, long offset)
        {
            long bytesToEnd = _streamSize - offset;
            int bytesInLine = bytesToEnd < 16 ? (int) bytesToEnd : 16;
            byte[] bytes = new byte[bytesInLine];
            Array.Copy(_buffer, offset - _bufferStart, bytes, 0, bytesInLine);

            List<LineSpan> spans = GetHighlightedSpans(offset, offset + 16);
            new LineRenderer(g, Font, top, (int) offset, bytes, spans).DrawLine();
        }

        private void UpdateStatusText()
        {
            long selectedBytes = _selectionEnd - _selectionStart;
            string text = "Selected " + selectedBytes + " bytes at offset " + _selectionStart;
            if (selectedBytes == 4)
            {
                long selectionValue = CollectSelectionValue();
                text += ". DWORD value " + selectionValue;
            }
            StatusTextChanged(this, new StatusTextEventArgs(text));
        }

        private long CollectSelectionValue()
        {
            long result = 0;
            _stream.Position = _selectionStart;
            long selectedBytes = _selectionEnd-_selectionStart;
            byte[] selection = new byte[selectedBytes];
            _stream.Read(selection, 0, (int) selectedBytes);
            int mask = 0;
            for (long offset = 0; offset < selectedBytes; offset++)
            {
                result += selection[offset] << mask;
                mask += 8;    
            }
            return result;
        }

        public Highlighter AddHighlighter(Color? textColor, Color? backgroundColor)
        {
            var result = new Highlighter(this, textColor, backgroundColor);
            _highlighters.Insert(0, result);
            return result;
        }

        private List<LineSpan> GetHighlightedSpans(long startOffset, long endOffset)
        {
            foreach (Highlighter highlighter in _highlighters)
            {
                if (highlighter.Intersects(startOffset, endOffset))
                {
                    var entireLine = new LineSpan(startOffset, endOffset, SystemColors.WindowText, SystemColors.Window, true);
                    return BreakIntoHighlightedSpans(entireLine);
                }
            }
            return null;
        }

        private List<LineSpan> BreakIntoHighlightedSpans(LineSpan lineSpan)
        {
            List<LineSpan> result = BreakIntoSpans(_highlighters[0], lineSpan);
            for (int i = 1; i < _highlighters.Count; i++)
            {
                var curHighlighter = _highlighters[i];
                var nextResult = new List<LineSpan>();
                result.ForEach(span => nextResult.AddRange(BreakIntoSpans(curHighlighter, span)));
                result = nextResult;
            }
            return result;
        }

        private static List<LineSpan> BreakIntoSpans(Highlighter highlighter, LineSpan span)
        {
            var result = new List<LineSpan>();
            if (highlighter.Intersects(span.Start, span.End))
            {
                if (highlighter.StartOffset > span.Start)
                {
                    result.Add(new LineSpan(span.Start, highlighter.StartOffset, span.TextColor, span.BackgroundColor, true));
                }
                result.Add(new LineSpan(Math.Max(span.Start, highlighter.StartOffset), Math.Min(span.End, highlighter.EndOffset),
                    highlighter.TextColor.HasValue ? highlighter.TextColor.Value : span.TextColor,
                    highlighter.BackgroundColor.HasValue ? highlighter.BackgroundColor.Value : span.BackgroundColor, false));
                if (highlighter.EndOffset < span.End)
                {
                    result.Add(new LineSpan(highlighter.EndOffset, span.End, span.TextColor, span.BackgroundColor, true));
                }
            }
            else
                result.Add(span);
            return result;
        }

        public event StatusTextEventHandler StatusTextChanged;
    }

    class LineRenderer
    {
        private Graphics _graphics;
        private Font _font;
        private int _top;
        private int _offset;
        private byte[] _data;
        private List<LineSpan> _spans;
        private StringBuilder _lineCharsBuilder;
        private int _lastX;
        private int _lastIndex;

        public LineRenderer(Graphics graphics, Font font, int top, int offset, byte[] data, List<LineSpan> spans)
        {
            _graphics = graphics;
            _font = font;
            _top = top;
            _offset = offset;
            _data = data;
            _spans = spans;
            _lineCharsBuilder = new StringBuilder(10 + 16 * 4 + 2);
        }

        public void DrawLine()
        {
            _lineCharsBuilder.Append(_offset.ToString("X8")).Append(": ");
            int bytesInLine = _data.Length;
            for (int i = 0; i < bytesInLine; i++)
            {
                string byteStr = _data[i].ToString("X2");
                _lineCharsBuilder.Append(byteStr).Append(" ");
            }
            _lineCharsBuilder.Append(' ', 2 + (16 - bytesInLine) * 3);
            string byteChars = Encoding.Default.GetString(_data).Replace('\n', (char)1).Replace('\r', (char)1);
            byteChars = byteChars.Replace('\0', (char)1);
            _lineCharsBuilder.Append(byteChars);
            _lineCharsBuilder.Append(' ', 16 - bytesInLine);

            if (_spans != null)
            {
                DrawSpan(10, SystemColors.WindowText, SystemColors.Window);
                int carry = 0;
                for (int i = 0; i < _spans.Count; i++ )
                {
                    var span = _spans[i];
                    int length = 3 * span.Length + carry;
                    if (i == _spans.Count - 1)
                        length--;
                    else if (!span.IncludeAdjacentWS)
                    {
                        length--;
                        carry = 1;
                    }
                    else
                    {
                        carry = 0;
                    }
                    DrawSpan(length, span.TextColor, span.BackgroundColor);
                }
                DrawSpan(3, SystemColors.WindowText, SystemColors.Window);
                foreach (LineSpan span in _spans)
                {
                    DrawSpan(span.Length, span.TextColor, span.BackgroundColor);
                }
            }
            else
            {
                TextRenderer.DrawText(_graphics, _lineCharsBuilder.ToString(), _font, new Point(0, _top), SystemColors.WindowText);
            }
        }

        private void DrawSpan(int chars, Color text, Color background)
        {
            Size defSize = new Size(100, 100);
            string part = _lineCharsBuilder.ToString(_lastIndex, chars);
            _lastIndex += chars;
            TextRenderer.DrawText(_graphics, part, _font, new Point(_lastX, _top), text, background);
            _lastX += TextRenderer.MeasureText(_graphics, part, _font, defSize, TextFormatFlags.NoPadding).Width;
        } 
    }

    struct LineSpan
    {
        private readonly Color _textColor;
        private readonly Color _backgroundColor;
        private readonly bool _includeAdjacentWS;

        public LineSpan(long start, long end, Color textColor, Color backgroundColor, bool includeAdjacentWS) : this()
        {
            _textColor = textColor;
            _includeAdjacentWS = includeAdjacentWS;
            _backgroundColor = backgroundColor;
            Start = start;
            End = end;
        }

        public Color TextColor { get { return _textColor; } }
        public Color BackgroundColor { get { return _backgroundColor; } }
        public bool IncludeAdjacentWS { get { return _includeAdjacentWS; } }

        public long Start { get; set; }
        public long End { get; set; }
        internal int Length { get { return (int) (End - Start); } }
    }

    class StatusTextEventArgs: EventArgs
    {
        private readonly string _text;

        public StatusTextEventArgs(string text)
        {
            _text = text;
        }

        public string Text
        {
            get { return _text; }
        }
    }

    delegate void StatusTextEventHandler(object sender, StatusTextEventArgs e);
}
