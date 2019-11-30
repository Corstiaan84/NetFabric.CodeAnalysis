﻿using NetFabric.Assertive;
using System;
using Xunit;

namespace NetFabric.CodeAnalysis.Reflection.UnitTests
{
    public partial class TypeExtensionsTests
    {
        public static TheoryData<string, Type> Properties =>
            new TheoryData<string, Type>
            {
                { "Property", typeof(int) },
                { "InheritedProperty", typeof(int) },
            };

        [Theory]
        [MemberData(nameof(Properties))]
        public void GetProperty_Should_ReturnProperty(string propertyName, Type propertyType)
        {
            // Arrange
            var type = typeof(TestData.PropertiesAndMethods);

            // Act
            var result = type.GetPublicProperty(propertyName);

            // Assert   
            result.Must()
                .BeNotNull()
                .EvaluatesTrue(property =>
                    property.Name == propertyName &&
                    property.PropertyType == propertyType);
        }

        public static TheoryData<string> ExplicitProperties =>
            new TheoryData<string>
            {
                "ExplicitProperty",
                "StaticProperty",
            };

        [Theory]
        [MemberData(nameof(ExplicitProperties))]
        public void GetProperty_With_ExplicitOrStaticProperties_Should_ReturnNull(string propertyName)
        {
            // Arrange
            var type = typeof(TestData.PropertiesAndMethods);

            // Act
            var result = type.GetPublicProperty(propertyName);

            // Assert   
            result.Must()
                .BeNull();
        }
    }
}