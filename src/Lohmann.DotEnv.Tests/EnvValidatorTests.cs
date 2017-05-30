using System;
using Xunit;

namespace Lohmann.DotEnv.Tests
{
    public class EnvValidatorTests
    {
        private class EnvModel
        {
            public string FOO { get; private set; }
            public string BAR { get; private set; }
            public int IGNORED { get; private set; }
        }

        [Fact]
        public void Validate_MissingEnvironmentVariablesAreReported()
        {
            var results = EnvValidator.Default.Validate(new[] { "FOO", "BAR" });
            Assert.True(results.HasValidationErrors);
            Assert.Equal(2, results.ValidationErrors.Count);
        }

        [Fact]
        public void Validate_NotMissingEnvironmentVariablesAreNotReported()
        {
            try
            {
                System.Environment.SetEnvironmentVariable("FOO", "VALUE");
                System.Environment.SetEnvironmentVariable("BAR", "VALUE");

                var results = EnvValidator.Default.Validate(new[] { "FOO", "BAR" });
                Assert.False(results.HasValidationErrors);
                Assert.Equal(0, results.ValidationErrors.Count);
            }
            finally
            {
                System.Environment.SetEnvironmentVariable("FOO", null);
                System.Environment.SetEnvironmentVariable("BAR", null);
            }
        }

        [Fact]
        public void Validate_ModelTypesAreScannedForPublicStringProperties()
        {
            var results = EnvValidator.Default.Validate<EnvModel>();
            Assert.True(results.HasValidationErrors);
            Assert.Equal(2, results.ValidationErrors.Count);
        }
    }
}
