﻿namespace OmniXaml.Tests.Parsers.MarkupExtensionParserTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using OmniXaml.Parsers.MarkupExtensions;
    using Resources;
    using Sprache;
    using ParseException = OmniXaml.ParseException;

    public class ParsingTests
    {
        [Fact]
        public void SimpleExtension()
        {
            var actual = MarkupExtensionParser.MarkupExtension.Parse("{Dummy}");
            Assert.Equal(new MarkupExtensionNode(new IdentifierNode("DummyExtension")), actual);
        }

        [Fact]
        public void PrefixedExtension()
        {
            var actual = MarkupExtensionParser.MarkupExtension.Parse("{x:Dummy}");
            Assert.Equal(new MarkupExtensionNode(new IdentifierNode("x", "DummyExtension")), actual);
        }

        [Fact]
        public void ExtensionWithTwoPositionalOptions()
        {
            var actual = MarkupExtensionParser.MarkupExtension.Parse("{Dummy Value1,Value2}");
            var options = new OptionsCollection { new PositionalOption("Value1"), new PositionalOption("Value2") };
            Assert.Equal(new MarkupExtensionNode(new IdentifierNode("DummyExtension"), options), actual);
        }

        [Fact]
        public void ExtensionWithDottedPositionalOption()
        {
            var actual = MarkupExtensionParser.MarkupExtension.Parse("{Dummy Direct.Value}");
            var options = new OptionsCollection { new PositionalOption("Direct.Value"), };
            Assert.Equal(new MarkupExtensionNode(new IdentifierNode("DummyExtension"), options), actual);
        }

        [Fact]
        public void DelimitedBy()
        {
            var identifier = from c in Parse.LetterOrDigit.Many() select new string(c.ToArray());

            var parser = from id in identifier.DelimitedBy(Parse.Char(',').Token()) select id;
            var parsed = parser.Parse("SomeValue   ,  AnotherValue");
            Assert.Equal(new[] { "SomeValue", "AnotherValue" }, parsed.ToList());
        }

        [Fact]
        public void ExtensionWithPositionalAndAssignmentOptions()
        {
            var actual = MarkupExtensionParser.MarkupExtension.Parse("{Dummy Value,Property='Some Value'}");
            var options = new OptionsCollection { new PositionalOption("Value"), new PropertyOption("Property", new StringNode("Some Value")) };
            Assert.Equal(new MarkupExtensionNode(new IdentifierNode("DummyExtension"), options), actual);
        }

        [Fact]
        public void ExtensionWithPositionalAndAssignmentOptionsAndSpaces()
        {
            var actual = MarkupExtensionParser.MarkupExtension.Parse("{Dummy Property='Some Value', OtherProperty=OtherValue}");
            var options = new OptionsCollection { new PropertyOption("Property", new StringNode("Some Value")), new PropertyOption("OtherProperty", new StringNode("OtherValue")) };
            Assert.Equal(new MarkupExtensionNode(new IdentifierNode("DummyExtension"), options), actual);
        }

        [Fact]
        public void PropertyWithQuotedValueAndSpaces()
        {
            var actual = MarkupExtensionParser.MarkupExtension.Parse("{Dummy Property= 'Some Value' }");
            var options = new OptionsCollection { new PropertyOption("Property", new StringNode("Some Value"))};
            Assert.Equal(new MarkupExtensionNode(new IdentifierNode("DummyExtension"), options), actual);
        }

        [Fact]
        public void PropertyWithDirectValueAndSpaces()
        {
            var actual = MarkupExtensionParser.MarkupExtension.Parse("{Dummy Property = SomeValue }");
            var options = new OptionsCollection { new PropertyOption("Property", new StringNode("SomeValue")) };
            Assert.Equal(new MarkupExtensionNode(new IdentifierNode("DummyExtension"), options), actual);
        }

        [Fact]
        public void PropertyWithMoreThanOneSpaceBetweenTokensPropertiesOnly()
        {
            var actual = MarkupExtensionParser.MarkupExtension.Parse("{Dummy   Property  =   SomeValue    }");
            var options = new OptionsCollection { new PropertyOption("Property", new StringNode("SomeValue")) };
            Assert.Equal(new MarkupExtensionNode(new IdentifierNode("DummyExtension"), options), actual);
        }

        [Fact]
        public void PropertyWithMoreThanOneSpaceBetweenTokensWithPositional()
        {
            var actual = MarkupExtensionParser.MarkupExtension.Parse("{Dummy  Arg1,  Arg2,   Property  =   SomeValue    }");
            var options = new OptionsCollection
            {
                new PositionalOption("Arg1"),
                new PositionalOption("Arg2"),
                new PropertyOption("Property", new StringNode("SomeValue"))
            };
            Assert.Equal(new MarkupExtensionNode(new IdentifierNode("DummyExtension"), options), actual);
        }

        [Fact]
        public void PropertyWithDirectValueAndSpacesMultipleAssignments()
        {
            var actual = MarkupExtensionParser.MarkupExtension.Parse("{Dummy Property = SomeValue , Other='Other value'}");
            var options = new OptionsCollection
            {
                new PropertyOption("Property", new StringNode("SomeValue")),
                new PropertyOption("Other", new StringNode("Other value")),
            };
            Assert.Equal(new MarkupExtensionNode(new IdentifierNode("DummyExtension"), options), actual);
        }

        [Fact]
        public void AssignmentOfDirectValue()
        {
            var actual = MarkupExtensionParser.Assignment.Parse("Property=SomeValue");
            Assert.Equal(new AssignmentNode("Property", new StringNode("SomeValue")), actual);
        }

        [Fact]
        public void AssignmentOfDottedDirectValue()
        {
            var actual = MarkupExtensionParser.Assignment.Parse("Property=Some.Value");
            Assert.Equal(new AssignmentNode("Property", new StringNode("Some.Value")), actual);
        }

        [Fact]
        public void AssignmentOfDirectValueWithColon()
        {
            var actual = MarkupExtensionParser.Assignment.Parse("Property=x:SomeValue");
            Assert.Equal(new AssignmentNode("Property", new StringNode("x:SomeValue")), actual);
        }

        [Fact]
        public void AssignmentOfQuotedValue()
        {
            var actual = MarkupExtensionParser.Assignment.Parse("Property='value with spaces'");
            Assert.Equal(new AssignmentNode("Property", new StringNode("value with spaces")), actual);
        }

        [Fact]
        public void ParsePositionalAndPropertyOptions()
        {
            var actual = MarkupExtensionParser.Options.Parse("value1,Property1=Value1,Property2='Some value'");
            var expected = new OptionsCollection(new List<Option>
            {
                new PositionalOption("value1"),
                new PropertyOption("Property1", new StringNode("Value1")),
                new PropertyOption("Property2", new StringNode("Some value"))
            });

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void ParsePropertyWithExtension()
        {
            var actual = MarkupExtensionParser.Assignment.Parse("Value={Dummy}");
            var markupExtensionNode = new MarkupExtensionNode(new IdentifierNode("DummyExtension"));
            Assert.Equal(new AssignmentNode("Value", markupExtensionNode), actual);
        }

        [Fact]
        public void ComposedExtensionTemplateBindingWithConverter()
        {
            var actual =
               MarkupExtensionParser.MarkupExtension.Parse("{TemplateBinding Path=IsFloatingWatermarkVisible, Converter={Type FooBar}}");

            var expected = MarkupExtensionNodeResources.ComposedExtensionTemplateBindingWithConverter();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AssignmentOfDirectValueWithSpaces()
        {
            var actual = MarkupExtensionParser.Assignment.Parse("Property = SomeValue");
            Assert.Equal(new AssignmentNode("Property", new StringNode("SomeValue")), actual);
        }

        [Fact]
        public void PositionalWithSpecialChars()
        {
            var actual = MarkupExtensionParser.MarkupExtension.Parse("{Binding Foo^.Bar}");
            var options = new OptionsCollection { new PositionalOption("Foo^.Bar") };
            Assert.Equal(new MarkupExtensionNode(new IdentifierNode("BindingExtension"), options), actual);
        }

        [Fact]
        public void ComposedExtension()
        {
            var actual =
                MarkupExtensionParser.MarkupExtension.Parse(
                    "{Binding Width, RelativeSource={RelativeSource FindAncestor, AncestorLevel=1, AncestorType={x:Type Grid}}}");

            var expected = MarkupExtensionNodeResources.ComposedExtension();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("MyIdentifier")]
        [InlineData("Identifier123")]
        [InlineData("Identifier_Suffix_123")]
        public void Identifier(string str)
        {
            var identifier = MarkupExtensionParser.Identifier.Parse(str);
            Assert.Equal(str, identifier);
        }

        [Theory]
        [InlineData("#other.Tag")]
        [InlineData("!HasErrors")]
        [InlineData("Name^.Length")]
        public void PositionalWithDot(string str)
        {
            var positional = MarkupExtensionParser.Positional.Parse(str);
            Assert.Equal(new PositionalOption(str), positional);
        }

        [Theory]
        [InlineData("{Dummy Hello")]
        [InlineData("{Dummy Property=SomeValue")]
        [InlineData("{Dummy Arg1, Arg2, Property='Some value'")]
        public void UnfinishedExtension(string str)
        {
            Assert.Throws<Sprache.ParseException>(() => MarkupExtensionParser.MarkupExtension.Parse(str));
        }
    }
}
