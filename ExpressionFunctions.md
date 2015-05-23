**`Parent`: structure**

> The parent instance of the current structure instance.

**`PrevSibling`: structure**

> The previous instance in the list of children of this instance's parent.

**`EndsWith(string1, string2)`: boolean**

> true if _string1_ ends with _string2_, false otherwise.

**`StructOffset`: int**

> Absolute offset of the start of the current structure instance from the start of file.

**`CurOffset`: int**

> Absolute offset of the start of the current field from the start of file.

**`ParentCount`: int**

> Number of parent instances for the current structure instance (0 for the top-level instance).

**`StructName`: string**

> Name of the structure definition for the current structure instance.

**`FileSize`: int**

> Size of the stream (file, blob etc.) from which the current structure instance was loaded.

**`SizeOf(name)`: int**

> Constant data size for the structure definition _name_.

**`ChildIndex`: int**

> The index of the current structure instance in the list of its parent's children.