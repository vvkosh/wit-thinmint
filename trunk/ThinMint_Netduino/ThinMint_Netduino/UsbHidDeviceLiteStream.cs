/* UsbHidDeviceMinistream
 * Copyright (c) 2011 Secret Labs LLC. All Rights Reserved.
 * 
 * Licensed under the Apache 2.0 open source license.
 */

using Microsoft.SPOT.Hardware.UsbClient;
using System;
using System.Threading;
using Microsoft.SPOT;

namespace ThinMint_Netduino
{
    public class UsbHidDeviceLiteStream : IDisposable
    {
        UsbStream _usbStream;

        byte _inputReportByteLength;
        byte _outputReportByteLength;

        byte[] _outputReportBuffer;

        byte[] _rxBuffer;
        int _rxBufferSize;
        int _rxBufferUsedBytes = 0; // number of characters in buffer
        object _rxBufferLockObject = new object();
        AutoResetEvent _rxBufferDataReceivedWaitHandle = new AutoResetEvent(false);

        bool _isDisposed = false;

        Thread _receiveThread;

        public UsbHidDeviceLiteStream(UsbStream usbStream, byte inputReportByteLength, byte outputReportByteLength)
        {
            _usbStream = usbStream;

            // create our output report buffer and set up our report sizes
            _inputReportByteLength = inputReportByteLength;
            _outputReportByteLength = outputReportByteLength;
            _outputReportBuffer = new byte[_outputReportByteLength];
            // create a receive buffer (32x our inputReportByteLength)
            lock (_rxBufferLockObject)
            {
                _rxBufferSize = _inputReportByteLength * 32;
                _rxBuffer = new byte[_rxBufferSize];
            }

            _receiveThread = new Thread(ReceiveThread);
            _receiveThread.Start();
        }

        private void ReceiveThread()
        {
            byte[] inputReportBuffer = new byte[_inputReportByteLength];
            int reportBytesRead = 0;

            while (!_isDisposed)
            {
                try
                {
                    reportBytesRead = _usbStream.Read(inputReportBuffer, 0, _inputReportByteLength);
                }
                catch
                {
                    // if we cannot read data, then dispose of our object and quit this thread
                    this.Dispose();
                    return;
                }
                if (reportBytesRead > 0)
                {
                    // report format:
                    // [report # is not in report]
                    // byte 0: length of data in report
                    // byte 1-n: data
                    byte dataBytesInThisReport = inputReportBuffer[0];
                    if (dataBytesInThisReport > 0)
                    {
                        lock (_rxBufferLockObject)
                        {
                            // if we have received more bytes then we have room, we will overflow here
                            int bytesToCopy = System.Math.Min(dataBytesInThisReport, _rxBufferSize - _rxBufferUsedBytes);
                            Array.Copy(inputReportBuffer, 1, _rxBuffer, _rxBufferUsedBytes, bytesToCopy);
                            _rxBufferUsedBytes += bytesToCopy;
                        }
                        try
                        {
                            _rxBufferDataReceivedWaitHandle.Set();
                        }
                        catch { }
                    }
                }

                // TODO: we should wait <1ms (maybe 0.1ms?)
                // wait 1ms before reading more data
                System.Threading.Thread.Sleep(1); // for 9ms-10ms response times (and not slowing everything else down much)
                //System.Threading.Thread.Sleep(0); // for 8ms response times (and slowing other operations down at least a bit--or maybe a lot)
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead;
            lock (_rxBufferLockObject)
                bytesRead = System.Math.Min(count, _rxBufferUsedBytes);

            // if there is no data available, then pause until there is.
            if (bytesRead == 0)
                _rxBufferDataReceivedWaitHandle.WaitOne();

            lock (_rxBufferLockObject)
            {
                bytesRead = System.Math.Min(count, _rxBufferUsedBytes);
                if (bytesRead > 0)
                {
                    Array.Copy(_rxBuffer, 0, buffer, offset, bytesRead);

                    Array.Copy(_rxBuffer, bytesRead, _rxBuffer, 0, _rxBufferUsedBytes - bytesRead);
                    _rxBufferUsedBytes -= bytesRead;
                }
            }

            return bytesRead;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            // report format:
            // [report # is not in report]
            // byte 0: length of data in report
            // byte 1-n: data
            byte maxDataBytesPerReport = (byte)(_outputReportByteLength - 1);

            for (int index = 0; index < count; index += maxDataBytesPerReport)
            {
                byte dataBytesInThisReport = (byte)System.Math.Min(count - index, maxDataBytesPerReport);

                // clear out the bytes in the report buffer (although this is not strictly necessary)
                Array.Clear(_outputReportBuffer, 0, _outputReportByteLength);
                // fill the report buffer with a length and data
                _outputReportBuffer[0] = dataBytesInThisReport;
                Array.Copy(buffer, offset + index, _outputReportBuffer, 1, dataBytesInThisReport);

                _usbStream.Write(_outputReportBuffer, 0, _outputReportByteLength);
            }
        }

        public void Dispose()
        {
            _isDisposed = true;

            _usbStream.Dispose();
        }
    }
}
