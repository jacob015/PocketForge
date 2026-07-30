using NUnit.Framework;
using PocketForge.Presentation;

namespace PocketForge.Tests.Editor
{
    public sealed class CompactNumberFormatterTests
    {
        [TestCase(0L, "0")]
        [TestCase(999L, "999")]
        [TestCase(1000L, "1K")]
        [TestCase(12500L, "12.5K")]
        [TestCase(1000000L, "1M")]
        [TestCase(1250000000L, "1.25B")]
        [TestCase(1000000000000L, "1T")]
        [TestCase(999950L, "1M")]
        public void Format_UsesSharedCompactSuffixPolicy(long value, string expected)
        {
            Assert.That(CompactNumberFormatter.Format(value), Is.EqualTo(expected));
        }

        [Test]
        public void Format_FloatingPowerUsesTheSameSuffixPolicy()
        {
            Assert.That(CompactNumberFormatter.Format(0.5d), Is.EqualTo("0.5"));
            Assert.That(CompactNumberFormatter.Format(1250.5d), Is.EqualTo("1.25K"));
        }
    }
}
