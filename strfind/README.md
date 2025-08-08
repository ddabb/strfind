# strfind

一个跨平台的文件搜索工具，可以查找指定目录下包含特定字符串的特定文件。

## 功能特点

- 支持搜索指定目录下的所有指定文件名的文件
- 支持检查文件是否包含特定字符串
- 支持忽略大小写选项
- 支持正则表达式搜索
- 支持多种输出格式（完整路径和文件名、仅路径、仅文件名）
- 如果未指定目录，会询问是否使用当前目录
- 提供详细的搜索结果，包括找到的文件数量和文件路径
- 支持多个.NET版本（.NET Core 3.1, .NET 6.0+）

## 安装

```bash
dotnet tool install --global strfind
```

## 使用方法

```bash
strfind -d <目录路径> -f <文件名> -s <搜索字符串> [-i] [-r] [-o <输出格式>]
```

### 参数说明

- `-d, --directory`: 要搜索的目录路径（可选，如果不提供会询问是否使用当前目录）
- `-f, --filename`: 要搜索的文件名（支持通配符，如*.config）
- `-s, --searchstring`: 要在文件中查找的字符串（如果使用-r选项，则可以是正则表达式）
- `-i, --ignorecase`: 搜索时忽略大小写（可选）
- `-r, --regex`: 使用正则表达式进行搜索（可选）
- `-o, --output`: 输出格式，可选值：
  - `path`: 仅显示文件路径（默认）
  - `name`: 仅显示文件名
  - `full`: 显示完整路径和文件名

### 示例

查找当前目录下所有名为"app.config"的文件中包含"connectionString"的文件：

```bash
strfind -f "app.config" -s "connectionString"
```

查找指定目录下所有配置文件中包含"ERP"的文件，忽略大小写：

```bash
strfind -d "C:\项目目录" -f "*.config" -s "ERP" -i
```

使用正则表达式查找所有包含"ERP_"开头的标识符的配置文件：

```bash
strfind -d "C:\项目目录" -f "*.config" -s "ERP_\w+" -r
```

只显示文件名：

```bash
strfind -d "C:\项目目录" -f "*.config" -s "connectionString" -o name
```

显示完整路径和文件名：

```bash
strfind -d "C:\项目目录" -f "*.config" -s "connectionString" -o full
```

## 系统要求

- .NET Core 3.1 或更高版本
- .NET 6.0 或更高版本

## 许可证

MIT