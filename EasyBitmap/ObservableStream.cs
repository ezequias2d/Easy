using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Easy
{
    public class ObservableStream : Stream, IObservable<(long end, long position)>
    {
        internal class Unsubscriber<BaggageInfo> : IDisposable
        {
            private List<IObserver<BaggageInfo>> _observers;
            private IObserver<BaggageInfo> _observer;

            internal Unsubscriber(List<IObserver<BaggageInfo>> observers, IObserver<BaggageInfo> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }
        private List<IObserver<(long end, long position)>> observers;

        private Stream _stream;


        public override bool CanRead
        {
            get
            {
                return _stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return _stream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return _stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return _stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return _stream.Position;
            }
            set
            {
                _stream.Position = value;
                Send();
            }
        }

        public ObservableStream(Stream stream)
        {
            _stream = stream;
            observers = new List<IObserver<(long end, long position)>>();
        }

        public override void Close()
        {
            _stream.Close();
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int output = _stream.Read(buffer, offset, count);
            Send();
            return output;
        }


        public override long Seek(long offset, SeekOrigin origin)
        {
            long output = _stream.Seek(offset, origin);
            Send();
            return output;
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
            Send();
        }

        public IDisposable Subscribe(IObserver<(long end, long position)> observer)
        {
            // Check whether observer is already registered. If not, add it
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
            return new Unsubscriber<(long end, long position)>(observers, observer);
        }

        private void Send()
        {
            foreach (IObserver<(long end, long position)> observer in observers)
            {
                observer.OnNext((Length, Position));
            }
        }
    }
}
