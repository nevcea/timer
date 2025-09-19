using Timer.core;
using Timer.ui;
using Timer.utils;

namespace Timer
{
    public partial class Form1 : Form
    {
        readonly System.Windows.Forms.Timer _tick = new() { Interval = 1000 };
        readonly System.Windows.Forms.Timer _debounce = new() { Interval = 120 };

        MaskedTextBox _txtTime = new();
        Button _btnStart = new();
        Button _btnReset = new();
        Button _btnMode = new();

        SmoothPanel _displayPanel = null!;
        SmoothPanel _buttonsHost = null!;
        FlowLayoutPanel _flow = null!;

        readonly TimerEngine _engine = new();
        AutoFontFitter _fitter;

        public Form1()
        {
            Text = "Timer";
            MinimumSize = new Size(560, 360);
            BackColor = Color.FromArgb(236, 238, 241);
            DoubleBuffered = true;
            KeyPreview = true;

            BuildUI();
            WireEvents();

            _fitter = new AutoFontFitter(_displayPanel, _txtTime); // UI 구성 후 안전하게 초기화

            RenderSeconds(0);
            _fitter.Apply();
        }

        void WireEvents()
        {
            _tick.Tick += (_, __) => OnTick();
            _debounce.Tick += (_, __) => { _debounce.Stop(); _fitter.Apply(); };

            KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Space) { ToggleStart(); e.Handled = true; }
                else if (e.KeyCode == Keys.R) { DoReset(); e.Handled = true; }
                else if (e.KeyCode == Keys.Enter && !_engine.Running && _engine.Mode == TimerMode.Countdown)
                { NormalizeInput(); e.Handled = true; }
            };
        }

        void BuildUI()
        {
            SuspendLayout();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = BackColor
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 12f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 53f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 35f));
            Controls.Add(layout);

            _displayPanel = new SmoothPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(24, 0, 24, 0),
                Padding = new Padding(20),
                BackColor = Color.FromArgb(248, 248, 248),
                BorderStyle = BorderStyle.FixedSingle
            };
            layout.Controls.Add(_displayPanel, 0, 1);

            _txtTime = new MaskedTextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = _displayPanel.BackColor,
                ForeColor = Color.FromArgb(15, 18, 22),
                TextAlign = HorizontalAlignment.Center,
                Mask = "00:00:00",
                PromptChar = '0',
                InsertKeyMode = InsertKeyMode.Overwrite,
                ShortcutsEnabled = false,
                AsciiOnly = true,
                Text = "00:00:00",
                WordWrap = false,
                Multiline = false,
                TextMaskFormat = MaskFormat.IncludeLiterals,
                CutCopyMaskFormat = MaskFormat.IncludeLiterals,
                ResetOnPrompt = false,
                ResetOnSpace = false,
                HideSelection = true
            };
            _displayPanel.Controls.Add(_txtTime);
            _displayPanel.Resize += (_, __) => { _debounce.Stop(); _debounce.Start(); };

            _buttonsHost = (SmoothPanel)UiKit.SmoothPanel(BackColor);
            layout.Controls.Add(_buttonsHost, 0, 2);

            _flow = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = BackColor
            };
            _buttonsHost.Controls.Add(_flow);
            _buttonsHost.Resize += (_, __) =>
            {
                _flow.Location = new Point(
                    (_buttonsHost.ClientSize.Width - _flow.Width) / 2,
                    (_buttonsHost.ClientSize.Height - _flow.Height) / 2
                );
            };

            _btnStart = UiKit.StyledButton("Start", 140);
            _btnReset = UiKit.StyledButton("Reset", 140);
            _btnMode = UiKit.StyledButton("Mode: Countdown", 200);

            _flow.Controls.AddRange(new Control[] { _btnStart, _btnReset, _btnMode });

            _btnStart.Click += (_, __) => ToggleStart();
            _btnReset.Click += (_, __) => DoReset();
            _btnMode.Click += (_, __) => ToggleMode();

            _txtTime.Enter += (_, __) => { if (IsLocked) _btnStart.Focus(); };
            _txtTime.MouseDown += (_, __) => { if (IsLocked) _btnStart.Focus(); };
            _txtTime.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter) { NormalizeInput(); e.SuppressKeyPress = true; }
            };
            _txtTime.Leave += (_, __) => NormalizeInput();

            ResumeLayout(true);
        }

        bool IsLocked => _engine.Running || _engine.Mode == TimerMode.Stopwatch;

        void UpdateEditability()
        {
            bool editable = !IsLocked;
            _txtTime.ReadOnly = !editable;
            _txtTime.TabStop = editable;
            _txtTime.ShortcutsEnabled = editable;
            _txtTime.Cursor = editable ? Cursors.IBeam : Cursors.Arrow;
            if (!editable && _txtTime.Focused) _btnStart.Focus();
        }

        void OnTick()
        {
            var (beep, stopped) = _engine.Tick(DateTime.UtcNow);
            RenderSeconds(_engine.ShownSeconds);
            if (beep) System.Media.SystemSounds.Beep.Play();
            if (stopped) { _btnStart.Text = "Start"; UpdateEditability(); }
        }

        void ToggleStart()
        {
            if (!_engine.Running)
            {
                if (_engine.Mode == TimerMode.Countdown)
                {
                    string current = ReadInputText();
                    if (!TimeText.TryParseFlexible(current, out int secs) || secs <= 0)
                    {
                        MessageBox.Show("형식이 올바르지 않습니다. 예) 00:10:00, 1:30, 90s, 1h30m, 1분 20초");
                        _txtTime.Focus();
                        _txtTime.SelectAll();
                        return;
                    }
                    _engine.StartCountdown(secs);
                    RenderSeconds(secs);
                }
                else
                {
                    string current = ReadInputText();
                    if (!TimeText.TryParseFlexible(current, out int s)) s = 0;
                    _engine.StartStopwatch(s);
                    RenderSeconds(s);
                }

                _tick.Start();
                _btnStart.Text = "Pause";
                UpdateEditability();
            }
            else
            {
                _engine.Pause();
                _tick.Stop();
                _btnStart.Text = "Start";
                RenderSeconds(_engine.ShownSeconds);
                UpdateEditability();
            }
        }

        void DoReset()
        {
            string raw = ReadInputText();
            int parsed = 0; bool has = TimeText.TryParseFlexible(raw, out parsed);

            if (_engine.ShownSeconds > 0 || (has && parsed > 0))
            {
                var ans = MessageBox.Show("현재 시간을 00:00:00으로 초기화할까요?",
                    "Reset 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (ans != DialogResult.Yes) return;
            }

            _tick.Stop();
            _engine.Reset();
            _btnStart.Text = "Start";
            RenderSeconds(0);
            UpdateEditability();
        }

        void ToggleMode()
        {
            if (_engine.Running)
            {
                MessageBox.Show("실행 중에는 모드를 바꿀 수 없습니다. 먼저 정지하세요.");
                return;
            }

            var next = (_engine.Mode == TimerMode.Countdown) ? TimerMode.Stopwatch : TimerMode.Countdown;
            _engine.SetMode(next);
            _btnMode.Text = (next == TimerMode.Countdown) ? "Mode: Countdown" : "Mode: Stopwatch";

            _tick.Stop();
            _btnStart.Text = "Start";
            RenderSeconds(0);
            UpdateEditability();
        }

        void NormalizeInput()
        {
            string raw = ReadInputText();
            if (TimeText.TryParseFlexible(raw, out int s))
                RenderSeconds(s);
        }

        string ReadInputText() => _txtTime.ReadMaskedText();

        void RenderSeconds(int totalSecs)
        {
            string s = TimeText.FormatHms(totalSecs < 0 ? 0 : totalSecs);
            _txtTime.SetMaskedText(s);
        }
    }
}