using TMPro;

namespace Assets.Scripts.UI.Validators
{
    internal class UShortInputValidator : TMP_InputValidator
    {
        public override char Validate(ref string text, ref int pos, char ch)
        {
            if (ushort.TryParse(ch.ToString(), out var num))
            {
                return ch;
            }

            return '0';
        }
    }
}
