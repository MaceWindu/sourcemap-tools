using System.Collections.Generic;
using System.Linq;
using Acornima;
using Acornima.Ast;
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

	protected override object? VisitArrowFunctionExpression(ArrowFunctionExpression node)
	{
		base.VisitArrowFunctionExpression(node);
		VisitFunction(node);
		return node;
	}

	protected override object? VisitFunctionExpression(FunctionExpression node)
	{
		base.VisitFunctionExpression(node);
		VisitFunction(node);
		return node;
	}

	protected override object? VisitFunctionDeclaration(FunctionDeclaration node)
	{
		base.VisitFunctionDeclaration(node);
		VisitFunction(node);
		return node;
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

	// Acornima uses 1-based line counter
	// https://github.com/estree/estree/blob/master/es5.md#node-objects
	private static SourcePosition GetSourcePosition(Position position) => new(position.Line - 1, position.Column);

	/// <summary>Gets the name and location information related to the function name binding for a function-like nodes.</summary>
	private IEnumerable<BindingInformation> GetBindings(Node node, int parentIndex)
	{
		var parent = TryGetParentAt(parentIndex);

		if (parent is not null)
		{
			foreach (var parentBinding in GetParentBindings(node, parent, parentIndex))
			{
				yield return parentBinding;
			}
		}

		// extract binding information from current node
		foreach (var binding in GetBindingFromNode(node))
		{
			yield return binding;
		}
	}

	/// <summary>Climbs up a compound expression to collect bindings contributed by the parent node.</summary>
	private IEnumerable<BindingInformation> GetParentBindings(Node node, Node parent, int parentIndex)
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

	private static IEnumerable<BindingInformation> GetBindingFromNode(Node node) => node switch
	{
		// function-like node: extract name from its identifier
		IFunction currentFunction => currentFunction.Id is not null ? GetBindingFromNode(currentFunction.Id) : [],
		// class expression: extract class name
		ClassExpression classExpression => classExpression.Id is not null ? GetBindingFromNode(classExpression.Id) : [],
		// variable declaration: extract variable name
		VariableDeclarator variableDeclarator => GetBindingFromNode(variableDeclarator.Id),
		// member expression: extract object+member names
		MemberExpression member => GetMemberBindings(member),
		// class method/property: extract its name
		MethodDefinition method => GetBindingFromNode(method.Key),
		Property property => GetBindingFromNode(property.Key),
		// identifier name (target branch for all named objects)
		Identifier identifier => GetIdentifierBinding(identifier),
		// literal-named members, e.g. obj['some-literal-name']
		Literal literal => [new BindingInformation(literal.Raw, GetSourcePosition(literal.Location.Start))],
		_ => [],
	};

	private static IEnumerable<BindingInformation> GetMemberBindings(MemberExpression member)
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

	private static IEnumerable<BindingInformation> GetIdentifierBinding(Identifier identifier)
		=> identifier.Name is not null
			? [new BindingInformation(identifier.Name, GetSourcePosition(identifier.Location.Start))]
			: [];
}
