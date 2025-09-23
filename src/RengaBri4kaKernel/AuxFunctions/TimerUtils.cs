using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.AuxFunctions
{
    internal class TimerUtils
    {
        public TimerUtils()
        {
            mTimer = new Stopwatch();
        }

        public static TimerUtils CreateInstance()
        {
            if (mInstance == null) mInstance = new TimerUtils();
            return mInstance;
        }

        public void Start()
        {
            mTimer.Reset();
            mTimer.Start();
        }

        public void Stop(bool getResult = true)
        {
            mTimer.Stop();
            string time = $"Обработка завершена!\nЗатраченное время {mTimer.Elapsed.TotalSeconds} с.";
            System.Windows.MessageBox.Show(time);
        }

        private static TimerUtils? mInstance;
        private Stopwatch? mTimer;
    }
}
