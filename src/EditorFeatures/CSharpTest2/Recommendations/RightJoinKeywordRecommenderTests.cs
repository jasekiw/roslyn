// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Recommendations;

[Trait(Traits.Feature, Traits.Features.KeywordRecommending)]
public sealed class RightJoinKeywordRecommenderTests : KeywordRecommenderTests
{
    [Fact]
    public Task TestNotAtRoot_Interactive()
        => VerifyAbsenceAsync(SourceCodeKind.Script,
@"$$");

    [Fact]
    public Task TestNotInEmptyStatement()
        => VerifyAbsenceAsync(AddInsideMethod(
@"$$"));

    [Fact]
    public Task TestNotAtEndOfPreviousClause()
        => VerifyAbsenceAsync(AddInsideMethod(
@"var q = from x in y$$"));

    [Fact]
    public Task TestNewClause()
        => VerifyKeywordAsync(AddInsideMethod(
            """
            var q = from x in y
                      $$
            """));

    [Fact]
    public Task TestAfterPreviousClause()
        => VerifyKeywordAsync(AddInsideMethod(
            """
            var v = from x in y
                      where x > y
                      $$
            """));

    [Fact]
    public Task TestAfterPreviousJoinClause()
        => VerifyKeywordAsync(AddInsideMethod(
            """
            var v = from x in y
                      rightjoin z in w on x equals z
                      $$
            """));

    [Fact]
    public Task TestBetweenClauses()
        => VerifyKeywordAsync(AddInsideMethod(
            """
            var q = from x in y
                      $$
                      from z in w
            """));

    [Fact]
    public Task TestNotAfterRightJoin()
        => VerifyAbsenceAsync(AddInsideMethod(
            """
            var q = from x in y
                      rightjoin $$
                      from z in w
            """));
}
