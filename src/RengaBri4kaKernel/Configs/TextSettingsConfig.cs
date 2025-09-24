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
        public int FontCapSize { get; set; } = 12;
        [XmlIgnore]
        public System.Windows.Media.Color FontColor { get; set; } = System.Windows.Media.Color.FromArgb(255, 0, 0, 0);

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
            byte a = byte.Parse(new string(hex[3], 2), NumberStyles.HexNumber);
            byte r = byte.Parse(new string(hex[0], 2), NumberStyles.HexNumber);
            byte g = byte.Parse(new string(hex[1], 2), NumberStyles.HexNumber);
            byte b = byte.Parse(new string(hex[2], 2), NumberStyles.HexNumber);

            return System.Windows.Media.Color.FromArgb(a, r, g, b);
        }

        public static string ToHex(System.Windows.Media.Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
        }

        public string FontName { get; set; } = "Arial";
        public bool IsBold { get; set; } = false;
        public bool IsItalic { get; set; } = false;
        public bool IsUnderline { get; set; } = false;
    }
}
