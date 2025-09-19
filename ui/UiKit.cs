namespace Timer.ui
{
    public static class UiKit
    {
        public static Button StyledButton(string text, int width)
        {
            var b = new Button
            {
                Text = text,
                Size = new Size(width, 44),
                Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
                FlatStyle = FlatStyle.System,
                Margin = new Padding(12, 0, 12, 0)
            };
            return b;
        }

        public static Panel SmoothPanel(Color back)
        {
            return new SmoothPanel { Dock = DockStyle.Fill, BackColor = back };
        }
    }

    // 기존 SmoothPanel을 별도 파일로 분리하거나 여기 둬도 OK
    public sealed class SmoothPanel : Panel
    {
        public SmoothPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
        protected override CreateParams CreateParams
        {
            get { var cp = base.CreateParams; cp.ExStyle |= 0x02000000; return cp; }
        }
    }
}
