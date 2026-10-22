using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Streams;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CasseiaCoreX.Models
{
    public static class ConvertFileModel
    {

        public static string GenerateUniqueFileName(string directory, string fileName)
        {
            string path = Path.Combine(directory, fileName);

            if (!File.Exists(path))
            {
                return path;
            }

            string name = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);

            int index = 2;

            while (true)
            {
                path = Path.Combine(directory, $"{name} ({index}){extension}");

                if (!File.Exists(path))
                {
                    return path;
                }

                index++;
            }
        }

        public class FFMPEGManager
        {
            public string ffmpegDir = Path.Combine(App.Root, "Assets", "ffmpeg", "bin");
            public string ffmpegPath = Path.Combine(App.Root, "Assets", "ffmpeg", "bin", "ffmpeg.exe");
            public string ffplayPath = Path.Combine(App.Root, "Assets", "ffmpeg", "bin", "ffplay.exe");

            public bool IsFFMPEGExist()
            {
                return File.Exists(ffmpegPath);
            }

            public async Task UnZipFFMPEG()
            {
                if (File.Exists(Path.Combine(App.Root, "Assets", "ffmpeg.zip")))
                {
                    await Task.Run(() =>
                    {
                        ZipFile.ExtractToDirectory(Path.Combine(App.Root, "Assets", "ffmpeg.zip"), Path.Combine(App.Root, "Assets", "ffmpeg"), true);
                    });
                }
                else
                {
                    throw new FileNotFoundException("ffmpeg 压缩包不存在，App 资源可能损坏，建议重新下载安装此 App");
                }
            }

            public event EventHandler<string> OutputReceived;

            public async Task FFMPEGCommand(string path, string args)
            {
                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    FileName = path,
                    Arguments = $"-progress pipe:1 -nostats -hide_banner {args}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                Process p = new Process();
                p.StartInfo = psi;
                List<string> errors = new();

                p.OutputDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                    {
                        OutputReceived?.Invoke(this, e.Data);
                    }
                };
                p.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                    {
                        OutputReceived?.Invoke(this, e.Data);
                        errors.Add(e.Data);
                    }
                };

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                await p.WaitForExitAsync();

                if (p.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg 执行失败，退出代码：{p.ExitCode}\n{string.Join(Environment.NewLine, errors)}");
                }
            }


        }

        public static class ImageConverter
        {


            public static async Task ConvertSingleImageCodec(string inputPath, string outputDirectory, string outputFileName, Guid encoderId)
            {
                // 图片转换
                StorageFile inputFile = await StorageFile.GetFileFromPathAsync(inputPath);

                using IRandomAccessStream inputStream = await inputFile.OpenAsync(FileAccessMode.Read);

                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(inputStream);

                using SoftwareBitmap bitmap = await decoder.GetSoftwareBitmapAsync();

                StorageFolder outputFolder = await StorageFolder.GetFolderFromPathAsync(outputDirectory);

                StorageFile outputFile = await outputFolder.CreateFileAsync(outputFileName, CreationCollisionOption.GenerateUniqueName);

                using IRandomAccessStream outputStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite);

                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(encoderId, outputStream);

                encoder.SetSoftwareBitmap(bitmap);

                await encoder.FlushAsync();
            }

            public static async Task ConvertSingleImage(string inputPath, string outputDirectory, string outputFileName, int qualityIndex = 0)
            {
                string qualityArg = "";
                string extraArgs = "";
                string? tmpFile = null;

                if (inputPath.EndsWith(".heic", StringComparison.OrdinalIgnoreCase) || inputPath.EndsWith(".heif", StringComparison.OrdinalIgnoreCase) || inputPath.EndsWith(".hif", StringComparison.OrdinalIgnoreCase))
                {
                    string tempFileName = $"{Guid.NewGuid()}.png";
                    await ConvertSingleImageCodec(inputPath, outputDirectory, tempFileName, BitmapEncoder.PngEncoderId);

                    tmpFile = Path.Combine(outputDirectory, tempFileName);
                    inputPath = tmpFile;
                }

                

                try
                {
                    if (outputFileName.EndsWith(".heic", StringComparison.OrdinalIgnoreCase) || outputFileName.EndsWith(".heif", StringComparison.OrdinalIgnoreCase) || outputFileName.EndsWith(".hif", StringComparison.OrdinalIgnoreCase))
                    {
                        await ConvertSingleImageCodec(inputPath, outputDirectory, outputFileName, BitmapEncoder.HeifEncoderId);
                        return;
                    }

                    if (outputFileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || outputFileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                    {
                        qualityIndex = qualityIndex switch
                        {
                            0 => 2,
                            1 => 8,
                            2 => 16,
                            _ => 2
                        };

                        qualityArg = $" -q:v {qualityIndex}";
                    }
                    else if (outputFileName.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                    {
                        if (qualityIndex == 0)
                        {
                            qualityArg = " -c:v libwebp -lossless 1";
                        }
                        else
                        {
                            qualityIndex = qualityIndex switch
                            {
                                1 => 75,
                                2 => 50,
                                _ => 75
                            };

                            qualityArg = $" -c:v libwebp -quality {qualityIndex}";
                        }
                    }
                    else if (outputFileName.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
                    {
                        extraArgs = " -vf \"scale=256:256:force_original_aspect_ratio=decrease,pad=256:256:(ow-iw)/2:(oh-ih)/2:color=0x00000000\"";
                    }
                    else if (outputFileName.EndsWith(".avif", StringComparison.OrdinalIgnoreCase))
                    {
                        qualityArg = qualityIndex switch
                        {
                            0 => " -c:v libaom-av1 -still-picture 1 -crf 20 -cpu-used 4",
                            1 => " -c:v libaom-av1 -still-picture 1 -crf 30 -cpu-used 6",
                            2 => " -c:v libaom-av1 -still-picture 1 -crf 40 -cpu-used 8",
                            _ => " -c:v libaom-av1 -still-picture 1 -crf 30 -cpu-used 6"
                        };
                    }

                    FFMPEGManager m = new();

                    m.OutputReceived += (s, e) =>
                    {
                        Console.WriteLine(e);
                    };

                    string outputPath = GenerateUniqueFileName(outputDirectory, outputFileName);
                    await m.FFMPEGCommand(m.ffmpegPath, $"-i \"{inputPath}\"{qualityArg}{extraArgs} -n \"{outputPath}\"");
                }
                finally
                {
                    if (tmpFile != null && File.Exists(tmpFile))
                    {
                        File.Delete(tmpFile);
                    }
                }
            }
        }

        public static class MediaConverter
        {
            public static readonly string[] SupportedInputExtensions =
            {
                ".mp4", ".m4v", ".mov", ".avi", ".wmv", ".mkv", ".webm",
                ".mpg", ".mpeg", ".ts", ".mts", ".m2ts", ".3gp", ".3g2",

                ".mp3", ".m4a", ".aac", ".wav", ".wma", ".flac",
                ".ogg", ".oga", ".opus", ".ac3"
            };
            public static readonly string[] AudioOutputExtensions =
            {
                ".mp3",
                ".m4a",
                ".m4a (alac)",
                ".aac",
                ".wav",
                ".flac",
                ".ogg",
                ".opus",
                ".wma"
            };

            private static readonly byte[] NCMCoreKey = Encoding.ASCII.GetBytes("hzHRAmso5kInbaxW");
            private static readonly byte[] NCMMetadataKey = Encoding.ASCII.GetBytes("#14ljk_!\\]&0U<'(");

            public class NCMMetadata
            {
                public string Name { get; set; } = "";
                public string Artist { get; set; } = "";
                public string Album { get; set; } = "";
                public string Format { get; set; } = "";
                public int Bitrate { get; set; }
                public int Duration { get; set; }
            }

            public class NCMDecodeResult
            {
                public string OutputPath { get; set; } = "";
                public string Format { get; set; } = "";
                public NCMMetadata? Metadata { get; set; }
                public byte[]? CoverData { get; set; }
            }

            public static async Task ConvertSingleMediaCodec(string inputPath, string outputDirectory, string outputFileName, MediaEncodingProfile targetProfile)
            {
                StorageFile inputFile = await StorageFile.GetFileFromPathAsync(inputPath);
                StorageFolder outputFolder = await StorageFolder.GetFolderFromPathAsync(outputDirectory);
                StorageFile outputFile = await outputFolder.CreateFileAsync(outputFileName, CreationCollisionOption.GenerateUniqueName);

                try
                {
                    MediaEncodingProfile outputProfile = await CreateOutputProfile(inputFile, targetProfile);

                    MediaTranscoder transcoder = new();

                    PrepareTranscodeResult prepareResult = await transcoder.PrepareFileTranscodeAsync(inputFile, outputFile, outputProfile);

                    if (!prepareResult.CanTranscode)
                    {
                        await outputFile.DeleteAsync();
                        throw new InvalidOperationException(GetFailureMessage(prepareResult.FailureReason));
                    }

                    await prepareResult.TranscodeAsync();
                }
                catch
                {
                    try
                    {
                        await outputFile.DeleteAsync();
                    }
                    catch
                    {
                    }

                    throw;
                }
            }

            public static async Task ConvertSingleMedia(string inputPath, string outputDirectory, string outputFileName)
            {
                string? tempFile = null;

                try
                {
                    if (inputPath.EndsWith(".ncm", StringComparison.OrdinalIgnoreCase))
                    {
                        string decodedPath = await ConvertNCM(inputPath, outputDirectory);
                        string decodedExtension = Path.GetExtension(decodedPath);
                        string targetExtension = Path.GetExtension(outputFileName);

                        if (decodedExtension.Equals(targetExtension, StringComparison.OrdinalIgnoreCase))
                        {
                            string _outputPath = GenerateUniqueFileName(outputDirectory, outputFileName);
                            File.Move(decodedPath, _outputPath);
                            return;
                        }

                        tempFile = decodedPath;
                        inputPath = decodedPath;
                    }

                    string extraArgs = "";
                    string extension = Path.GetExtension(outputFileName).ToLowerInvariant();

                    if (outputFileName.EndsWith(".m4a (alac)", StringComparison.OrdinalIgnoreCase))
                    {
                        extraArgs = " -map 0:a:0 -map 0:v? -c:a alac -c:v copy -map_metadata 0 -disposition:v:0 attached_pic";
                        outputFileName = outputFileName.Replace(".m4a (alac)", ".m4a");
                    }
                    else if (AudioOutputExtensions.Contains(extension))
                    {
                        extraArgs = " -vn";
                    }

                    FFMPEGManager m = new();

                    m.OutputReceived += (s, e) =>
                    {
                        Console.WriteLine(e);
                    };

                    string outputPath = GenerateUniqueFileName(outputDirectory, outputFileName);

                    await m.FFMPEGCommand(m.ffmpegPath, $"-i \"{inputPath}\"{extraArgs} -n \"{outputPath}\"");
                }
                finally
                {
                    if (tempFile != null && File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
            }

            private static async Task<MediaEncodingProfile> CreateOutputProfile(StorageFile inputFile, MediaEncodingProfile targetProfile)
            {
                MediaEncodingProfile sourceProfile = await MediaEncodingProfile.CreateFromFileAsync(inputFile);

                MediaEncodingProfile outputProfile = targetProfile;

                if (sourceProfile.Video != null && outputProfile.Video != null)
                {
                    outputProfile.Video.Width = sourceProfile.Video.Width;
                    outputProfile.Video.Height = sourceProfile.Video.Height;
                    outputProfile.Video.Bitrate = sourceProfile.Video.Bitrate;

                    outputProfile.Video.FrameRate.Numerator = sourceProfile.Video.FrameRate.Numerator;
                    outputProfile.Video.FrameRate.Denominator = sourceProfile.Video.FrameRate.Denominator;

                    outputProfile.Video.PixelAspectRatio.Numerator = sourceProfile.Video.PixelAspectRatio.Numerator;
                    outputProfile.Video.PixelAspectRatio.Denominator = sourceProfile.Video.PixelAspectRatio.Denominator;
                }

                if (sourceProfile.Audio != null && outputProfile.Audio != null)
                {
                    outputProfile.Audio.Bitrate = sourceProfile.Audio.Bitrate;
                    outputProfile.Audio.SampleRate = sourceProfile.Audio.SampleRate;
                    outputProfile.Audio.ChannelCount = sourceProfile.Audio.ChannelCount;
                    outputProfile.Audio.BitsPerSample = sourceProfile.Audio.BitsPerSample;
                }

                return outputProfile;
            }

            private static string GetFailureMessage(TranscodeFailureReason failureReason)
            {
                return failureReason switch
                {
                    TranscodeFailureReason.CodecNotFound => "缺少转换所需的媒体编解码器。",
                    TranscodeFailureReason.InvalidProfile => "当前输入文件与目标媒体格式不兼容。",
                    TranscodeFailureReason.Unknown => "媒体转换失败，Media Foundation 未提供具体错误原因。",
                    _ => $"媒体转换失败：{failureReason}"
                };
            }

            private static int ReadNCMInt32(Stream stream)
            {
                byte[] buffer = new byte[4];
                stream.ReadExactly(buffer);
                return BitConverter.ToInt32(buffer, 0);
            }

            private static byte[] DecryptNCMAes(byte[] key, byte[] data)
            {
                using Aes aes = Aes.Create();

                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key;

                using ICryptoTransform decryptor = aes.CreateDecryptor();

                return decryptor.TransformFinalBlock(data, 0, data.Length);
            }

            private static byte[] BuildNCMKeyBox(byte[] key, int offset, int length)
            {
                byte[] keyBox = new byte[256];

                for (int i = 0; i < 256; i++)
                {
                    keyBox[i] = (byte)i;
                }

                byte lastByte = 0;
                int keyOffset = 0;

                for (int i = 0; i < 256; i++)
                {
                    byte swap = keyBox[i];
                    byte c = (byte)((swap + lastByte + key[offset + keyOffset]) & 0xff);

                    keyOffset++;

                    if (keyOffset >= length)
                    {
                        keyOffset = 0;
                    }

                    keyBox[i] = keyBox[c];
                    keyBox[c] = swap;
                    lastByte = c;
                }

                return keyBox;
            }

            private static void DecodeNCMChunk(Span<byte> buffer, byte[] keyBox, ref long position)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    int j = (int)((position + i + 1) & 0xff);

                    buffer[i] ^= keyBox[(keyBox[j] + keyBox[(keyBox[j] + j) & 0xff]) & 0xff];
                }

                position += buffer.Length;
            }

            private static string DetectNCMAudioFormat(ReadOnlySpan<byte> data)
            {
                if (data.Length >= 4 &&
                    data[0] == 0x66 &&
                    data[1] == 0x4C &&
                    data[2] == 0x61 &&
                    data[3] == 0x43)
                {
                    return "flac";
                }

                if (data.Length >= 3 &&
                    data[0] == 0x49 &&
                    data[1] == 0x44 &&
                    data[2] == 0x33)
                {
                    return "mp3";
                }

                // 部分 MP3 没有 ID3 标签，直接从 MPEG Frame 开始
                if (data.Length >= 2 &&
                    data[0] == 0xFF &&
                    (data[1] & 0xE0) == 0xE0)
                {
                    return "mp3";
                }

                return "";
            }

            private static NCMMetadata? ParseNCMMetadata(string json)
            {
                try
                {
                    using JsonDocument document = JsonDocument.Parse(json);

                    JsonElement root = document.RootElement;

                    NCMMetadata metadata = new();

                    if (root.TryGetProperty("musicName", out JsonElement name))
                    {
                        metadata.Name = name.GetString() ?? "";
                    }

                    if (root.TryGetProperty("album", out JsonElement album))
                    {
                        metadata.Album = album.GetString() ?? "";
                    }

                    if (root.TryGetProperty("artist", out JsonElement artist))
                    {
                        if (artist.ValueKind == JsonValueKind.Array)
                        {
                            List<string> artists = new();

                            foreach (JsonElement item in artist.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.Array && item.GetArrayLength() > 0)
                                {
                                    string? artistName = item[0].GetString();

                                    if (!string.IsNullOrWhiteSpace(artistName))
                                    {
                                        artists.Add(artistName);
                                    }
                                }
                            }

                            metadata.Artist = string.Join("/", artists);
                        }
                        else if (artist.ValueKind == JsonValueKind.String)
                        {
                            metadata.Artist = artist.GetString() ?? "";
                        }
                    }

                    if (root.TryGetProperty("format", out JsonElement format))
                    {
                        metadata.Format = format.GetString() ?? "";
                    }

                    if (root.TryGetProperty("bitrate", out JsonElement bitrate) && bitrate.TryGetInt32(out int bitrateValue))
                    {
                        metadata.Bitrate = bitrateValue;
                    }

                    if (root.TryGetProperty("duration", out JsonElement duration) && duration.TryGetInt32(out int durationValue))
                    {
                        metadata.Duration = durationValue;
                    }

                    return metadata;
                }
                catch
                {
                    return null;
                }
            }

            private static async Task<NCMDecodeResult> DecodeNCMToTemp(string inputPath, string outputDirectory)
            {
                if (!File.Exists(inputPath))
                {
                    throw new FileNotFoundException("NCM 文件不存在。", inputPath);
                }

                await using FileStream inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, 0x8000, true);

                byte[] header = new byte[8];
                inputStream.ReadExactly(header);

                if (!header.SequenceEqual(Encoding.ASCII.GetBytes("CTENFDAM")))
                {
                    throw new InvalidDataException("不是有效的 NCM 文件。");
                }

                // 文件头后还有两个字节
                inputStream.Seek(2, SeekOrigin.Current);

                int keyLength = ReadNCMInt32(inputStream);

                if (keyLength <= 0)
                {
                    throw new InvalidDataException("NCM 密钥数据无效。");
                }

                byte[] encryptedKey = new byte[keyLength];
                inputStream.ReadExactly(encryptedKey);

                for (int i = 0; i < encryptedKey.Length; i++)
                {
                    encryptedKey[i] ^= 0x64;
                }

                byte[] decryptedKey = DecryptNCMAes(NCMCoreKey, encryptedKey);

                if (decryptedKey.Length <= 17)
                {
                    throw new InvalidDataException("NCM 音频密钥损坏。");
                }

                byte[] keyBox = BuildNCMKeyBox(decryptedKey, 17, decryptedKey.Length - 17);

                int metadataLength = ReadNCMInt32(inputStream);

                NCMMetadata? metadata = null;

                if (metadataLength > 0)
                {
                    byte[] encryptedMetadata = new byte[metadataLength];
                    inputStream.ReadExactly(encryptedMetadata);

                    for (int i = 0; i < encryptedMetadata.Length; i++)
                    {
                        encryptedMetadata[i] ^= 0x63;
                    }

                    if (encryptedMetadata.Length > 22)
                    {
                        string base64 = Encoding.UTF8.GetString(encryptedMetadata, 22, encryptedMetadata.Length - 22);
                        byte[] metadataData = Convert.FromBase64String(base64);
                        byte[] decryptedMetadata = DecryptNCMAes(NCMMetadataKey, metadataData);

                        if (decryptedMetadata.Length > 6)
                        {
                            string json = Encoding.UTF8.GetString(decryptedMetadata, 6, decryptedMetadata.Length - 6).TrimEnd('\0');
                            metadata = ParseNCMMetadata(json);
                        }
                    }
                }

                // CRC32 + 图片版本
                inputStream.Seek(5, SeekOrigin.Current);

                int coverFrameLength = ReadNCMInt32(inputStream);
                int imageLength = ReadNCMInt32(inputStream);

                if (coverFrameLength < 0 || imageLength < 0 || imageLength > coverFrameLength)
                {
                    throw new InvalidDataException("NCM 封面数据结构无效。");
                }

                byte[]? coverData = null;

                if (imageLength > 0)
                {
                    coverData = new byte[imageLength];
                    inputStream.ReadExactly(coverData);
                }

                int coverPadding = coverFrameLength - imageLength;

                if (coverPadding > 0)
                {
                    inputStream.Seek(coverPadding, SeekOrigin.Current);
                }

                byte[] buffer = new byte[0x8000];
                int bytesRead = await inputStream.ReadAsync(buffer);

                if (bytesRead <= 0)
                {
                    throw new InvalidDataException("NCM 中没有找到音频数据。");
                }

                long position = 0;

                DecodeNCMChunk(buffer.AsSpan(0, bytesRead), keyBox, ref position);

                string format = DetectNCMAudioFormat(buffer.AsSpan(0, bytesRead));

                if (string.IsNullOrEmpty(format))
                {
                    format = metadata?.Format?.ToLowerInvariant() ?? "";
                }

                if (format is not "mp3" and not "flac")
                {
                    throw new InvalidDataException("无法识别 NCM 中的原始音频格式。");
                }

                if (metadata != null)
                {
                    metadata.Format = format;
                }

                string outputFileName = $"{Path.GetFileNameWithoutExtension(inputPath)}.{format}";
                string outputPath = GenerateUniqueFileName(outputDirectory, outputFileName);

                await using FileStream outputStream = new FileStream(outputPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 0x8000, true);

                await outputStream.WriteAsync(buffer.AsMemory(0, bytesRead));

                while ((bytesRead = await inputStream.ReadAsync(buffer)) > 0)
                {
                    DecodeNCMChunk(buffer.AsSpan(0, bytesRead), keyBox, ref position);
                    await outputStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                }

                return new NCMDecodeResult
                {
                    OutputPath = outputPath,
                    Format = format,
                    Metadata = metadata,
                    CoverData = coverData
                };
            }

            public static async Task<string> ConvertNCM(string inputPath, string outputDirectory)
            {
                string? tempAudioPath = null;
                string? tempCoverPath = null;

                try
                {
                    NCMDecodeResult result = await DecodeNCMToTemp(inputPath, outputDirectory);

                    tempAudioPath = result.OutputPath;

                    string extension = "." + result.Format;
                    string outputFileName = Path.GetFileNameWithoutExtension(inputPath) + extension;
                    string outputPath = GenerateUniqueFileName(outputDirectory, outputFileName);

                    string metadataArgs = "";

                    if (result.Metadata != null)
                    {
                        metadataArgs += $" -metadata title=\"{EscapeFFMPEGMetadata(result.Metadata.Name)}\"";
                        metadataArgs += $" -metadata artist=\"{EscapeFFMPEGMetadata(result.Metadata.Artist)}\"";
                        metadataArgs += $" -metadata album=\"{EscapeFFMPEGMetadata(result.Metadata.Album)}\"";
                    }

                    string coverArgs = "";

                    if (result.CoverData is { Length: > 0 })
                    {
                        string coverExtension = IsPNG(result.CoverData) ? ".png" : ".jpg";

                        tempCoverPath = Path.Combine(outputDirectory, $"{Guid.NewGuid()}{coverExtension}");

                        await File.WriteAllBytesAsync(tempCoverPath, result.CoverData);

                        coverArgs = $" -i \"{tempCoverPath}\" -map 0:a -map 1:v -c:a copy -c:v copy -disposition:v attached_pic";
                    }
                    else
                    {
                        coverArgs = " -map 0:a -c:a copy";
                    }

                    FFMPEGManager manager = new();

                    manager.OutputReceived += (s, e) =>
                    {
                        Console.WriteLine(e);
                    };

                    await manager.FFMPEGCommand(manager.ffmpegPath, $"-i \"{tempAudioPath}\"{coverArgs}{metadataArgs} -n \"{outputPath}\"");

                    return outputPath;
                }
                finally
                {
                    if (tempAudioPath != null && File.Exists(tempAudioPath))
                    {
                        File.Delete(tempAudioPath);
                    }

                    if (tempCoverPath != null && File.Exists(tempCoverPath))
                    {
                        File.Delete(tempCoverPath);
                    }
                }
            }

            private static bool IsPNG(byte[] data)
            {
                return data.Length >= 8 &&
                       data[0] == 0x89 &&
                       data[1] == 0x50 &&
                       data[2] == 0x4E &&
                       data[3] == 0x47 &&
                       data[4] == 0x0D &&
                       data[5] == 0x0A &&
                       data[6] == 0x1A &&
                       data[7] == 0x0A;
            }

            private static string EscapeFFMPEGMetadata(string value)
            {
                return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
            }


        }
    }
}

