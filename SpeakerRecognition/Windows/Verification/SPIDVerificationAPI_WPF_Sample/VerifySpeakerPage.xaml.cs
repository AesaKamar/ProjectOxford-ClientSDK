﻿// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Project Oxford: http://ProjectOxford.ai
// 
// ProjectOxford SDK Github:
// https://github.com/Microsoft/ProjectOxfordSDK-Windows
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using NAudio.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SPIDVerificationAPI_WPF_Sample
{
    /// <summary>
    /// Interaction logic for VerifySpeakerPage.xaml
    /// </summary>
    public partial class VerifySpeakerPage : Page
    {
        private string _subscriptionKey;
        private string _speakerId = null;
        private WaveIn _waveIn;
        private WaveFileWriter _fileWriter;
        private Stream _stream;
        private VerificationServiceHttpClientHelper _helper;
        /// <summary>
        /// Initialization constructor for the verify speaker page
        /// </summary>
        public VerifySpeakerPage()
        {
            InitializeComponent();
            _subscriptionKey = ((MainWindow)Application.Current.MainWindow).SubscriptionKey;
            IsolatedStorageHelper _storageHelper = IsolatedStorageHelper.getInstance();
            _speakerId = _storageHelper.readValue(MainWindow.SPEAKER_FILENAME);
            initializeRecorder();
            _helper = new VerificationServiceHttpClientHelper();
            _helper.SubscriptionKey = _subscriptionKey;
            string userPhrase = _storageHelper.readValue(MainWindow.SPEAKER_PHRASE_FILENAME);
            userPhraseTxt.Text = userPhrase;
        }
        /// <summary>
        /// Initialize NAudio recorder instance
        /// </summary>
        private void initializeRecorder()
        {
            _waveIn = new WaveIn();
            _waveIn.DeviceNumber = 0;
            int sampleRate = 16000; // 8 kHz
            int channels = 1; // mono
            _waveIn.WaveFormat = new WaveFormat(sampleRate, channels);
            _waveIn.DataAvailable += waveIn_DataAvailable;
            _waveIn.RecordingStopped += waveSource_RecordingStopped;
        }
        /// <summary>
        /// A listener called when the recording stops
        /// </summary>
        /// <param name="sender">Sender object responsible for event</param>
        /// <param name="e">A set of arguments sent to the listener</param>
        private void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            _fileWriter.Dispose();
            _fileWriter = null;
            _stream.Seek(0, SeekOrigin.Begin);
            //Dispose recorder object
            _waveIn.Dispose();
            initializeRecorder();
            enrollSpeaker(_stream);
        }
        /// <summary>
        /// A method that's called whenever there's a chunk of recorded audio is recorded
        /// </summary>
        /// <param name="sender">The sender object responsible for the event</param>
        /// <param name="e">The arguments of the event object</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (_fileWriter == null)
            {
                _stream = new IgnoreDisposeStream(new MemoryStream());
                _fileWriter = new WaveFileWriter(_stream, _waveIn.WaveFormat);
            }
            _fileWriter.WriteData(e.Buffer, 0, e.BytesRecorded);
        }
        private async void enrollSpeaker(Stream audioStream)
        {
            try
            {
                setStatus("Verifying..");
                Stopwatch sw = Stopwatch.StartNew();
                VerificationResult response = await _helper.VerifyAsync(audioStream, _speakerId);
                sw.Stop();
                setStatus("Verification Done, Elapsed Time: " + sw.Elapsed);
                statusResTxt.Text = response.Result;
                confTxt.Text = response.Confidence;
            }
            catch (Exception exception)
            {
                setStatus(exception.Message);
            }
        }
        private void setStatus(string status)
        {
            ((MainWindow)Application.Current.MainWindow).Log(status);
        }
        private void stopRecordBtn_Click_1(object sender, RoutedEventArgs e)
        {
            recordBtn.IsEnabled = true;
            stopRecordBtn.IsEnabled = false;
            _waveIn.StopRecording();
        }
        private void recordBtn_Click_1(object sender, RoutedEventArgs e)
        {
            recordBtn.IsEnabled = false;
            stopRecordBtn.IsEnabled = true;
            _waveIn.StartRecording();
            setStatus("Recording...");
        }
    }
}
