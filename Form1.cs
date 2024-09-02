using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gallery
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckFolder();
            BrowseForImages();
            panel3.Visible = false;
            textBox1.Text = subFolderPath;
        }

        private const int cGrip = 16;
        private const int cCaption = 32;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {
                Point pos = new Point(m.LParam.ToInt32());
                pos = this.PointToClient(pos);
                if (pos.Y < cCaption)
                {
                    m.Result = (IntPtr)2;
                    return;
                }

                if (pos.X >= this.ClientSize.Width - cGrip && pos.Y >= this.ClientSize.Height - cGrip)
                {
                    m.Result = (IntPtr)17;
                    return;
                }
            }
            base.WndProc(ref m);
        }

        List<string> FoundImages;
        string subFolderPath;
        bool subFolderbool = false;
        List<PictureBox> pictureBoxList = new List<PictureBox>();
        string GlobalImagePath = "none";
        PictureBox GlobalPictureBox;

        private void CheckFolder()
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string subFolderName = "ImageGallery";

            subFolderPath = Path.Combine(appDirectory, subFolderName);

            if (!Directory.Exists(subFolderPath))
            {
                Directory.CreateDirectory(subFolderPath);
                Console.WriteLine($"Folder '{subFolderName}' został utworzony w {subFolderPath}");
                subFolderbool = true;
            }
            else
            {
                Console.WriteLine($"Folder '{subFolderName}' już istnieje w {subFolderPath}");
                subFolderbool = true;
            }
        }

        private void BrowseForImages()
        {
            if (subFolderbool)
            {
                label1.Text = "Forlder found 1";
                FoundImages = Directory.GetFiles(subFolderPath, "*.*")
                    .Where(file => file.ToLower().EndsWith("jpg") || file.ToLower().EndsWith("gif")
                                 || file.ToLower().EndsWith("png") || file.ToLower().EndsWith("bmp"))
                    .ToList();
                label1.Text = "Forlder found 2";
                LoadImages();
            }
            else
            {
                label1.Text = "Forlder not found";
            }
        }

        

        private void LoadImages()
        {
            if (FoundImages.Count > 0)
            {
                Panel imagePanel = panel2;
                imagePanel.AutoScroll = true;
                imagePanel.VerticalScroll.Visible = false;
                imagePanel.HorizontalScroll.Visible = false;

                foreach(PictureBox Oldimagebox in imagePanel.Controls)
                {
                    imagePanel.Controls.Remove(Oldimagebox);
                    pictureBoxList.Remove(Oldimagebox);
                    pictureBoxList.Clear();
                }

                int counter = 0;
                int x = 50;
                int y = 10;
                int margin = 15;

                foreach (string imagePath in FoundImages)
                {
                    PictureBox imagebox = new PictureBox();
                    imagebox.SizeMode = PictureBoxSizeMode.CenterImage;
                    imagebox.Size = new Size(155, 155);
                    imagebox.Location = new Point(x, y);
                    imagebox.Paint += (s, e) => PaintPictureBox(s, e, imagePath);

                    pictureBoxList.Add(imagebox);
                    imagePanel.Controls.Add(imagebox);

                    x += imagebox.Width + margin;
                    if (x + imagebox.Width + margin > imagePanel.Width)
                    {
                        x = 50;
                        y += imagebox.Height + margin;
                    }

                    counter++;
                    imagebox.BringToFront();
                    
                    imagebox.Tag = imagePath;
                    imagebox.Click += FormMouseDown;
                }

                label1.Text = $"{counter} images loaded";
            }
            else
            {
                label1.Text = "No images found";
            }
        }

        private void PaintPictureBox(object sender, PaintEventArgs e, string imagePath)
        {
            PictureBox pb = sender as PictureBox;
            if (pb != null)
            {
                if (!File.Exists(imagePath))
                {
                    panel2.Controls.Remove(pb);
                    pictureBoxList.Remove(pb);
                    pb.Dispose();
                    UpdatePictureBoxLocation();
                    return;
                }

                Image img = Image.FromFile(imagePath);
                Rectangle srcRect;
                Rectangle destRect = new Rectangle(0, 0, pb.Width, pb.Height);

                float imgAspect = (float)img.Width / img.Height;
                float pbAspect = (float)pb.Width / pb.Height;

                if (imgAspect > pbAspect)
                {
                    int scaledWidth = (int)(img.Height * pbAspect);
                    srcRect = new Rectangle((img.Width - scaledWidth) / 2, 0, scaledWidth, img.Height);
                }
                else
                {
                    int scaledHeight = (int)(img.Width / pbAspect);
                    srcRect = new Rectangle(0, (img.Height - scaledHeight) / 2, img.Width, scaledHeight);
                }

                e.Graphics.DrawImage(img, destRect, srcRect, GraphicsUnit.Pixel);
                img.Dispose();
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        public void FormMouseDown(object sender, EventArgs e)
        {
            PictureBox currentBox = (PictureBox)sender;
            string SelectedImagePath = currentBox.Tag.ToString();

            if (!File.Exists(SelectedImagePath))
            {
                panel2.Controls.Remove(currentBox);
                pictureBoxList.Remove(currentBox);
                currentBox.Dispose();
                UpdatePictureBoxLocation();
                return;
            }

            ShowFullImage(SelectedImagePath);
            GlobalPictureBox = currentBox;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void ShowFullImage(string SelectedImagePath)
        {
            DisposeCurrentImage();
            FullPictureBox.Image = Image.FromFile(SelectedImagePath);
            FullPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            panel3.Visible = true;
            GlobalImagePath = SelectedImagePath;
            string fileName = Path.GetFileName(SelectedImagePath);
            label2.Text = fileName;
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            panel3.Visible = false;
            DisposeCurrentImage();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void DisposeCurrentImage()
        {
            if (FullPictureBox.Image != null)
            {
                FullPictureBox.Image.Dispose();
                FullPictureBox.Image = null;
            }
        }

        private void DeleteButton2_Click(object sender, EventArgs e)
        {
            pictureBoxList.Remove(GlobalPictureBox);
            GlobalPictureBox.Dispose();
            DisposeCurrentImage();
            File.Delete(GlobalImagePath);
            panel3.Visible = false;
            UpdatePictureBoxLocation();
        }

        private void UpdatePictureBoxLocation()
        {
            int counter = 0;
            int x = 50;
            int y = 10;
            int margin = 15;

            foreach (PictureBox imagebox in pictureBoxList)
            {

                imagebox.Location = new Point(x, y - panel2.VerticalScroll.Value);
                x += imagebox.Width + margin;
                if (x + imagebox.Width + margin > panel2.Width)
                {
                    x = 50;
                    y += imagebox.Height + margin;
                }
                counter++;
            }
            label1.Text = $"{counter} images loaded.";
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            AddingImage();
            UpdatePictureBoxLocation();
        }

        private void AddingImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Image Files(*.PNG;*.JPG;*.GIF)|*.PNG;*.JPG;*.GIF";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                int x = 50;
                int y = 10;
                int margin = 15;

                foreach(string imagePath in openFileDialog.FileNames)
                {
                    //PictureBox imagebox = new PictureBox();
                    //imagebox.SizeMode = PictureBoxSizeMode.CenterImage;
                    //imagebox.Size = new Size(155, 155);
                    //imagebox.Location = new Point(x, y);
                    //
                    //
                    //pictureBoxList.Add(imagebox);
                    //panel2.Controls.Add(imagebox);
                    //
                    //x += imagebox.Width + margin;
                    //if (x + imagebox.Width + margin > panel2.Width)
                    //{
                    //    x = 50;
                    //    y += imagebox.Height + margin;
                    //}

                    string fileName = Path.GetFileName(imagePath);
                    string savePath = Path.Combine(subFolderPath, fileName);
                    Image img = Image.FromFile(imagePath);
                    img.Save(savePath);
                    img.Dispose();

                    //imagebox.Paint += (s, e) => PaintPictureBox(s, e, savePath);
                    //imagebox.BringToFront();
                    //imagebox.Tag = savePath;
                    //imagebox.Click += FormMouseDown;
                }
                BrowseForImages();
            }
        }
    }
}
