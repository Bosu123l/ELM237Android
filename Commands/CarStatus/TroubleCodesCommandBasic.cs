using Android.Bluetooth;
using Android.Util;
using OBDProject.Utils;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OBDProject.Commands.CarStatus
{
    public class TroubleCodesCommandBasic : BasicCommand
    {
        protected static char[] DtcLetters = { 'P', 'C', 'B', 'U' };
        protected static char[] HexArray = "0123456789ABCDEF".ToCharArray();

        protected static Regex NewLine = new Regex(@"\r\n");
        protected static Regex NewLineAndSpecialCharsRegex = new Regex(@"[\r\n].:");
        protected static Regex NewLineWithNumbersRegex = new Regex(@"^43|[\r\n]43|[\r\n]");

        private string _rawData;
        private readonly SemaphoreSlim _semaphoreSlim;

        public TroubleCodesCommandBasic(byte[] command, BluetoothSocket socket, SemaphoreSlim semaphoreSlim,
            LogManager logManager)
            : base(command, socket, string.Empty, new object(), 0, logManager)
        {
            Source = string.Empty;
            _semaphoreSlim = semaphoreSlim;
        }

        public override async Task ReadResult()
        {
            await _semaphoreSlim.WaitAsync();
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
            finally
            {
                _semaphoreSlim.Release();
            }
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
                    a = (byte)Socket.InputStream.ReadByte();
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
                Socket.InputStream.Flush();
            }
        }

        protected override void PrepereFindResult()
        {
            string builder = string.Empty;
            string result = _rawData;

            string workingData;
            int startIndex = 0;

            string oneFrame = ClearResponseByRegex(result, NewLine);

            if (oneFrame.Length <= 16 && oneFrame.Length % 4 == 0)
            {
                workingData = oneFrame;
                startIndex = 4;
            }
            else if (result.Contains(":"))
            {
                workingData = ClearResponseByRegex(oneFrame, NewLineAndSpecialCharsRegex);
                startIndex = 7;
            }
            else
            {
                workingData = ClearResponseByRegex(result, NewLineWithNumbersRegex);
            }
            try
            {
                int workingDataLength = workingData.Length;
                for (int begin = startIndex; begin < workingDataLength; begin += 4)
                {
                    string temp = string.Empty;
                    byte firstByte = HexStringToByte(workingData[begin].ToString());
                    int ch1 = ((firstByte & 0xC0) >> 6);
                    int ch2 = ((firstByte & 0x30) >> 4);
                    temp += DtcLetters[ch1];
                    temp += HexArray[ch2];
                    temp += workingData.Substring(begin + 1, begin + 4);

                    if (temp.Equals("P0000"))
                    {
                        return;
                    }
                    builder += string.Format("{0}{1}", temp, System.Environment.NewLine);
                }
            }
            catch (Exception e)
            {
                LogManager.WarringWriteLine(e.Message);
            }
            finally
            {
                base.OnResponse(builder.ToString());
            }
        }

        private string ClearResponseByRegex(string data, Regex regexPattern)
        {
            return regexPattern.Replace(data, string.Empty);
        }

        protected byte HexStringToByte(string ch)
        {
            return (byte)(Convert.ToInt32(ch, 16) << 4);
        }
    }
}