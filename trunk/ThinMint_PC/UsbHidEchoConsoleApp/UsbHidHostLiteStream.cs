/* UsbHostLiteStream
 * Copyright (c) 2011 Secret Labs LLC. All Rights Reserved.
 *
 * Licensed under the Apache 2.0 open source license
 * 
 */
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace UsbHidEchoConsoleApp
{
    public class UsbHidHostLiteStream : IDisposable
    {
        SafeFileHandle _primaryHandle = null;
        SafeFileHandle _readHandle = null;
        SafeFileHandle _writeHandle = null;

        byte _inputReportByteLength;
        byte _outputReportByteLength;

        const byte DEFAULT_REPORT_NUMBER = 0x00;

        byte[] _rxBuffer;
        int _rxBufferSize;
        int _rxBufferUsedBytes = 0; // number of characters in buffer
        object _rxBufferLockObject = new object();

        public UsbHidHostLiteStream(string pathName)
        {
            // create an initial connection to the device
            _primaryHandle = WinApi.CreateFile(pathName, 0, WinApi.FILE_SHARE_READ | WinApi.FILE_SHARE_WRITE, IntPtr.Zero, WinApi.OPEN_EXISTING, 0, IntPtr.Zero);

            if (_primaryHandle.IsInvalid)
            {
                // operation failed
                int lastError = Marshal.GetLastWin32Error();
                switch (lastError)
                {
                    case WinApi.ERROR_FILE_NOT_FOUND:
                        throw new System.IO.FileNotFoundException();
                    case WinApi.ERROR_ACCESS_DENIED:
                        throw new System.IO.IOException("Access denied.");
                    default:
                        throw new System.IO.IOException("Cannot connect to " + pathName + ".");
                }
            }

            // get our HID device's input and output report size
            IntPtr ptrPreparsedData = IntPtr.Zero;
            try
            {
                WinApi.HIDP_CAPS capabilities = new WinApi.HIDP_CAPS();
                WinApi.HidD_GetPreparsedData(_primaryHandle, out ptrPreparsedData);
                if (WinApi.HidP_GetCaps(ptrPreparsedData, ref capabilities))
                {
                    _inputReportByteLength = (byte)capabilities.InputReportByteLength;
                    _outputReportByteLength = (byte)capabilities.OutputReportByteLength;
                }
                else
                {
                    throw new System.IO.IOException("Could not retrieve device capabilities");
                }
            }
            finally
            {
                if (ptrPreparsedData != IntPtr.Zero)
                    WinApi.HidD_FreePreparsedData(ptrPreparsedData);
            }

            // create a receive buffer (32x our inputReportByteLength)
            lock (_rxBufferLockObject)
            {
                _rxBufferSize = _inputReportByteLength * 32;
                _rxBuffer = new byte[_rxBufferSize];
            }


            // create a read handle
            _readHandle = WinApi.CreateFile(pathName, WinApi.GENERIC_READ, WinApi.FILE_SHARE_READ | WinApi.FILE_SHARE_WRITE, IntPtr.Zero, WinApi.OPEN_EXISTING, WinApi.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
            if (_readHandle.IsInvalid)
            {
                // operation failed
                int lastError = Marshal.GetLastWin32Error();
                switch (lastError)
                {
                    case WinApi.ERROR_FILE_NOT_FOUND:
                        throw new System.IO.FileNotFoundException();
                    case WinApi.ERROR_ACCESS_DENIED:
                        throw new System.IO.IOException("Access denied.");
                    default:
                        throw new System.IO.IOException("Cannot connect to " + pathName + ".");
                }
            }

            // clear out the read buffer (to ensure we're starting with an empty buffer)
            WinApi.HidD_FlushQueue(_readHandle);

            // create a write handle
            _writeHandle = WinApi.CreateFile(pathName, WinApi.GENERIC_WRITE, WinApi.FILE_SHARE_READ | WinApi.FILE_SHARE_WRITE, IntPtr.Zero, WinApi.OPEN_EXISTING, 0, IntPtr.Zero);
            if (_writeHandle.IsInvalid)
            {
                // operation failed
                int lastError = Marshal.GetLastWin32Error();
                switch (lastError)
                {
                    case WinApi.ERROR_FILE_NOT_FOUND:
                        throw new System.IO.FileNotFoundException();
                    case WinApi.ERROR_ACCESS_DENIED:
                        throw new System.IO.IOException("Access denied.");
                    default:
                        throw new System.IO.IOException("Cannot connect to " + pathName + ".");
                }
            }
        }

        private int ReadReport(byte[] reportBuffer, int timeoutMilliseconds)
        {
            int bytesRead = 0;

            IntPtr ptrReadOverlapped = IntPtr.Zero;
            IntPtr ptrReportBuffer = IntPtr.Zero;

            try
            {
                NativeOverlapped readOverlapped = new NativeOverlapped();
                readOverlapped.OffsetLow = 0;
                readOverlapped.OffsetHigh = 0;
                IntPtr readEventWaitHandle = WinApi.CreateEvent(IntPtr.Zero, false, false, string.Empty);
                readOverlapped.EventHandle = readEventWaitHandle;

                ptrReadOverlapped = Marshal.AllocHGlobal(Marshal.SizeOf(readOverlapped));
                Marshal.StructureToPtr(readOverlapped, ptrReadOverlapped, false);

                ptrReportBuffer = Marshal.AllocHGlobal(reportBuffer.Length);

                reportBuffer[0] = DEFAULT_REPORT_NUMBER;

                int success = WinApi.ReadFile(_readHandle, ptrReportBuffer, reportBuffer.Length, ref bytesRead, ptrReadOverlapped);
                if (success == 0)
                {
                    int lastWin32Error = Marshal.GetLastWin32Error();
                    if (lastWin32Error == WinApi.ERROR_IO_PENDING)
                    {
                        int result = WinApi.WaitForSingleObject(readEventWaitHandle, timeoutMilliseconds); // wait until the read completes (or a number of milliseconds)
                        if (result == WinApi.WAIT_OBJECT_0)
                        {
                            // the operation completed successfully
                            WinApi.GetOverlappedResult(_readHandle, ptrReadOverlapped, ref bytesRead, false);
                        }
                        else if (result == WinApi.WAIT_TIMEOUT)
                        {
                            // no data available
                            WinApi.CancelIo(_readHandle);
                            bytesRead = 0;
                        }
                        else
                        {
                            // an error occured; throw an exception
                            throw new System.IO.IOException();
                        }
                    }
                    else
                    {
                        // cancel all pending IO
                        WinApi.CancelIo(_readHandle);

                        // and throw an exception
                        throw new System.IO.IOException();
                    }
                }
            }
            finally
            {
                if (ptrReportBuffer != IntPtr.Zero)
                {
                    // copy the report buffer's unmanaged array to the managed array
                    Marshal.Copy(ptrReportBuffer, reportBuffer, 0, bytesRead);
                    // free the unmanaged array
                    Marshal.FreeHGlobal(ptrReportBuffer);
                }
                if (ptrReadOverlapped != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptrReadOverlapped);
            }

            return bytesRead;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            // first, check and see if there is data available in the receive buffer
            int bytesInReceiveBuffer = 0;
            lock (_rxBufferLockObject)
                bytesInReceiveBuffer = _rxBufferUsedBytes;

            // if we have enough bytes in the receive buffer already, return them.
            if (bytesInReceiveBuffer >= count)
            {
                lock (_rxBufferLockObject)
                {
                    Array.Copy(_rxBuffer, 0, buffer, offset, count);
                    Array.Copy(_rxBuffer, count, _rxBuffer, 0, _rxBufferUsedBytes - count);
                    _rxBufferUsedBytes -= count;
                    return count;
                }
            }
            else
            {
                bool getAllAvailableData;
                int timeout;
                if (bytesInReceiveBuffer > 0)
                {
                    // bytes available, but not what was asked for; try to get any additional data available (but do not wait for timeout)
                    timeout = 0;
                    getAllAvailableData = true;
                }
                else
                {
                    // no data available; wait for the first available data and return it
                    timeout = -1; // infinite
                    getAllAvailableData = false;
                }

                int reportBytesRead = 0;
                byte[] overflowData = new byte[] { }; // capture any "overflow data" for close situations where we are trying to read almost the entire buffer (within one report length)
                do
                {
                    // in the special case that we're about to overflow, do not gather any more data.
                    if (_rxBufferSize - bytesInReceiveBuffer < _inputReportByteLength)
                        break;

                    byte[] reportBuffer = new byte[_inputReportByteLength];
                    reportBytesRead = ReadReport(reportBuffer, timeout);
                    if (reportBytesRead > 0)
                    {
                        // a report was received
                        // Copy the received data to buffer for the application to use.
                        if (reportBuffer[0] == DEFAULT_REPORT_NUMBER) // discard all reports except for default ones
                        {
                            int dataBytesInThisReport = reportBuffer[1];
                            if (dataBytesInThisReport > 0)
                            {
                                lock (_rxBufferLockObject)
                                {
                                    // NOTE: if we get more data than will fit in our buffer, it will overflow here.
                                    int bytesToCopy = System.Math.Min(dataBytesInThisReport, _rxBufferSize - _rxBufferUsedBytes);
                                    Array.Copy(reportBuffer, 2, _rxBuffer, _rxBufferUsedBytes, bytesToCopy);
                                    _rxBufferUsedBytes += bytesToCopy;
                                    bytesInReceiveBuffer = _rxBufferUsedBytes;
                                }
                            }
                        }
                    }

                    // if we were only supposed to get one report, return what we have now.
                    if (!getAllAvailableData)
                        break;
                } while (reportBytesRead > 0 && bytesInReceiveBuffer < count);

                // now, return the data which is available to the caller
                lock (_rxBufferLockObject)
                {
                    int bytesToCopy = System.Math.Min(count, _rxBufferUsedBytes);
                    Array.Copy(_rxBuffer, 0, buffer, offset, bytesToCopy);
                    Array.Copy(_rxBuffer, bytesToCopy, _rxBuffer, 0, _rxBufferUsedBytes - bytesToCopy);
                    _rxBufferUsedBytes -= bytesToCopy;
                    return bytesToCopy;
                }
            }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            // report format:
            // byte 0: report #
            // byte 1: length of data in report
            // byte 2-n: data
            byte maxDataBytesPerReport = (byte)(_outputReportByteLength - 2);

            for (int index = 0; index < count; index += maxDataBytesPerReport)
            {
                byte dataBytesInThisReport = (byte)System.Math.Min(count - index, maxDataBytesPerReport);

                byte[] reportBuffer = new byte[_outputReportByteLength];
                reportBuffer[0] = DEFAULT_REPORT_NUMBER;
                reportBuffer[1] = dataBytesInThisReport;
                Array.Copy(buffer, offset + index, reportBuffer, 2, dataBytesInThisReport);

                int bytesWritten = 0;

                int iResult = WinApi.WriteFile(_writeHandle, reportBuffer, reportBuffer.Length, ref bytesWritten, IntPtr.Zero);
                if (iResult == 0)
                {

                    if (!_writeHandle.IsInvalid)
                        _writeHandle.Close();
                    throw new System.IO.IOException();
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_writeHandle != null && !_writeHandle.IsInvalid && !_writeHandle.IsClosed)
            {
                _writeHandle.Dispose();
                _writeHandle = null;
            }

            if (_readHandle != null && !_readHandle.IsInvalid && !_readHandle.IsClosed)
            {
                _readHandle.Dispose();
                _readHandle = null;
            }

            if (_primaryHandle != null && !_primaryHandle.IsInvalid && !_primaryHandle.IsClosed)
            {
                _primaryHandle.Dispose();
                _primaryHandle = null;
            }
        }

        #endregion
    }
}
