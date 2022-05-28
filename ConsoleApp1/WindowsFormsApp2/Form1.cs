using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.FFMPEG;
using FFMpegSharp;
using FFMpegSharp.FFMPEG;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
private string inputaudiofolderpath = Application.StartupPath + "\\audios";
private string inputimagefolderpath = Application.StartupPath + "\\images";
private string resizedimagefolderpath = Application.StartupPath + "\\resized";
private string outputvideofolderpath = Application.StartupPath + "\\videos";
        public Form1()
        {
            InitializeComponent();
            foreach (FileInfo file in new DirectoryInfo(Application.StartupPath + "\\resized").GetFiles())
            {
                file.Delete();
            }
        }
        private Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }

            return result;
        }
        private void button2_Click(object sender, EventArgs e)
        {

            createVideo(txtvideoname.Text);

        }

        private void createVideo(string finalvideofilename)
        {
            clearfolders(resizedimagefolderpath);
            clearfolders(outputvideofolderpath);
            if (getListOfFiles(inputaudiofolderpath).Length < 1)
            {
                MessageBox.Show("No audio file found");
                return;
            }
            if (getListOfFiles(inputimagefolderpath).Length < 1)
            {
                MessageBox.Show("No image file found");
                return;
            }

            string inputaudiofilename = getListOfFiles(inputaudiofolderpath)[0].FullName;
            FileInfo inputaudio = new FileInfo(inputaudiofilename);
            foreach (FileInfo file in getListOfFiles(inputimagefolderpath))
            {
                var fileguid = Guid.NewGuid();
                Image src = Image.FromFile(file.FullName, true); ;
                Bitmap imageinitial = (Bitmap)src;
                Bitmap image = ResizeBitmap(imageinitial, 512, 512);
                image.Save(resizedimagefolderpath + "\\"+ fileguid + ".png");
            }

            FFMpeg videoencoder = new FFMpeg();
            var outguid = Guid.NewGuid();
            FileInfo[] imagelist = getListOfFiles(resizedimagefolderpath);
            int totalimagesneeded = calculatetotalimage(inputaudiofilename, 10);
            int totalloop = totalimagesneeded / imagelist.Length;
            totalloop=totalloop == 0 ? 1 : totalloop;
            ImageInfo[] slides = new ImageInfo[totalimagesneeded+2];

            for (int j = 0; j < totalloop+1; j++)
            {

                for (int i = 0; i < imagelist.Length; i++)
                {
                    if ((j * imagelist.Length) + i < totalimagesneeded+2)
                    {
                        slides[(j * imagelist.Length) + i] = ImageInfo.FromPath(imagelist[i].FullName);
                    }
                }
            }

            var videoextenstion = finalvideofilename.Split('.')[1];
            var combinedimagevideo= videoencoder.JoinImageSequence(new FileInfo(outputvideofolderpath + "\\" + outguid+"."+ videoextenstion), 0.1, slides);
            videoencoder.ReplaceAudio(combinedimagevideo, inputaudio, new FileInfo(outputvideofolderpath + "\\" + finalvideofilename));
            MessageBox.Show("completed");
        }

        private FileInfo[]  getListOfFiles(string folderpath)
        {
            DirectoryInfo info = new DirectoryInfo(folderpath);

            return info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
        }
        [DllImport("winmm.dll")]
        private static extern uint mciSendString(
            string command,
            StringBuilder returnValue,
            int returnLength,
            IntPtr winHandle);
        private int GetSoundLength(string fileName)
        {
            StringBuilder lengthBuf = new StringBuilder(32);

            mciSendString(string.Format("open \"{0}\" type waveaudio alias wave", fileName), null, 0, IntPtr.Zero);
            mciSendString("status wave length", lengthBuf, lengthBuf.Capacity, IntPtr.Zero);
            mciSendString("close wave", null, 0, IntPtr.Zero);

            int length = 0;
            int.TryParse(lengthBuf.ToString(), out length);

            return length;
        }

        private int calculatetotalimage(string audiofilepath,int secondsbeetweenimages)
        {
            int audiolengthinms = GetSoundLength(audiofilepath);
            int audiolengthinsec =(int) Math.Ceiling((decimal)audiolengthinms / 1000);
            int returnval =(int) Math.Ceiling((decimal)audiolengthinsec / secondsbeetweenimages);
            return returnval;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            clearfolders(inputaudiofolderpath);
            if (openFileDialogaudio.ShowDialog() == DialogResult.OK)
            {
File.Copy(openFileDialogaudio.FileName, inputaudiofolderpath+"\\"+ openFileDialogaudio.SafeFileName);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            clearfolders(inputimagefolderpath);
            openFileDialogimage.Multiselect = true;
            if (openFileDialogimage.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < openFileDialogimage.FileNames.Length; i++)
                {
                    File.Copy(openFileDialogimage.FileNames[i], inputimagefolderpath + "\\" + openFileDialogimage.SafeFileNames[i]);

                }
            }
        }

        private void clearfolders(string folderpath)
        {
            if (!Directory.Exists(folderpath))
            {
                Directory.CreateDirectory(folderpath);
            }
            else
            {
                new DirectoryInfo(folderpath).GetFiles().AsParallel().ForAll((f) => f.Delete());

            }
        }
    }
}
