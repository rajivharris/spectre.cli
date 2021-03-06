using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Shouldly;
using Spectre.Cli.Exceptions;
using Spectre.Cli.Testing;
using Spectre.Cli.Testing.Data.Commands;
using Xunit;

namespace Spectre.Cli.Tests
{
    public sealed partial class CommandAppTests
    {
        public sealed class FlagValues
        {
            [SuppressMessage("Performance", "CA1812", Justification = "It's OK")]
            private sealed class FlagSettings : CommandSettings
            {
                [CommandOption("--serve [PORT]")]
                public FlagValue<int> Serve { get; set; }
            }

            [SuppressMessage("Performance", "CA1812", Justification = "It's OK")]
            private sealed class FlagSettingsWithNullableValueType : CommandSettings
            {
                [CommandOption("--serve [PORT]")]
                public FlagValue<int?> Serve { get; set; }
            }

            [SuppressMessage("Performance", "CA1812", Justification = "It's OK")]
            private sealed class FlagSettingsWithOptionalOptionButNoFlagValue : CommandSettings
            {
                [CommandOption("--serve [PORT]")]
                public int Serve { get; set; }
            }

            [SuppressMessage("Performance", "CA1812", Justification = "It's OK")]
            private sealed class FlagSettingsWithDefaultValue : CommandSettings
            {
                [CommandOption("--serve [PORT]")]
                [DefaultValue(987)]
                public FlagValue<int> Serve { get; set; }
            }

            [Fact]
            public void Should_Throw_If_Command_Option_Value_Is_Optional_But_Type_Is_Not_A_Flag_Value()
            {
                // Given
                var app = new CommandApp();
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                    config.AddCommand<GenericCommand<FlagSettingsWithOptionalOptionButNoFlagValue>>("foo");
                });

                // When
                var result = Record.Exception(() => app.Run(new[] { "foo", "--serve", "123" }));

                // Then
                result.ShouldBeOfType<ConfigurationException>().And(ex =>
                {
                    ex.Message.ShouldBe("The option 'serve' has an optional value but does not implement IFlagValue.");
                });
            }

            [Fact]
            public void Should_Set_Flag_And_Value_If_Both_Were_Provided()
            {
                // Given
                var app = new CommandAppFixture();
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                    config.AddCommand<GenericCommand<FlagSettings>>("foo");
                });

                // When
                var (result, _, _, settings) = app.Run(new[]
                {
                    "foo", "--serve", "123",
                });

                // Then
                result.ShouldBe(0);
                settings.ShouldBeOfType<FlagSettings>().And(flag =>
                {
                    flag.Serve.IsSet.ShouldBeTrue();
                    flag.Serve.Value.ShouldBe(123);
                });
            }

            [Fact]
            public void Should_Only_Set_Flag_If_No_Value_Was_Provided()
            {
                // Given
                var app = new CommandAppFixture();
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                    config.AddCommand<GenericCommand<FlagSettings>>("foo");
                });

                // When
                var (result, _, _, settings) = app.Run(new[]
                {
                    "foo", "--serve",
                });

                // Then
                result.ShouldBe(0);
                settings.ShouldBeOfType<FlagSettings>().And(flag =>
                {
                    flag.Serve.IsSet.ShouldBeTrue();
                    flag.Serve.Value.ShouldBe(0);
                });
            }

            [Fact]
            public void Should_Set_Value_To_Default_Value_If_None_Was_Explicitly_Set()
            {
                // Given
                var app = new CommandAppFixture();
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                    config.AddCommand<GenericCommand<FlagSettingsWithDefaultValue>>("foo");
                });

                // When
                var (result, _, _, settings) = app.Run(new[]
                {
                    "foo", "--serve",
                });

                // Then
                result.ShouldBe(0);
                settings.ShouldBeOfType<FlagSettingsWithDefaultValue>().And(flag =>
                {
                    flag.Serve.IsSet.ShouldBeTrue();
                    flag.Serve.Value.ShouldBe(987);
                });
            }

            [Fact]
            public void Should_Create_Unset_Instance_If_Flag_Was_Not_Set()
            {
                // Given
                var app = new CommandAppFixture();
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                    config.AddCommand<GenericCommand<FlagSettings>>("foo");
                });

                // When
                var (result, _, _, settings) = app.Run(new[]
                {
                    "foo",
                });

                // Then
                result.ShouldBe(0);
                settings.ShouldBeOfType<FlagSettings>().And(flag =>
                {
                    flag.Serve.IsSet.ShouldBeFalse();
                    flag.Serve.Value.ShouldBe(0);
                });
            }

            [Fact]
            public void Should_Create_Unset_Instance_With_Null_For_Nullable_Value_Type_If_Flag_Was_Not_Set()
            {
                // Given
                var app = new CommandAppFixture();
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                    config.AddCommand<GenericCommand<FlagSettingsWithNullableValueType>>("foo");
                });

                // When
                var (result, _, _, settings) = app.Run(new[]
                {
                    "foo",
                });

                // Then
                result.ShouldBe(0);
                settings.ShouldBeOfType<FlagSettingsWithNullableValueType>().And(flag =>
                {
                    flag.Serve.IsSet.ShouldBeFalse();
                    flag.Serve.Value.ShouldBeNull();
                });
            }

            [Theory]
            [InlineData("Foo", true, "Set=True, Value=Foo")]
            [InlineData("Bar", false, "Set=False, Value=Bar")]
            public void Should_Return_Correct_String_Representation_From_ToString(
                string value,
                bool isSet,
                string expected)
            {
                // Given
                var flag = new FlagValue<string>();
                flag.Value = value;
                flag.IsSet = isSet;

                // When
                var result = flag.ToString();

                // Then
                result.ShouldBe(expected);
            }

            [Theory]
            [InlineData(true, "Set=True")]
            [InlineData(false, "Set=False")]
            public void Should_Return_Correct_String_Representation_From_ToString_If_Value_Is_Not_Set(
                bool isSet,
                string expected)
            {
                // Given
                var flag = new FlagValue<string>();
                flag.IsSet = isSet;

                // When
                var result = flag.ToString();

                // Then
                result.ShouldBe(expected);
            }
        }
    }
}
