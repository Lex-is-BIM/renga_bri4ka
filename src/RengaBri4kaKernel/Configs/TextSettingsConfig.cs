using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RengaBri4kaKernel.Configs
{
    public class TextSettingsConfig : ConfigIO
    {
        public int FontCapSize { get; set; } = 2;
        [XmlIgnore]
        public System.Windows.Media.Color FontColor { get; set; } = System.Windows.Media.Color.FromArgb(255, 0, 0, 0);

        public Renga.Color ToRengaFontColor()
        {
            return new Renga.Color() { Red = FontColor.R, Green = FontColor.G, Blue = FontColor.B, Alpha = FontColor.A };
        }

        [XmlElement("FontColor")]
        public string FontColorHex
        {
            get
            {
                return ToHex(FontColor);
            }
            set
            {
                FontColor = FromHex(value);
            }
        }
        public static System.Windows.Media.Color FromHex(string hex)
        {
            hex = hex.Replace("#", "");
            if (hex.Length == 8) // RRGGBBAA format
            {
                byte a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
                byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                return System.Windows.Media.Color.FromArgb(a, r, g, b);
            }
            else if (hex.Length == 6) // RRGGBB format (assume fully opaque)
            {
                byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                return System.Windows.Media.Color.FromArgb(255, r, g, b);
            }
            else if (hex.Length == 4) // RGBA format (each character doubled)
            {
                byte a = byte.Parse(new string(hex[3], 2), NumberStyles.HexNumber);
                byte r = byte.Parse(new string(hex[0], 2), NumberStyles.HexNumber);
                byte g = byte.Parse(new string(hex[1], 2), NumberStyles.HexNumber);
                byte b = byte.Parse(new string(hex[2], 2), NumberStyles.HexNumber);
                return System.Windows.Media.Color.FromArgb(a, r, g, b);
            }
            else if (hex.Length == 3) // RGB format (each character doubled)
            {
                byte r = byte.Parse(new string(hex[0], 2), NumberStyles.HexNumber);
                byte g = byte.Parse(new string(hex[1], 2), NumberStyles.HexNumber);
                byte b = byte.Parse(new string(hex[2], 2), NumberStyles.HexNumber);
                return System.Windows.Media.Color.FromArgb(255, r, g, b);
            }
            else return System.Windows.Media.Color.FromArgb(255, 0, 0, 0);
        }

        public static string ToHex(System.Windows.Media.Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
        }

        public string FontName { get; set; } = "Arial";
        public bool IsBold { get; set; } = false;
        public bool IsItalic { get; set; } = false;
        public bool IsUnderline { get; set; } = false;

        public Renga.FontStyle GetFontStyle()
        {
            sbyte b = 0;
            sbyte i = 0;
            sbyte u = 0;
            if (IsBold) b = 1;
            if (IsItalic) i = 1;
            if (IsUnderline) u = 1;
            return new Renga.FontStyle() { Bold = b, Italic = i, Underline = u };
        }

        public static TextSettingsConfig Default() => new TextSettingsConfig();
    }
}
