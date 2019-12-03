﻿using System;
using Xunit;

namespace NetFabric.Reflection.UnitTests
{
    public partial class TypeExtensionsTests
    {
        public static TheoryData<string, Type> InstanceProperties =>
            new TheoryData<string, Type>
            {
                { "Property", typeof(int) },
                { "InheritedProperty", typeof(int) },
            };

        [Theory]
        [MemberData(nameof(InstanceProperties))]
        public void GetPublicProperty_Should_ReturnProperty(string propertyName, Type propertyType)
        {
            // Arrange
            var type = typeof(TestData.PropertiesAndMethods);

            // Act
            var result = type.GetPublicProperty(propertyName);

            // Assert   
            Assert.NotNull(result);
            Assert.Equal(propertyName, result.Name);
            Assert.Equal(propertyType, result.PropertyType);
        }

        public static TheoryData<string> ExplicitInstanceProperties =>
            new TheoryData<string>
            {
                "ExplicitProperty",
                "StaticProperty",
            };

        [Theory]
        [MemberData(nameof(ExplicitInstanceProperties))]
        public void GetPublicProperty_With_ExplicitOrStaticProperties_Should_ReturnNull(string propertyName)
        {
            // Arrange
            var type = typeof(TestData.PropertiesAndMethods);

            // Act
            var result = type.GetPublicProperty(propertyName);

            // Assert   
            Assert.Null(result);
        }
    }
}
