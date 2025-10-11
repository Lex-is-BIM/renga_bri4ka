using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.Configs
{
    public class ShadowCalcParametersConfig : ConfigIO
    {
        public string Name { get; set; } = "Средние параметры для г. Москва";
        public double Latitude { get; set; } = 55.6984;
        public double Longitude { get; set; } = 37.6055;
        public int TimeZoneOffset { get; set; } = 0;
        public DateTime Date { get; set; } = DateTime.Now;
        public double GroundElevation { get; set; } = 0;
    }
}
