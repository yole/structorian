using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Structorian
{
    class HexDump: ScrollableControl
    {
        private Stream _stream;
        private int _streamSize;
        private int _charHeight;
        private int _topLine;
        private int _lineCount;
        private int _visibleLines;
        private int _visibleWholeLines;
        private int _selectionStart = 0;
        private int _selectionEnd = 1;

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
                    _streamSize = (int) _stream.Length;
                    _lineCount = (_streamSize/16 + 1);
                    AdjustScrollbars();
                    Invalidate();
                }
            }
        }
        
        public void SelectBytes(int startOffset, int count)
        {
            _selectionStart = Math.Max(0, Math.Min(startOffset, _streamSize - 1));
            _selectionEnd = Math.Max(1, Math.Min(startOffset+count, _streamSize));
            ScrollInView();
            Invalidate();
        }

        private void ScrollInView()
        {
            int selStartLine = _selectionStart/16;
            int selEndLine = _selectionEnd/16;
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
            VerticalScroll.Maximum = _lineCount;
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
            int newTopLine = _topLine;
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
        
        private void SetTopLine(int newTopLine)
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
                VerticalScroll.Value = _topLine;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                int clickOffset = GetOffsetAt(e.X, e.Y);
                if (clickOffset != -1 && (clickOffset < _selectionStart || clickOffset >= _selectionEnd))
                {
                    SelectBytes(clickOffset, 1);
                }
            }
        }

        private int GetOffsetAt(int x, int y)
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

            int result = _topLine*16 + row*16 + byteCol;
            if (result >= _streamSize) return -1;
            return result;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_stream == null) return;
            int offset = _topLine * 16;
            int y = 0;
            for (int i = 0; i < _visibleLines; i++)
            {
                DrawHexLine(e.Graphics, y, offset);
                offset += 16;
                y += _charHeight;
                if (offset >= _streamSize)
                    break;
            }
        }

        private void DrawHexLine(Graphics g, int top, int offset)
        {
            StringBuilder lineCharsBuilder = new StringBuilder(10 + 16 * 4 + 2);
            lineCharsBuilder.Append(offset.ToString("X8")).Append(": ");

            int bytesInLine = Math.Min(16, _streamSize - offset);
            _stream.Position = offset;
            byte[] bytes = new byte[bytesInLine];
            _stream.Read(bytes, 0, bytesInLine);
            for (int i = 0; i < bytesInLine; i++)
            {
                string byteStr = bytes[i].ToString("X2");
                lineCharsBuilder.Append(byteStr).Append(" ");
            }
            lineCharsBuilder.Append(' ', 2 + (16 - bytesInLine)*3);
            string byteChars = Encoding.Default.GetString(bytes).Replace('\n', (char) 1).Replace('\r', (char) 1);
            byteChars = byteChars.Replace('\0', (char) 1);
            lineCharsBuilder.Append(byteChars);

            int selectionStartInLine = (_selectionStart < offset) ? 0 : _selectionStart - offset;
            int selectionEndInLine = (_selectionEnd >= offset + 16) ? 16 : _selectionEnd - offset;
            if (selectionStartInLine < 16 && selectionEndInLine > 0)
            {
                Size defSize = new Size(100, 100);
                int x = 0;
                int bound1 = 10 + 3*selectionStartInLine;
                int bound2 = 10 + 3*selectionEndInLine - 1;
                int bound3 = 10 + 3*16 + 2 + selectionStartInLine;
                int bound4 = 10 + 3*16 + 2 + selectionEndInLine;
                string part1 = lineCharsBuilder.ToString(0, bound1);
                string part2 = lineCharsBuilder.ToString(bound1, bound2 - bound1);
                string part3 = lineCharsBuilder.ToString(bound2, bound3 - bound2);
                string part4 = lineCharsBuilder.ToString(bound3, bound4 - bound3);
                string part5 = lineCharsBuilder.ToString(bound4, lineCharsBuilder.Length - bound4);
                TextRenderer.DrawText(g, part1, Font, new Point(0, top), SystemColors.WindowText);
                x += TextRenderer.MeasureText(g, part1, Font, defSize, TextFormatFlags.NoPadding).Width;
                TextRenderer.DrawText(g, part2, Font, new Point(x, top),
                                      SystemColors.HighlightText, SystemColors.Highlight);
                x += TextRenderer.MeasureText(g, part2, Font, defSize, TextFormatFlags.NoPadding).Width;
                TextRenderer.DrawText(g, part3, Font, new Point(x, top), SystemColors.WindowText);
                x += TextRenderer.MeasureText(g, part3, Font, defSize, TextFormatFlags.NoPadding).Width;
                TextRenderer.DrawText(g, part4, Font, new Point(x, top),
                                      SystemColors.HighlightText, SystemColors.Highlight);
                x += TextRenderer.MeasureText(g, part4, Font, defSize, TextFormatFlags.NoPadding).Width;
                TextRenderer.DrawText(g, part5, Font, new Point(x, top), SystemColors.WindowText);
            }
            else
            {
                TextRenderer.DrawText(g, lineCharsBuilder.ToString(), Font, new Point(0, top), SystemColors.WindowText);
            }
        }
    }
}
