using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace KmpDDL
{
    public class KmpLibrary
    {
        public async Task KMPSearchAsync(string pattern, string text, Action<int, TimeSpan, List<int>> resultCallback)
        {
            int patternLength = pattern.Length;
            int textLength = text.Length;

            int[] lps = ComputeLPSArray(pattern);

            int foundCount = 0;
            List<int> foundIndexes = new List<int>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int threads = GetSelectedThreadCount();
            threads = Environment.ProcessorCount;

            await Task.Run(() =>
            {
                Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = threads }, t =>
                {
                    int start = t * (textLength / threads);
                    int end = (t == threads - 1) ? textLength : (t + 1) * (textLength / threads);

                    for (int i = start; i < end;)
                    {
                        int j = 0;

                        while (j < patternLength && i < end)
                        {
                            if (pattern[j] == text[i])
                            {
                                j++;
                                i++;
                            }
                            else
                            {
                                if (j != 0)
                                    j = lps[j - 1];
                                else
                                    i++;
                            }
                        }

                        if (j == patternLength)
                        {
                            Interlocked.Increment(ref foundCount);
                            lock (foundIndexes)
                            {
                                foundIndexes.Add(i - j);
                            }
                        }
                    }
                });
            });

            stopwatch.Stop();
            TimeSpan timeElapsed = stopwatch.Elapsed;

            resultCallback(foundCount, timeElapsed, foundIndexes);
        }

        private int[] ComputeLPSArray(string pattern)
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
            // Możesz dostosować tę metodę do swoich potrzeb
            return 1;
        }
    }
}
