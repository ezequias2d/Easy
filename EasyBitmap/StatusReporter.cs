using System;
using System.Collections.Generic;
using System.Text;

namespace Easy
{
    public abstract class StatusReporter : IObserver<(long end, long position)>
    {
        internal Status status;

        private Status preStatus;
        private double preValue = -1;

        public enum Status
        {
            Filter,
            Compress
        }
        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {

        }

        public void OnNext((long end, long position) value)
        {
            double newValue = (double)value.position / (double)value.end;
            if (Math.Abs(newValue - preValue) > 0.01d || preStatus != status)
            {
                UpdateStatus(status, newValue);
                preStatus = status;
                preValue = newValue;
            }
        }

        public abstract void UpdateStatus(Status status, double complete);
    }
}
