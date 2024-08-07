﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge;
namespace BioCore
{
    public partial class ColorTool : Form
    {
        private ColorS colors = new ColorS(65535, 65535, 65535);
        private int bitsPerPx = 16;
        /* A property. */
        public ColorS Color
        {
            get
            {
                return colors;
            }
            set
            {
                colors = value;
            }
        }

        /// It updates the GUI
        public void UpdateGUI()
        {
            colors = new ColorS((ushort)redBox.Value, (ushort)greenBox.Value, (ushort)blueBox.Value);
            colorPanel.BackColor = System.Drawing.Color.FromArgb(colors.R / ushort.MaxValue,colors.G / ushort.MaxValue,colors.B / ushort.MaxValue);
            if (rBar.Value != redBox.Value)
                redBox.Value = rBar.Value;
            if (gBar.Value != greenBox.Value)
                greenBox.Value = gBar.Value;
            if (bBar.Value != blueBox.Value)
                blueBox.Value = bBar.Value;
        }
        
        /* A constructor. */
        public ColorTool()
        {
            InitializeComponent();
            UpdateGUI();
        }
        /* A constructor. */
        public ColorTool(ColorS col, int bitPerPixel)
        {
            InitializeComponent();
            this.bitsPerPx = bitPerPixel;
            if(bitsPerPx == 8)
            {
                rBar.Maximum = 255;
                gBar.Maximum = 255;
                bBar.Maximum = 255;
                redBox.Maximum = 255;
                greenBox.Maximum = 255;
                blueBox.Maximum = 255;
            }
            if (rBar.Maximum <= col.R)
                rBar.Value = rBar.Maximum;
            if (gBar.Maximum <= col.G)
                gBar.Value = gBar.Maximum;
            if (bBar.Maximum <= col.B)
                bBar.Value = bBar.Maximum;
            UpdateGUI();
        }

        private void redBox_ValueChanged(object sender, EventArgs e)
        {
            UpdateGUI();
        }

        private void greenBox_ValueChanged(object sender, EventArgs e)
        {
            UpdateGUI();
        }

        private void blueBox_ValueChanged(object sender, EventArgs e)
        {
            UpdateGUI();
        }

        private void rEnbaled_CheckedChanged(object sender, EventArgs e)
        {
            Tools.rEnabled = rEnbaled.Checked;
        }

        private void gEnabled_CheckedChanged(object sender, EventArgs e)
        {
            Tools.gEnabled = gEnabled.Checked;
        }

        private void bEnabled_CheckedChanged(object sender, EventArgs e)
        {
            Tools.bEnabled = bEnabled.Checked;
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelBut_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void rBar_Scroll(object sender, EventArgs e)
        {
            UpdateGUI();
        }

        private void gBar_Scroll(object sender, EventArgs e)
        {
            UpdateGUI();
        }

        private void bBar_Scroll(object sender, EventArgs e)
        {
            UpdateGUI();
        }
    }
}
