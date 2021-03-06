﻿using System.Windows;
using System.Windows.Media;
using SitePlugin;
using System.IO;
using System.Text;
using System.Xml;
using System;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Common;
namespace MultiCommentViewer.Test
{

    public class IOTest : IIo
    {
        public string ReadFile(string path)
        {
            var totalWaitTime = 0;
            string s = null;
            if (!File.Exists(path))
            {
                return s;
            }
            while (totalWaitTime < 5000)
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    using (var sr = new StreamReader(fs))
                    {
                        s = sr.ReadToEnd();
                    }
                    break;
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex.Message);
                    var waitTime = GetRandomLong(10, 500);
                    Debug.WriteLine($"読み込みに失敗したため{waitTime}ミリ秒待ちます");
                    Thread.Sleep(waitTime);
                    totalWaitTime += waitTime;
                }
            }
            return s;
        }
        public async Task<string> ReadFileAsync(string path)
        {
            var totalWaitTime = 0;
            string s = null;
            while (totalWaitTime < 5000)
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    using (var sr = new StreamReader(fs))
                    {
                        s = await sr.ReadToEndAsync();
                    }
                    break;
                }
                catch(FileNotFoundException)
                {
                    break;
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex.Message);
                    var waitTime = GetRandomLong(10, 500);
                    Debug.WriteLine($"読み込みに失敗したため{waitTime}ミリ秒待ちます");
                    await Task.Delay(waitTime);
                    totalWaitTime += waitTime;
                }
            }
            return s;
        }
        public void WriteFile(string path, string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);

            var totalWaitTime = 0;
            while (totalWaitTime < 5000)
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                    {
                        fs.Write(bytes, 0, bytes.Length);
                    }
                    break;
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex.Message);
                    var waitTime = GetRandomLong(10, 500);
                    Debug.WriteLine($"書き込みに失敗したため{waitTime}ミリ秒待ちます");
                    Thread.Sleep(waitTime);
                    totalWaitTime += waitTime;
                }
            }
        }
        public async Task WriteFileAsync(string path, string s)
        {
            await Task.Yield();
            var bytes = Encoding.UTF8.GetBytes(s);

            var totalWaitTime = 0;
            while (totalWaitTime < 5000)
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                    {
                        fs.Write(bytes, 0, bytes.Length);
                    }
                    break;
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex.Message);
                    var waitTime = GetRandomLong(10, 500);
                    Debug.WriteLine($"書き込みに失敗したため{waitTime}ミリ秒待ちます");
                    await Task.Delay(waitTime);
                    totalWaitTime += waitTime;
                }
            }
        }
        protected virtual int GetRandomLong(int min, int max)
        {
            byte[] bs = new byte[sizeof(int)];
            System.Security.Cryptography.RNGCryptoServiceProvider rng =
                new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bs);

            int i = System.BitConverter.ToInt32(bs, 0);
            return Math.Abs(i % max) + min;
        }
    }
    public class OptionsLoaderTest:IOptionsSerializer
    {
        public IOptions Load(string path, IIo io)
        {
            var options = new DynamicOptionsTest();
            var s = io.ReadFile(path);
            options.Deserialize(s);
            return options;
        }
        public async Task<IOptions> LoadAsync(string path, IIo io)
        {
            var options = new DynamicOptionsTest();
            var s = await io.ReadFileAsync(path);
            options.Deserialize(s);
            return options;
        }
        public async Task WriteAsync(string path, IIo io, IOptions options)
        {
            var s = "";
            if(options is DynamicOptionsTest dynamic)
            {
                s = dynamic.Serialize();
            }
            await io.WriteFileAsync(path, s);
        }
    }

}
