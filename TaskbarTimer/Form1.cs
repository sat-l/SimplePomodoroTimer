using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Media;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace TaskbarTimer
{
    public enum ClockState
    {
        Started = 0,
        Completed = 1,
        HalfTime = 2,
        TenMinMark = 3,
    }


    public partial class Form1 : Form
    {
        private BackgroundWorker bgw;

        private static bool IsTimerOn = false;
        private static DateTime endTime;
        private static DateTime halfTime;
        private static DateTime startTime;
        private static int durationInMins;

        private static bool finalClose = false;

        public Form1()
        {
            InitializeComponent();
            AddButtons();
            bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            bgw.DoWork += Bgw_DoWork;
            bgw.ProgressChanged += Bgw_ProgressChanged;
            bgw.RunWorkerAsync();
        }

        private void Bgw_DoWork(object? sender, DoWorkEventArgs e)
        {
            while (true)
            {
                // report progress
                // 1 sec
                Thread.Sleep(1000);
                ClockState state = ClockState.Started;
                if (halfTime != DateTime.MaxValue && halfTime.Subtract(DateTime.UtcNow) <= TimeSpan.Zero)
                {
                    state = ClockState.HalfTime;
                    halfTime = DateTime.MaxValue;
                }
                else if (endTime != DateTime.MaxValue && endTime.Subtract(DateTime.UtcNow) <= TimeSpan.Zero)
                {
                    state = ClockState.Completed;
                }

                bgw.ReportProgress(0, state);
            }
        }

        private void Bgw_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (IsTimerOn)
            {
                var remainingTime = endTime.Subtract(DateTime.UtcNow);
                var state = (ClockState?)e.UserState;

                if (!state.HasValue)
                    return;

                switch (state)
                {
                    case ClockState.Completed:
                        // exit
                        ResetState();
                        this.WindowState = FormWindowState.Normal;
                        SoundPlayer simpleSound = new SoundPlayer(@".\winfantasia.wav");
                        simpleSound.Play();
                        notifyIcon1.BalloonTipTitle = "pomodoro timer";
                        notifyIcon1.BalloonTipText = $"TIME UP!";
                        notifyIcon1.ShowBalloonTip(5);
                        return;
                    case ClockState.HalfTime:
                        simpleSound = new SoundPlayer(@".\bell.wav");
                        simpleSound.Play();
                        break;
                }


                if (remainingTime < TimeSpan.FromMinutes(1))
                {
                    this.Text = remainingTime.ToString("ss") + " sec";
                }
                else
                {
                    this.Text = remainingTime.ToString("mm") + " min";
                }
            }
            else
            {
                this.Text = "Pomodoro timer";
            }
        }

        private List<Button> timerButtons = new List<Button>();

        private void AddButtons()
        {
            var timerVals = new int[] { 5, 10, 15, 25, 55 };
            for (int i = 0; i < 5; i++)
            {
                var btn = new Button();
                btn.Text = String.Format($"{timerVals[i]} mins");
                btn.Tag = timerVals[i];
                btn.Click += Btn_Click;
                timerButtons.Add(btn);
                flpPanel.Controls.Add(btn);

                var menuItem = tstripMenuStart.DropDownItems.Add(btn.Text);
                menuItem.Tag = timerVals[i];
                menuItem.Click += Btn_Click;
            }
        }

        // start timer
        private void Btn_Click(object? sender, EventArgs e)
        {
            int? timerVal = null;
            if (sender is Button)
                timerVal = (int?)(sender as Control)?.Tag;
            else if (sender is ToolStripItem)
                timerVal = (int?)(sender as ToolStripItem)?.Tag;


            if (!timerVal.HasValue)
            {
                return;
            }

            startTime = DateTime.UtcNow;
            endTime = startTime.AddMinutes(timerVal.Value);
            durationInMins = (int)(endTime - startTime).TotalMinutes;
            halfTime = startTime.AddMinutes(durationInMins / 2);

            foreach (var btn in timerButtons)
            {
                btn.Enabled = false;
            }
            tstripMenuStart.Enabled = false;
            IsTimerOn = true;

            this.WindowState = FormWindowState.Minimized;
            SoundPlayer simpleSound = new SoundPlayer(@".\bell.wav");
            simpleSound.Play();
            notifyIcon1.BalloonTipTitle = "pomodoro timer";
            notifyIcon1.BalloonTipText = $"timer set for {timerVal.Value} minutes";
            notifyIcon1.ShowBalloonTip(5);
        }



        private void btnReset_Click(object sender, EventArgs e)
        {
            ResetState();
        }

        private void ResetState()
        {
            IsTimerOn = false;

            foreach (var btn in timerButtons)
            {
                btn.Enabled = true;
            }
            tstripMenuStart.Enabled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            e.Cancel = !finalClose;
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetState();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            finalClose = true;
            this.Close();
        }
    }
}