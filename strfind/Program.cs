﻿using System;
using System.IO;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

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
                description: "要在文件中查找的字符串（包含特殊字符时请使用引号包围）")
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

            // 添加忽略目录选项
            var excludeDirsOption = new Option<string[]>(
                "--exclude",
                description: "要忽略的目录名称，多个目录用逗号分隔，例如: bin,obj,node_modules")
            {
                IsRequired = false,
                AllowMultipleArgumentsPerToken = true
            };
            excludeDirsOption.AddAlias("-e");
            rootCommand.AddOption(excludeDirsOption);

            // 设置命令处理程序
            rootCommand.SetHandler((directory, fileName, searchString, ignoreCase, useRegex, outputFormat, excludeDirs) =>
            {
                FindFiles(directory, fileName, searchString, ignoreCase, useRegex, outputFormat, excludeDirs);
            }, directoryOption, fileNameOption, searchStringOption, ignoreCaseOption, regexOption, outputFormatOption, excludeDirsOption);

            // 解析命令行参数
            return await rootCommand.InvokeAsync(args);
        }

        static void FindFiles(string directory, string fileName, string searchString, bool ignoreCase = false, bool useRegex = false, string outputFormat = "path", string[] excludeDirs = null)
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
                Console.WriteLine($"搜索字符串: \"{searchString}\"");
                Console.WriteLine($"忽略大小写: {(ignoreCase ? "是" : "否")}");
                Console.WriteLine($"使用正则表达式: {(useRegex ? "是" : "否")}");
                
                // 显示忽略的目录
                if (excludeDirs != null && excludeDirs.Length > 0)
                {
                    Console.WriteLine($"忽略目录: {string.Join(", ", excludeDirs)}");
                }
                
                Console.WriteLine("----------------------------------------");

                // 获取所有匹配的文件（自定义搜索以排除指定目录）
                var files = GetFilesExcludingDirectories(directory, fileName, excludeDirs);
                
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
                
                // 处理特殊字符，避免命令行解析问题
                string processedSearchString = searchString;
                
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
                            
                            // 对正则表达式进行转义处理，避免特殊字符导致的解析错误
                            try
                            {
                                isMatch = System.Text.RegularExpressions.Regex.IsMatch(content, processedSearchString, regexOptions);
                            }
                            catch (ArgumentException ex)
                            {
                                Console.WriteLine($"正则表达式解析错误: {ex.Message}");
                                Console.WriteLine("请检查您的正则表达式语法或使用引号包围搜索字符串");
                                return;
                            }
                        }
                        else
                        {
                            // 使用普通字符串搜索
                            if (ignoreCase)
                            {
                                isMatch = content.IndexOf(processedSearchString, StringComparison.OrdinalIgnoreCase) >= 0;
                            }
                            else
                            {
                                isMatch = content.Contains(processedSearchString);
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

        /// <summary>
        /// 获取指定目录下的所有文件，排除指定的子目录
        /// </summary>
        /// <param name="rootDirectory">根目录</param>
        /// <param name="searchPattern">搜索模式</param>
        /// <param name="excludeDirectories">要排除的目录名称数组</param>
        /// <returns>符合条件的文件路径列表</returns>
        static string[] GetFilesExcludingDirectories(string rootDirectory, string searchPattern, string[] excludeDirectories)
        {
            var result = new List<string>();
            
            // 如果排除目录为空，初始化为空数组
            if (excludeDirectories == null)
            {
                excludeDirectories = Array.Empty<string>();
            }

            try
            {
                // 获取根目录中的所有文件
                foreach (var file in Directory.GetFiles(rootDirectory, searchPattern))
                {
                    result.Add(file);
                }

                // 递归处理子目录
                foreach (var subDir in Directory.GetDirectories(rootDirectory))
                {
                    // 检查当前目录名是否在排除列表中
                    string dirName = Path.GetFileName(subDir);
                    
                    // 如果目录名在排除列表中，则跳过
                    if (excludeDirectories.Any(exclude => 
                        string.Equals(dirName, exclude, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    // 递归处理非排除的子目录
                    result.AddRange(GetFilesExcludingDirectories(subDir, searchPattern, excludeDirectories));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"访问目录 '{rootDirectory}' 时出错: {ex.Message}");
            }

            return result.ToArray();
        }
    }
}