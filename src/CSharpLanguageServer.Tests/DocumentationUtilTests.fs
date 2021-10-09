open NUnit.Framework
open CSharpLanguageServer.DocumentationUtil

[<TestCase(
    "",
    "", "",
    "")>]
[<TestCase(
    "<summary>doc string</summary>",
    "", "",
    "doc string")>]
[<TestCase(
    "<summary>\r\ndoc string\r\n\r\n</summary>",
    "", "",
    "doc string")>]
[<TestCase(
    "<summary>doc string</summary>\r\n <param name=\"x\">y</param>",
    "", "",
    "doc string\r\n" +
    "- Param `x`: y")>]
[<TestCase(
    "\r\n\
      <summary>Gets the standard error output stream.</summary>\r\n\
      <returns>A <see cref=\"T:System.IO.TextWriter\" /> that represents the standard error output stream.</returns>\r\n\
",
    "", "",
    "Gets the standard error output stream.\r\n\
- Returns: A `System.IO.TextWriter` that represents the standard error output stream."
)>]
[<TestCase(
    "\r\n\
            <summary>\r\n\
            Asserts that a condition is true. If the condition is false the method throws\r\n\
            an <see cref=\"T:NUnit.Framework.AssertionException\" />.\r\n\
            </summary>\r\n\
            <param name=\"condition\">The evaluated condition</param>\r\n\
 \r\n\
    ",
    "", "",
    "Asserts that a condition is true. If the condition is false the method throws \
an `NUnit.Framework.AssertionException`.\r\n\
- Param `condition`: The evaluated condition"
)>]
[<TestCase(
    "\r\n\
      <summary>Writes a string to the text stream, followed by a line terminator.</summary>\r\n\
      <param name=\"value\">The string to write. If <paramref name=\"value\" /> is <see langword=\"null\" />, only the line terminator is written.</param>\r\n\
      <exception cref=\"T:System.ObjectDisposedException\">The <see cref=\"T:System.IO.TextWriter\" /> is closed.</exception>\r\n\
      <exception cref=\"T:System.IO.IOException\">An I/O error occurs.</exception>\r\n\
    ",
    "", "",
    "Writes a string to the text stream, followed by a line terminator.\r\n\
- Param `value`: The string to write. If `value` is `null`, only the line terminator is written.\r\n\
- Exception `System.ObjectDisposedException`: The `System.IO.TextWriter` is closed.\r\n\
- Exception `System.IO.IOException`: An I/O error occurs."
)>]
let testFormatDocXml (xml, typeName, typeAssemblyName, expectedMD) =
    Assert.AreEqual(
        expectedMD,
        formatDocXml xml typeName typeAssemblyName)
