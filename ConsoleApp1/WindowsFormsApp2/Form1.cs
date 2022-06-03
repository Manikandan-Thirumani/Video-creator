using FFMpegSharp;
using FFMpegSharp.FFMPEG;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form, IDisposable
    {
private string inputaudiofolderpath = Application.StartupPath + "\\audios";
private string inputimagefolderpath = Application.StartupPath + "\\images";
private string resizedimagefolderpath = Application.StartupPath + "\\resized";
private string outputvideofolderpath = Application.StartupPath + "\\videos";
        public Form1()
        {
            InitializeComponent();
            //foreach (FileInfo file in new DirectoryInfo(Application.StartupPath + "\\resized").GetFiles())
            //{
            //    file.Delete();
            //}
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

            createVideo(txtvideoname.Text,0.2);
        }

        private void createVideo(string finalvideofilename, double framerate)
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

            if (string.IsNullOrWhiteSpace(finalvideofilename))
            {
                MessageBox.Show("please provide video file name");
                return;
            }

            string inputaudiofilename = getListOfFiles(inputaudiofolderpath)[0].FullName;
            FileInfo inputaudio = new FileInfo(inputaudiofilename);

            foreach (FileInfo file in getListOfFiles(inputimagefolderpath))
            {
                var fileguid = Guid.NewGuid();
                Image src = Image.FromFile(file.FullName, true);
                ;
                Bitmap imageinitial = (Bitmap)src;
                Bitmap image = ResizeBitmap(imageinitial, 512, 512);
                image.Save(resizedimagefolderpath + "\\" + fileguid + ".png");
                image.Dispose();
                src.Dispose();
            }

            var outguid = Guid.NewGuid();
            FileInfo[] imagelist = getListOfFiles(resizedimagefolderpath);
            int totalimagesneeded = calculatetotalimage(inputaudiofilename, (int)(1 / framerate));
            int totalloop = totalimagesneeded / imagelist.Length;
            totalloop = totalloop == 0 ? 1 : totalloop;
            ImageInfo[] slides = new ImageInfo[totalimagesneeded + 2];

            for (int j = 0; j < totalloop + 1; j++)
            {

                for (int i = 0; i < imagelist.Length; i++)
                {
                    if ((j * imagelist.Length) + i < totalimagesneeded + 2)
                    {
                        slides[(j * imagelist.Length) + i] = ImageInfo.FromPath(imagelist[i].FullName);
                    }
                }
            }

            var videoextenstion = finalvideofilename.Split('.')[1];
            using (var videoencoder = new FFMpeg())
            {
                var combinedimagevideo = videoencoder.JoinImageSequence(
                    new FileInfo(outputvideofolderpath + "\\" + outguid + "." + videoextenstion), framerate, slides);
                videoencoder.ReplaceAudio(combinedimagevideo, inputaudio,
                    new FileInfo(outputvideofolderpath + "\\" + finalvideofilename));
                combinedimagevideo = null;
            }
            MessageBox.Show("Video created successfully");

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
            TagLib.File file = TagLib.File.Create(audiofilepath);
            int audiolengthinsec = (int)file.Properties.Duration.TotalSeconds;
            int returnval =(int) Math.Ceiling((decimal)audiolengthinsec / secondsbeetweenimages);
            return returnval;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            clearfolders(inputaudiofolderpath);

            if (openFileDialogaudio.ShowDialog() == DialogResult.OK)
            {
                File.Copy(openFileDialogaudio.FileName, inputaudiofolderpath + "\\" + openFileDialogaudio.SafeFileName);
                lblaudiomsg.Text = "1 audiofile uploaded";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            clearfolders(inputimagefolderpath);
            openFileDialogimage.Multiselect = true;
            if (openFileDialogimage.ShowDialog() == DialogResult.OK)
            {
                if (openFileDialogimage.FileNames.Length == 1)
                {
                    MessageBox.Show("Please select more than 1 image");
                    return;
                }
                for (int i = 0; i < openFileDialogimage.FileNames.Length; i++)
                {
                    File.Copy(openFileDialogimage.FileNames[i], inputimagefolderpath + "\\" + openFileDialogimage.SafeFileNames[i]);

                }

                lblimagefilesmsg.Text = openFileDialogimage.FileNames.Length.ToString() + " image files uploaded";
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
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                new DirectoryInfo(folderpath).GetFiles().AsParallel().ForAll((f) => f.Delete());

            }
        }
    }
}
