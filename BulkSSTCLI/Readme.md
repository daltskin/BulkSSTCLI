# Bulk Speech to Text CLI - with output support for Custom Voice Font Training
Cross platform CLI to that uses Azure Cognitive Services Speech Service to iterate a directory of audio samples (wav files) and produce a single text file (1 per directory) of all detected speech.  The transcript file produced is in the format required for training a Custom Voice Font.

To learn more about Microsoft Cognitive Speech Service, please see here: https://docs.microsoft.com/en-us/azure/cognitive-services/Speech-Service/ and to learn more about Microsoft Cognitive Custom Voice see here: https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/how-to-customize-voice-font

## Speech To Text API Key
To retrieve your Speech To Text API key start here: https://azure.microsoft.com/en-gb/try/cognitive-services/

## CLI Arguments

| Argument name | shortcut | example |
|----|----|----|
| BulkSSTCLI | -k | asdfasdfasdfsaf |
| Region | -r | Norteurope | 
| SourcePath | -p | c:\samples

## Usage

### Run SST and generate text file output
The CLI will recursively scan all sub-directories.  However, if you are generating transcripts specifically for Custom Voice Font, put all the samples in a single directory to run the CLI:

```
BulkSSTCLI.exe -k *yourspeechapikey* -r northeurope -p c:\samples
```

### Example output:

Assuming you have 3 wav files in the `c:\samples` directory

* 0000000001.wav
* 0000000002.wav
* 0000000003.wav

A new file `samples.txt` will be created `c:\samples\samples.txt` containing the recognised speech from each wav file eg:

```
0000000001	Some recognised speech to text from file 0000000001.wav
0000000002	Some recognised speech to text from file 0000000002.wav
0000000003	Some recognised speech to text from file 0000000003.wav
```

The format of the text output is ready for use with [Custom Voice Font Training](https://docs.microsoft.com/en-us/azure/cognitive-services/Speech-Service/how-to-customize-voice-font)

Note, you'll need to ensure your samples are split into separate audio files (each maximum of 60 seconds long), numerically named.  You can a split a large audio file easily using [ffmpeg](https://www.ffmpeg.org) eg:

```
ffmpeg -i "Some.mp3" -f segment -segment_time 59 -ar 16000 -ac 1 %03d.wav
```

Then run this CLI on the directory containing all the output wav files.  This will then generate a single txt file.  Take all the wav files, zip them up and you have everything you need to train your own Custom Voice Font.