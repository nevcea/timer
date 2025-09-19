namespace Timer.UI
{
    public sealed class SmoothPanel : Panel
    {
        public SmoothPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }
    }
}