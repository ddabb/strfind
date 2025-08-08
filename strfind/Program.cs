﻿using System;
using System.IO;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace StrFind
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // 创建根命令
            var rootCommand = new RootCommand("一个跨平台的文件搜索工具，可以查找指定目录下包含特定字符串的特定文件");

            // 添加目录参数
            var directoryOption = new Option<string>(
                "--directory",
                description: "要搜索的目录路径")
            {
                IsRequired = false
            };
            directoryOption.AddAlias("-d");
            rootCommand.AddOption(directoryOption);

            // 添加文件名参数
            var fileNameOption = new Option<string>(
                "--filename",
                description: "要搜索的文件名，默认为所有文件")
            {
                IsRequired = false
            };
            fileNameOption.AddAlias("-f");
            fileNameOption.SetDefaultValue("*");
            rootCommand.AddOption(fileNameOption);

            // 添加搜索字符串参数
            var searchStringOption = new Option<string>(
                "--searchstring",
                description: "要在文件中查找的字符串")
            {
                IsRequired = true
            };
            searchStringOption.AddAlias("-s");
            rootCommand.AddOption(searchStringOption);

            // 添加不区分大小写选项
            var ignoreCaseOption = new Option<bool>(
                "--ignorecase",
                description: "搜索时忽略大小写")
            {
                IsRequired = false
            };
            ignoreCaseOption.AddAlias("-i");
            rootCommand.AddOption(ignoreCaseOption);

            // 添加正则表达式选项
            var regexOption = new Option<bool>(
                "--regex",
                description: "使用正则表达式进行搜索")
            {
                IsRequired = false
            };
            regexOption.AddAlias("-r");
            rootCommand.AddOption(regexOption);

            // 添加输出格式选项
            var outputFormatOption = new Option<string>(
                "--output",
                description: "输出格式: full(完整路径和文件名), path(仅路径，默认), name(仅文件名)")
            {
                IsRequired = false
            };
            outputFormatOption.AddAlias("-o");
            outputFormatOption.SetDefaultValue("path");
            rootCommand.AddOption(outputFormatOption);

            // 设置命令处理程序
            rootCommand.SetHandler((directory, fileName, searchString, ignoreCase, useRegex, outputFormat) =>
            {
                FindFiles(directory, fileName, searchString, ignoreCase, useRegex, outputFormat);
            }, directoryOption, fileNameOption, searchStringOption, ignoreCaseOption, regexOption, outputFormatOption);

            // 解析命令行参数
            return await rootCommand.InvokeAsync(args);
        }

        static void FindFiles(string directory, string fileName, string searchString, bool ignoreCase = false, bool useRegex = false, string outputFormat = "path")
        {
            try
            {
                // 如果目录为空，使用当前目录
                if (string.IsNullOrEmpty(directory))
                {
                    directory = Directory.GetCurrentDirectory();
                    Console.WriteLine($"未指定搜索目录，使用当前目录: '{directory}'");
                }
                
                // 如果文件名为空，使用默认值 "*"（所有文件）
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = "*";
                    Console.WriteLine("未指定文件名，搜索所有文件");
                }

                // 检查目录是否存在
                if (!Directory.Exists(directory))
                {
                    Console.WriteLine($"错误: 目录 '{directory}' 不存在");
                    return;
                }

                Console.WriteLine($"正在搜索目录: {directory}");
                Console.WriteLine($"文件名: {fileName}");
                Console.WriteLine($"搜索字符串: {searchString}");
                Console.WriteLine($"忽略大小写: {(ignoreCase ? "是" : "否")}");
                Console.WriteLine($"使用正则表达式: {(useRegex ? "是" : "否")}");
                Console.WriteLine("----------------------------------------");

                // 获取所有匹配的文件
                var files = Directory.GetFiles(directory, fileName, SearchOption.AllDirectories);
                
                if (files.Length == 0)
                {
                    if (fileName == "*")
                        Console.WriteLine($"在目录 '{directory}' 中未找到任何文件");
                    else
                        Console.WriteLine($"在目录 '{directory}' 中未找到名为 '{fileName}' 的文件");
                    return;
                }

                Console.WriteLine($"找到 {files.Length} 个匹配文件名的文件，正在检查内容...");
                
                int matchCount = 0;
                StringComparison comparison = ignoreCase ? 
                    StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                
                // 检查每个文件是否包含搜索字符串
                foreach (var file in files)
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        bool isMatch = false;
                        
                        if (useRegex)
                        {
                            // 使用正则表达式搜索
                            var regexOptions = ignoreCase ? 
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase : 
                                System.Text.RegularExpressions.RegexOptions.None;
                            
                            isMatch = System.Text.RegularExpressions.Regex.IsMatch(content, searchString, regexOptions);
                        }
                        else
                        {
                            // 使用普通字符串搜索
                            if (ignoreCase)
                            {
                                isMatch = content.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0;
                            }
                            else
                            {
                                isMatch = content.Contains(searchString);
                            }
                        }
                        
                        if (isMatch)
                        {
                            string fileBaseName = Path.GetFileName(file);
                            
                            // 根据输出格式显示结果
                            switch (outputFormat.ToLower())
                            {
                                case "path":
                                    Console.WriteLine(file);
                                    break;
                                case "name":
                                    Console.WriteLine(fileBaseName);
                                    break;
                                case "full":
                                    Console.WriteLine($"{file} (文件名: {fileBaseName})");
                                    break;
                                default:
                                    Console.WriteLine(file);
                                    break;
                            }
                            
                            matchCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"读取文件 '{file}' 时出错: {ex.Message}");
                    }
                }

                Console.WriteLine("----------------------------------------");
                if (useRegex)
                {
                    Console.WriteLine($"搜索完成，共找到 {matchCount} 个匹配正则表达式 '{searchString}' 的文件");
                    
                    if (matchCount == 0)
                    {
                        Console.WriteLine($"未找到匹配正则表达式 '{searchString}' 的文件");
                    }
                }
                else
                {
                    Console.WriteLine($"搜索完成，共找到 {matchCount} 个包含 '{searchString}' 的文件");
                    
                    if (matchCount == 0)
                    {
                        Console.WriteLine($"未找到包含 '{searchString}' 的文件");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
            }
        }
    }
}
