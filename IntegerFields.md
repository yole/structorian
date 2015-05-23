Integer fields are probably the most commonly used field types in
Structorian.  All integer fields can be used in expressions.

The following integer field types are supported:

| i | Signed decimal numbers |
|:--|:-----------------------|
| u | Unsigned decimal numbers |
| x | Hexadecimal numbers    |
| bits | Binary numbers         |
| enum | Enums                  |
| set | Sets                   |

The following attributes are common for all integer field types:

**size** = 1 | 2 | 4

> Specifies the size of the integer field in bytes. For each field
> type, a number of standard aliases is registered for each possible
> field size. For example, the types `i8`, `i16` and `i32` can be
> used instead of `i [size=1]`, `i [size=2]` and `i [size=4]`.

**value** = _expression_

> Specifies the expression that is evaluated to obtain the value
> of the field. If the `value` attribute is specified, the field
> does not consume any bytes from the data file, and the `size`
> attribute must not be specified.

> Here is an example of using a calculated hexadecimal field::

```
      [hidden] u8 _lsb;
      [hidden] u8 _msb;
      x wheel_value [value=(_lsb + (_msb << 7))];
```

Integer fields can also be used within bitfields (described in detail
below, with examples). For fields within bitfields, the following
attributes must be specified:

**frombit** = _number_

> Starting bit of the value. Must be within range from 0 to the
> size of the containing bitfield minus one.

**tobit** = _number_

> Ending bit of the value. Must be within range from the
> `frombit` value to the size of the containing bitfield minus one.
> For a value that occupies 1 bit, the `frombit` and `tobit` values
> should be equal.

For `i`, `u`, `x`, and `bits` fields no other attributes are supported.
The `i`, `u` and `x` fields are shown as decimal or hexadecimal numbers,
and the `bits` fields are shown as a sequence of binary digits with the
specified size. For `enum` and `set` fields, however, an additional
attribute is required:

**enum** = _enumName_   (required)

> Specifies the name of the enum used to interpret the value. It is
> not required that the enum should be defined before the field
> declaration in the file.

> When an enum field is loaded, Structorian searches the enum
> definition for a constant that has a value equal to the value
> loaded from the data file. If such a constant is found, the name
> of the constant is displayed. If the constant is not found,
> the loaded value is shown as a number and highlighted with
> the unknown data color.

> When a set field is loaded, Structorian loops through all nonzero
> bits in the loaded value. For each nonzero bit, it searches for
> a constant with the value equal to the index of the bit.
> If the constant is found, its name is appended to the displayed
> field value. Otherwise, Structorian adds ```<bitN>``` to the
> displayed field value, where N is the index of the bit,
> and highlights the value with the unknown data color.

> This is best illustrated by an example. Consider the following
> definitions::

```
      enum WorldMapAreaFlags { Visible, "Can be visited"=2, Visited }
      ...
      set32 Flags [enum=WorldMapAreaFlags];
```

> If the value loaded from the data file is equal to 0, Structorian
> will just show an empty value in the cell, since no bits are set.

> If the loaded value is 1, Structorian will show "Visible".
> The bit with the index 0 is set, so Structorian displays the name
> of the constant with the value 0.

> Now suppose the loaded value is 7. The bits with the indexes
> 0, 1, 2 are set in the value. However, there are only constants
> with values 0 and 2; there is no constant for the value 1.
> Therefore, Structorian will display "Visible, 

&lt;bit1&gt;

,
> Can be visited" and highlight the field in blue.