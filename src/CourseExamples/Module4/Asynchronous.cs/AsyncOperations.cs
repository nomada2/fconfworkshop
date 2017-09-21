﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Functional.Async;

namespace ConsoleApplication1
{
    class AsyncOperations
    {
        void ReadFileBlocking(string filePath, Action<byte[]> process)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open,
                                          FileAccess.Read, FileShare.Read))
            {
                byte[] buffer = new byte[fileStream.Length];
                int bytesRead = fileStream.Read(buffer, 0, buffer.Length);
                process(buffer);
            }
        }


        //  Read from the file system asynchronously
        IAsyncResult ReadFileNoBlocking(string filePath, Action<byte[]> process)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open,
                                    FileAccess.Read, FileShare.Read, 0x1000,
                                                     FileOptions.Asynchronous))
            {
                byte[] buffer = new byte[fileStream.Length];
                var state = Tuple.Create(buffer, fileStream, process);
                return fileStream.BeginRead(buffer, 0, buffer.Length,
                                              EndReadCallback, state);
            }
        }

        void EndReadCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as Tuple<byte[], FileStream, Action<byte[]>>;
            using (state.Item2) state.Item2.EndRead(ar);
            state.Item3(state.Item1);
        }


        async void ReadFileNoBlockingAsync(string filePath, Action<byte[]> process)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open,
                                            FileAccess.Read, FileShare.Read, 0x1000,
                                            FileOptions.Asynchronous))
            {
                byte[] buffer = new byte[fileStream.Length];
                int bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
                await Task.Run(async () => process(buffer));
            }
        }


        private Func<string, Task<byte[]>> downloadSiteIcon = async domain =>
        {
            var response = await new
                HttpClient().GetAsync($"http://{domain}/favicon.ico");
            return await response.Content.ReadAsByteArrayAsync();
        };



        //  Download an image(icon) from the network asynchronously
        async Task DownloadIconAsync(string domain, string fileDestination)
        {
            using (FileStream stream = new FileStream(fileDestination,
                            FileMode.Create, FileAccess.Write,
                            FileShare.Write, 0x1000, FileOptions.Asynchronous))
                await new HttpClient()
                    .GetAsync($"http://{domain}/favicon.ico")
                    .Bind(async content => await
                        content.Content.ReadAsByteArrayAsync())
                    .Map(async bytes => await stream.WriteAsync(bytes, 0,
                                                                bytes.Length));
        }

        async Task DownloadIconAsyncLINQ(string domain, string fileDestination)
        {
            using (FileStream stream = new FileStream(fileDestination,
                            FileMode.Create, FileAccess.Write, FileShare.Write,
                            0x1000, FileOptions.Asynchronous))
                await (from response in new HttpClient()
                                            .GetAsync($"http://{domain}/favicon.ico")
                       from bytes in response.Content.ReadAsByteArrayAsync()
                       select stream.WriteAsync(bytes, 0, bytes.Length));
        }

        void CooperativeCancellation()
        {
            CancellationTokenSource ctsOne = new CancellationTokenSource(); // #A
            CancellationTokenSource ctsTwo = new CancellationTokenSource();// #A
            CancellationTokenSource ctsComposite = CancellationTokenSource.CreateLinkedTokenSource(ctsOne.Token, ctsTwo.Token); // #B

            CancellationToken ctsCompositeToken = ctsComposite.Token;

            Task.Factory.StartNew(async () =>
            {
                var webClient = new WebClient();
                ctsCompositeToken.Register(() => webClient.CancelAsync());
                await webClient.DownloadStringTaskAsync("http://www.manning.com");
            }, ctsComposite.Token);

        }
    }
}
