// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp
{
    internal partial class Binder
    {
        // A binder that finds query variables (BoundRangeVariableSymbol) and can bind them
        // to the appropriate rewriting involving lambda parameters when transparent identifiers are involved.
        private sealed class WithQueryLambdaParametersBinder : WithLambdaParametersBinder
        {
            private readonly RangeVariableMap _rangeVariableMap;
            private readonly MultiDictionary<string, RangeVariableSymbol> _parameterMap;

            public WithQueryLambdaParametersBinder(LambdaSymbol lambdaSymbol, RangeVariableMap rangeVariableMap, Binder next)
                : base(lambdaSymbol, next)
            {
                _rangeVariableMap = rangeVariableMap;
                _parameterMap = new MultiDictionary<string, RangeVariableSymbol>();
                foreach (var qv in rangeVariableMap.Keys)
                {
                    _parameterMap.Add(qv.Name, qv);
                }
            }

            protected override BoundExpression BindRangeVariable(SimpleNameSyntax node, RangeVariableSymbol qv, BindingDiagnosticBag diagnostics)
            {
                Debug.Assert(!qv.IsTransparent);

                BoundExpression translation;
                ImmutableArray<string> path;
                if (_rangeVariableMap.TryGetValue(qv, out path))
                {
                    if (path.IsEmpty)
                    {
                        // the range variable maps directly to a use of the parameter of that name
                        var value = base.parameterMap[qv.Name];
                        Debug.Assert(value.Count == 1);
                        translation = new BoundParameter(node, value.Single());
                    }
                    else
                    {
                        // if the query variable map for this variable is non empty, we always start with the current
                        // lambda's first parameter, which is a transparent identifier.
                        Debug.Assert(base.lambdaSymbol.Parameters[0].Name.StartsWith(transparentIdentifierPrefix, StringComparison.Ordinal));
                        translation = new BoundParameter(node, base.lambdaSymbol.Parameters[0]);
                        for (int i = path.Length - 1; i >= 0; i--)
                        {
                            translation.WasCompilerGenerated = true;
                            var nextField = path[i];
                            translation = SelectField(node, translation, nextField, diagnostics);
                        }

                        translation = GuardMaybeNullTransparentIdentifiers(node, translation, path, diagnostics);
                    }

                    return new BoundRangeVariable(node, qv, translation, translation.Type);
                }

                return base.BindRangeVariable(node, qv, diagnostics);
            }

            /// <summary>
            /// Wraps <paramref name="value"/>, the access <c>P.path[^1]...path[0]</c>, as
            /// <c>R == null ? default : value</c> for every receiver <c>R</c> along the path that is a transparent
            /// identifier which may be null (the outer side of a <c>rightjoin</c>). Outer receivers are checked first.
            /// </summary>
            private BoundExpression GuardMaybeNullTransparentIdentifiers(SimpleNameSyntax node, BoundExpression value, ImmutableArray<string> path, BindingDiagnosticBag diagnostics)
            {
                var maybeNull = _rangeVariableMap.MaybeNullTransparentIdentifiers;
                if (maybeNull.IsEmpty || value.Type is not { } type || type.IsErrorType())
                {
                    return value;
                }

                var parameter = base.lambdaSymbol.Parameters[0];

                // The receiver of path[i] is the parameter when i is the last index, and the member path[i + 1] otherwise.
                for (int i = 0; i < path.Length; i++)
                {
                    string receiverName = i == path.Length - 1 ? parameter.Name : path[i + 1];
                    if (!maybeNull.Contains(receiverName))
                    {
                        continue;
                    }

                    BoundExpression receiver = new BoundParameter(node, parameter) { WasCompilerGenerated = true };
                    for (int j = path.Length - 1; j > i; j--)
                    {
                        receiver = SelectField(node, receiver, path[j], BindingDiagnosticBag.Discarded);
                        receiver.WasCompilerGenerated = true;
                    }

                    value = MakeNullGuard(node, receiver, value, type, diagnostics);
                }

                return value;
            }

            private BoundExpression MakeNullGuard(SimpleNameSyntax node, BoundExpression receiver, BoundExpression value, TypeSymbol type, BindingDiagnosticBag diagnostics)
            {
                var objectType = GetSpecialType(SpecialType.System_Object, diagnostics, node);
                var boolType = GetSpecialType(SpecialType.System_Boolean, diagnostics, node);

                CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = GetNewCompoundUseSiteInfo(diagnostics);
                var receiverConversion = Conversions.ClassifyImplicitConversionFromExpression(receiver, objectType, ref useSiteInfo);
                diagnostics.Add(node, useSiteInfo);

                var left = CreateConversion(receiver, receiverConversion, objectType, diagnostics);
                var nullLiteral = new BoundLiteral(node, ConstantValue.Null, type: null) { WasCompilerGenerated = true };
                var right = CreateConversion(nullLiteral, Conversion.NullLiteral, objectType, diagnostics);
                left.WasCompilerGenerated = true;
                right.WasCompilerGenerated = true;

                var isNull = new BoundBinaryOperator(node, BinaryOperatorKind.ObjectEqual, ConstantValue.NotAvailable, methodOpt: null, constrainedToTypeOpt: null, LookupResultKind.Viable, left, right, boolType) { WasCompilerGenerated = true };
                var defaultValue = new BoundDefaultExpression(node, type) { WasCompilerGenerated = true };

                return new BoundConditionalOperator(node, isRef: false, isNull, defaultValue, value, constantValueOpt: null, naturalTypeOpt: type, wasTargetTyped: false, type) { WasCompilerGenerated = true };
            }

            private BoundExpression SelectField(SimpleNameSyntax node, BoundExpression receiver, string name, BindingDiagnosticBag diagnostics)
            {
                var receiverType = receiver.Type as NamedTypeSymbol;
                if ((object)receiverType == null || !receiverType.IsAnonymousType)
                {
                    // We only construct transparent query variables using anonymous types, so if we're trying to navigate through
                    // some other type, we must have some query API where the types don't match up as expected.
                    var info = new CSDiagnosticInfo(ErrorCode.ERR_UnsupportedTransparentIdentifierAccess, name, new FormattedSymbol(receiver.ExpressionSymbol ?? receiverType, SymbolDisplayFormat.CSharpErrorMessageNoParameterNamesFormat));
                    if (receiver.Type?.IsErrorType() != true)
                    {
                        Error(diagnostics, info, node);
                    }

                    return new BoundBadExpression(
                        node,
                        LookupResultKind.Empty,
                        ImmutableArray.Create<Symbol>(receiver.ExpressionSymbol),
                        ImmutableArray.Create(AdjustBadExpressionChild(BindToTypeForErrorRecovery(receiver))),
                        new ExtendedErrorTypeSymbol(this.Compilation, "", 0, info));
                }

                LookupResult lookupResult = LookupResult.GetInstance();
                LookupOptions options = LookupOptions.MustBeInstance;
                CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = GetNewCompoundUseSiteInfo(diagnostics);
                LookupMembersWithFallback(lookupResult, receiver.Type, name, 0, ref useSiteInfo, basesBeingResolved: null, options: options);
                diagnostics.Add(node, useSiteInfo);

                var result = BindMemberOfType(node, node, name, 0, invoked: false, indexed: false, receiver, default(SeparatedSyntaxList<TypeSyntax>), default(ImmutableArray<TypeWithAnnotations>), lookupResult, BoundMethodGroupFlags.None, diagnostics);
                lookupResult.Free();
                return result;
            }

            internal override void LookupSymbolsInSingleBinder(
                LookupResult result, string name, int arity, ConsList<TypeSymbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
            {
                Debug.Assert(result.IsClear);

                if ((options & LookupOptions.NamespaceAliasesOnly) != 0)
                {
                    return;
                }

                foreach (var rangeVariable in _parameterMap[name])
                {
                    result.MergeEqual(originalBinder.CheckViability(rangeVariable, arity, options, null, diagnose, ref useSiteInfo));
                }
            }

            internal override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
            {
                if (options.CanConsiderMembers())
                {
                    foreach (var kvp in _parameterMap)
                    {
                        result.AddSymbol(null, kvp.Key, 0);
                    }
                }
            }
        }
    }
}
