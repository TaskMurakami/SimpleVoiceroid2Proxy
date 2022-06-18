using AI.Talk.Editor.Api;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace SimpleVoiceroid2Proxy
{
    public class AIVoiceroidEngine : IDisposable
    {

        private TtsControl _ttsControl = new();
        private readonly Channel<string> queue = Channel.CreateUnbounded<string>();
        private List<String> _voicePresets = new();
        private bool IsInterrupt = false;

        public AIVoiceroidEngine()
        {
            try
            {
                var availableHosts = _ttsControl.GetAvailableHostNames();
                if (availableHosts.Length == 0)
                {
                    Program.Logger.Warn($"A.I.VOICE のホストが見つかりません");
                    return;
                }

                _ttsControl.Initialize(availableHosts[0]);
                if (_ttsControl.Status == HostStatus.NotRunning)
                {
                    _ttsControl.StartHost();
                }
                if (_ttsControl.Status == HostStatus.NotConnected)
                {
                    _ttsControl.Connect();
                    this.PrintInfo();
                }

                _voicePresets = new List<String>(_ttsControl.VoiceNames);
                if (_voicePresets.Count == 0)
                {
                    Program.Logger.Warn($"A.I.VOICE が見つかりません");
                    return;
                }

                this.initUI();
                this.PrintPresets();
                this.PrintInterruptMode();

                Program.Logger.System($"CONNECTED");


            }
            catch (Exception exc)
            {
                Program.Logger.Error(exc);
            }
        }
        public void PrintInfo ()
        {
            Program.Logger.Info(_ttsControl.GetAvailableHostNames()[0].ToString());
            Program.Logger.Info(_ttsControl.Version);
        }
        public void PrintHelp()
        {
            Program.Logger.Info($"<F1> このヘルプを再表示");
            Program.Logger.Info($"<F2> インタラプトモードに切り替え");
            Program.Logger.Info($"<F3> 話者を切り替え");
            Program.Logger.Info($"<SPACE> コンソールの表示をクリアする");
        }
        private void initUI()
        {
            _ttsControl.ClearListItems();
            _ttsControl.TextEditMode = 0;
        }

        public String ChangeInterruptMode()
        {
            this.IsInterrupt = !this.IsInterrupt;
            if (this.IsInterrupt)
                return "インタラプトモード ON";
            else
                return "インタラプトモード OFF";
        }

        public String ChangePresets()
        {
            int cur = _voicePresets.IndexOf(_ttsControl.CurrentVoicePresetName);
            cur++;
            if (cur == _voicePresets.Count) cur = 0;
            _ttsControl.CurrentVoicePresetName = _voicePresets[cur];
            return _voicePresets[cur];
        }
        public void PrintInterruptMode()
        {
            String isInterrupt = "ON";
            if (this.IsInterrupt)
                isInterrupt = "ON";
            else
                isInterrupt = "OFF";
            Program.Logger.Info($"インタラプトモード {isInterrupt}");

        }
        public void PrintPresets()
        {
            String currentVoicePresets = _ttsControl.CurrentVoicePresetName;
            for (int i = 0; i < _voicePresets.Count; i++)
            {
                String isSelected = (currentVoicePresets == _voicePresets[i]) ? "* " : "";
                Program.Logger.Info($"{isSelected}{_voicePresets[i]}");
            }
        }

        public void Toggle ()
        {
            if (_ttsControl.Status == HostStatus.Busy)
            {
                _ttsControl.Stop();
                Program.Logger.System($"CANCELED");
            }
        }

        public async Task TalkAsync(string text)
        {
            if (_ttsControl.Status == HostStatus.NotConnected)
            {
                _ttsControl.Connect();
                Program.Logger.System($"RECONNECTED");
            }


            var CntRetry = 0;
            while (_ttsControl.Status == HostStatus.Busy)
            {
                CntRetry++;
                Program.Logger.Busy($"Delay...{CntRetry}");
                if (this.IsInterrupt) _ttsControl.Stop();
                await Task.Delay(100);
            }


            if (_ttsControl.Status == HostStatus.Busy)
            {

            }
            else
            {
                _ttsControl.Text = text;
                Program.Logger.Play($"{text}");
                _ttsControl.Play();
            }

            await queue.Writer.WriteAsync(text);
        }


        public void Dispose()
        {
            _ttsControl.Stop();
            _ttsControl.Disconnect();
            _ttsControl.TerminateHost();


            Program.Logger.System($"切断");
        }
    }
}