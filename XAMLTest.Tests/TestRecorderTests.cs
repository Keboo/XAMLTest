﻿using System.Runtime.CompilerServices;

namespace XamlTest.Tests;

[TestClass]
public class TestRecorderTests
{
    [NotNull]
    public TestContext? TestContext { get; set; }

    [ClassInitialize]
    public static void ClassInit(TestContext _)
    {
        foreach (var file in new TestRecorder(new Simulators.App()).EnumerateScreenshots())
        {
            File.Delete(file);
        }
    }

    [TestCleanup]
    public void ClassCleanup()
    {
        foreach (var file in new TestRecorder(new Simulators.App()).EnumerateScreenshots())
        {
            File.Delete(file);
        }
    }

    [TestMethod]
    public async Task SaveScreenshot_SavesImage()
    {
        await using IApp app = new Simulators.App();
        await using TestRecorder testRecorder = new(app);

        Assert.IsNotNull(await testRecorder.SaveScreenshot());

        string? file = testRecorder.EnumerateScreenshots()
            .Where(x => Path.GetFileName(Path.GetDirectoryName(x)) == nameof(TestRecorderTests) &&
                   Path.GetFileName(x).StartsWith(nameof(SaveScreenshot_SavesImage)))
            .Single();

        string? fileName = Path.GetFileName(file);
        Assert.AreEqual(nameof(TestRecorderTests), Path.GetFileName(Path.GetDirectoryName(file)));
        Assert.AreEqual($"{nameof(SaveScreenshot_SavesImage)}{GetLineNumber(-9)}-1.jpg", fileName);
        testRecorder.Success(skipInputStateCheck:true);
    }

    [TestMethod]
    public async Task SaveScreenshot_WithSuffix_SavesImage()
    {
        await using var app = new Simulators.App();
        await using TestRecorder testRecorder = new(app);

        Assert.IsNotNull(await testRecorder.SaveScreenshot("MySuffix"));

        var file = testRecorder.EnumerateScreenshots()
            .Where(x => Path.GetFileName(Path.GetDirectoryName(x)) == nameof(TestRecorderTests) &&
                   Path.GetFileName(x).StartsWith(nameof(SaveScreenshot_WithSuffix_SavesImage)))
            .Single();

        var fileName = Path.GetFileName(file);
        Assert.AreEqual(nameof(TestRecorderTests), Path.GetFileName(Path.GetDirectoryName(file)));
        Assert.AreEqual($"{nameof(SaveScreenshot_WithSuffix_SavesImage)}MySuffix{GetLineNumber(-9)}-1.jpg", fileName);
        testRecorder.Success(true);
    }

    [TestMethod]
    public async Task TestRecorder_WhenExceptionThrown_DoesNotRethrow()
    {
        await using var app = new Simulators.App();
        await using TestRecorder testRecorder = new(app);
        await Assert.ThrowsExactlyAsync<NotImplementedException>(async () => await app.InitializeWithDefaults(null!));
    }

    [TestMethod]
    public async Task TestRecorder_WithInvalidXAML_DoesNotRethrow()
    {
        await using var app = await App.StartRemote();
        await using TestRecorder testRecorder = new(app);
        await app.InitializeWithDefaults();
        await Assert.ThrowsExactlyAsync<XamlTestException>(async () => await app.CreateWindowWithContent("<InvalidContent />"));
    }

    [TestMethod]
    public async Task TestRecorder_WithCtorSuffix_AppendsToAllFileNames()
    {
        await using var app = new Simulators.App();
        await using TestRecorder testRecorder = new(app, "CtorSuffix");

        Assert.IsNotNull(await testRecorder.SaveScreenshot("OtherSuffix1"));
        Assert.IsNotNull(await testRecorder.SaveScreenshot("OtherSuffix2"));

        var files = testRecorder.EnumerateScreenshots()
            .Where(x => Path.GetFileName(Path.GetDirectoryName(x)) == nameof(TestRecorderTests) &&
                   Path.GetFileName(x).StartsWith(nameof(TestRecorder_WithCtorSuffix_AppendsToAllFileNames)))
            .ToList();

        Assert.AreEqual(2, files.Count);
        var file1Name = Path.GetFileName(files[0]);
        var file2Name = Path.GetFileName(files[1]);
        Assert.AreEqual($"{nameof(TestRecorder_WithCtorSuffix_AppendsToAllFileNames)}CtorSuffixOtherSuffix1{GetLineNumber(-11)}-1.jpg", file1Name);
        Assert.AreEqual($"{nameof(TestRecorder_WithCtorSuffix_AppendsToAllFileNames)}CtorSuffixOtherSuffix2{GetLineNumber(-11)}-2.jpg", file2Name);
        testRecorder.Success(skipInputStateCheck:true);
    }

    private static int GetLineNumber(int offset = 0, [CallerLineNumber] int lineNumber = 0)
        => lineNumber + offset;
}
