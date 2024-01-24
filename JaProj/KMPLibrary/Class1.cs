using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KMPLibrary
{
    public class KmpLibrary
    {
        public int SelectedThreadCount { get; set; } = 1;
        public async Task KMPSearchAsync(string pattern, string text, Action<int, TimeSpan, List<int>> resultCallback)
        {
            int patternLength = pattern.Length;
            int textLength = text.Length;

            int[] lps = ComputeLPSArray(pattern);

            int foundCount = 0;
            List<int> foundIndexes = new List<int>();
            bool[] alreadyFound = new bool[textLength];

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int threads = SelectedThreadCount;
            threads = Environment.ProcessorCount;

            await Task.Run(() =>
            {
                Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = threads }, t =>
                {
                    for (int i = t; i < textLength; i += threads)
                    {
                        int j = 0;
                        int localI = i; // Use a local variable to store the current index

                        while (j < patternLength && localI < textLength)
                        {
                            if (pattern[j] == text[localI])
                            {
                                j++;
                                localI++;
                            }
                            else
                            {
                                if (j != 0)
                                    j = lps[j - 1];
                                else
                                    localI++;
                            }
                        }

                        if (j == patternLength)
                        {
                            lock (foundIndexes)
                            {
                                if (!alreadyFound[localI - patternLength])
                                {
                                    Interlocked.Increment(ref foundCount);
                                    foundIndexes.Add(localI - patternLength);
                                    alreadyFound[localI - patternLength] = true;
                                }
                            }
                        }
                    }
                });
            });

            stopwatch.Stop();
            TimeSpan timeElapsed = stopwatch.Elapsed;

            resultCallback(foundCount, timeElapsed, foundIndexes);
        }



        public int[] ComputeLPSArray(string pattern)
        {
            int patternLength = pattern.Length;
            int[] lps = new int[patternLength];
            int len = 0;
            int i = 1;

            lps[0] = 0;

            while (i < patternLength)
            {
                if (pattern[i] == pattern[len])
                {
                    len++;
                    lps[i] = len;
                    i++;
                }
                else
                {
                    if (len != 0)
                    {
                        len = lps[len - 1];
                    }
                    else
                    {
                        lps[i] = 0;
                        i++;
                    }
                }
            }

            return lps;
        }

        private int GetSelectedThreadCount()
        {
            return SelectedThreadCount;
        }
    }
}
