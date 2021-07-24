namespace SystemSw_Core.Extron.Devices
{
    public sealed class TestCommDevice : ICommunicationDevice
    {
        private const string Identify = "V1 A1 T1 P0 S0 Z0 R0 QSC1.11 QPC1.11 M4";
        
        private const string qsc = "1.11";
        private const string qpc = "1.11";
        private const int max = 4;  

        private string response = "";
        private bool isDisposed = false;

        public bool IsOpen { get; private set; }


        public void Close()
        {
            AssertDisposed();
            IsOpen = false;
        }

        public void Dispose() => isDisposed = true;

        public void Open()
        {
            AssertDisposed();
            IsOpen = true;
        }

        public string ReadLine()
        {
            AssertDisposed();
            var r = response;
            response = "";
            return r;
        }

        public void Write(string text)
        {
            AssertDisposed();
            switch (text[0])
            {
                case 'I':
                case 'i':
                    response = Identify;
                    break;
                case 'Q':
                case 'q':
                    response = $"QSC{qsc}\r\nQPC{qpc}";
                    break;
                case 'b':
                    response = "MUT 1";
                    break;
                case 'B':
                    response = "MUT 0";
                    break;
                case '+':
                    response = "AMUT 1";
                    break;
                case '-':
                    response = "AMUT 0";
                    break;
                case '[':
                case ']':
                case '(':
                case ')':
                    break;
            }
            switch (text[1])
            {
                case '!':
                    response = $"C{text[0]}";
                    break;
                case '&':
                    response = $"V{text[0]}";
                    break;
                case '$':
                    response = $"A{text[0]}";
                    break;
            }
        }


        private void AssertDisposed() 
        {
            if (isDisposed) throw new System.ObjectDisposedException($"{nameof(TestCommDevice)}");
        }
    }
}