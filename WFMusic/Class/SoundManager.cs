using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using Un4seen.Bass.AddOn.Tags;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading;

namespace BassManager
{
    public class SM
    {
        // LOCAL VARS
        private int _stream = 0;
        private string _fileName = String.Empty;
        private int _tickCounter = 0;
        private float _gainDB = 0f;
        private DSPPROC _myDSPAddr = null;
        private SYNCPROC _sync = null;
        private int _syncer = 0;
        private int _deviceLatencyMS = 0; // device latency in milliseconds
        private int _deviceLatencyBytes = 0; // device latency in bytes
        private Visuals _vis = new Visuals(); // visuals class instance
        private int _updateInterval = 50; // 50ms
        private long _pos = 0;
        private Un4seen.Bass.BASSTimer _updateTimer = null;
        private TAG_INFO tagInfo;
        private BASS_CHANNELINFO info;
        private int spectrumWidth;
        private int spectrumHeight;
        private DelegateValue delegateVal;
        private bool playPause = false;
        private Form form;
        private Thread th;

        public BASS_CHANNELINFO Info
        {
            get
            {
                return info;
            }

            set
            {
                info = value;
            }
        }
        public TAG_INFO TagInfo
        {
            get
            {
                return tagInfo;
            }

            set
            {
                tagInfo = value;
            }
        }
        public long Pos
        {
            get
            {
                return _pos;
            }

            set
            {
                _pos = value;
            }
        }

        public struct DelegateValue
        {
            public int playProcessValue;
            public string timeInfoValue;
            public string cpuInfoValue;
            public bool playProcessEndValue;
        }

        public SM(Form form)
        {
            this.form = form;

            if (Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
            {
                BASS_INFO info = new BASS_INFO();
                Bass.BASS_GetInfo(info);
                Console.WriteLine(info.ToString());
                _deviceLatencyMS = info.latency;
            }
            else
                Console.WriteLine("Bass_Init error!");

            // create a secure timer
            _updateTimer = new Un4seen.Bass.BASSTimer(_updateInterval);
            _updateTimer.Tick += new EventHandler(timerUpdate_Tick);

            _sync = new SYNCPROC(EndPosition);

            th = new Thread(new ThreadStart(DelegateValueUpdate));
            th.Start();
        }

        //设置播放源文件
        public void SelectFile(string fileName)
        {
            this._fileName = fileName;

            if (_stream != 0)
            {
                _updateTimer.Stop();
                Bass.BASS_StreamFree(_stream);
            }

            // create the stream
            _stream = Bass.BASS_StreamCreateFile(_fileName, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN);
            // set dsp
            _myDSPAddr = new DSPPROC(MyDSPGainUnsafe);
            Bass.BASS_ChannelSetDSP(_stream, _myDSPAddr, IntPtr.Zero, 2);

            //if (_stream != 0)
            //{
            //    // get some channel info
            //    //info = new BASS_CHANNELINFO();
            //    //Bass.BASS_ChannelGetInfo(_stream, info);
            //    // get the tags...
            //    //tagInfo = new TAG_INFO(_fileName);
            //}

            GetWaveForm();

        }

        //获取歌曲信息
        public List<TAG_INFO> GetTagInfo(List<string> files)
        {
            List<TAG_INFO> list = new List<TAG_INFO>();

            foreach (var file in files)
            {
                SelectFile(file);
                list.Add(this.tagInfo);
            }

            return list;
        }

        //开始
        public void Start()
        {
            if (_fileName != String.Empty)
            {
                if (_stream != 0)
                {
                    playPause = false;
                    // used in RMS
                    _30mslength = (int)Bass.BASS_ChannelSeconds2Bytes(_stream, 0.03); // 30ms window                                                                                 // latency from milliseconds to bytes
                    _deviceLatencyBytes = (int)Bass.BASS_ChannelSeconds2Bytes(_stream, _deviceLatencyMS / 1000.0);

                    if (WF2 != null && WF2.IsRendered)
                    {
                        // make sure playback and wave form are in sync, since
                        // we rended with 16-bit but play here with 32-bit
                        WF2.SyncPlayback(_stream);

                        //long cuein = WF2.GetMarker("CUE");
                        long cueout = WF2.GetMarker("END");

                        //int cueinFrame = WF2.Position2Frames(cuein);
                        //int cueoutFrame = WF2.Position2Frames(cueout);
                        //Console.WriteLine("CueIn at {0}sec.; CueOut at {1}sec.", WF2.Frame2Seconds(cueinFrame), WF2.Frame2Seconds(cueoutFrame));

                        //if (cuein >= 0)
                        //{
                        //    Bass.BASS_ChannelSetPosition(_stream, cuein);
                        //}
                        if (cueout >= 0)
                        {
                            Bass.BASS_ChannelRemoveSync(_stream, _syncer);
                            _syncer = Bass.BASS_ChannelSetSync(_stream, BASSSync.BASS_SYNC_POS, cueout, _sync, IntPtr.Zero);
                        }
                    }

                    if (_stream != 0 && Bass.BASS_ChannelPlay(_stream, false))
                    {
                        _updateTimer.Start();
                    }
                    else
                    {
                        Console.WriteLine("Error={0}", Bass.BASS_ErrorGetCode());
                    }

                }

            }
        }

        //暂停
        public void Pause()
        {
            _updateTimer.Stop();
            Bass.BASS_ChannelPause(_stream);
            playPause = true;
        }

        //停止
        public void Stop()
        {
            _updateTimer.Stop();
            //Bass.BASS_ChannelStop(_stream);

            if (WF2 != null && WF2.IsRenderingInProgress)
                WF2.RenderStop();

            Bass.BASS_StreamFree(_stream);
            _stream = 0;
        }

        //关闭bass
        public void CloseBass()
        {
            _updateTimer.Tick -= new EventHandler(timerUpdate_Tick);
            // close bass
            Bass.BASS_Stop();
            Bass.BASS_Free();
            th.Abort();
        }

        //读取完成播放
        public delegate void ReadProcessDelegate();
        public ReadProcessDelegate ReadProcessComplete;

        //播放完成更新
        public delegate void EndProcessDelegate();
        public EndProcessDelegate EndProcessComplete;

        //数值进度等更新
        public delegate void ValueUpdateDelegate(int pos, string time, string cpu);
        public ValueUpdateDelegate DelegateValueUpdateFun;

        private void timerUpdate_Tick(object sender, System.EventArgs e)
        {
            // here we gather info about the stream, when it is playing...
            if (Bass.BASS_ChannelIsActive(_stream) == BASSActive.BASS_ACTIVE_PLAYING)
            {
                // the stream is still playing...
            }
            else
            {
                // the stream is NOT playing anymore...
                _updateTimer.Stop();
                return;
            }

            // from here on, the stream is for sure playing...
            _tickCounter++;
            long pos = Bass.BASS_ChannelGetPosition(_stream); // position in bytes
            long len = Bass.BASS_ChannelGetLength(_stream); // length in bytes
            this._pos = pos;

            if (_tickCounter == 5)
            {
                // display the position every 250ms (since timer is 50ms)
                _tickCounter = 0;
                double totaltime = Bass.BASS_ChannelBytes2Seconds(_stream, len); // the total time length
                double elapsedtime = Bass.BASS_ChannelBytes2Seconds(_stream, pos); // the elapsed time length
                double remainingtime = totaltime - elapsedtime;

                delegateVal.timeInfoValue = String.Format("{0:#0.00} / {1:#0.00}", Utils.FixTimespan(elapsedtime, "MMSS"), Utils.FixTimespan(totaltime, "MMSS")); //Utils.FixTimespan(remainingtime, "MMSS")
                delegateVal.cpuInfoValue = String.Format("Bass-CPU: {0:0.00}%", Bass.BASS_GetCPU());

            }

            // display the level bars
            int peakL = 0;
            int peakR = 0;
            // for testing you might also call RMS_2, RMS_3 or RMS_4
            //RMS(_stream, out peakL, out peakR);
            RMS_2(_stream, out peakL, out peakR);
            // level to dB
            double dBlevelL = Utils.LevelToDB(peakL, 65535);
            double dBlevelR = Utils.LevelToDB(peakR, 65535);
            //RMS_2(_stream, out peakL, out peakR);
            //RMS_3(_stream, out peakL, out peakR);
            //RMS_4(_stream, out peakL, out peakR);

            //**//
            // update the position
            UpdatePosition(pos, len);
            // update spectrum
            UpdateSpectrum();
        }

        //更新时间等信息
        private void DelegateValueUpdate()
        {
            while (true)
            {
                if (!playPause)
                {
                    DelegateValueUpdateFun?.Invoke(
                        this.delegateVal.playProcessValue,
                        this.delegateVal.timeInfoValue,
                        this.delegateVal.cpuInfoValue
                        );
                }
                Thread.Sleep(5);
            }
        }

        private void EndPosition(int handle, int channel, int data, IntPtr user)
        {
            Bass.BASS_ChannelStop(channel);
            playPause = true;
            this.delegateVal.playProcessValue = 0;
            this.delegateVal.timeInfoValue = "";
            this.delegateVal.cpuInfoValue = "";

            EndProcessComplete?.Invoke();
        }


        #region VU (peak) level meter

        // this method is a simple demo!
        // See the other implementations (RMS_2, RMS_3, RMS_4) for other examples
        // As you can see, there are many ways to interact with unmanaged code!

        private int _30mslength = 0;
        private float[] _rmsData;     // our global data buffer used at RMS

        private void RMS(int channel, out int peakL, out int peakR)
        {
            float maxL = 0f;
            float maxR = 0f;
            int length = _30mslength; // 30ms window already set at buttonPlay_Click
            int l4 = length / 4; // the number of 32-bit floats required (since length is in bytes!)

            // increase our data buffer as needed
            if (_rmsData == null || _rmsData.Length < l4)
                _rmsData = new float[l4];

            // Note: this is a special mechanism to deal with variable length c-arrays.
            // In fact we just pass the address (reference) to the first array element to the call.
            // However the .Net marshal operation will copy N array elements (so actually fill our float[]).
            // N is determined by the size of our managed array, in this case N=l4
            length = Bass.BASS_ChannelGetData(channel, _rmsData, length);

            l4 = length / 4; // the number of 32-bit floats received

            for (int a = 0; a < l4; a++)
            {
                float absLevel = Math.Abs(_rmsData[a]);
                // decide on L/R channel
                if (a % 2 == 0)
                {
                    // L channel
                    if (absLevel > maxL)
                        maxL = absLevel;
                }
                else
                {
                    // R channel
                    if (absLevel > maxR)
                        maxR = absLevel;
                }
            }

            // limit the maximum peak levels to +6bB = 0xFFFF = 65535
            // the peak levels will be int values, where 32767 = 0dB!
            // and a float value of 1.0 also represents 0db.
            peakL = (int)Math.Round(32767f * maxL) & 0xFFFF;
            peakR = (int)Math.Round(32767f * maxR) & 0xFFFF;
        }

        // works as well, and should just demo the use of GCHandles
        private void RMS_2(int channel, out int peakL, out int peakR)
        {
            float maxL = 0f;
            float maxR = 0f;
            int length = _30mslength; // 30ms window already set at buttonPlay_Click
            int l4 = length / 4; // the number of 32-bit floats required (since length is in bytes!)

            // increase our data buffer as needed
            if (_rmsData == null || _rmsData.Length < l4)
                _rmsData = new float[l4];

            // create a handle to a managed object and pin it,
            // so that the Garbage Collector will not remove it
            GCHandle hGC = GCHandle.Alloc(_rmsData, GCHandleType.Pinned);
            try
            {
                // this will hand over an IntPtr to our managed data object
                length = Bass.BASS_ChannelGetData(channel, hGC.AddrOfPinnedObject(), length);
            }
            finally
            {
                // free the pinned handle, so that the Garbage Collector can use it
                hGC.Free();
            }

            l4 = length / 4; // the number of 32-bit floats received

            for (int a = 0; a < l4; a++)
            {
                // decide on L/R channel
                if (a % 2 == 0)
                {
                    // L channel
                    if (_rmsData[a] > maxL)
                        maxL = _rmsData[a];
                }
                else
                {
                    // R channel
                    if (_rmsData[a] > maxR)
                        maxR = _rmsData[a];
                }
            }

            // limit the maximum peak levels to +6bB = 0xFFFF = 65535
            // the peak levels will be int values, where 32767 = 0dB!
            // and a float value of 1.0 also represents 0db.
            peakL = (int)Math.Round(32767f * maxL) & 0xFFFF;
            peakR = (int)Math.Round(32767f * maxR) & 0xFFFF;
        }

        // works as well (even if the slowest), and should just demo the use of Marshal.Copy
        private void RMS_3(int channel, out int peakL, out int peakR)
        {
            float maxL = 0f;
            float maxR = 0f;
            int length = _30mslength; // 30ms window already set at buttonPlay_Click
            int l4 = length / 4; // the number of 32-bit floats required (since length is in bytes!)

            // increase our data buffer as needed
            if (_rmsData == null || _rmsData.Length < l4)
                _rmsData = new float[l4];

            IntPtr buffer = IntPtr.Zero;
            // allocate a buffer of that size for unmanaged code
            buffer = Marshal.AllocCoTaskMem(length);
            try
            {
                // get the data
                length = Bass.BASS_ChannelGetData(channel, buffer, length);
                l4 = length / 4; // the number of 32-bit floats received
                                 // copy the data from unmanaged BASS to our local managed application
                Marshal.Copy(buffer, _rmsData, 0, l4);

                for (int a = 0; a < l4; a++)
                {
                    // decide on L/R channel
                    if (a % 2 == 0)
                    {
                        // L channel
                        if (_rmsData[a] > maxL)
                            maxL = _rmsData[a];
                    }
                    else
                    {
                        // R channel
                        if (_rmsData[a] > maxR)
                            maxR = _rmsData[a];
                    }
                }
            }
            finally
            {
                // free the allocated unmanaged memory
                Marshal.FreeCoTaskMem(buffer);
            }

            // limit the maximum peak levels to +6bB = 0xFFFF = 65535
            // the peak levels will be int values, where 32767 = 0dB!
            // and a float value of 1.0 also represents 0db.
            peakL = (int)Math.Round(32767f * maxL) & 0xFFFF;
            peakR = (int)Math.Round(32767f * maxR) & 0xFFFF;
        }

        // works as well, and should just demo the use of unsafe code blocks
        private void RMS_4(int channel, out int peakL, out int peakR)
        {
            float maxL = 0f;
            float maxR = 0f;
            int length = _30mslength; // 30ms window already set at buttonPlay_Click
            int l4 = length / 4; // the number of 32-bit floats required (since length is in bytes!)

            // increase our data buffer as needed
            if (_rmsData == null || _rmsData.Length < l4)
                _rmsData = new float[l4];

            unsafe
            {
                fixed (float* p = _rmsData) // equivalent to p = &_rmsData[0]
                {
                    length = Bass.BASS_ChannelGetData(channel, (IntPtr)p, length);
                }
            }
            l4 = length / 4; // the number of 32-bit floats received

            for (int a = 0; a < l4; a++)
            {
                // decide on L/R channel
                if (a % 2 == 0)
                {
                    // L channel
                    if (_rmsData[a] > maxL)
                        maxL = _rmsData[a];
                }
                else
                {
                    // R channel
                    if (_rmsData[a] > maxR)
                        maxR = _rmsData[a];
                }
            }

            // limit the maximum peak levels to +6bB = 0xFFFF = 65535
            // the peak levels will be int values, where 32767 = 0dB!
            // and a float value of 1.0 also represents 0db.
            peakL = (int)Math.Round(32767f * maxL) & 0xFFFF;
            peakR = (int)Math.Round(32767f * maxR) & 0xFFFF;
        }

        #endregion

        #region DSP (gain) routines 

        // this will be our local buffer
        // we use it outside of MyDSPGain for better performance and to reduce
        // the need to alloc it everytime MyDSPGain is called!
        private float[] _data;

        // this local member keeps the amplification level as a float value
        private float _gainAmplification = 1;

        // the DSP callback - safe!
        private void MyDSPGain(int handle, int channel, System.IntPtr buffer, int length, int user)
        {
            if (_gainAmplification == 1f || length == 0 || buffer == IntPtr.Zero)
                return;

            // length is in bytes, so the number of floats to process is:
            // length / 4 : byte = 8-bit, float = 32-bit
            int l4 = length / 4;

            // increase the data buffer as needed
            if (_data == null || _data.Length < l4)
                _data = new float[l4];

            // copy from unmanaged to managed memory
            Marshal.Copy(buffer, _data, 0, l4);

            // apply the gain, assumeing using 32-bit floats (no clipping here ;-)
            for (int a = 0; a < l4; a++)
                _data[a] = _data[a] * _gainAmplification;

            // copy back from managed to unmanaged memory
            Marshal.Copy(_data, 0, buffer, l4);
        }

        // another alternative in using a DSP callback is using UNSAFE code
        // this allows you to use pointers pretty much like in C/C++!!!
        // this is fast, efficient, but is NOT safe (e.g. no overflow handling, no type checking etc.)
        // But also there is no need to Marshal and Copy any data between managed and unmanaged code
        // So be careful!
        // Also note: you need to compile your app with the /unsafe option!
        unsafe private void MyDSPGainUnsafe(int handle, int channel, IntPtr buffer, int length, IntPtr user)
        {
            if (_gainAmplification == 1f || length == 0 || buffer == IntPtr.Zero)
                return;

            // length is in bytes, so the number of floats to process is:
            // length / 4 : byte = 8-bit, float = 32-bit
            int l4 = length / 4;

            float* data = (float*)buffer;
            for (int a = 0; a < l4; a++)
            {
                data[a] = data[a] * _gainAmplification;
                // alternatively you can also use:
                // *data = *data * _gainAmplification;
                // data++;  // moves the pointer to the next float
            }
        }

        public void SetGainDB(float val)
        {
            try
            {
                this._gainDB = val;
                // convert the _gainDB value to a float
                _gainAmplification = (float)Math.Pow(10d, this._gainDB / 20d);
            }
            catch
            {
                this._gainDB = 0f;
                _gainAmplification = 1f;
            }
        }

        #endregion

        #region Wave Form 

        // zoom helper varibales
        private int _zoomStart = -1;
        private int _zoomEnd = -1;

        private Un4seen.Bass.Misc.WaveForm WF2 = null;
        private void GetWaveForm()
        {
            // unzoom...(display the whole wave form)
            _zoomStart = -1;
            _zoomEnd = -1;
            // render a wave form
            WF2 = new WaveForm(this._fileName, new WAVEFORMPROC(MyWaveFormCallback), this.form);
            WF2.FrameResolution = 0.01f; // 10ms are nice
            WF2.CallbackFrequency = 2000; // every 30 seconds rendered (3000*10ms=30sec)
            WF2.DrawWaveForm = WaveForm.WAVEFORMDRAWTYPE.Stereo;
            WF2.DrawMarker = WaveForm.MARKERDRAWTYPE.Line | WaveForm.MARKERDRAWTYPE.Name | WaveForm.MARKERDRAWTYPE.NamePositionAlternate;
            WF2.MarkerLength = 0.75f;
            // our playing stream will be in 32-bit float!
            // but here we render with 16-bit (default) - just to demo the WF2.SyncPlayback method
            WF2.RenderStart(true, BASSFlag.BASS_DEFAULT);
        }

        private void MyWaveFormCallback(int framesDone, int framesTotal, TimeSpan elapsedTime, bool finished)
        {
            if (finished)
            {
                //Console.WriteLine("Finished rendering in {0}sec.", elapsedTime);
                //Console.WriteLine("FramesRendered={0} of {1}", WF2.FramesRendered, WF2.FramesToRender);
                // eg.g use this to save the rendered wave form...
                //WF.WaveFormSaveToFile( @"C:\test.wf" );

                // auto detect silence at beginning and end
                long cuein = 0;
                long cueout = 0;
                WF2.GetCuePoints(ref cuein, ref cueout, -25.0, -42.0, -1, -1);
                WF2.AddMarker("CUE", cuein);
                WF2.AddMarker("END", cueout);

                ReadProcessComplete?.Invoke();
            }
        }

        #endregion


        #region Spectrum

        private int specIdx = 1;
        private int voicePrintIdx = 0;
        public delegate void SpectrumDelegate(Bitmap img);
        public SpectrumDelegate SpectrumUpdate;
        private void UpdateSpectrum()
        {
            Bitmap imgSpectrum = null;
            switch (specIdx)
            {
                // normal spectrum (width = resolution)
                case 0:
                    imgSpectrum = _vis.CreateSpectrum(_stream, this.spectrumWidth, this.spectrumHeight, Color.Lime, Color.Red, Color.Black, false, false, false);
                    break;
                // normal spectrum (full resolution)
                case 1:
                    imgSpectrum = _vis.CreateSpectrum(_stream, this.spectrumWidth, this.spectrumHeight, Color.SteelBlue, Color.Pink, Color.Black, false, true, true);
                    break;
                // line spectrum (width = resolution)
                case 2:
                    imgSpectrum = _vis.CreateSpectrumLine(_stream, this.spectrumWidth, this.spectrumHeight, Color.Lime, Color.Red, Color.Black, 5, 2, false, false, false);
                    break;
                // line spectrum (full resolution)
                case 3:
                    imgSpectrum = _vis.CreateSpectrumLine(_stream, this.spectrumWidth, this.spectrumHeight, Color.SteelBlue, Color.Pink, Color.Black, 16, 4, false, true, true);
                    break;
                // ellipse spectrum (width = resolution)
                case 4:
                    imgSpectrum = _vis.CreateSpectrumEllipse(_stream, this.spectrumWidth, this.spectrumHeight, Color.Lime, Color.Red, Color.Black, 1, 2, false, false, false);
                    break;
                // ellipse spectrum (full resolution)
                case 5:
                    imgSpectrum = _vis.CreateSpectrumEllipse(_stream, this.spectrumWidth, this.spectrumHeight, Color.SteelBlue, Color.Pink, Color.Black, 2, 4, false, true, true);
                    break;
                // dot spectrum (width = resolution)
                case 6:
                    imgSpectrum = _vis.CreateSpectrumDot(_stream, this.spectrumWidth, this.spectrumHeight, Color.Lime, Color.Red, Color.Black, 1, 0, false, false, false);
                    break;
                // dot spectrum (full resolution)
                case 7:
                    imgSpectrum = _vis.CreateSpectrumDot(_stream, this.spectrumWidth, this.spectrumHeight, Color.SteelBlue, Color.Pink, Color.Black, 2, 1, false, false, true);
                    break;
                // peak spectrum (width = resolution)
                case 8:
                    imgSpectrum = _vis.CreateSpectrumLinePeak(_stream, this.spectrumWidth, this.spectrumHeight, Color.SeaGreen, Color.LightGreen, Color.Orange, Color.Black, 2, 1, 2, 10, false, false, false);
                    break;
                // peak spectrum (full resolution)
                case 9:
                    imgSpectrum = _vis.CreateSpectrumLinePeak(_stream, this.spectrumWidth, this.spectrumHeight, Color.GreenYellow, Color.RoyalBlue, Color.DarkOrange, Color.Black, 23, 5, 3, 5, false, true, true);
                    break;
                // wave spectrum (width = resolution)
                case 10:
                    imgSpectrum = _vis.CreateSpectrumWave(_stream, this.spectrumWidth, this.spectrumHeight, Color.Yellow, Color.Orange, Color.Black, 1, false, false, false);
                    break;
                // dancing beans spectrum (width = resolution)
                case 11:
                    imgSpectrum = _vis.CreateSpectrumBean(_stream, this.spectrumWidth, this.spectrumHeight, Color.Chocolate, Color.DarkGoldenrod, Color.Black, 4, false, false, true);
                    break;
                // dancing text spectrum (width = resolution)
                case 12:
                    imgSpectrum = _vis.CreateSpectrumText(_stream, this.spectrumWidth, this.spectrumHeight, Color.White, Color.Tomato, Color.Black, "BASS .NET IS GREAT PIECE! UN4SEEN ROCKS...", false, false, true);
                    break;
            }

            SpectrumUpdate?.Invoke(imgSpectrum);
        }

        public void SetSpectrumArea(int w, int h)
        {
            this.spectrumWidth = w;
            this.spectrumHeight = h;
        }

        public void SetSpectrumType(int type)
        {
            this.specIdx = type;
        }


        #endregion Spectrum

        #region Position Update
        private void UpdatePosition(long pos, long len)
        {
            double pbv = 100; //process bar value

            if (len == 0 || pos < 0)
            {
                return;
            }

            double bpi = len / (double)pbv; //bytes per interval


            pos -= _deviceLatencyBytes;

            int x = (int)Math.Round(pos / bpi); //x 范围 [(0 - 100)%]

            delegateVal.playProcessValue = x;
        }

        public void SetPosition(int x)
        {

            if (WF2 == null)
                return;

            long pos = WF2.GetBytePositionFromX(x, 100, _zoomStart, _zoomEnd);
            Bass.BASS_ChannelSetPosition(_stream, pos);

        }
        #endregion


    }
}
