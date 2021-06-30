function a()
{
}
window.foo = a;
a.prototype =
{
	b: function () {
		return a.a(void 0)
	}
};
a.a = function (b) {
	return b.length
};
function c() {
	return (new a).b()
}
window.foo.bar = a.b;
window.foo.bar2 = a.a;
window.bar = c;
window.onerror = function (b, e, f, g, d) {
	d
		? document.getElementById("callstackdisplay").innerText = d.stack
		: window.event.error && (document.getElementById("callstackdisplay").innerText = window.event.error.stack)
};
window.onload = function () {
	document.getElementById("crashbutton").addEventListener("click", function () {
		console.log(c())
	})
};

//[\"mynamespace.objectWithMethods\",\"window\",\"prototype\",\"prototypeMethodLevel1\",\"mynamespace.objectWithMethods.propertyMethodLevel2\",\"propertyMethodLevel2\",\"x\",\"e\",\"length\",\"GlobalFunction\",\"mynamespace.objectWithMethods.prototypeMethodLevel1\",\"onerror\",\"window.onerror\",\"message\",\"source\",\"lineno\",\"colno\",\"error\",\"document\",\"getElementById\",\"innerText\",\"stack\",\"event\",\"onload\",