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

        public HexDump()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
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

        private List<LineSpan> GetHighlightedSpans(long startOffset, long endOffset)
        {
            int selectionStartInLine = (int)((_selectionStart < startOffset) ? 0 : _selectionStart - startOffset);
            int lineLength = (int) (endOffset - startOffset);
            int selectionEndInLine = (int)((_selectionEnd >= endOffset) ? lineLength : _selectionEnd - startOffset);
            if (selectionStartInLine < 16 && selectionEndInLine > 0 && selectionStartInLine != selectionEndInLine)
            {
                var result = new List<LineSpan>();
                if (selectionStartInLine > 0)
                    result.Add(new LineSpan(0, selectionStartInLine, SystemColors.WindowText, SystemColors.Window));
                result.Add(new LineSpan(selectionStartInLine, selectionEndInLine, SystemColors.HighlightText, SystemColors.Highlight));
                if (selectionEndInLine < lineLength)
                    result.Add(new LineSpan(selectionEndInLine, lineLength, SystemColors.WindowText, SystemColors.Window));
                return result;
            }
            return null;
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
                foreach (LineSpan span in _spans)
                {
                    int length = 3*(span.End - span.Start) + carry;
                    if (span.BackgroundColor != SystemColors.Window)
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
                DrawSpan(2, SystemColors.WindowText, SystemColors.Window);
                foreach (LineSpan span in _spans)
                {
                    DrawSpan(span.End - span.Start, span.TextColor, span.BackgroundColor);
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

        public LineSpan(int start, int end, Color textColor, Color backgroundColor) : this()
        {
            _textColor = textColor;
            _backgroundColor = backgroundColor;
            Start = start;
            End = end;
        }

        public LineSpan(Color textColor, Color backgroundColor): this()
        {
            _textColor = textColor;
            _backgroundColor = backgroundColor;
        }

        public Color TextColor
        {
            get { return _textColor; }
        }

        public Color BackgroundColor
        {
            get { return _backgroundColor; }
        }

        public int Start { get; set; }
        public int End { get; set; }
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
