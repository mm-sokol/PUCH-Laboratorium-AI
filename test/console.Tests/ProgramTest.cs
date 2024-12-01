using System;
using Xunit;

public class ProgramTests
{
    [Fact]
    public void TestHelloWorldOutput()
    {
        // Arrange
        var expectedOutput = "Hello, World!\r\n";
        
        // Act
        using (var sw = new System.IO.StringWriter())
        {
            Console.SetOut(sw);  // Redirect Console output to StringWriter
            Program.Main(null);   // Call Main method

            var actualOutput = sw.ToString();
            
            // Assert
            Assert.Equal(expectedOutput, actualOutput);
        }
    }
}
