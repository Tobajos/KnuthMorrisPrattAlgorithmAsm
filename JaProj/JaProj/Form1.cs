using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using KMPLibrary;

namespace JaProj
{
    public partial class App : Form
    {
        private string textFromFile = string.Empty;
        private KmpLibrary kmpLibrary = new KmpLibrary();
        public App()
        {
            InitializeComponent();
        }
        private int GetSelectedThreadCount()
        {
            int numberOfThreads = (int)numericUpDown1.Value;
            return numberOfThreads;
        }

       private async void button2_Click(object sender, EventArgs e)
        {
            string pattern = textBox1.Text;

            if (!string.IsNullOrEmpty(textFromFile) && !string.IsNullOrEmpty(pattern))
            {
                textBox2.Text = string.Empty;

                kmpLibrary.SelectedThreadCount = GetSelectedThreadCount();

                await Task.Run(async () => await kmpLibrary.KMPSearchAsync(pattern, textFromFile, UpdateUIWithSearchResults));
            }
            else
            {
                textBox3.Text = "Please upload the file and input the pattern before starting the search.";
            }
        }   

        private void UpdateUIWithSearchResults(int foundCount, TimeSpan timeElapsed, List<int> foundIndexes)
        {
            foundIndexes.Sort(); 

            textBox3.Text = $"Number of Found Patterns: {foundCount}\n";
            textBox2.Text = "Indexes of Found Pattern: ";

            foreach (int index in foundIndexes)
            {
                textBox2.Text += index + ", ";
            }

            textBox4.Text = "Execution Time: " + timeElapsed.TotalMilliseconds + " ms";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "Text Files|*.txt",
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader reader = new StreamReader(openFileDialog.FileName))
                {
                    textFromFile = reader.ReadToEnd();
                    textBox3.Text = "File loaded.";
                }
            }
        }

        [DllImport(@"C:\Users\Tobiasz\Desktop\Nowy folder\JaProj\x64\Debug\JAAsm.dll")]
        public static extern int MyProc1(IntPtr text, int textLength, IntPtr pattern, int patternLength, IntPtr lps, IntPtr foundIndexes);

        private async void ResultAssembly(string myText, string myPattern)
        {
            byte[] textBytes = Encoding.ASCII.GetBytes(myText);
            byte[] patternBytes = Encoding.ASCII.GetBytes(myPattern);

            int textLength = textBytes.Length;
            int patternLength = patternBytes.Length;

            IntPtr textPtr = Marshal.AllocHGlobal(textLength);
            Marshal.Copy(textBytes, 0, textPtr, textLength);

            IntPtr patternPtr = Marshal.AllocHGlobal(patternLength);
            Marshal.Copy(patternBytes, 0, patternPtr, patternLength);

            int[] lpsArray = kmpLibrary.ComputeLPSArray(myPattern);

            byte[] lpsBytes = new byte[lpsArray.Length * sizeof(int)];
            Buffer.BlockCopy(lpsArray, 0, lpsBytes, 0, lpsBytes.Length);

            IntPtr lpsPtr = Marshal.AllocHGlobal(lpsBytes.Length);
            Marshal.Copy(lpsBytes, 0, lpsPtr, lpsBytes.Length);

            int[] foundIndexesArray = new int[1000000];
            IntPtr foundIndexesPtr = Marshal.UnsafeAddrOfPinnedArrayElement(foundIndexesArray, 0);

            Stopwatch stopwatch = new Stopwatch();
            int userSelectedThreads = GetSelectedThreadCount();

            
            int defaultThreads = Environment.ProcessorCount;
            int threads = userSelectedThreads > 0 ? userSelectedThreads : defaultThreads;

            stopwatch.Start();

            await Task.Run(() =>
            {
                Parallel.For(0, threads, t =>
                { 
                    MyProc1(textPtr, textLength, patternPtr, patternLength, lpsPtr, foundIndexesPtr);
                                        
                });
            });

            stopwatch.Stop();

            List<int> foundIndexes = new List<int>();
            for (int i = 0; i < foundIndexesArray.Length; i++)
            {
                if (i == 0 && foundIndexesArray[i] == 0)
                {
                    foundIndexes.Add(foundIndexesArray[i]);
                }
                else if (foundIndexesArray[i] != 0)
                {
                    foundIndexes.Add(foundIndexesArray[i]);
                }
            }

            textBox2.Text = "Indexes of Found Pattern: " + string.Join(", ", foundIndexes);
            textBox3.Text = $"Number of Found Patterns: {foundIndexes.Count}\n"; // Używam Count zamiast result, ponieważ wynik jest przetwarzany równolegle
            TimeSpan timeElapsed = stopwatch.Elapsed;
            textBox4.Text = "Execution Time: " + timeElapsed.TotalMilliseconds + " ms";

            Marshal.FreeHGlobal(textPtr);
            Marshal.FreeHGlobal(patternPtr);
            Marshal.FreeHGlobal(lpsPtr);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
            string pattern = textBox1.Text;

            if (!string.IsNullOrEmpty(textFromFile) && !string.IsNullOrEmpty(pattern))
            {
                textBox2.Text = string.Empty;

              
                ResultAssembly(textFromFile, textBox1.Text);
            }
            else
            {
                textBox3.Text = "Please upload the file and input the pattern before starting the search.";
            }
        }

    }
}