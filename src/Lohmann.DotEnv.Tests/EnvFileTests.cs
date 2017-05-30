using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Lohmann.DotEnv.Tests
{
    public class EnvFileTests
    {
        #region Test Helper
        private class MockEnvironmentProvider : IEnvironmentVariableProvider
        {
            public Dictionary<string, string> EnvironmentDictionary { get; }
                = new Dictionary<string, string>();

            string IEnvironmentVariableProvider.Get(string variable)
            {
                return EnvironmentDictionary.ContainsKey(variable) ? EnvironmentDictionary[variable] : string.Empty;
            }

            void IEnvironmentVariableProvider.Set(string variable, string value)
            {
                EnvironmentDictionary.Add(variable, value);
            }
        }
    
        private Stream CreateStreamFromLines(string[] lines)
        {
            var str = String.Join(Environment.NewLine, lines);
            return new MemoryStream(Encoding.UTF8.GetBytes(str));
        }
        #endregion

        [Fact]
        public async Task EmptyStream_NoEnvironmentVariablesSet()
        {
            var mockEnvironmentProvider = new MockEnvironmentProvider();
            var envFile = new EnvFile(mockEnvironmentProvider);
            await envFile.LoadAsync(Stream.Null);
            Assert.Equal(0, mockEnvironmentProvider.EnvironmentDictionary.Count);
        }

        [Fact]
        public async Task NoKeyValuesInStream_NoEnvironmentVariablesSet()
        {
            var lines = new string[]
            {
                "sdfjois",
                "# asjdlksa"
            };

            using (var stream = CreateStreamFromLines(lines))
            {
                var mockEnvironmentProvider = new MockEnvironmentProvider();
                var envFile = new EnvFile(mockEnvironmentProvider);
                await envFile.LoadAsync(stream);
                Assert.Equal(0, mockEnvironmentProvider.EnvironmentDictionary.Count);
            }
        }

        [Fact]
        public async Task EmptyValueInStream_EnvironmentVariableSetWithEmptyString()
        {
            var lines = new string[]
            {
                "FOO=   ",
                "BAR="
            };

            using (var stream = CreateStreamFromLines(lines))
            {
                var mockEnvironmentProvider = new MockEnvironmentProvider();
                var envFile = new EnvFile(mockEnvironmentProvider);
                await envFile.LoadAsync(stream);
                Assert.Equal(2, mockEnvironmentProvider.EnvironmentDictionary.Count);
                Assert.Equal(string.Empty, mockEnvironmentProvider.EnvironmentDictionary["FOO"]);
                Assert.Equal(string.Empty, mockEnvironmentProvider.EnvironmentDictionary["BAR"]);
            }
        }

        [Fact]
        public async Task KeyValueStream_EnvironmentVariablesSet()
        {
            var lines = new string[]
            {
                "# SECTION FOO",
                "ONE=bar",
                "TWO=Buzz213  ",
                "  THREE=Buzz21 3   ",
                "FOUR = Buzz213",
                "FIVE=\"quoted\"",
                "SIX = 'abs8dsw=='",
                ""
            };

            using (var stream = CreateStreamFromLines(lines))
            {
                var mockEnvironmentProvider = new MockEnvironmentProvider();
                var envFile = new EnvFile(mockEnvironmentProvider);
                await envFile.LoadAsync(stream);
                Assert.Equal(6, mockEnvironmentProvider.EnvironmentDictionary.Count);
                Assert.Equal("bar", mockEnvironmentProvider.EnvironmentDictionary["ONE"]);
                Assert.Equal("Buzz213", mockEnvironmentProvider.EnvironmentDictionary["TWO"]);
                Assert.Equal("Buzz21 3", mockEnvironmentProvider.EnvironmentDictionary["THREE"]);
                Assert.Equal("Buzz213", mockEnvironmentProvider.EnvironmentDictionary["FOUR"]);
                Assert.Equal("abs8dsw==", mockEnvironmentProvider.EnvironmentDictionary["SIX"]);
            }
        }

        [Fact]
        public async Task LoadDefaultDotEnvFile_EnvironmentVariablesSet()
        {
            var mockEnvironmentProvider = new MockEnvironmentProvider();
            var envFile = new EnvFile(mockEnvironmentProvider);
            await envFile.LoadAsync();
            Assert.Equal(1, mockEnvironmentProvider.EnvironmentDictionary.Count);
            Assert.Equal("BAR", mockEnvironmentProvider.EnvironmentDictionary["FOO"]);
        }

        [Fact]
        public async Task LoadNonDefaultDotEnvFile_EnvironmentVariablesSet()
        {
            var mockEnvironmentProvider = new MockEnvironmentProvider();
            var envFile = new EnvFile(mockEnvironmentProvider);
            await envFile.LoadAsync("./.env-not-default");
            
            Assert.Equal(1, mockEnvironmentProvider.EnvironmentDictionary.Count);
            Assert.Equal("BUZZ", mockEnvironmentProvider.EnvironmentDictionary["FIZZ"]);
        }
    }
}
