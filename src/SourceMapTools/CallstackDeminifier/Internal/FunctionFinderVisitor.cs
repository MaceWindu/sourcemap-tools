using System.Collections.Generic;
using System.Linq;
using Esprima;
using Esprima.Ast;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapTools.CallstackDeminifier.Internal;

/// <summary>
/// This will visit all function nodes in the JavaScript abstract
/// syntax tree and create an entry in the function map that describes
/// the start and and location of the function.
/// </summary>
internal sealed class FunctionFinderVisitor(SourceMap sourceMap) : AstVisitorWithStack
{
	private readonly SourceMap _sourceMap = sourceMap;
	private readonly List<FunctionMapEntry> _functionMap = [];

	internal IReadOnlyList<FunctionMapEntry> GetFunctionMap()
	{
		// Sort in descending order by start position.  This allows the first result found in a linear search to be the "closest function to the [consumer's] source position".
		//
		// ATTN: It may be possible to do this with an ascending order sort, followed by a series of binary searches on rows & columns.
		//       Our current profiles show the memory pressure being a bigger issue than the stack lookup, so I'm leaving this for now.
		_functionMap.Sort(static (x, y) => y.Start.CompareTo(x.Start));

		return _functionMap;
	}

	protected override object VisitArrowFunctionExpression(ArrowFunctionExpression arrowFunctionExpression)
	{
		base.VisitArrowFunctionExpression(arrowFunctionExpression);
		VisitFunction(arrowFunctionExpression);
		return arrowFunctionExpression;
	}

	protected override object VisitFunctionExpression(FunctionExpression functionExpression)
	{
		base.VisitFunctionExpression(functionExpression);
		VisitFunction(functionExpression);
		return functionExpression;
	}

	protected override object VisitFunctionDeclaration(FunctionDeclaration functionDeclaration)
	{
		base.VisitFunctionDeclaration(functionDeclaration);
		VisitFunction(functionDeclaration);
		return functionDeclaration;
	}

	private void VisitFunction(IFunction function)
	{
		var bindings = GetBindings((Node)function, 1).ToList();

		// empty bindings => local unnamed function
		if (bindings.Count > 0)
		{
			var functionMapEntry = new FunctionMapEntry(
				bindings,
				_sourceMap.GetDeminifiedMethodName(bindings),
				GetSourcePosition(function.Body.Location.Start),
				GetSourcePosition(function.Body.Location.End));

			_functionMap.Add(functionMapEntry);
		}
	}

	// esprima use 1-based line counter
	// https://github.com/estree/estree/blob/master/es5.md#node-objects
	private static SourcePosition GetSourcePosition(Position position) => new(position.Line - 1, position.Column);

	/// <summary>
	/// Gets the name and location information related to the function name binding for a function-like nodes.
	/// </summary>
	private IEnumerable<BindingInformation> GetBindings(Node node, int parentIndex)
	{
		var parent = TryGetParentAt(parentIndex);

		if (parent != null)
		{
			// walk another branch of compound expression
			if (parent is MemberExpression memberExpression)
			{
				if (node == memberExpression.Object)
				{
					foreach (var parentBinding in GetBindings(parent, parentIndex + 1))
					{
						yield return parentBinding;
					}
				}
			}
			else if (parent is AssignmentPattern assignmentPattern)
			{
				if (node == assignmentPattern.Right)
				{
					foreach (var parentBinding in GetBindings(assignmentPattern.Left, parentIndex + 1))
					{
						yield return parentBinding;
					}
				}
			}
			else if (parent is AssignmentExpression assignment)
			{
				if (node == assignment.Right)
				{
					foreach (var parentBinding in GetBindings(assignment.Left, parentIndex + 1))
					{
						yield return parentBinding;
					}
				}
			}
			else if (parent is BinaryExpression binary)
			{
				if (node == binary.Right)
				{
					foreach (var parentBinding in GetBindings(binary.Left, parentIndex + 1))
					{
						yield return parentBinding;
					}
				}
			}
			// stop parent analysis on statement level
			else if (parent is Statement)
			{
			}
			// other non-statement (e.g. expression) nodes: climb-up
			else
			{
				foreach (var parentBinding in GetBindings(parent, parentIndex + 1))
				{
					yield return parentBinding;
				}
			}
		}

		// extract binding information from current node
		foreach (var binding in GetBindingFromNode(node))
		{
			yield return binding;
		}
	}

	private static IEnumerable<BindingInformation> GetBindingFromNode(Node node)
	{
		// try to extract name from function-like node
		if (node is IFunction currentFunction)
		{
			if (currentFunction.Id != null)
			{
				foreach (var binding in GetBindingFromNode(currentFunction.Id))
				{
					yield return binding;
				}
			}
		}
		// extract class name from class expression
		else if (node is ClassExpression classExpression)
		{
			if (classExpression.Id != null)
			{
				foreach (var binding in GetBindingFromNode(classExpression.Id))
				{
					yield return binding;
				}
			}
		}
		// extract variable name from variable declaration
		else if (node is VariableDeclarator variableDeclarator)
		{
			foreach (var binding in GetBindingFromNode(variableDeclarator.Id))
			{
				yield return binding;
			}
		}
		// extract object+member names from member expression
		else if (node is MemberExpression member)
		{
			foreach (var binding in GetBindingFromNode(member.Object))
			{
				yield return binding;
			}

			foreach (var binding in GetBindingFromNode(member.Property))
			{
				yield return binding;
			}
		}
		// extract class method name
		else if (node is MethodDefinition method)
		{
			foreach (var binding in GetBindingFromNode(method.Key))
			{
				yield return binding;
			}
		}
		// extract class property name
		else if (node is Property property)
		{
			foreach (var binding in GetBindingFromNode(property.Key))
			{
				yield return binding;
			}
		}
		// extract identifier name (target branch for all named objects)
		else if (node is Identifier identifier)
		{
			if (identifier.Name != null)
			{
				yield return new BindingInformation(identifier.Name, GetSourcePosition(identifier.Location.Start));
			}
		}
		// extract identifier name for literal-named members
		// e.g. obj['some-literal-name']
		else if (node is Literal literal)
		{
			yield return new BindingInformation(literal.Raw, GetSourcePosition(literal.Location.Start));
		}

		yield break;
	}
}
