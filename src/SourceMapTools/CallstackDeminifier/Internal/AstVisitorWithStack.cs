using System.Collections.Generic;
using Esprima.Ast;
using Esprima.Utils;

namespace SourcemapTools.CallstackDeminifier.Internal;

// extension of esprima visitor with following additions:
// - provides access to parent nodes stack (AncestorNodes extension enumerates whole AST tree on each call)
internal abstract class AstVisitorWithStack : AstVisitor
{
	private readonly List<Node> _parentStack = [];
	protected IReadOnlyList<Node> ParentStack => _parentStack;

	/// <summary>
	/// Returns parent node at specified position.
	/// </summary>
	/// <param name="offset">Zero index value returns current node; one corresponds to direct parent of current node.</param>
	/// <returns>Returns parent node at specified position or null for out-of-range index.</returns>
	protected Node? TryGetParentAt(int offset) => _parentStack.Count < offset + 1 ? null : _parentStack[_parentStack.Count - 1 - offset];

	public override object Visit(Node node)
	{
		_parentStack.Add(node);
		base.Visit(node);
		_parentStack.RemoveAt(_parentStack.Count - 1);
		return node;
	}
}
