namespace BulkSSTCLI
{
    using Microsoft.CognitiveServices.Speech;
    using Microsoft.CognitiveServices.Speech.Audio;
    using PowerArgs;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    [TabCompletion(HistoryToSave = 10)]
    [ArgExample("BulkSSTCLI.exe -k {subscriptionapikey} -r {region} -p c:\\samples", "using arguments")]
    public class SpeechToText
    {
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("Speech To Text API Key")]
        [ArgShortcut("-k")]
        public string SubscriptionKey { get; set; }

        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("Region")]
        [ArgShortcut("-r")]
        public string Region { get; set; }

        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("Directory path (parent containing subfolders) where wav samples are stored")]
        [ArgShortcut("-p")]
        public string SourcePath { get; set; }

        [HelpHook]
        public bool Help { get; set; }

        private SpeechConfig config;

        public async Task Main()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            config = SpeechConfig.FromSubscription(SubscriptionKey, Region);
            DirectoryInfo root = new DirectoryInfo(SourcePath);
            await IterateFolders(root);

            stopwatch.Stop();
            Console.WriteLine($"Total time: {stopwatch.Elapsed}");
        }

        private async Task IterateFolders(DirectoryInfo parentFolder)
        {
            await IterateFiles(parentFolder);
            foreach (var folder in parentFolder.GetDirectories())
            {
                await IterateFolders(folder);
            }
        }

        private async Task IterateFiles(DirectoryInfo parentFolder)
        {
            Console.WriteLine($"Scanning files within: {parentFolder.FullName}");
            string outputFile = $"{parentFolder.FullName}\\{parentFolder.Name}.txt";

            foreach (var file in parentFolder.GetFiles("*.wav"))
            {
                Console.WriteLine($"Processing {file.Name}");
                StringBuilder sstOutput = new StringBuilder();
                var stopRecognition = new TaskCompletionSource<int>();

                // Creates a speech recognizer using file as audio input.
                using (var audioInput = AudioConfig.FromWavFileInput(file.FullName))
                {
                    using (var recognizer = new SpeechRecognizer(config, audioInput))
                    {
                        //recognizer.Recognizing += (s, e) =>
                        //{
                        //    Console.Write($"{e.Result.Text}");
                        //};

                        recognizer.Canceled += (s, e) =>
                        {
                            if (e.Reason == CancellationReason.Error)
                            {
                                Console.WriteLine($"Error: {e.ErrorDetails}");
                            }
                            stopRecognition.TrySetResult(0);
                        };

                        recognizer.Recognized += (s, e) =>
                        {
                            if (e.Result.Reason == ResultReason.RecognizedSpeech)
                            {
                                sstOutput.Append(e.Result.Text);
                            }
                        };

                        recognizer.SessionStopped += (s, e) =>
                        {
                            Console.WriteLine($"Appending output to {outputFile}");
                            File.AppendAllText(outputFile, $"{file.Name.Replace(file.Extension, "")}\t{sstOutput.ToString()}{Environment.NewLine}");
                            stopRecognition.TrySetResult(0);
                        };

                        // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                        // Waits for completion & end
                        Task.WaitAny(new[] { stopRecognition.Task });
                        await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
