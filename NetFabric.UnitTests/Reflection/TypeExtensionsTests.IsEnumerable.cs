﻿using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace NetFabric.Reflection.UnitTests
{
    public partial class TypeExtensionsTests
    {
        public static TheoryData<Type, Type, Type, Type, Type, Type> Enumerables =>
            new TheoryData<Type, Type, Type, Type, Type, Type>
            {
                { 
                    typeof(TestData.Enumerable<>).MakeGenericType(typeof(int)),
                    typeof(TestData.Enumerable<>).MakeGenericType(typeof(int)),
                    typeof(TestData.Enumerator<>).MakeGenericType(typeof(int)),
                    typeof(TestData.Enumerator<>).MakeGenericType(typeof(int)),
                    null,
                    typeof(int)
                },
                { 
                    typeof(TestData.ExplicitEnumerable),
                    typeof(IEnumerable),
                    typeof(IEnumerator),
                    typeof(IEnumerator),
                    null,
                    typeof(object)
                },
                { 
                    typeof(TestData.ExplicitEnumerable<>).MakeGenericType(typeof(int)),
                    typeof(IEnumerable<>).MakeGenericType(typeof(int)),
                    typeof(IEnumerator<>).MakeGenericType(typeof(int)),
                    typeof(IEnumerator),
                    typeof(IDisposable),
                    typeof(int)
                },
                { 
                    typeof(TestData.RangeEnumerable),
                    typeof(TestData.RangeEnumerable),
                    typeof(TestData.RangeEnumerable.Enumerator),
                    typeof(TestData.RangeEnumerable.Enumerator),
                    null,
                    typeof(int)
                },
            };

        [Theory]
        [MemberData(nameof(Enumerables))]
        public void IsEnumerable_Should_ReturnTrue(Type type, Type getEnumeratorDeclaringType, Type currentDeclaringType, Type moveNextDeclaringType, Type disposeDeclaringType, Type itemType)
        {
            // Arrange

            // Act
            var result = type.IsEnumerable(out var enumerableInfo);

            // Assert   
            Assert.True(result);

            Assert.Equal(getEnumeratorDeclaringType, enumerableInfo.EnumerableType);
            Assert.Equal(enumerableInfo.GetEnumerator?.ReturnType, enumerableInfo.EnumeratorType);
            Assert.Equal(itemType, enumerableInfo.ItemType);

            Assert.NotNull(enumerableInfo.GetEnumerator);
            Assert.Equal("GetEnumerator", enumerableInfo.GetEnumerator.Name);
            Assert.Equal(getEnumeratorDeclaringType, enumerableInfo.GetEnumerator.DeclaringType);
            Assert.Empty(enumerableInfo.GetEnumerator.GetParameters());

            Assert.NotNull(enumerableInfo.Current);
            Assert.Equal("Current", enumerableInfo.Current.Name);
            Assert.Equal(currentDeclaringType, enumerableInfo.Current.DeclaringType);
            Assert.Equal(itemType, enumerableInfo.Current.PropertyType);

            Assert.NotNull(enumerableInfo.MoveNext);
            Assert.Equal("MoveNext", enumerableInfo.MoveNext.Name);
            Assert.Equal(moveNextDeclaringType, enumerableInfo.MoveNext.DeclaringType);
            Assert.Empty(enumerableInfo.MoveNext.GetParameters());

            if (disposeDeclaringType is null)
            {
                Assert.Null(enumerableInfo.Dispose);
            }
            else
            {
                Assert.NotNull(enumerableInfo.Dispose);
                Assert.Equal("Dispose", enumerableInfo.Dispose.Name);
                Assert.Equal(disposeDeclaringType, enumerableInfo.Dispose.DeclaringType);
                Assert.Empty(enumerableInfo.Dispose.GetParameters());
            }
        }

        public static TheoryData<Type, Type, Type, Type, Type, Type> InvalidEnumerables =>
            new TheoryData<Type, Type, Type, Type, Type, Type>
            {
                { 
                    typeof(TestData.MissingGetEnumeratorEnumerable),
                    null,
                    null,
                    null,
                    null,
                    null
                },
                { 
                    typeof(TestData.MissingCurrentEnumerable),
                    typeof(TestData.MissingCurrentEnumerable),
                    null,
                    typeof(TestData.MissingCurrentEnumerator),
                    null,
                    null
                },
                { 
                    typeof(TestData.MissingMoveNextEnumerable<int>),
                    typeof(TestData.MissingMoveNextEnumerable<int>),
                    typeof(TestData.MissingMoveNextEnumerator<int>),
                    null,
                    null,
                    typeof(int)
                },
            };

        [Theory]
        [MemberData(nameof(InvalidEnumerables))]
        public void IsEnumerable_With_MissingFeatures_Should_ReturnFalse(Type type, Type getEnumeratorDeclaringType, Type currentDeclaringType, Type moveNextDeclaringType, Type disposeDeclaringType, Type itemType)
        {
            // Arrange

            // Act
            var result = type.IsEnumerable(out var enumerableInfo);

            // Assert   
            Assert.False(result);

            Assert.Equal(getEnumeratorDeclaringType, enumerableInfo.EnumerableType);
            Assert.Equal(enumerableInfo.GetEnumerator?.ReturnType, enumerableInfo.EnumeratorType);
            Assert.Equal(itemType, enumerableInfo.ItemType);

            if (getEnumeratorDeclaringType is null)
            {
                Assert.Null(enumerableInfo.GetEnumerator);
            }
            else
            {
                Assert.NotNull(enumerableInfo.GetEnumerator);
                Assert.Equal("GetEnumerator", enumerableInfo.GetEnumerator.Name);
                Assert.Equal(getEnumeratorDeclaringType, enumerableInfo.GetEnumerator.DeclaringType);
                Assert.Empty(enumerableInfo.GetEnumerator.GetParameters());
            }

            if (currentDeclaringType is null)
            {
                Assert.Null(enumerableInfo.Current);
            }
            else
            {
                Assert.NotNull(enumerableInfo.Current);
                Assert.Equal("Current", enumerableInfo.Current.Name);
                Assert.Equal(currentDeclaringType, enumerableInfo.Current.DeclaringType);
                Assert.Equal(itemType, enumerableInfo.Current.PropertyType);
            }

            if (moveNextDeclaringType is null)
            {
                Assert.Null(enumerableInfo.MoveNext);
            }
            else
            {
                Assert.NotNull(enumerableInfo.MoveNext);
                Assert.Equal("MoveNext", enumerableInfo.MoveNext.Name);
                Assert.Equal(moveNextDeclaringType, enumerableInfo.MoveNext.DeclaringType);
                Assert.Empty(enumerableInfo.MoveNext.GetParameters());
            }

            if (disposeDeclaringType is null)
            {
                Assert.Null(enumerableInfo.Dispose);
            }
            else
            {
                Assert.NotNull(enumerableInfo.Dispose);
                Assert.Equal("Dispose", enumerableInfo.Dispose.Name);
                Assert.Equal(disposeDeclaringType, enumerableInfo.Dispose.DeclaringType);
                Assert.Empty(enumerableInfo.Dispose.GetParameters());
            }
        }
    }
}
