/* Based on the Base 64 VLQ implementation in Closure Compiler:
 * https://github.com/google/closure-compiler/blob/master/src/com/google/debugging/sourcemap/Base64VLQ.java
 *
 * Copyright 2011 The Closure Compiler Authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System.Collections.Generic;

namespace SourcemapTools.SourcemapParser.Internal;

/// <summary>
/// This class provides a mechanism for converting a Base64 Variable-length quantity (VLQ)
/// into an array of integers.
/// </summary>
public static class Base64VlqDecoder
{
	/// <summary>
	/// Converts to a two-complement value from a value where the sign bit is
	/// is placed in the least significant bit.For example, as decimals:
	/// <list type="bullet">
	/// <item>2 (10 binary) becomes 1, 3 (11 binary) becomes -1;</item>
	/// <item>4 (100 binary) becomes 2, 5 (101 binary) becomes -2.</item>
	/// </list>
	/// </summary>
	private static int FromVlqSigned(int value)
	{
		var negate = (value & 1) == 1;
		value >>= 1;
		return negate ? -value : value;
	}

	/// <summary>
	/// Returns a list of integers corresponding to an input string that is Base64 VLQ encoded.
	/// </summary>
	public static IReadOnlyList<int> Decode(string input)
	{
		var result = new List<int>();
		var charProvider = new Base64CharProvider(input);

		while (!charProvider.IsEmpty())
		{
			result.Add(DecodeNextInteger(charProvider));
		}

		return result;
	}

	private sealed class Base64CharProvider(string backString)
	{
		private readonly string _backingString = backString;
		private int _currentIndex;

		internal char ReadNextCharacter()
		{
			var nextChar = _backingString[_currentIndex];
			_currentIndex++;
			return nextChar;
		}

		/// <summary>
		/// Returns true when no more characters can be provided.
		/// </summary>
		internal bool IsEmpty() => _currentIndex >= _backingString.Length;
	}

	/// <summary>
	/// Reads characters from the Base64CharProvider until a complete integer value has been extracted.
	/// </summary>
	private static int DecodeNextInteger(Base64CharProvider charProvider)
	{
		var result = 0;
		bool continuation;
		var shift = 0;
		do
		{
			var c = charProvider.ReadNextCharacter();
			var digit = Base64Converter.FromBase64(c);
			continuation = (digit & Base64VlqConstants.VlqContinuationBit) != 0;
			digit &= Base64VlqConstants.VlqBaseMask;
			result += digit << shift;
			shift += Base64VlqConstants.VlqBaseShift;
		} while (continuation);

		return FromVlqSigned(result);
	}
}
