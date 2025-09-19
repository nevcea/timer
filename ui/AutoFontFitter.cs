namespace Timer.ui
{
    public sealed class AutoFontFitter
    {
        readonly Control _host;
        readonly Control _target;
        readonly string _sample;
        readonly int _min;
        readonly int _max;

        Size _lastHostSize = Size.Empty;
        int _lastFitFont;

        public AutoFontFitter(Control host, Control target, string sample = "00:00:00", int min = 28, int max = 220)
        {
            _host = host;
            _target = target;
            _sample = sample;
            _min = min;
            _max = max;
        }

        public void Apply()
        {
            if (_host.ClientSize.Width <= 0 || _host.ClientSize.Height <= 0) return;

            if (_lastHostSize == _host.ClientSize && _lastFitFont > 0)
            {
                CenterOnly();
                return;
            }

            int lo = _min, hi = _max, best = _min;
            while (lo <= hi)
            {
                int mid = lo + hi >> 1;
                using var baseFont = new Font(SystemFonts.DefaultFont, FontStyle.Bold);
                using var f = new Font(baseFont.FontFamily, mid, FontStyle.Bold, GraphicsUnit.Point);
                var size = TextRenderer.MeasureText(_sample, f, new Size(int.MaxValue, int.MaxValue),
                                                    TextFormatFlags.NoPadding);
                if (size.Width <= _host.ClientSize.Width - 40 && size.Height <= _host.ClientSize.Height - 40)
                {
                    best = mid; lo = mid + 1;
                }
                else hi = mid - 1;
            }

            if (best != _lastFitFont)
            {
                _target.Font = new Font(SystemFonts.DefaultFont.FontFamily, best, FontStyle.Bold, GraphicsUnit.Point);
                _lastFitFont = best;
            }

            var measured = TextRenderer.MeasureText(_sample, _target.Font, new Size(int.MaxValue, int.MaxValue),
                                                    TextFormatFlags.NoPadding);

            _target.Size = new Size(
                Math.Min(measured.Width + 10, _host.ClientSize.Width - 20),
                Math.Min(measured.Height + 6, _host.ClientSize.Height - 20)
            );

            CenterOnly();
            _lastHostSize = _host.ClientSize;
        }

        public void CenterOnly()
        {
            _target.Location = new Point(
                (_host.ClientSize.Width - _target.Width) / 2,
                (_host.ClientSize.Height - _target.Height) / 2
            );
        }
    }
}