﻿//---------------------------------------------------------------------
// <copyright file="TypePromotionUtilsTests.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Test.OData.Query.TDD.Tests.Semantic
{
    using System.Linq;
    using Microsoft.OData.Core.UriParser.TreeNodeKinds;
    using Microsoft.OData.Edm.Library;
    using Microsoft.Test.OData.Query.TDD.Tests.TestUtilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using FluentAssertions;
    using Microsoft.OData.Edm;
    using Microsoft.OData.Core.UriParser;
    using Microsoft.OData.Core.UriParser.Semantic;

    /// <summary>
    /// Unit and short-span integration tests for the methods on TypePromotionUtils.
    /// </summary>
    [TestClass]
    public class TypePromotionUtilsTests
    {
        #region PromoteOperandTypes Tests (For Binary Operators)
        [TestMethod]
        public void EqualsOnComplexAndNullIsSupported()
        {
            IEdmTypeReference left = HardCodedTestModel.GetPersonAddressProp().Type;
            IEdmTypeReference right = null;
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValueOpenPropertyAccessNode(new ConstantNode(null)/*parent*/, "myOpenPropertyname"); // open property's TypeReference is null
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeTrue();
            left.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonAddressProp().Type);
            right.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonAddressProp().Type);
        }

        [TestMethod]
        public void NotEqualsOnNullAndComplexIsSupported()
        {
            IEdmTypeReference left = null;
            IEdmTypeReference right = HardCodedTestModel.GetPersonAddressProp().Type;
            SingleValueNode leftNode = new SingleValueOpenPropertyAccessNode(new ConstantNode(null)/*parent*/, "myOpenPropertyname"); // open property's TypeReference is null
            SingleValueNode rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.NotEqual, leftNode, rightNode, out left, out right);

            result.Should().BeTrue();
            left.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonAddressProp().Type);
            right.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonAddressProp().Type);
        }

        [TestMethod]
        public void EqualsOnComplexAndSameComplexIsSupported()
        {
            IEdmTypeReference left = HardCodedTestModel.GetPersonAddressProp().Type;
            IEdmTypeReference right = HardCodedTestModel.GetPersonAddressProp().Type;
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeTrue();
            left.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonAddressProp().Type);
            right.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonAddressProp().Type);
        }

        [TestMethod]
        public void EqualsOnComplexAndOtherComplexIsNotSupported()
        {
            var otherComplexType = new EdmComplexTypeReference(new EdmComplexType("NS", "OtherComplex"), true);
            IEdmTypeReference left = HardCodedTestModel.GetPersonAddressProp().Type;
            IEdmTypeReference right = otherComplexType;
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeFalse();
            left.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonAddressProp().Type);
            right.ShouldBeEquivalentTo(otherComplexType);
        }

        [TestMethod]
        public void OtherOperandsWithComplexAreNotSupported()
        {
            IEdmTypeReference left = HardCodedTestModel.GetPersonAddressProp().Type;
            IEdmTypeReference right = HardCodedTestModel.GetPersonAddressProp().Type;
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.GreaterThan, leftNode, rightNode, out left, out right);

            result.Should().BeFalse();
            left.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonAddressProp().Type);
            right.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonAddressProp().Type);
        }

        [TestMethod]
        public void EqualsOnEntityAndNullIsSupported()
        {
            IEdmTypeReference left = HardCodedTestModel.GetPersonTypeReference();
            IEdmTypeReference right = null;
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValueOpenPropertyAccessNode(new ConstantNode(null)/*parent*/, "myOpenPropertyname"); // open property's TypeReference is null
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeTrue();
            left.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonTypeReference());
            right.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonTypeReference());
        }

        [TestMethod]
        public void NotEqualsOnNullAndEntityIsSupported()
        {
            IEdmTypeReference left = HardCodedTestModel.GetPersonTypeReference();
            IEdmTypeReference right = null;
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValueOpenPropertyAccessNode(new ConstantNode(null)/*parent*/, "myOpenPropertyname"); // open property's TypeReference is null
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.NotEqual, leftNode, rightNode, out left, out right);

            result.Should().BeTrue();
            left.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonTypeReference());
            right.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonTypeReference());
        }

        [TestMethod]
        public void EqualsOnEntityAndSameEntityIsSupported()
        {
            IEdmTypeReference left = HardCodedTestModel.GetPersonTypeReference();
            IEdmTypeReference right = HardCodedTestModel.GetPersonTypeReference();
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeTrue();
            left.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonTypeReference());
            right.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonTypeReference());
        }

        [TestMethod]
        public void EqualsOnParentEntityAndChildEntityIsSupported()
        {
            IEdmTypeReference left = HardCodedTestModel.GetPersonTypeReference();
            IEdmTypeReference right = HardCodedTestModel.GetEmployeeTypeReference();
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeTrue();
            left.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonTypeReference());
            right.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonTypeReference());
        }

        [TestMethod]
        public void EqualsOnChildEntityAndParentEntityIsSupported()
        {
            IEdmTypeReference left = HardCodedTestModel.GetEmployeeTypeReference();
            IEdmTypeReference right = HardCodedTestModel.GetPersonTypeReference();
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeTrue();
            left.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonTypeReference());
            right.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonTypeReference());
        }

        [TestMethod]
        public void EqualsOnTwoDifferentEntitiesIsNotSupported()
        {
            IEdmTypeReference left = HardCodedTestModel.GetDogTypeReference();
            IEdmTypeReference right = HardCodedTestModel.GetPersonTypeReference();
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeFalse();
            left.ShouldBeEquivalentTo(HardCodedTestModel.GetDogTypeReference());
            right.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonTypeReference());
        }

        [TestMethod]
        public void EqualsOnEntityAndComplexIsNotSupported()
        {
            IEdmTypeReference left = HardCodedTestModel.GetPersonTypeReference();
            IEdmTypeReference right = HardCodedTestModel.GetPersonAddressProp().Type;
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeFalse();
            left.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonTypeReference());
            right.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonAddressProp().Type);
        }

        [TestMethod]
        public void EqualsOnComplexAndEntityIsNotSupported()
        {
            IEdmTypeReference left = HardCodedTestModel.GetPersonAddressProp().Type;
            IEdmTypeReference right = HardCodedTestModel.GetPersonTypeReference();
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeFalse();
            left.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonAddressProp().Type);
            right.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonTypeReference());
        }

        [TestMethod]
        public void EqualsOnEntitiesWithDifferentNullabilityIsSupported()
        {
            var notNullableType = new EdmEntityTypeReference(HardCodedTestModel.GetPersonType(), false);
            var nullableType = new EdmEntityTypeReference(HardCodedTestModel.GetPersonType(), true);

            IEdmTypeReference left = notNullableType;
            IEdmTypeReference right = nullableType;
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeTrue();
            left.ShouldBeEquivalentTo(nullableType);
            right.ShouldBeEquivalentTo(nullableType);

            // Reverse order
            left = nullableType;
            right = notNullableType;
            leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeTrue();
            left.ShouldBeEquivalentTo(nullableType);
            right.ShouldBeEquivalentTo(nullableType);
        }

        [TestMethod]
        public void EqualsOnComplexTypesWithDifferentNullabilityIsSupported()
        {
            var notNullableType = new EdmComplexTypeReference(HardCodedTestModel.GetAddressType(), false);
            var nullableType = new EdmComplexTypeReference(HardCodedTestModel.GetAddressType(), true);

            IEdmTypeReference left = notNullableType;
            IEdmTypeReference right = nullableType;
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeTrue();
            left.ShouldBeEquivalentTo(nullableType);
            right.ShouldBeEquivalentTo(nullableType);

            // Reverse order
            left = nullableType;
            right = notNullableType;
            leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeTrue();
            left.ShouldBeEquivalentTo(nullableType);
            right.ShouldBeEquivalentTo(nullableType);
        }

        [TestMethod]
        public void EqualsOnPrimitiveAndEntityIsNotSupported()
        {
            IEdmTypeReference left = EdmCoreModel.Instance.GetInt32(true);
            IEdmTypeReference right = HardCodedTestModel.GetPersonTypeReference();
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeFalse();
            left.ShouldBeEquivalentTo(EdmCoreModel.Instance.GetInt32(true));
            right.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonTypeReference());
        }

        [TestMethod]
        public void EqualsOnPrimitiveAndComplexIsNotSupported()
        {
            IEdmTypeReference left = EdmCoreModel.Instance.GetInt32(true);
            IEdmTypeReference right = HardCodedTestModel.GetPersonAddressProp().Type;
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeFalse();
            left.ShouldBeEquivalentTo(EdmCoreModel.Instance.GetInt32(true));
            right.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonAddressProp().Type);
        }

        [TestMethod]
        public void EqualsOnEntityAndPrimitiveIsNotSupported()
        {
            IEdmTypeReference left = HardCodedTestModel.GetPersonTypeReference();
            IEdmTypeReference right = EdmCoreModel.Instance.GetInt32(true);
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeFalse();
            left.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonTypeReference());
            right.ShouldBeEquivalentTo(EdmCoreModel.Instance.GetInt32(true));
        }

        [TestMethod]
        public void EqualsOnComplexAndPrimitiveIsNotSupported()
        {
            IEdmTypeReference left = HardCodedTestModel.GetPersonAddressProp().Type;
            IEdmTypeReference right = EdmCoreModel.Instance.GetInt32(true);
            SingleValueNode leftNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", left));
            SingleValueNode rightNode = new SingleValuePropertyAccessNode(new ConstantNode(null)/*parent*/, new EdmStructuralProperty(new EdmEntityType("MyNamespace", "MyEntityType"), "myPropertyName", right));
            var result = TypePromotionUtils.PromoteOperandTypes(BinaryOperatorKind.Equal, leftNode, rightNode, out left, out right);

            result.Should().BeFalse();
            left.ShouldBeEquivalentTo(HardCodedTestModel.GetPersonAddressProp().Type);
            right.ShouldBeEquivalentTo(EdmCoreModel.Instance.GetInt32(true));
        }
        #endregion

        #region PromoteOperandType Tests (For Unary Operators)

        [TestMethod]
        public void NegateOnEntityTypeIsNotSupported()
        {
            IEdmTypeReference type = HardCodedTestModel.GetPersonAddressProp().Type;
            var result = TypePromotionUtils.PromoteOperandType(UnaryOperatorKind.Negate, ref type);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void NegateOnComplexTypeIsNotSupported()
        {
            IEdmTypeReference type = HardCodedTestModel.GetPersonAddressProp().Type;
            var result = TypePromotionUtils.PromoteOperandType(UnaryOperatorKind.Negate, ref type);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void NegateOnNumericTypeIsSupported()
        {
            IEdmTypeReference type = EdmCoreModel.Instance.GetInt32(false);
            var result = TypePromotionUtils.PromoteOperandType(UnaryOperatorKind.Negate, ref type);

            result.Should().BeTrue();
            type.ShouldBeEquivalentTo(EdmCoreModel.Instance.GetInt32(false));
        }

        [TestMethod]
        public void NegateOnDateTimeOffsetIsNotSupported()
        {
            IEdmTypeReference type = EdmCoreModel.Instance.GetTemporal(EdmPrimitiveTypeKind.DateTimeOffset, false);
            var result = TypePromotionUtils.PromoteOperandType(UnaryOperatorKind.Negate, ref type);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void NegateOnTimeOfDayIsNotSupported()
        {
            IEdmTypeReference type = EdmCoreModel.Instance.GetTemporal(EdmPrimitiveTypeKind.TimeOfDay, false);
            var result = TypePromotionUtils.PromoteOperandType(UnaryOperatorKind.Negate, ref type);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void NegateOnNullIsSupported()
        {
            IEdmTypeReference type = null;
            var result = TypePromotionUtils.PromoteOperandType(UnaryOperatorKind.Negate, ref type);

            result.Should().BeTrue();
            type.Should().BeNull();
        }

        [TestMethod]
        public void NotOnComplexTypeIsNotSupported()
        {
            IEdmTypeReference type = HardCodedTestModel.GetPersonAddressProp().Type;
            var result = TypePromotionUtils.PromoteOperandType(UnaryOperatorKind.Not, ref type);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void NotOnNumericTypeIsNotSupported()
        {
            IEdmTypeReference type = EdmCoreModel.Instance.GetInt32(false);
            var result = TypePromotionUtils.PromoteOperandType(UnaryOperatorKind.Not, ref type);

            result.Should().BeFalse();
        }
        #endregion

        [TestMethod]
        public void TryFindFunctionSignatureWithNumberOfArgumentsReturnsAFunctionSignatureIfMatchFound()
        {
            // regression test for: FunctionCallBinder should validate that the number of parameters matches for canonical function calls on open properties
            FunctionSignatureWithReturnType[] functions = new FunctionSignatureWithReturnType[]
            {
                new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDateTimeOffset(true), new IEdmTypeReference[] {EdmCoreModel.Instance.GetString(true)}),
            };

            // we specifically want to find just the first function that matches the number of arguments, we don't care about
            // ambiguity here because we're already in an ambiguous case where we don't know what kind of types 
            // those arguments are.
            functions.FirstOrDefault(candidateFunction => candidateFunction.ArgumentTypes.Count() == 1).Should().NotBeNull();
        }

        [TestMethod]
        public void TryFundFunctionSignatureAllowsAmbiguousFunctionCall()
        {
            FunctionSignatureWithReturnType first = new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDateTimeOffset(true), new IEdmTypeReference[] { EdmCoreModel.Instance.GetString(true) });
            FunctionSignatureWithReturnType second = new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDateTimeOffset(true), new IEdmTypeReference[] { EdmCoreModel.Instance.GetInt32(true) });
            FunctionSignatureWithReturnType[] functions = new FunctionSignatureWithReturnType[]
            {
                first,
                second
            };

            // we specifically want to find just the first function that matches the number of arguments, we don't care about
            // ambiguity here because we're already in an ambiguous case where we don't know what kind of types 
            // those arguments are.
            functions.FirstOrDefault(candidateFunction => candidateFunction.ArgumentTypes.Count() == 1).Should().Be(first);
        }

        [TestMethod]
        public void TryFindFunctionSignatureWithNumberOfArgumentsReturnsNothingfNoMatchFound()
        {
            // regression test for: FunctionCallBinder should validate that the number of parameters matches for canonical function calls on open properties
            FunctionSignatureWithReturnType[] functions = new FunctionSignatureWithReturnType[]
            {
                new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDateTimeOffset(true), new IEdmTypeReference[] {EdmCoreModel.Instance.GetString(true)}),
            };

            // we specifically want to find just the first function that matches the number of arguments, we don't care about
            // ambiguity here because we're already in an ambiguous case where we don't know what kind of types 
            // those arguments are.
            functions.FirstOrDefault(candidateFunction => candidateFunction.ArgumentTypes.Count() == 2).Should().BeNull();
        }

        [TestMethod]
        public void PrimitiveTypesOfSameKindCanConvertToEachOther()
        {
            var stringType = new EdmStringTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.String), true);
            var primitiveType = new EdmPrimitiveTypeReference(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.String), true);
            Assert.IsTrue(TypePromotionUtils.CanConvertTo(null, stringType, primitiveType));
            Assert.IsTrue(TypePromotionUtils.CanConvertTo(null, primitiveType, stringType));
        }
    }
}
