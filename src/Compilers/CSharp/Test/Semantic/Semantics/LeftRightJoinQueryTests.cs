// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests;

public sealed class LeftRightJoinQueryTests : CompilingTestBase
{
    private const string Model = """

        record Student(int Id, string Name, int DepartmentId);
        record Department(int Id, string Name);
        """;

    private const string Data = """
        using System.Linq;

        var students = new[] { new Student(1, "Ada", 1), new Student(2, "Grace", 2), new Student(3, "Linus", 0) };
        var departments = new[] { new Department(1, "Math"), new Department(1, "Logic"), new Department(3, "Chemistry") };

        """;

    private static string? IfCoreClr(string expectedOutput) => ExecutionConditionUtil.IsCoreClr ? expectedOutput : null;

    private static CSharpCompilation CreateNet100Compilation(string source, CSharpParseOptions? parseOptions = null)
        => CreateCompilation(
            source,
            targetFramework: TargetFramework.Net100,
            parseOptions: parseOptions ?? TestOptions.RegularPreview,
            options: TestOptions.ReleaseExe.WithNullableContextOptions(NullableContextOptions.Enable));

    [Fact]
    public void LeftJoin_FollowedBySelect()
    {
        var source = Data + """
            var q = from s in students
                    leftjoin d in departments on s.DepartmentId equals d.Id
                    select s.Name + ":" + (d?.Name ?? "-");
            System.Console.WriteLine(string.Join(",", q));
            """ + Model;

        CompileAndVerify(CreateNet100Compilation(source), expectedOutput: IfCoreClr("Ada:Math,Ada:Logic,Grace:-,Linus:-"), verify: Verification.Skipped)
            .VerifyDiagnostics();
    }

    [Fact]
    public void RightJoin_FollowedBySelect()
    {
        var source = Data + """
            var q = from s in students
                    rightjoin d in departments on s.DepartmentId equals d.Id
                    select (s?.Name ?? "-") + ":" + d.Name;
            System.Console.WriteLine(string.Join(",", q));
            """ + Model;

        CompileAndVerify(CreateNet100Compilation(source), expectedOutput: IfCoreClr("Ada:Math,Ada:Logic,-:Chemistry"), verify: Verification.Skipped)
            .VerifyDiagnostics();
    }

    [Fact]
    public void LeftJoin_FollowedByOtherClauses()
    {
        var source = Data + """
            var q = from s in students
                    leftjoin d in departments on s.DepartmentId equals d.Id
                    where s.Id > 1 || d?.Name == "Logic"
                    leftjoin m in departments on s.Id equals m.Id
                    orderby s.Name descending
                    select s.Name + ":" + (d?.Name ?? "-") + ":" + (m?.Name ?? "-");
            System.Console.WriteLine(string.Join(",", q));
            """ + Model;

        CompileAndVerify(CreateNet100Compilation(source), expectedOutput: IfCoreClr("Linus:-:Chemistry,Grace:-:-,Ada:Logic:Math,Ada:Logic:Logic"), verify: Verification.Skipped)
            .VerifyDiagnostics();
    }

    [Fact]
    public void RightJoin_AfterTwoRangeVariables()
    {
        var source = Data + """
            var q = from s in students
                    from n in new[] { 10 }
                    rightjoin d in departments on s.DepartmentId equals d.Id
                    select (s?.Name ?? "-") + ":" + d.Name;
            System.Console.WriteLine(string.Join(",", q));
            """ + Model;

        CompileAndVerify(CreateNet100Compilation(source), expectedOutput: IfCoreClr("Ada:Math,Ada:Logic,-:Chemistry"), verify: Verification.Skipped)
            .VerifyDiagnostics();
    }

    [Fact]
    public void RightJoin_AfterTwoRangeVariables_FollowedByOtherClauses()
    {
        var source = Data + """
            var q = from s in students
                    from n in new[] { 10 }
                    rightjoin d in departments on s.DepartmentId equals d.Id
                    where d.Id > 0
                    orderby d.Name
                    select (s?.Name ?? "-") + ":" + n + ":" + d.Name;
            System.Console.WriteLine(string.Join(",", q));
            """ + Model;

        CompileAndVerify(CreateNet100Compilation(source), expectedOutput: IfCoreClr("-:0:Chemistry,Ada:10:Logic,Ada:10:Math"), verify: Verification.Skipped)
            .VerifyDiagnostics();
    }

    [Fact]
    public void RightJoin_AfterLet_ValueTypeRangeVariableIsDefault()
    {
        var source = Data + """
            var q = from s in students
                    let k = s.Id * 10
                    rightjoin d in departments on s.DepartmentId equals d.Id
                    select k + ":" + d.Name;
            System.Console.WriteLine(string.Join(",", q));
            """ + Model;

        CompileAndVerify(CreateNet100Compilation(source), expectedOutput: IfCoreClr("10:Math,10:Logic,0:Chemistry"), verify: Verification.Skipped)
            .VerifyDiagnostics();
    }

    [Fact]
    public void RightJoin_AfterTwoRangeVariables_OverQueryable()
    {
        var source = Data + """
            var q = from s in students.AsQueryable()
                    from n in new[] { 10 }
                    rightjoin d in departments on s.DepartmentId equals d.Id
                    select (s == null ? "-" : s.Name) + ":" + n + ":" + d.Name;
            System.Console.WriteLine(string.Join(",", q));
            """ + Model;

        // A range variable read through the outer side of a rightjoin has no tracked null state, so the
        // null check does not suppress the warning.
        CompileAndVerify(CreateNet100Compilation(source), expectedOutput: IfCoreClr("Ada:10:Math,Ada:10:Logic,-:0:Chemistry"), verify: Verification.Skipped)
            .VerifyDiagnostics(
                // (8,35): warning CS8602: Dereference of a possibly null reference.
                //         select (s == null ? "-" : s.Name) + ":" + n + ":" + d.Name;
                Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "s").WithLocation(8, 35));
    }

    [Fact]
    public void LeftJoin_WithRangeVariableType()
    {
        var source = Data + """
            object[] boxed = departments;
            var q = from s in students
                    leftjoin Department d in boxed on s.DepartmentId equals d.Id
                    select s.Name + ":" + (d?.Name ?? "-");
            System.Console.WriteLine(string.Join(",", q));
            """ + Model;

        CompileAndVerify(CreateNet100Compilation(source), expectedOutput: IfCoreClr("Ada:Math,Ada:Logic,Grace:-,Linus:-"), verify: Verification.Skipped)
            .VerifyDiagnostics();
    }

    [Fact]
    public void LeftJoin_OverQueryable_CallsQueryableLeftJoin()
    {
        var source = Data + """
            var q = from s in students.AsQueryable()
                    leftjoin d in departments on s.DepartmentId equals d.Id
                    select s.Name + ":" + (d == null ? "-" : d.Name);
            System.Console.WriteLine(((System.Linq.Expressions.MethodCallExpression)q.Expression).Method.Name);
            System.Console.WriteLine(string.Join(",", q));
            """ + Model;

        CompileAndVerify(CreateNet100Compilation(source), expectedOutput: IfCoreClr("""
            LeftJoin
            Ada:Math,Ada:Logic,Grace:-,Linus:-
            """), verify: Verification.Skipped)
            .VerifyDiagnostics();
    }

    [Fact]
    public void QueryPattern_CallsLeftJoinAndRightJoinMethods()
    {
        var source = """
            using System;

            class Source
            {
                public Source LeftJoin(int[] inner, Func<int, int> outerKey, Func<int, int> innerKey, Func<int, int, int> result)
                {
                    Console.Write("LeftJoin ");
                    return this;
                }

                public Source RightJoin(int[] inner, Func<int, int> outerKey, Func<int, int> innerKey, Func<int, int, int> result)
                {
                    Console.Write("RightJoin ");
                    return this;
                }
            }

            class Program
            {
                static void Main()
                {
                    _ = from x in new Source() leftjoin y in new[] { 1 } on x equals y select x + y;
                    _ = from x in new Source() rightjoin y in new[] { 1 } on x equals y select x + y;
                }
            }
            """;

        CompileAndVerify(source, parseOptions: TestOptions.RegularPreview, expectedOutput: "LeftJoin RightJoin ").VerifyDiagnostics();
    }

    [Fact]
    public void LeftJoin_SourceWithoutLeftJoin()
    {
        var source = """
            class Source
            {
                public Source Select(System.Func<int, int> f) => this;
            }

            class Program
            {
                static void Main()
                {
                    _ = from x in new Source() leftjoin y in new[] { 1 } on x equals y select x;
                }
            }
            """;

        CreateCompilation(source, parseOptions: TestOptions.RegularPreview).VerifyDiagnostics(
            // (10,23): error CS1936: Could not find an implementation of the query pattern for source type 'Source'.  'LeftJoin' not found.
            //         _ = from x in new Source() leftjoin y in new[] { 1 } on x equals y select x;
            Diagnostic(ErrorCode.ERR_QueryNoProvider, "new Source()").WithArguments("Source", "LeftJoin").WithLocation(10, 23));
    }

    [Fact]
    public void LeftJoin_WithInto()
    {
        var source = Data + """
            var q = from s in students
                    leftjoin d in departments on s.DepartmentId equals d.Id into g
                    select g;
            """ + Model;

        CreateNet100Compilation(source).VerifyDiagnostics(
            // (6,65): error CS9404: A 'leftjoin' clause cannot have an 'into' continuation; use 'join ... into' instead
            //         leftjoin d in departments on s.DepartmentId equals d.Id into g
            Diagnostic(ErrorCode.ERR_OuterJoinWithInto, "into").WithArguments("leftjoin").WithLocation(6, 65));
    }

    [Fact]
    public void LeftJoin_LanguageVersion()
    {
        var source = Data + """
            var q = from s in students
                    leftjoin d in departments on s.DepartmentId equals d.Id
                    select d?.Name;
            """ + Model;

        CreateNet100Compilation(source, TestOptions.Regular14).VerifyDiagnostics(
            // (6,9): error CS8652: The feature 'leftjoin and rightjoin query clauses' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
            //         leftjoin d in departments on s.DepartmentId equals d.Id
            Diagnostic(ErrorCode.ERR_FeatureInPreview, "leftjoin").WithArguments("leftjoin and rightjoin query clauses").WithLocation(6, 9));
        CreateNet100Compilation(source, TestOptions.RegularPreview).VerifyDiagnostics();
    }

    [Fact]
    public void Nullability_QueryResultType()
    {
        var source = Data + """
            var b = from s in students
                    leftjoin d in departments on s.DepartmentId equals d.Id
                    select new { s, d };
            var c = from s in students
                    leftjoin d in departments on s.DepartmentId equals d.Id
                    where s.Id > 0
                    select d;
            var e = from s in students
                    join d in departments on s.DepartmentId equals d.Id
                    select (Department?)d;
            """ + Model;

        var comp = CreateNet100Compilation(source);
        comp.VerifyDiagnostics();
        var tree = comp.SyntaxTrees[0];
        var model = comp.GetSemanticModel(tree);
        var format = SymbolDisplayFormat.TestFormat.WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
        var types = tree.GetRoot().DescendantNodes().OfType<VariableDeclaratorSyntax>()
            .Where(v => v.Initializer!.Value is QueryExpressionSyntax)
            .Select(v => v.Identifier.Text + ": " + model.GetTypeInfo(v.Initializer!.Value).Type.ToDisplayString(format));

        AssertEx.Equal(
            [
                "b: System.Collections.Generic.IEnumerable<<anonymous type: Student s, Department? d>>",
                "c: System.Collections.Generic.IEnumerable<Department?>",
                // Queries without leftjoin/rightjoin keep their existing (not reinferred) type.
                "e: System.Collections.Generic.IEnumerable<Department>",
            ],
            types);
    }

    [Fact]
    public void Nullability_LeftJoinInnerIsMaybeNull()
    {
        var source = Data + """
            _ = from s in students
                leftjoin d in departments on s.DepartmentId equals d.Id
                select s.Name + d.Name;
            _ = from s in students
                leftjoin d in departments on s.DepartmentId equals d.Id
                where s.Id > 0
                select s.Name + d.Name;
            _ = from s in students
                leftjoin d in departments on s.DepartmentId equals d.Id
                let e = d
                select e.Name;
            _ = from s in students
                leftjoin d in departments on s.DepartmentId equals d.Id
                select new { s, d } into x
                select x.d.Name;
            """ + Model;

        CreateNet100Compilation(source).VerifyDiagnostics(
            // (7,21): warning CS8602: Dereference of a possibly null reference.
            //     select s.Name + d.Name;
            Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "d").WithLocation(7, 21),
            // (11,21): warning CS8602: Dereference of a possibly null reference.
            //     select s.Name + d.Name;
            Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "d").WithLocation(11, 21),
            // (15,12): warning CS8602: Dereference of a possibly null reference.
            //     select e.Name;
            Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "e").WithLocation(15, 12),
            // (19,12): warning CS8602: Dereference of a possibly null reference.
            //     select x.d.Name;
            Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "x.d").WithLocation(19, 12));
    }

    [Fact]
    public void Nullability_RightJoinOuterIsMaybeNull()
    {
        var source = Data + """
            _ = from s in students
                rightjoin d in departments on s.DepartmentId equals d.Id
                select s.Name + d.Name;
            _ = from s in students
                rightjoin d in departments on s.DepartmentId equals d.Id
                orderby d.Id
                select s.Name + d.Name;
            """ + Model;

        CreateNet100Compilation(source).VerifyDiagnostics(
            // (7,12): warning CS8602: Dereference of a possibly null reference.
            //     select s.Name + d.Name;
            Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "s").WithLocation(7, 12),
            // (11,12): warning CS8602: Dereference of a possibly null reference.
            //     select s.Name + d.Name;
            Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "s").WithLocation(11, 12));
    }

    [Fact]
    public void Nullability_RightJoinCompoundOuterIsMaybeNull()
    {
        var source = Data + """
            _ = from s in students
                let k = s.Id
                rightjoin d in departments on s.DepartmentId equals d.Id
                select s.Name + k + d.Name;
            """ + Model;

        CreateNet100Compilation(source).VerifyDiagnostics(
            // (8,12): warning CS8602: Dereference of a possibly null reference.
            //     select s.Name + k + d.Name;
            Diagnostic(ErrorCode.WRN_NullReferenceReceiver, "s").WithLocation(8, 12));
    }

    [Fact]
    public void Nullability_NullCheckWithinClause()
    {
        var source = Data + """
            _ = from s in students
                leftjoin d in departments on s.DepartmentId equals d.Id
                select d != null ? d.Name : "-";
            _ = from s in students
                leftjoin d in departments on s.DepartmentId equals d.Id
                where s.Id > 0
                select d != null ? d.Name : "-";
            """ + Model;

        CreateNet100Compilation(source).VerifyDiagnostics();
    }

    [Fact]
    public void Nullability_QueriesWithoutOuterJoinAreUnchanged()
    {
        var source = """
            using System.Linq;

            string?[] xs = [null];
            _ = from x in xs select x.Length;
            _ = from x in xs join y in xs on x equals y select x.Length + y.Length;
            """;

        CreateNet100Compilation(source).VerifyDiagnostics();
    }

    [Fact]
    public void Nullability_ValueTypeRangeVariable()
    {
        var source = """
            using System.Linq;

            var q = from x in new[] { 1, 2 }
                    leftjoin y in new[] { 2 } on x equals y
                    select x * 10 + y;
            System.Console.WriteLine(string.Join(",", q));
            """;

        CompileAndVerify(CreateNet100Compilation(source), expectedOutput: IfCoreClr("10,22"), verify: Verification.Skipped)
            .VerifyDiagnostics();
    }

    [Fact]
    public void SemanticModel_QueryClauseInfo()
    {
        var source = Data + """
            _ = from s in students
                leftjoin d in departments on s.DepartmentId equals d.Id
                rightjoin e in departments on s.DepartmentId equals e.Id
                select s;
            """ + Model;

        var comp = CreateNet100Compilation(source);
        comp.VerifyDiagnostics();

        var tree = comp.SyntaxTrees.Single();
        var model = comp.GetSemanticModel(tree);
        var joins = tree.GetRoot().DescendantNodes().OfType<JoinClauseSyntax>().ToArray();

        Assert.Equal(
            ["LeftJoin", "RightJoin"],
            joins.Select(j => model.GetQueryClauseInfo(j).OperationInfo.Symbol!.Name));
        Assert.Equal(
            "System.Linq.Enumerable",
            model.GetQueryClauseInfo(joins[0]).OperationInfo.Symbol!.ContainingType.ToTestDisplayString());
    }

    [Fact]
    public void IOperation_LeftJoin()
    {
        var source = """
            using System.Linq;

            class C
            {
                void M(int[] xs, int[] ys)
                {
                    var q = /*<bind>*/from x in xs
                            leftjoin y in ys on x equals y
                            select x + y/*</bind>*/;
                }
            }
            """;

        var expectedOperationTree = @"
ITranslatedQueryOperation (OperationKind.TranslatedQuery, Type: System.Collections.Generic.IEnumerable<System.Int32>) (Syntax: 'from x in x ... elect x + y')
  Expression:
    IInvocationOperation (System.Collections.Generic.IEnumerable<System.Int32> System.Linq.Enumerable.LeftJoin<System.Int32, System.Int32, System.Int32, System.Int32>(this System.Collections.Generic.IEnumerable<System.Int32> outer, System.Collections.Generic.IEnumerable<System.Int32> inner, System.Func<System.Int32, System.Int32> outerKeySelector, System.Func<System.Int32, System.Int32> innerKeySelector, System.Func<System.Int32, System.Int32, System.Int32> resultSelector)) (OperationKind.Invocation, Type: System.Collections.Generic.IEnumerable<System.Int32>, IsImplicit) (Syntax: 'leftjoin y  ...  x equals y')
      Instance Receiver:
        null
      Arguments(5):
          IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: outer) (OperationKind.Argument, Type: null, IsImplicit) (Syntax: 'from x in xs')
            IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: System.Collections.Generic.IEnumerable<System.Int32>, IsImplicit) (Syntax: 'from x in xs')
              Conversion: CommonConversion (Exists: True, IsIdentity: False, IsNumeric: False, IsReference: True, IsUserDefined: False) (MethodSymbol: null)
              Operand:
                IParameterReferenceOperation: xs (OperationKind.ParameterReference, Type: System.Int32[]) (Syntax: 'xs')
            InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
            OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
          IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: inner) (OperationKind.Argument, Type: null, IsImplicit) (Syntax: 'ys')
            IConversionOperation (TryCast: False, Unchecked) (OperationKind.Conversion, Type: System.Collections.Generic.IEnumerable<System.Int32>, IsImplicit) (Syntax: 'ys')
              Conversion: CommonConversion (Exists: True, IsIdentity: False, IsNumeric: False, IsReference: True, IsUserDefined: False) (MethodSymbol: null)
              Operand:
                IParameterReferenceOperation: ys (OperationKind.ParameterReference, Type: System.Int32[]) (Syntax: 'ys')
            InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
            OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
          IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: outerKeySelector) (OperationKind.Argument, Type: null, IsImplicit) (Syntax: 'x')
            IDelegateCreationOperation (OperationKind.DelegateCreation, Type: System.Func<System.Int32, System.Int32>, IsImplicit) (Syntax: 'x')
              Target:
                IAnonymousFunctionOperation (Symbol: lambda expression) (OperationKind.AnonymousFunction, Type: null, IsImplicit) (Syntax: 'x')
                  IBlockOperation (1 statements) (OperationKind.Block, Type: null, IsImplicit) (Syntax: 'x')
                    IReturnOperation (OperationKind.Return, Type: null, IsImplicit) (Syntax: 'x')
                      ReturnedValue:
                        IParameterReferenceOperation: x (OperationKind.ParameterReference, Type: System.Int32) (Syntax: 'x')
            InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
            OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
          IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: innerKeySelector) (OperationKind.Argument, Type: null, IsImplicit) (Syntax: 'y')
            IDelegateCreationOperation (OperationKind.DelegateCreation, Type: System.Func<System.Int32, System.Int32>, IsImplicit) (Syntax: 'y')
              Target:
                IAnonymousFunctionOperation (Symbol: lambda expression) (OperationKind.AnonymousFunction, Type: null, IsImplicit) (Syntax: 'y')
                  IBlockOperation (1 statements) (OperationKind.Block, Type: null, IsImplicit) (Syntax: 'y')
                    IReturnOperation (OperationKind.Return, Type: null, IsImplicit) (Syntax: 'y')
                      ReturnedValue:
                        IParameterReferenceOperation: y (OperationKind.ParameterReference, Type: System.Int32) (Syntax: 'y')
            InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
            OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
          IArgumentOperation (ArgumentKind.Explicit, Matching Parameter: resultSelector) (OperationKind.Argument, Type: null, IsImplicit) (Syntax: 'x + y')
            IDelegateCreationOperation (OperationKind.DelegateCreation, Type: System.Func<System.Int32, System.Int32, System.Int32>, IsImplicit) (Syntax: 'x + y')
              Target:
                IAnonymousFunctionOperation (Symbol: lambda expression) (OperationKind.AnonymousFunction, Type: null, IsImplicit) (Syntax: 'x + y')
                  IBlockOperation (1 statements) (OperationKind.Block, Type: null, IsImplicit) (Syntax: 'x + y')
                    IReturnOperation (OperationKind.Return, Type: null, IsImplicit) (Syntax: 'x + y')
                      ReturnedValue:
                        IBinaryOperation (BinaryOperatorKind.Add) (OperationKind.Binary, Type: System.Int32) (Syntax: 'x + y')
                          Left:
                            IParameterReferenceOperation: x (OperationKind.ParameterReference, Type: System.Int32) (Syntax: 'x')
                          Right:
                            IParameterReferenceOperation: y (OperationKind.ParameterReference, Type: System.Int32) (Syntax: 'y')
            InConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
            OutConversion: CommonConversion (Exists: True, IsIdentity: True, IsNumeric: False, IsReference: False, IsUserDefined: False) (MethodSymbol: null)
";

        VerifyOperationTreeAndDiagnosticsForTest<QueryExpressionSyntax>(
            source,
            expectedOperationTree,
            TargetFramework.Net100,
            DiagnosticDescription.None,
            parseOptions: TestOptions.RegularPreview);
    }
}
