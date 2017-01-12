using Android.Bluetooth;
using Android.Util;
using OBDProject.Utils;
using System;
using System.Text;
using System.Threading.Tasks;

namespace OBDProject.Commands.CarStatus
{
    public class TroubleCodesCommand : BasicCommand
    {
        protected static char[] DtcLetters = { 'P', 'C', 'B', 'U' };
        protected static char[] HexArray = "0123456789ABCDEF".ToCharArray();

        private string _rawData;

        public TroubleCodesCommand(BluetoothSocket socket, object readFromDeviceLock, int position,
            LogManager logManager)
            : base(Encoding.ASCII.GetBytes("03\r"), socket, string.Empty, readFromDeviceLock, position, logManager)
        {
            Source = string.Empty;
        }

        public override async Task ReadResult()
        {
            await Task.Run(() =>
            {
                lock (ReadFromDeviceLock)
                {
                    try
                    {
                        SendCommand();
                        _rawData = CheckForErrors(ReadRawData());
                        PrepereFindResult();
                    }
                    catch (Exception ex)
                    {
                        Log.Error("++++++ERROR++++++", ex.Message);
                        LogManager.ErrorWriteLine(ex.Message);
                    }
                }
            });
        }

        protected override string ReadRawData()
        {
            StringBuilder builder = new StringBuilder();
            char c;
            int a = 0;
            try
            {
                while (true)
                {
                    a = (byte)_socket.InputStream.ReadByte();
                    if (a == -1)
                    {
                        break;
                    }
                    c = (char)a;
                    if (c == '>')
                    {
                        break;
                    }
                    if (c != ' ')
                    {
                        builder.Append(c);
                    }
                }
                return builder.ToString().Trim();
            }
            catch (Exception ex)
            {
                LogManager.ErrorWriteLine(ex.Message);
                throw new Exception(string.Format("Error while data was readed: {0}", ex.Message));
            }
            finally
            {
                _socket.InputStream.Flush();
            }
        }

        protected override void PrepereFindResult()
        {
            StringBuilder builder = new StringBuilder();
            string result = _rawData;

            string workingData;
            int startIndex = 0;

            string oneFrame = result.Replace("[\r\n]", string.Empty);

            if (oneFrame.Length <= 16 && oneFrame.Length % 4 == 0)
            {
                workingData = oneFrame;
                startIndex = 4;
            }
            else if (result.Contains(":"))
            {
                workingData = oneFrame.Replace("[\r\n].:", string.Empty);
                startIndex = 7;
            }
            else
            {
                workingData = result.Replace("^43|[\r\n]43|[\r\n]", string.Empty);
            }
            for (int begin = startIndex; begin < workingData.Length; begin += 4)
            {
                string temp = string.Empty;
                byte firstByte = hexStringToByte(workingData[begin].ToString());
                int ch1 = ((firstByte & 0xC0) >> 6);
                int ch2 = ((firstByte & 0x30) >> 4);
                temp += DtcLetters[ch1];
                temp += HexArray[ch2];
                temp += workingData.Substring(begin + 1, begin + 4);

                if (temp.Equals("P0000"))
                {
                    return;
                }
                builder.Append(temp);
                builder.Append('\n');
            }
            base.OnResponse(builder.ToString());
        }

        private byte hexStringToByte(string ch)
        {
            return (byte)(Convert.ToInt32(ch, 16) << 4);
        }
    }
}