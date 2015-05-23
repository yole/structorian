The following attributes are common for all field types:

**tag** = _string_   (default attribute)

> The field tag is its name that is shown to the user. It is shown
> in the field view and in the column captions of the table view.
> For hidden fields, the tag value is ignored. If the tag doesn't
> contain spaces or other characters that require string quoting,
> it can also be used to refer to the field in expressions.
> If the tag is not specified, the default tag "Unknown" is used.

**id** = _string_

> The field ID is used to refer to the field in expressions.
> It should be specified only if the tag contains spaces or other
> characters that require string quoting.

> Example:

```
      i32 "BIFF count" [id=BIFFCount];
```

**hidden** = 1|0

> Fields that have the `hidden` attribute are not shown to the user.
> This attribute can be used for various marker or offset fields.

> Example:

```
      [hidden] str [len=2] PESignature;
      if (PESignature == "PE")
      {
        child PEHeader [offset=NewHdrOffset];
      }
```

**readonly** = 1|0

> You can set a `readonly` attribute on a field to disallow editing
> this field. This can be helpful for fields that cannot be changed
> without corrupting the structure of the file, like various offset
> fields.