// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests;

public sealed class LeftRightJoinParsingTests : ParsingTests
{
    public LeftRightJoinParsingTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void LeftJoin_FollowedBySelect()
    {
        UsingExpression("from s in ss leftjoin d in ds on s.K equals d.K select d", TestOptions.RegularPreview);
        N(SyntaxKind.QueryExpression);
        {
            N(SyntaxKind.FromClause);
            {
                N(SyntaxKind.FromKeyword);
                N(SyntaxKind.IdentifierToken, "s");
                N(SyntaxKind.InKeyword);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "ss");
                }
            }
            N(SyntaxKind.QueryBody);
            {
                N(SyntaxKind.JoinClause);
                {
                    N(SyntaxKind.LeftJoinKeyword);
                    N(SyntaxKind.IdentifierToken, "d");
                    N(SyntaxKind.InKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "ds");
                    }
                    N(SyntaxKind.OnKeyword);
                    N(SyntaxKind.SimpleMemberAccessExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "s");
                        }
                        N(SyntaxKind.DotToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "K");
                        }
                    }
                    N(SyntaxKind.EqualsKeyword);
                    N(SyntaxKind.SimpleMemberAccessExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "d");
                        }
                        N(SyntaxKind.DotToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "K");
                        }
                    }
                }
                N(SyntaxKind.SelectClause);
                {
                    N(SyntaxKind.SelectKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "d");
                    }
                }
            }
        }
        EOF();
    }

    [Fact]
    public void RightJoin_FollowedBySelect()
    {
        UsingExpression("from d in ds rightjoin s in ss on d.K equals s.K select s", TestOptions.RegularPreview);
        N(SyntaxKind.QueryExpression);
        {
            N(SyntaxKind.FromClause);
            {
                N(SyntaxKind.FromKeyword);
                N(SyntaxKind.IdentifierToken, "d");
                N(SyntaxKind.InKeyword);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "ds");
                }
            }
            N(SyntaxKind.QueryBody);
            {
                N(SyntaxKind.JoinClause);
                {
                    N(SyntaxKind.RightJoinKeyword);
                    N(SyntaxKind.IdentifierToken, "s");
                    N(SyntaxKind.InKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "ss");
                    }
                    N(SyntaxKind.OnKeyword);
                    N(SyntaxKind.SimpleMemberAccessExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "d");
                        }
                        N(SyntaxKind.DotToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "K");
                        }
                    }
                    N(SyntaxKind.EqualsKeyword);
                    N(SyntaxKind.SimpleMemberAccessExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "s");
                        }
                        N(SyntaxKind.DotToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "K");
                        }
                    }
                }
                N(SyntaxKind.SelectClause);
                {
                    N(SyntaxKind.SelectKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "s");
                    }
                }
            }
        }
        EOF();
    }

    [Fact]
    public void LeftJoin_WithRangeVariableType()
    {
        UsingExpression("from s in ss leftjoin D d in ds on s.K equals d.K select d", TestOptions.RegularPreview);
        N(SyntaxKind.QueryExpression);
        {
            N(SyntaxKind.FromClause);
            {
                N(SyntaxKind.FromKeyword);
                N(SyntaxKind.IdentifierToken, "s");
                N(SyntaxKind.InKeyword);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "ss");
                }
            }
            N(SyntaxKind.QueryBody);
            {
                N(SyntaxKind.JoinClause);
                {
                    N(SyntaxKind.LeftJoinKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "D");
                    }
                    N(SyntaxKind.IdentifierToken, "d");
                    N(SyntaxKind.InKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "ds");
                    }
                    N(SyntaxKind.OnKeyword);
                    N(SyntaxKind.SimpleMemberAccessExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "s");
                        }
                        N(SyntaxKind.DotToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "K");
                        }
                    }
                    N(SyntaxKind.EqualsKeyword);
                    N(SyntaxKind.SimpleMemberAccessExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "d");
                        }
                        N(SyntaxKind.DotToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "K");
                        }
                    }
                }
                N(SyntaxKind.SelectClause);
                {
                    N(SyntaxKind.SelectKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "d");
                    }
                }
            }
        }
        EOF();
    }

    [Fact]
    public void LeftJoin_WithInto_ParsesAndIsReportedByBinder()
    {
        UsingExpression("from s in ss leftjoin d in ds on s.K equals d.K into g select g", TestOptions.RegularPreview);
        N(SyntaxKind.QueryExpression);
        {
            N(SyntaxKind.FromClause);
            {
                N(SyntaxKind.FromKeyword);
                N(SyntaxKind.IdentifierToken, "s");
                N(SyntaxKind.InKeyword);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "ss");
                }
            }
            N(SyntaxKind.QueryBody);
            {
                N(SyntaxKind.JoinClause);
                {
                    N(SyntaxKind.LeftJoinKeyword);
                    N(SyntaxKind.IdentifierToken, "d");
                    N(SyntaxKind.InKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "ds");
                    }
                    N(SyntaxKind.OnKeyword);
                    N(SyntaxKind.SimpleMemberAccessExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "s");
                        }
                        N(SyntaxKind.DotToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "K");
                        }
                    }
                    N(SyntaxKind.EqualsKeyword);
                    N(SyntaxKind.SimpleMemberAccessExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "d");
                        }
                        N(SyntaxKind.DotToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "K");
                        }
                    }
                    N(SyntaxKind.JoinIntoClause);
                    {
                        N(SyntaxKind.IntoKeyword);
                        N(SyntaxKind.IdentifierToken, "g");
                    }
                }
                N(SyntaxKind.SelectClause);
                {
                    N(SyntaxKind.SelectKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "g");
                    }
                }
            }
        }
        EOF();
    }

    [Fact]
    public void LeftJoin_CSharp14()
    {
        UsingExpression("from s in ss leftjoin d in ds on s.K equals d.K select d", TestOptions.Regular14,
            // (1,14): error CS8652: The feature 'leftjoin and rightjoin query clauses' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
            // from s in ss leftjoin d in ds on s.K equals d.K select d
            Diagnostic(ErrorCode.ERR_FeatureInPreview, "leftjoin").WithArguments("leftjoin and rightjoin query clauses").WithLocation(1, 14));
        N(SyntaxKind.QueryExpression);
        {
            N(SyntaxKind.FromClause);
            {
                N(SyntaxKind.FromKeyword);
                N(SyntaxKind.IdentifierToken, "s");
                N(SyntaxKind.InKeyword);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "ss");
                }
            }
            N(SyntaxKind.QueryBody);
            {
                N(SyntaxKind.JoinClause);
                {
                    N(SyntaxKind.LeftJoinKeyword);
                    N(SyntaxKind.IdentifierToken, "d");
                    N(SyntaxKind.InKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "ds");
                    }
                    N(SyntaxKind.OnKeyword);
                    N(SyntaxKind.SimpleMemberAccessExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "s");
                        }
                        N(SyntaxKind.DotToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "K");
                        }
                    }
                    N(SyntaxKind.EqualsKeyword);
                    N(SyntaxKind.SimpleMemberAccessExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "d");
                        }
                        N(SyntaxKind.DotToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "K");
                        }
                    }
                }
                N(SyntaxKind.SelectClause);
                {
                    N(SyntaxKind.SelectKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "d");
                    }
                }
            }
        }
        EOF();
    }

    [Fact]
    public void LeftJoin_AfterTypePattern()
    {
        UsingExpression("from x in xs where x is T leftjoin y in ys on x equals y select y", TestOptions.RegularPreview);
        N(SyntaxKind.QueryExpression);
        {
            N(SyntaxKind.FromClause);
            {
                N(SyntaxKind.FromKeyword);
                N(SyntaxKind.IdentifierToken, "x");
                N(SyntaxKind.InKeyword);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "xs");
                }
            }
            N(SyntaxKind.QueryBody);
            {
                N(SyntaxKind.WhereClause);
                {
                    N(SyntaxKind.WhereKeyword);
                    N(SyntaxKind.IsExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "x");
                        }
                        N(SyntaxKind.IsKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                }
                N(SyntaxKind.JoinClause);
                {
                    N(SyntaxKind.LeftJoinKeyword);
                    N(SyntaxKind.IdentifierToken, "y");
                    N(SyntaxKind.InKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "ys");
                    }
                    N(SyntaxKind.OnKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "x");
                    }
                    N(SyntaxKind.EqualsKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "y");
                    }
                }
                N(SyntaxKind.SelectClause);
                {
                    N(SyntaxKind.SelectKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "y");
                    }
                }
            }
        }
        EOF();
    }

    [Fact]
    public void LeftJoin_AfterPropertyPattern()
    {
        UsingExpression("from x in xs where x is { P: true } leftjoin y in ys on x equals y select y", TestOptions.RegularPreview);
        N(SyntaxKind.QueryExpression);
        {
            N(SyntaxKind.FromClause);
            {
                N(SyntaxKind.FromKeyword);
                N(SyntaxKind.IdentifierToken, "x");
                N(SyntaxKind.InKeyword);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "xs");
                }
            }
            N(SyntaxKind.QueryBody);
            {
                N(SyntaxKind.WhereClause);
                {
                    N(SyntaxKind.WhereKeyword);
                    N(SyntaxKind.IsPatternExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "x");
                        }
                        N(SyntaxKind.IsKeyword);
                        N(SyntaxKind.RecursivePattern);
                        {
                            N(SyntaxKind.PropertyPatternClause);
                            {
                                N(SyntaxKind.OpenBraceToken);
                                N(SyntaxKind.Subpattern);
                                {
                                    N(SyntaxKind.NameColon);
                                    {
                                        N(SyntaxKind.IdentifierName);
                                        {
                                            N(SyntaxKind.IdentifierToken, "P");
                                        }
                                        N(SyntaxKind.ColonToken);
                                    }
                                    N(SyntaxKind.ConstantPattern);
                                    {
                                        N(SyntaxKind.TrueLiteralExpression);
                                        {
                                            N(SyntaxKind.TrueKeyword);
                                        }
                                    }
                                }
                                N(SyntaxKind.CloseBraceToken);
                            }
                        }
                    }
                }
                N(SyntaxKind.JoinClause);
                {
                    N(SyntaxKind.LeftJoinKeyword);
                    N(SyntaxKind.IdentifierToken, "y");
                    N(SyntaxKind.InKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "ys");
                    }
                    N(SyntaxKind.OnKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "x");
                    }
                    N(SyntaxKind.EqualsKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "y");
                    }
                }
                N(SyntaxKind.SelectClause);
                {
                    N(SyntaxKind.SelectKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "y");
                    }
                }
            }
        }
        EOF();
    }

    [Fact]
    public void LeftJoin_WithPredefinedType_AfterTypePattern()
    {
        UsingExpression("from x in xs where x is T leftjoin int y in ys on x equals y select y", TestOptions.RegularPreview);
        N(SyntaxKind.QueryExpression);
        {
            N(SyntaxKind.FromClause);
            {
                N(SyntaxKind.FromKeyword);
                N(SyntaxKind.IdentifierToken, "x");
                N(SyntaxKind.InKeyword);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "xs");
                }
            }
            N(SyntaxKind.QueryBody);
            {
                N(SyntaxKind.WhereClause);
                {
                    N(SyntaxKind.WhereKeyword);
                    N(SyntaxKind.IsExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "x");
                        }
                        N(SyntaxKind.IsKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                }
                N(SyntaxKind.JoinClause);
                {
                    N(SyntaxKind.LeftJoinKeyword);
                    N(SyntaxKind.PredefinedType);
                    {
                        N(SyntaxKind.IntKeyword);
                    }
                    N(SyntaxKind.IdentifierToken, "y");
                    N(SyntaxKind.InKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "ys");
                    }
                    N(SyntaxKind.OnKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "x");
                    }
                    N(SyntaxKind.EqualsKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "y");
                    }
                }
                N(SyntaxKind.SelectClause);
                {
                    N(SyntaxKind.SelectKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "y");
                    }
                }
            }
        }
        EOF();
    }

    [Fact]
    public void LeftJoinDesignation_FollowedByJoin()
    {
        UsingExpression("from x in xs where x is T leftjoin join y in ys on x equals y select y", TestOptions.RegularPreview);
        N(SyntaxKind.QueryExpression);
        {
            N(SyntaxKind.FromClause);
            {
                N(SyntaxKind.FromKeyword);
                N(SyntaxKind.IdentifierToken, "x");
                N(SyntaxKind.InKeyword);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "xs");
                }
            }
            N(SyntaxKind.QueryBody);
            {
                N(SyntaxKind.WhereClause);
                {
                    N(SyntaxKind.WhereKeyword);
                    N(SyntaxKind.IsPatternExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "x");
                        }
                        N(SyntaxKind.IsKeyword);
                        N(SyntaxKind.DeclarationPattern);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "T");
                            }
                            N(SyntaxKind.SingleVariableDesignation);
                            {
                                N(SyntaxKind.IdentifierToken, "leftjoin");
                            }
                        }
                    }
                }
                N(SyntaxKind.JoinClause);
                {
                    N(SyntaxKind.JoinKeyword);
                    N(SyntaxKind.IdentifierToken, "y");
                    N(SyntaxKind.InKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "ys");
                    }
                    N(SyntaxKind.OnKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "x");
                    }
                    N(SyntaxKind.EqualsKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "y");
                    }
                }
                N(SyntaxKind.SelectClause);
                {
                    N(SyntaxKind.SelectKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "y");
                    }
                }
            }
        }
        EOF();
    }

    [Fact]
    public void RightJoinDesignation_FollowedBySelect()
    {
        UsingExpression("from x in xs where x is T rightjoin select x", TestOptions.RegularPreview);
        N(SyntaxKind.QueryExpression);
        {
            N(SyntaxKind.FromClause);
            {
                N(SyntaxKind.FromKeyword);
                N(SyntaxKind.IdentifierToken, "x");
                N(SyntaxKind.InKeyword);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "xs");
                }
            }
            N(SyntaxKind.QueryBody);
            {
                N(SyntaxKind.WhereClause);
                {
                    N(SyntaxKind.WhereKeyword);
                    N(SyntaxKind.IsPatternExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "x");
                        }
                        N(SyntaxKind.IsKeyword);
                        N(SyntaxKind.DeclarationPattern);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "T");
                            }
                            N(SyntaxKind.SingleVariableDesignation);
                            {
                                N(SyntaxKind.IdentifierToken, "rightjoin");
                            }
                        }
                    }
                }
                N(SyntaxKind.SelectClause);
                {
                    N(SyntaxKind.SelectKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "x");
                    }
                }
            }
        }
        EOF();
    }

    [Fact]
    public void LeftJoinDesignation_FollowedByLogicalAnd()
    {
        UsingExpression("from x in xs where x is T leftjoin && leftjoin.P select x", TestOptions.RegularPreview);
        N(SyntaxKind.QueryExpression);
        {
            N(SyntaxKind.FromClause);
            {
                N(SyntaxKind.FromKeyword);
                N(SyntaxKind.IdentifierToken, "x");
                N(SyntaxKind.InKeyword);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "xs");
                }
            }
            N(SyntaxKind.QueryBody);
            {
                N(SyntaxKind.WhereClause);
                {
                    N(SyntaxKind.WhereKeyword);
                    N(SyntaxKind.LogicalAndExpression);
                    {
                        N(SyntaxKind.IsPatternExpression);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "x");
                            }
                            N(SyntaxKind.IsKeyword);
                            N(SyntaxKind.DeclarationPattern);
                            {
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken, "T");
                                }
                                N(SyntaxKind.SingleVariableDesignation);
                                {
                                    N(SyntaxKind.IdentifierToken, "leftjoin");
                                }
                            }
                        }
                        N(SyntaxKind.AmpersandAmpersandToken);
                        N(SyntaxKind.SimpleMemberAccessExpression);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "leftjoin");
                            }
                            N(SyntaxKind.DotToken);
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "P");
                            }
                        }
                    }
                }
                N(SyntaxKind.SelectClause);
                {
                    N(SyntaxKind.SelectKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "x");
                    }
                }
            }
        }
        EOF();
    }

    [Fact]
    public void LeftJoinVarDesignation_FollowedBySelect()
    {
        UsingExpression("from x in xs where x is var leftjoin select x", TestOptions.RegularPreview);
        N(SyntaxKind.QueryExpression);
        {
            N(SyntaxKind.FromClause);
            {
                N(SyntaxKind.FromKeyword);
                N(SyntaxKind.IdentifierToken, "x");
                N(SyntaxKind.InKeyword);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "xs");
                }
            }
            N(SyntaxKind.QueryBody);
            {
                N(SyntaxKind.WhereClause);
                {
                    N(SyntaxKind.WhereKeyword);
                    N(SyntaxKind.IsPatternExpression);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "x");
                        }
                        N(SyntaxKind.IsKeyword);
                        N(SyntaxKind.VarPattern);
                        {
                            N(SyntaxKind.VarKeyword);
                            N(SyntaxKind.SingleVariableDesignation);
                            {
                                N(SyntaxKind.IdentifierToken, "leftjoin");
                            }
                        }
                    }
                }
                N(SyntaxKind.SelectClause);
                {
                    N(SyntaxKind.SelectKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "x");
                    }
                }
            }
        }
        EOF();
    }

    [Fact]
    public void LeftJoinDesignation_OutsideQuery()
    {
        UsingExpression("x is T leftjoin", TestOptions.RegularPreview);
        N(SyntaxKind.IsPatternExpression);
        {
            N(SyntaxKind.IdentifierName);
            {
                N(SyntaxKind.IdentifierToken, "x");
            }
            N(SyntaxKind.IsKeyword);
            N(SyntaxKind.DeclarationPattern);
            {
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "T");
                }
                N(SyntaxKind.SingleVariableDesignation);
                {
                    N(SyntaxKind.IdentifierToken, "leftjoin");
                }
            }
        }
        EOF();
    }

    [Fact]
    public void LeftJoinAndRightJoin_AsIdentifiers()
    {
        UsingExpression("from leftjoin in xs let rightjoin = leftjoin select rightjoin into leftjoin where leftjoin select leftjoin", TestOptions.RegularPreview);
        N(SyntaxKind.QueryExpression);
        {
            N(SyntaxKind.FromClause);
            {
                N(SyntaxKind.FromKeyword);
                N(SyntaxKind.IdentifierToken, "leftjoin");
                N(SyntaxKind.InKeyword);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "xs");
                }
            }
            N(SyntaxKind.QueryBody);
            {
                N(SyntaxKind.LetClause);
                {
                    N(SyntaxKind.LetKeyword);
                    N(SyntaxKind.IdentifierToken, "rightjoin");
                    N(SyntaxKind.EqualsToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "leftjoin");
                    }
                }
                N(SyntaxKind.SelectClause);
                {
                    N(SyntaxKind.SelectKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "rightjoin");
                    }
                }
                N(SyntaxKind.QueryContinuation);
                {
                    N(SyntaxKind.IntoKeyword);
                    N(SyntaxKind.IdentifierToken, "leftjoin");
                    N(SyntaxKind.QueryBody);
                    {
                        N(SyntaxKind.WhereClause);
                        {
                            N(SyntaxKind.WhereKeyword);
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "leftjoin");
                            }
                        }
                        N(SyntaxKind.SelectClause);
                        {
                            N(SyntaxKind.SelectKeyword);
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "leftjoin");
                            }
                        }
                    }
                }
            }
        }
        EOF();
    }

    [Fact]
    public void SyntaxFactory_OverloadsWithoutKeywordCreateJoin()
    {
        var a = SyntaxFactory.IdentifierName("a");
        var b = SyntaxFactory.IdentifierName("b");
        var ds = SyntaxFactory.IdentifierName("ds");

        var clauses = new[]
        {
            SyntaxFactory.JoinClause("d", ds, a, b),
            SyntaxFactory.JoinClause(SyntaxFactory.Identifier("d"), ds, a, b),
            SyntaxFactory.JoinClause(null, SyntaxFactory.Identifier("d"), ds, a, b, null),
        };

        foreach (var clause in clauses)
        {
            Assert.Equal(SyntaxKind.JoinKeyword, clause.JoinKeyword.Kind());
            Assert.Equal("join d in ds on a equals b", clause.NormalizeWhitespace().ToFullString());
        }

        var leftJoin = SyntaxFactory.JoinClause(SyntaxFactory.Token(SyntaxKind.LeftJoinKeyword), "d", ds, a, b);
        Assert.Equal("leftjoin d in ds on a equals b", leftJoin.NormalizeWhitespace().ToFullString());
    }
}
