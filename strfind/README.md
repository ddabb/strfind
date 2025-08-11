# strfind

一个跨平台的文件搜索工具，可以查找指定目录下包含特定字符串的特定文件。

## 功能特点

- 支持在指定目录下搜索包含特定字符串的文件
- 支持文件名过滤
- 支持正则表达式搜索
- 支持忽略大小写
- 支持多种输出格式
- 支持忽略指定目录（如bin、obj、node_modules等）
- 支持特殊字符搜索（使用引号包围）

## 安装

```bash
dotnet tool install --global strfind
```

## 使用方法

```bash
strfind -s "要搜索的字符串" [选项]
```

### 选项

- `-s, --searchstring <STRING>`: 要在文件中查找的字符串（必需）
- `-d, --directory <PATH>`: 要搜索的目录路径（默认为当前目录）
- `-f, --filename <PATTERN>`: 要搜索的文件名（默认为所有文件 "*"）
- `-i, --ignorecase`: 搜索时忽略大小写
- `-r, --regex`: 使用正则表达式进行搜索
- `-o, --output <FORMAT>`: 输出格式，可选值：
  - `path`: 仅显示文件路径（默认）
  - `name`: 仅显示文件名
  - `full`: 显示完整路径和文件名
- `-e, --exclude <DIRS>`: 要忽略的目录名称，多个目录用逗号分隔，例如: bin,obj,node_modules

### 示例

搜索当前目录下包含"Hello"的所有文件：
```bash
strfind -s "Hello"
```

搜索指定目录下的所有.cs文件中包含"TODO"的文件：
```bash
strfind -s "TODO" -d "C:\Projects\MyProject" -f "*.cs"
```

使用正则表达式搜索，忽略大小写：
```bash
strfind -s "error\s*:\s*\d+" -r -i
```

搜索时忽略bin、obj和node_modules目录：
```bash
strfind -s "api" -e bin,obj,node_modules
```

搜索包含特殊字符的字符串（使用引号包围）：
```bash
strfind -s "<<<<<<< HEAD"
```

## 版本历史

### 1.0.2
- 添加忽略指定目录功能
- 改进特殊字符处理
- 优化错误提示

### 1.0.1
- 修复了一些bug
- 改进了输出格式

### 1.0.0
- 初始版本发布