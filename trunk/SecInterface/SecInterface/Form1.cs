using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace SecInterface
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {


        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            CurrentPSIOut.Text = textBox1.Text;
            CurrentRPMOut.Text = textBox1.Text;
            ProsPSIOut.Text = textBox1.Text;
            ProsRPMOut.Text = textBox1.Text;
        }

        private void ProsPSIOut_TextChanged(object sender, EventArgs e)
        {
            ProsPSIOut.BackColor = Color.White;
            ProsRPMOut.BackColor = Color.Yellow;
            ProsRPMOut.Text = Calculator.GetProspectiveRPM(float.Parse(ProsPSIOut.Text)).ToString();
        }

        private void ProsRPMOut_TextChanged(object sender, EventArgs e)
        {
            ProsRPMOut.BackColor = Color.White;
            ProsPSIOut.BackColor = Color.Yellow;
            ProsPSIOut.Text = Calculator.GetProspectivePSI(float.Parse(ProsRPMOut.Text)).ToString();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Chart.Row = 1;
            Chart.Column = 1;
            Chart.Data = textBox2.Text;
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            Chart.Row = 1;
            Chart.Column = 2;
            Chart.Data = textBox3.Text;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            Chart.Row = 2;
            Chart.Column = 1;
            Chart.Data = textBox4.Text;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            Chart.Row = 2;
            Chart.Column = 2;
            Chart.Data = textBox5.Text;
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            Chart.Row = 3;
            Chart.Column = 1;
            Chart.Data = textBox6.Text;
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            Chart.Row = 3;
            Chart.Column = 2;
            Chart.Data = textBox7.Text;
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            Chart.Row = 4;
            Chart.Column = 1;
            Chart.Data = textBox8.Text;
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            Chart.Row = 4;
            Chart.Column = 2;
            Chart.Data = textBox9.Text;
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            Chart.Row = 5;
            Chart.Column = 1;
            Chart.Data = textBox10.Text;
        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {
            Chart.Row = 5;
            Chart.Column = 2;
            Chart.Data = textBox11.Text;
        }
    }
}
