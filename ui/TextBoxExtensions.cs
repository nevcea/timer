namespace Timer.ui
{
    public static class TextBoxExtensions
    {
        public static string ReadMaskedText(this MaskedTextBox tb)
            => tb.MaskedTextProvider?.ToDisplayString() ?? tb.Text;

        public static void SetMaskedText(this MaskedTextBox tb, string text)
        {
            var prev = tb.TextMaskFormat;
            tb.TextMaskFormat = MaskFormat.IncludeLiterals;
            if (tb.Text != text) tb.Text = text;
            tb.TextMaskFormat = prev;
        }
    }
}