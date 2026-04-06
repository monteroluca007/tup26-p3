using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

const string BookId = "apuntes-programacion-iii";
const string BookTitle = "Apuntes de Programacion III";
const string BookLanguage = "es";
const string BookAuthor = "Adrián Di Battista";
const string OutputFileName = "apuntes-programacion-iii.epub";
var coverCandidates = new[] { "portada.png", "portada.jpg", "portada.jpeg" };

var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "120-programa-de-programacion-iii.md"
};

var root = ResolveRoot(args, excluded);
var outputPath = Path.Combine(root, OutputFileName);
var coverPath = ResolveCoverPath(root, coverCandidates);
var coverFileName = Path.GetFileName(coverPath);
var coverMediaType = GetCoverMediaType(coverFileName);

var markdownFiles = Directory
    .GetFiles(root, "*.md")
    .Where(path => !excluded.Contains(Path.GetFileName(path)))
    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
    .ToList();

if (markdownFiles.Count == 0)
{
    Console.Error.WriteLine("No se encontraron archivos Markdown para incluir.");
    return 1;
}

BuildEpub(markdownFiles, outputPath, coverPath, coverFileName, coverMediaType);
Console.WriteLine(Path.GetFileName(outputPath));
return 0;

static void BuildEpub(List<string> markdownFiles, string outputPath, string coverPath, string coverFileName, string coverMediaType)
{
    var now = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssK");
    var css = """
body { font-family: serif; line-height: 1.45; margin: 5%; }
h1, h2, h3, h4, h5, h6 { line-height: 1.2; margin-top: 1.2em; }
pre { background: #f4f4f4; padding: 0.8em; white-space: pre-wrap; }
code { font-family: monospace; }
pre code { display: block; line-height: 1.5; }
.tok-comment { color: #6a737d; }
.tok-string, .tok-char { color: #0a7b34; }
.tok-number { color: #8a3ffc; }
.tok-keyword { color: #9a3412; font-weight: 600; }
.tok-type { color: #0f5ea8; }
.tok-var { color: #8b5e00; }
.tok-command { color: #0f5ea8; font-weight: 600; }
blockquote { border-left: 0.25em solid #999; margin-left: 0; padding-left: 1em; color: #444; }
hr { border: none; border-top: 1px solid #bbb; margin: 1.5em 0; }
.chapter-header { margin-bottom: 2.5em; padding-bottom: 0.8em; border-bottom: 1px solid #bbb; }
.chapter-kicker { margin: 0; text-transform: uppercase; letter-spacing: 0.08em; font-size: 0.8em; color: #666; }
.book-title { text-align: center; margin-top: 20%; }
.toc-list li { margin: 0.4em 0; }
.cover-page { margin: 0; padding: 0; }
.cover-frame { margin: 0 auto; text-align: center; }
.cover-frame img { display: block; width: 100%; height: auto; }
""";

    var chapters = new List<Chapter>();
    for (int index = 0; index < markdownFiles.Count; index++)
    {
        var path = markdownFiles[index];
        var source = File.ReadAllText(path, Encoding.UTF8);
        var title = FirstHeading(source, Path.GetFileNameWithoutExtension(path));
        var chapterFile = $"chapter-{index + 1:00}.xhtml";
        var xhtml = MarkdownToXhtml(source, title, index + 1);
        chapters.Add(new Chapter(chapterFile, title, xhtml));
    }

    var tocItems = string.Join(
        "\n",
        chapters.Select((chapter, index) =>
            $"        <li><a href=\"{XmlEscape(chapter.FileName)}\">Capitulo {index + 1}: {XmlEscape(chapter.Title)}</a></li>")
    );

    var indexBody = $"""
<section epub:type="frontmatter toc">
  <div class="book-title">
    <h1>{XmlEscape(BookTitle)}</h1>
    <p>Indice general</p>
  </div>
  <nav epub:type="toc" id="toc">
    <ol class="toc-list">
{tocItems}
    </ol>
  </nav>
</section>
""";

    var navXhtml = WrapXhtmlPage("Indice", indexBody);
    var coverXhtml = BuildCoverPage(coverFileName);

    var manifestItems = new List<string>
    {
        "    <item id=\"cover\" href=\"cover.xhtml\" media-type=\"application/xhtml+xml\"/>",
        $"    <item id=\"cover-image\" href=\"{coverFileName}\" media-type=\"{coverMediaType}\" properties=\"cover-image\"/>",
        "    <item id=\"nav\" href=\"nav.xhtml\" media-type=\"application/xhtml+xml\" properties=\"nav\"/>",
        "    <item id=\"css\" href=\"styles.css\" media-type=\"text/css\"/>"
    };

    for (int index = 0; index < chapters.Count; index++)
    {
        manifestItems.Add(
            $"    <item id=\"chap{index + 1}\" href=\"{chapters[index].FileName}\" media-type=\"application/xhtml+xml\"/>");
    }

    var spineItems = new List<string>
    {
        "    <itemref idref=\"cover\"/>",
        "    <itemref idref=\"nav\"/>"
    };

    for (int index = 0; index < chapters.Count; index++)
    {
        spineItems.Add($"    <itemref idref=\"chap{index + 1}\"/>");
    }

    var opf = $"""
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://www.idpf.org/2007/opf" unique-identifier="bookid" version="3.0">
  <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
    <dc:identifier id="bookid">{XmlEscape(BookId)}</dc:identifier>
    <dc:title>{XmlEscape(BookTitle)}</dc:title>
    <dc:language>{XmlEscape(BookLanguage)}</dc:language>
    <dc:creator>{XmlEscape(BookAuthor)}</dc:creator>
    <dc:date>{XmlEscape(now)}</dc:date>
  </metadata>
  <manifest>
{string.Join("\n", manifestItems)}
  </manifest>
  <spine>
{string.Join("\n", spineItems)}
  </spine>
</package>
""";

    var containerXml = """
<?xml version="1.0" encoding="utf-8"?>
<container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
  <rootfiles>
    <rootfile full-path="OEBPS/content.opf" media-type="application/oebps-package+xml"/>
  </rootfiles>
</container>
""";

    if (File.Exists(outputPath))
    {
        File.Delete(outputPath);
    }

    using var stream = File.Create(outputPath);
    using var zip = new ZipArchive(stream, ZipArchiveMode.Create);

    WriteTextEntry(zip, "mimetype", "application/epub+zip", CompressionLevel.NoCompression);
    WriteTextEntry(zip, "META-INF/container.xml", containerXml, CompressionLevel.Optimal);
    WriteTextEntry(zip, "OEBPS/styles.css", css, CompressionLevel.Optimal);
    WriteBinaryEntry(zip, $"OEBPS/{coverFileName}", File.ReadAllBytes(coverPath), CompressionLevel.Optimal);
    WriteTextEntry(zip, "OEBPS/cover.xhtml", coverXhtml, CompressionLevel.Optimal);
    WriteTextEntry(zip, "OEBPS/nav.xhtml", navXhtml, CompressionLevel.Optimal);
    WriteTextEntry(zip, "OEBPS/content.opf", opf, CompressionLevel.Optimal);

    foreach (var chapter in chapters)
    {
        WriteTextEntry(zip, $"OEBPS/{chapter.FileName}", chapter.Xhtml, CompressionLevel.Optimal);
    }
}

static string ResolveRoot(string[] args, HashSet<string> excluded)
{
    var effectiveArgs = args.Where(arg => arg != "--").ToArray();

    if (effectiveArgs.Length > 0 && Directory.Exists(effectiveArgs[0]))
    {
        return Path.GetFullPath(effectiveArgs[0]);
    }

    var candidates = new List<string?>
    {
        Directory.GetCurrentDirectory(),
        AppContext.BaseDirectory,
        Environment.GetEnvironmentVariable("PWD"),
    };

    foreach (var candidate in candidates)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            continue;
        }

        var found = FindProjectRoot(candidate, excluded);
        if (found is not null)
        {
            return found;
        }
    }

    throw new DirectoryNotFoundException(
        "No se pudo encontrar la carpeta de apuntes. Ejecuta el programa desde esa carpeta o pasala como argumento.");
}

static string? FindProjectRoot(string startDirectory, HashSet<string> excluded)
{
    var current = new DirectoryInfo(Path.GetFullPath(startDirectory));

    while (current is not null)
    {
        if (ResolveCoverPathOrNull(current.FullName) is not null)
        {
            var markdownCount = Directory
                .GetFiles(current.FullName, "*.md")
                .Count(path => !excluded.Contains(Path.GetFileName(path)));

            if (markdownCount > 0)
            {
                return current.FullName;
            }
        }

        current = current.Parent;
    }

    return null;
}

static string ResolveCoverPath(string root, IEnumerable<string> candidates)
{
    var coverPath = ResolveCoverPathOrNull(root, candidates);
    if (coverPath is null)
    {
        throw new FileNotFoundException(
            $"No se encontro ninguna portada. Se busco: {string.Join(", ", candidates)}");
    }

    return coverPath;
}

static string? ResolveCoverPathOrNull(string root, IEnumerable<string>? candidates = null)
{
    foreach (var candidate in candidates ?? new[] { "portada.png", "portada.jpg", "portada.jpeg" })
    {
        var path = Path.Combine(root, candidate);
        if (File.Exists(path))
        {
            return path;
        }
    }

    return null;
}

static string GetCoverMediaType(string coverFileName)
{
    return Path.GetExtension(coverFileName).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".jpg" => "image/jpeg",
        ".jpeg" => "image/jpeg",
        _ => "application/octet-stream",
    };
}

static string BuildCoverPage(string coverFileName)
{
    var body = $"""
<section epub:type="cover" class="cover-page">
  <div class="cover-frame">
    <img src="{coverFileName}" alt="Portada de Apuntes de Programacion III" />
  </div>
</section>
""";

    return WrapXhtmlPage("Portada", body);
}

static void WriteTextEntry(ZipArchive zip, string entryName, string content, CompressionLevel compressionLevel)
{
    var entry = zip.CreateEntry(entryName, compressionLevel);
    using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
    writer.Write(content);
}

static void WriteBinaryEntry(ZipArchive zip, string entryName, byte[] content, CompressionLevel compressionLevel)
{
    var entry = zip.CreateEntry(entryName, compressionLevel);
    using var entryStream = entry.Open();
    entryStream.Write(content, 0, content.Length);
}

static string WrapXhtmlPage(string title, string body)
{
    return $"""
<?xml version="1.0" encoding="utf-8"?>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops" xml:lang="{BookLanguage}">
  <head>
    <title>{XmlEscape(title)}</title>
    <link rel="stylesheet" type="text/css" href="styles.css" />
  </head>
  <body>
    {body}
  </body>
</html>
""";
}

static string MarkdownToXhtml(string markdownText, string chapterTitle, int chapterNumber)
{
    markdownText = StripLeadingTitle(markdownText, chapterTitle);
    var lines = markdownText.Replace("\r\n", "\n").Split('\n');
    var parts = new List<string>();
    var paragraph = new List<string>();
    var codeLines = new List<string>();
    var listStack = new Stack<string>();
    var inCode = false;
    var codeLanguage = "";
    var inBlockquote = false;
    var skippedFirstH1 = false;

    void FlushParagraph()
    {
        if (paragraph.Count == 0)
        {
            return;
        }

        var joined = string.Join(" ", paragraph.Select(line => line.Trim())).Trim();
        if (joined.Length > 0)
        {
            parts.Add($"<p>{InlineMarkdown(joined)}</p>");
        }

        paragraph.Clear();
    }

    void CloseLists()
    {
        while (listStack.Count > 0)
        {
            parts.Add($"</{listStack.Pop()}>");
        }
    }

    void CloseBlockquote()
    {
        if (!inBlockquote)
        {
            return;
        }

        FlushParagraph();
        CloseLists();
        parts.Add("</blockquote>");
        inBlockquote = false;
    }

    foreach (var rawLine in lines)
    {
        var line = rawLine;
        var stripped = line.Trim();

        if (stripped.StartsWith("```", StringComparison.Ordinal))
        {
            FlushParagraph();
            CloseLists();
            if (inBlockquote)
            {
                CloseBlockquote();
            }

            if (inCode)
            {
                parts.Add(RenderCodeBlock(string.Join("\n", codeLines), codeLanguage));
                codeLines.Clear();
                codeLanguage = "";
                inCode = false;
            }
            else
            {
                inCode = true;
                codeLanguage = stripped[3..].Trim().ToLowerInvariant();
            }

            continue;
        }

        if (inCode)
        {
            codeLines.Add(line);
            continue;
        }

        if (stripped.Length == 0)
        {
            FlushParagraph();
            CloseLists();
            CloseBlockquote();
            continue;
        }

        if (stripped == "---")
        {
            FlushParagraph();
            CloseLists();
            CloseBlockquote();
            parts.Add("<hr />");
            continue;
        }

        if (stripped.StartsWith(">", StringComparison.Ordinal))
        {
            FlushParagraph();
            CloseLists();
            if (!inBlockquote)
            {
                parts.Add("<blockquote>");
                inBlockquote = true;
            }

            var quoteText = stripped[1..].TrimStart();
            parts.Add($"<p>{InlineMarkdown(quoteText)}</p>");
            continue;
        }

        CloseBlockquote();

        var headingMatch = Regex.Match(stripped, @"^(#{1,6})\s+(.*)$");
        if (headingMatch.Success)
        {
            FlushParagraph();
            CloseLists();
            var level = headingMatch.Groups[1].Value.Length;
            var title = headingMatch.Groups[2].Value.Trim();
            if (level == 1 && !skippedFirstH1)
            {
                skippedFirstH1 = true;
                continue;
            }

            parts.Add($"<h{level} id=\"{Slugify(title)}\">{InlineMarkdown(title)}</h{level}>");
            continue;
        }

        var orderedMatch = Regex.Match(stripped, @"^(\d+)\.\s+(.*)$");
        var unorderedMatch = Regex.Match(stripped, @"^[-*]\s+(.*)$");
        if (orderedMatch.Success || unorderedMatch.Success)
        {
            FlushParagraph();
            var tag = orderedMatch.Success ? "ol" : "ul";
            var content = orderedMatch.Success ? orderedMatch.Groups[2].Value : unorderedMatch.Groups[1].Value;
            if (listStack.Count == 0 || listStack.Peek() != tag)
            {
                CloseLists();
                parts.Add($"<{tag}>");
                listStack.Push(tag);
            }

            parts.Add($"<li>{InlineMarkdown(content.Trim())}</li>");
            continue;
        }

        paragraph.Add(line);
    }

    FlushParagraph();
    CloseLists();
    CloseBlockquote();
    if (inCode)
    {
        parts.Add(RenderCodeBlock(string.Join("\n", codeLines), codeLanguage));
    }

    var body = string.Join("\n", parts);
    var chapterBody = $"""
<section epub:type="chapter">
  <header class="chapter-header">
    <p class="chapter-kicker">Capitulo {chapterNumber}</p>
    <h1>{InlineMarkdown(chapterTitle)}</h1>
  </header>
  {body}
</section>
""";

    return WrapXhtmlPage(chapterTitle, chapterBody);
}

static string RenderCodeBlock(string code, string language)
{
    var lang = language.ToLowerInvariant();

    if (lang is "cs" or "csharp")
    {
        var keywords =
            "using|namespace|class|record|struct|interface|enum|public|private|protected|internal|static|" +
            "void|int|string|bool|var|new|return|if|else|switch|case|default|break|continue|for|foreach|" +
            "while|do|try|catch|finally|throw|null|true|false|this|base|out|ref|in|is|as|params";

        var patterns = new (string Name, string Pattern)[]
        {
            ("comment", @"//[^\n]*"),
            ("string", "\"(?:\\\\.|[^\"\\\\])*\""),
            ("char", @"'(?:\\.|[^'\\])+'"),
            ("number", @"\b\d+(?:\.\d+)?\b"),
            ("keyword", $@"\b(?:{keywords})\b"),
            ("type", @"\b(?:Console|List|File|Directory|Path|Environment|Exception|ConsoleKeyInfo|ConsoleKey)\b"),
        };

        return HighlightRegex(code, patterns, "language-csharp");
    }

    if (lang is "bash" or "sh" or "zsh" or "shell")
    {
        var keywords = "if|then|else|fi|for|in|do|done|case|esac|while|function";
        var patterns = new (string Name, string Pattern)[]
        {
            ("comment", @"#[^\n]*"),
            ("string", "\"(?:\\\\.|[^\"\\\\])*\"|'(?:\\\\.|[^'\\\\])*'"),
            ("var", @"\$[A-Za-z_][A-Za-z0-9_]*|\$\{[^}]+\}"),
            ("number", @"\b\d+\b"),
            ("keyword", $@"\b(?:{keywords})\b"),
            ("command", @"^(?:\s*)(?:dotnet|git|cd|ls|cat|rg|sed|python3|bash|zsh|mkdir|cp|mv|rm)\b"),
        };

        return HighlightRegex(code, patterns, "language-shell");
    }

    var languageClass = lang.Length > 0 ? $"language-{lang}" : "";
    return RenderPlainCode(code, languageClass);
}

static string HighlightRegex(string code, IEnumerable<(string Name, string Pattern)> patterns, string languageClass)
{
    var combinedPattern = string.Join("|", patterns.Select(pattern => $"(?<{pattern.Name}>{pattern.Pattern})"));
    var regex = new Regex(combinedPattern, RegexOptions.Multiline);
    var pieces = new StringBuilder();
    var last = 0;

    foreach (Match match in regex.Matches(code))
    {
        var start = match.Index;
        var end = match.Index + match.Length;
        if (start > last)
        {
            pieces.Append(WebUtility.HtmlEncode(code[last..start]));
        }

        var tokenType = patterns.First(pattern => match.Groups[pattern.Name].Success).Name;
        pieces.Append($"<span class=\"tok-{tokenType}\">{WebUtility.HtmlEncode(code[start..end])}</span>");
        last = end;
    }

    if (last < code.Length)
    {
        pieces.Append(WebUtility.HtmlEncode(code[last..]));
    }

    return $"<pre><code class=\"{languageClass}\">{pieces}</code></pre>";
}

static string RenderPlainCode(string code, string languageClass)
{
    var classAttribute = languageClass.Length > 0 ? $" class=\"{languageClass}\"" : "";
    return $"<pre><code{classAttribute}>{WebUtility.HtmlEncode(code)}</code></pre>";
}

static string StripLeadingTitle(string markdownText, string chapterTitle)
{
    var lines = markdownText.Replace("\r\n", "\n").Split('\n').ToList();
    for (int index = 0; index < lines.Count; index++)
    {
        var stripped = lines[index].Trim();
        if (stripped.Length == 0)
        {
            continue;
        }

        if (stripped == $"# {chapterTitle}")
        {
            var remainder = lines.Skip(index + 1).ToList();
            while (remainder.Count > 0 && string.IsNullOrWhiteSpace(remainder[0]))
            {
                remainder.RemoveAt(0);
            }

            return string.Join("\n", remainder);
        }

        break;
    }

    return markdownText;
}

static string FirstHeading(string markdownText, string fallback)
{
    foreach (var line in markdownText.Replace("\r\n", "\n").Split('\n'))
    {
        var stripped = line.Trim();
        if (stripped.StartsWith("# ", StringComparison.Ordinal))
        {
            return stripped[2..].Trim();
        }
    }

    return fallback;
}

static string InlineMarkdown(string text)
{
    var encoded = WebUtility.HtmlEncode(text);
    encoded = Regex.Replace(encoded, @"`([^`]+)`", "<code>$1</code>");
    encoded = Regex.Replace(encoded, @"\*\*([^*]+)\*\*", "<strong>$1</strong>");
    encoded = Regex.Replace(encoded, @"\*([^*]+)\*", "<em>$1</em>");
    encoded = Regex.Replace(encoded, @"\[([^\]]+)\]\(([^)]+)\)", "<a href=\"$2\">$1</a>");
    return encoded;
}

static string Slugify(string text)
{
    var normalized = text.Normalize(NormalizationForm.FormD);
    var builder = new StringBuilder();

    foreach (var ch in normalized)
    {
        var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
        if (category == System.Globalization.UnicodeCategory.NonSpacingMark)
        {
            continue;
        }

        builder.Append(ch);
    }

    var ascii = builder.ToString();
    ascii = Regex.Replace(ascii, @"[^a-zA-Z0-9]+", "-").Trim('-').ToLowerInvariant();
    return ascii.Length > 0 ? ascii : "section";
}

static string XmlEscape(string text) => WebUtility.HtmlEncode(text);

sealed record Chapter(string FileName, string Title, string Xhtml);
