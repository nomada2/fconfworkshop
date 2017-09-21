﻿using System;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using System.Net;

namespace DataflowPipeline
{
    public class DataflowPipeline
    {
        public static void Start()
        {
            const int bc = 1;
            // Download a book as a string
            var downloadBook = new TransformBlock<string, string>(uri =>
            {
                Console.WriteLine("Downloading the book...");

                return new WebClient().DownloadString(uri);
            }, new ExecutionDataflowBlockOptions() { BoundedCapacity  = bc});


            // splits text into an array of strings.
            var createWordList = new TransformBlock<string, string[]>(text =>
            {
                Console.WriteLine("Creating list of words...");

                // Remove punctuation
                char[] tokens = text.ToArray();
                for (int i = 0; i < tokens.Length; i++)
                {
                    if (!char.IsLetter(tokens[i]))
                        tokens[i] = ' ';
                }
                text = new string(tokens);

                return text.Split(new char[] { ' ' },
                   StringSplitOptions.RemoveEmptyEntries);
            }, new ExecutionDataflowBlockOptions() { BoundedCapacity = bc });

            // Remove short words and return the count
            var filterWordList = new TransformBlock<string[], int>(words =>
            {
                Console.WriteLine("Counting words...");

                var wordList = words.Where(word => word.Length > 3).OrderBy(word => word)
                   .Distinct().ToArray();
                return wordList.Count();
            }, new ExecutionDataflowBlockOptions() { BoundedCapacity = bc });

            var printWordCount = new ActionBlock<int>(wordcount =>
            {
                Console.WriteLine("Found {0} words",
                   wordcount);
            });

            downloadBook.LinkTo(createWordList);
            createWordList.LinkTo(filterWordList);
            filterWordList.LinkTo(printWordCount);

            // For each completion task in the pipeline, create a continuation task
            // that marks the next block in the pipeline as completed.
            // A completed dataflow block processes any buffered elements, but does
            // not accept new elements.
            //

            downloadBook.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)createWordList).Fault(t.Exception);
                else createWordList.Complete();
            });
            createWordList.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)filterWordList).Fault(t.Exception);
                else filterWordList.Complete();
            });
            filterWordList.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)printWordCount).Fault(t.Exception);
                else printWordCount.Complete();
            });

            // Download Origin of Species
            downloadBook.Post("http://www.gutenberg.org/files/2009/2009.txt");
            downloadBook.Post("http://www.gutenberg.org/files/2010/2010.txt");
            downloadBook.Post("http://www.gutenberg.org/files/2011/2011.txt");

            // Mark the head of the pipeline as complete.
            downloadBook.Complete();

            printWordCount.Completion.Wait();

            Console.WriteLine("Finished. Press any key to exit.");
            Console.ReadLine();

        }
    }
}
